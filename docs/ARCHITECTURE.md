# Architecture

This document describes how the saga orchestrator is actually put together - the layers, the main types, how a saga flows through the system, and the trade-offs behind the bigger decisions.

## Overview

The project is a single .NET 10 executable (`SagaOrchestrator`, `dotnet-saga-orchestrator.csproj`) that ships both as a library-style API (everything wired through `IServiceCollection` extensions) and a runnable demo (`Program.cs` at the repo root, which builds a 3-step order-processing saga and drives it end to end). Tests (`tests/`, xUnit) and BenchmarkDotNet benchmarks (`Benchmarks/`, `benchmarks/`) live in the same repo but are excluded from the main compile via `Compile Remove` in the csproj.

It implements the **orchestration flavor** of the saga pattern: a central orchestrator holds the saga state, executes steps in order, and runs compensating transactions in reverse order when a step fails. There is no choreography/event-sourcing variant here - state lives in repositories, not in an event stream.

## Layer breakdown

Layout under `src/` follows a fairly classic onion, with `Core` at the center:

```
src/
  Core/            domain models, enums, exceptions, builders, utilities (no external deps)
  Application/     services, DTOs, mappers, validators (orchestration logic lives here)
  Data/            repository interfaces + in-memory implementations
  Infrastructure/  cross-cutting: logging, telemetry, resilience, events, caching, ...
  Configuration/   DI registration (IServiceCollection extensions) and options
  Presentation/    thin CLI handler and commands
```

### Core (`src/Core`)

- **Domain models** (`Domain/Models`): `Saga`, `SagaStep`, `SagaDefinition`, `SagaStepDefinition`, `CompensationTransaction`, `SagaEvent`, `SagaDebugSnapshot`. These are behavior-carrying entities, not anemic DTOs - e.g. `Saga.Initialize(definition, maxRetries, timeoutSeconds)` builds the step list from a definition, and `SagaStep.Compensate()` mutates step state.
- **Enums** (`Domain/Enums`): `SagaStatus`, `SagaStepStatus`, `CompensationStatus`, `CompensationStrategy` - the state machines are expressed through these.
- **Utilities**: `SagaIdGenerator` (prefixed ids: `saga_…`, `step_…`, `corr_…`, plus validators), `RetryPolicy`, `TimeoutPolicy`.
- **Builders**: `SagaStepBuilder` - fluent construction of step definitions.
- **Constants**: `SagaConstants` holds the defaults (`DefaultMaxRetries`, `DefaultSagaTimeoutSeconds`, etc.) that services fall back to when the caller passes nothing.

### Application (`src/Application`)

The orchestration brain. Main services:

- **`SagaOrchestrationService`** - the central coordinator. `CreateSagaAsync(definition)` validates the definition and materializes a `Saga`; `StartSagaAsync` transitions it to running; `ExecuteNextStepAsync` executes exactly one step (the demo loop in `Program.cs` calls it per step). On failure it hands off to compensation.
- **`CompensationService`** - reverse-order undo: `BeginCompensationAsync(saga)` creates `CompensationTransaction`s for completed steps, `ExecuteNextCompensationAsync` runs them one at a time, `RetryCompensationAsync` / `CheckTimeoutsAsync` handle stuck compensations.
- **`SagaDefinitionService`** - CRUD + validation over saga definitions (create definition, add steps, validate via `SagaDefinitionValidator`).
- **`MetricsService` / `HealthCheckService`** - read-side aggregation over the repositories (counts, statuses, uptime). Both are interface-backed (`IMetricsService`, `IHealthCheckService`).
- **`SagaEventPublisher`, `SagaVisualizationService`** - event fan-out and rendering of saga state for humans.

DTOs (`CreateSagaRequest`, `SagaResponse`, `SagaCommandResult`) plus `SagaResponseMapper` keep external shapes separate from domain entities.

### Data (`src/Data/Repositories`)

Four repository interfaces - `ISagaRepository`, `ISagaStepRepository`, `ISagaDefinitionRepository`, `ICompensationTransactionRepository` - each with an `InMemory*` implementation backed by concurrent dictionaries.

**Decision: in-memory persistence only, behind interfaces.** Rationale: the interesting logic is the orchestration state machine, not storage plumbing; in-memory keeps the project runnable with zero setup. Trade-off: sagas do not survive a process restart, which for a real deployment defeats a good part of the point of sagas. The interfaces are the seam - a SQL/Redis implementation slots in by swapping the DI registrations (see `AddSagaRepositories`). This is the number-one known limitation.

### Infrastructure (`src/Infrastructure`)

Cross-cutting concerns, each in its own folder:

