# CLAUDE.md

Orchestration-style saga library for .NET 10 (`SagaOrchestrator` assembly): ordered step execution with reverse-order compensating transactions, retry/timeout policies, in-memory persistence, and a runnable demo in `Program.cs`.

## Build

- SDK: .NET 10 (`global.json` pins `10.0.100`, rollForward latestMinor). C# latest, nullable + implicit usings enabled (`Directory.Build.props`).
- `dotnet restore`
- `dotnet build -c Release` (or `make build`)
- `dotnet run` runs the demo in `Program.cs`
- `dotnet publish -c Release -o ./publish`
- Docker: `make docker-build`, full stack via `docker-compose up -d` (`docker-compose.yml`)

## Test

- Framework: xUnit + FluentAssertions + Moq + NSubstitute. Project: `tests/dotnet-saga-orchestrator.Tests/`.
- `dotnet test` (all), or after a Release build: `dotnet test -c Release --no-build --verbosity minimal` (`make test`)
- Single test: `dotnet test --filter "FullyQualifiedName~SagaLifecycleTests"`
- CI (`.github/workflows/ci.yml`, `build.yml`): restore -> build Release -> test Release with trx logger.
- `TypeNamingConventionTests` fails the build if any public type has a doubled suffix (e.g. `FooExtensionsExtensions`); do not create such names.

## Lint / Format

- `dotnet format` (`make format`); check-only: `dotnet format --verify-no-changes` (`make check-format`)
- `make lint` = `dotnet build /p:EnforceCodeStyleInBuild=true`
- Style rules live in `.editorconfig` (4-space indent, `var` only when type is apparent, PascalCase types/members, camelCase locals). Warnings are not errors.

## Layout and entry points

Single csproj at repo root (`dotnet-saga-orchestrator.csproj`); `tests/**` and `benchmarks/**` are excluded via `Compile Remove`. Solution: `dotnet-saga-orchestrator.sln`.

```
Program.cs                 demo entry point (DI setup, 3-step order saga)
src/Core/                  domain models, enums, exceptions, builders, utilities (no external deps)
  Domain/Models            Saga, SagaStep, SagaDefinition, CompensationTransaction, SagaEvent
  Domain/Enums             SagaStatus, SagaStepStatus, CompensationStatus, CompensationStrategy
  Utilities                SagaIdGenerator, RetryPolicy, TimeoutPolicy
src/Application/Services   SagaOrchestrationService, CompensationService, SagaDefinitionService,
                           MetricsService, HealthCheckService
src/Data/Repositories      I*Repository interfaces + InMemory* implementations
src/Infrastructure/        logging, telemetry, resilience (CircuitBreaker), events, caching, http, cli visualization
src/Configuration/         SagaOptions, ServiceConfiguration, AddSagaOrchestrator() DI extensions
src/Presentation/Cli       SagaCliCommand parsing and handlers
tests/                     xUnit tests (flat, one file per type under test)
Benchmarks/, benchmarks/   BenchmarkDotNet projects
examples/                  standalone usage samples
docs/                      per-type docs; docs/ARCHITECTURE.md is the design reference
```

Wiring: `services.AddSagaOrchestrator()` registers everything; resolve `SagaDefinitionService` and `SagaOrchestrationService`. Execution is one step per `ExecuteNextStepAsync` call; failures hand off to `CompensationService`.

## Conventions

- Root namespace `SagaOrchestrator`; sub-namespaces mirror folders (`SagaOrchestrator.Core.Utilities`, `SagaOrchestrator.Application.Services`, ...). Tests use `SagaOrchestrator.Tests`.
- Every file starts with `#nullable enable` and the author header comment block; keep it.
- Companion-file pattern per type: `Foo.cs`, `FooExtensions.cs`, `FooJsonExtensions.cs`, `FooValidation.cs`. Tests mirror it: `FooTests.cs`, `FooExtensionsTests.cs`, `FooValidationTests.cs`.
- Constructor injection with `_camelCase` readonly fields and `ArgumentNullException` guards; services are interface-backed where consumed externally (`ISagaRepository`, `IMetricsService`).
- Async methods end in `Async`; IDs are prefixed strings from `SagaIdGenerator` (`saga_`, `step_`, `corr_`, `req_`, `trace_`).
- Defaults come from `SagaConstants`; do not hardcode retries/timeouts.
- XML doc comments on public members (`GenerateDocumentationFile` is on).
- Add documentation for new public types under `docs/<TypeName>.md` and a short section in `README.md`.