- `Logging` - `ISagaLogger` / `SagaLogger` (structured saga-scoped logging over `ILogger`), plus a `LoggingMiddleware`.
- `Resilience` - `CircuitBreaker` (closed/open/half-open, covered by `CircuitBreakerTests` and `CircuitBreakerRecoveryTests`).
- `RateLimiting` - `RateLimiter` behind `IRateLimiter`.
- `Events` - `EventBus` (`IEventBus`) + `EventObserver` (`ISagaEventObserver`) for in-process pub/sub of saga lifecycle events.
- `Telemetry` - `SagaActivitySource` for `System.Diagnostics.Activity`-based tracing.
- `Debugging` - `SagaDebuggerService` / `ISagaDebugger` and `SagaDebugTimeline`: captures `SagaDebugSnapshot`s and timeline entries so you can replay what a saga did.
- `Caching`, `Serialization` (`SagaJsonSerializer`), `Http` (`HttpClientFactory`), `Integration` (`IServiceRegistry`/`ServiceRegistry`, `WebhookHandler`), `Messaging` (`SagaMessageTemplates`), `Visualization` (`SagaStateRenderer`), `BackgroundWorkers`, `Context` (`RequestContext`), `Formatting`.

**Decision: infrastructure is opt-in.** Only the core orchestration path (repos, logger, three services) is registered by `AddSagaOrchestrator`; the rest is wired by `InfrastructureConfiguration`, `DebuggerServiceExtensions`, `VisualizationServiceExtensions`. Trade-off: you can forget to register something a feature needs, but the default container stays small and the demo has no dead weight.

### Configuration (`src/Configuration`)

`ServiceConfiguration.AddSagaOrchestrator()` is the front door: registers the four in-memory repositories, `ISagaLogger`, and the three core services as singletons (safe because the repositories are thread-safe and the services are stateless over them). Granular variants exist for partial wiring: `AddSagaRepositories()` (repos only) and `AddSagaServices()` (services only, bring your own repos). `SagaOptions` / `DebuggerOptions` / `InfrastructureConfiguration` hold tunables.

### Presentation (`src/Presentation/Cli`)

`CliHandler` + `SagaCliCommand` - a thin command layer over the application services. Deliberately dumb: parse, delegate, format via `OutputFormatter`.

## Data flow

Happy path (this is exactly what `Program.cs` demonstrates):

```
SagaDefinitionService.CreateDefinitionAsync + AddStepAsync
        v
SagaDefinitionValidator (fail fast on a bad definition)
        v
SagaOrchestrationService.CreateSagaAsync   -> Saga.Initialize copies steps from the definition
        v
StartSagaAsync                             -> SagaStatus: Created -> Running
        v
ExecuteNextStepAsync (called per step)     -> step status transitions, retries via RetryPolicy,
        |                                     timeouts via TimeoutPolicy, persisted after each change
        v
all steps Completed                        -> SagaStatus: Completed, CompletedAt stamped
```

Failure path:

```
step fails after maxRetries
        v
CompensationService.BeginCompensationAsync -> one CompensationTransaction per completed step,
        |                                     queued in REVERSE execution order
        v
ExecuteNextCompensationAsync (per txn)     -> calls the step's compensation endpoint
        v
all compensated -> SagaStatus: Compensated   (partial failure -> RetryCompensationAsync / CheckTimeoutsAsync)
```

**Decision: step-at-a-time execution driven by the caller** (`ExecuteNextStepAsync` rather than a single `RunToCompletionAsync`). Rationale: the caller controls pacing, can checkpoint/observe between steps, and testing individual transitions is trivial. Trade-off: it pushes the driving loop onto the consumer; there is no built-in background executor that pulls pending sagas forward (the `BackgroundWorkers` folder is the natural home for one).

## Extension points

- **Persistence** - implement the four repository interfaces, register them instead of the `InMemory*` ones (use `AddSagaServices()` + your own repo registrations).
- **Logging** - implement `ISagaLogger`.
- **Events** - subscribe `ISagaEventObserver`s to the `IEventBus`.
- **Step endpoints** - steps are defined by action/compensation URLs (`SagaStepDefinition`), so any HTTP-reachable service participates without code changes here.
- **Options** - `AddSagaOrchestrator(options => …)` to override `SagaOptions` defaults.

## Known limitations

1. **No durable storage** - in-memory repos only; a crash loses in-flight sagas.
2. **No background driver** - nothing advances sagas autonomously; the consumer owns the loop.
3. **Core services are concrete classes** - `SagaOrchestrationService`, `CompensationService`, `SagaDefinitionService` are registered and consumed as classes, not interfaces, so mocking them means wrapping or virtual members. Repos and infra are interface-backed; the service layer never got the same treatment.
4. **Companion-file sprawl** - many types carry sibling `*Extensions.cs` / `*JsonExtensions.cs` / `*Validation.cs` files of mixed usefulness (some even stacked, e.g. `…JsonExtensionsJsonExtensions.cs`). Consolidation is overdue.
5. **HTTP step execution is simulated** - step/compensation URLs are modeled in the domain, but the demo path does not perform real outbound HTTP calls against them.
