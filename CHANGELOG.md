# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.2] - 2026-03-19

### Fixed
- Fix compensating transaction not executing when step fails with timeout
- Added regression test for the fix

## [2.0.0] - 2026-03-18

### Added
- Add distributed saga debugger with time-travel inspection
- Docker support with multi-stage builds
- Health check endpoints (/health, /health/ready)
- Integration test suite with xUnit
- Migration guide from v1.x

### Changed
- Upgraded to .NET 10.0
- Modern C# features (records, primary constructors)
- Improved API consistency

### Fixed
- Various edge cases found through testing

## [Unreleased]

### Planned
- Database persistence (SQL Server, PostgreSQL) via Entity Framework Core
- REST API endpoints (ASP.NET Core)
- Distributed tracing (OpenTelemetry)
- Message queue integration (RabbitMQ, Kafka)
- Web dashboard for saga monitoring

## [2.0.0] - 2026-03-01

### Changed
- Default container port changed from 80 to 8080 (non-root best practice)
- Docker runtime base image switched from `dotnet/runtime` to `dotnet/aspnet` for health check middleware and future REST API support
- Multi-stage Dockerfile rewritten with layer-cached restore for faster rebuilds
- Docker Compose file updated to Compose Specification (removed deprecated `version` key)
- HEALTHCHECK start period increased to 10s for cold-start reliability
- Container user hardened with restricted shell (`/sbin/nologin`)

### Added
- `docs/MIGRATION_v2.md` - migration guide covering all breaking changes from v1.x to v2.0

### Breaking
- Port 80 is no longer used; update all port mappings and health check URLs to 8080
- `ASPNETCORE_URLS` now defaults to `http://+:8080`

## [1.0.0] - 2025-09-22

### Added
- Five complete example projects: OrderProcessing, MoneyTransfer, TravelBooking, AdvancedRetries, CompensationStrategies, MetricsMonitoring
- Comprehensive documentation: getting-started guide, API reference, architecture overview, deployment guide, FAQ
- Docker support with Dockerfile and docker-compose.yml
- GitHub Actions CI/CD workflow with build, test, and NuGet publish pipelines
- CodeQL security scanning workflow
- Dependabot configuration for automated dependency updates
- Makefile for build automation
- NuGet packaging metadata with README embed
- `.editorconfig` for consistent code style

### Changed
- Expanded README with 10+ usage examples covering all major features
- Improved error messages throughout for better developer experience
- Enhanced XML documentation on all public APIs

### Fixed
- Concurrent access race condition in `CacheService` under high parallelism
- Edge case in `SagaTimeoutWorker` where a saga completing at the exact timeout boundary could be incorrectly marked as timed out
- `RetryPolicy` validation now rejects zero-delay configurations that could cause tight busy-loops

## [0.9.0] - 2025-08-25

### Added
- Full xUnit test suite: `SagaLifecycleTests`, `RetryPolicyTests`, `InfrastructureAndExtensionsTests`
- `FluentAssertions` and `Moq` dependencies in test project
- Test project wired into solution with correct `ProjectReference`
- `SagaVisualizationService` for ASCII state-machine rendering
- `VisualizationServiceExtensions` for DI registration

### Fixed
- `CompensationWorker` could skip a compensation record when the repository enumeration was modified concurrently
- `EventProcessingWorker` subscriber leak on repeated subscribe/unsubscribe cycles

## [0.8.0] - 2025-08-04

### Added
- CLI interface (`CliHandler`, `SagaCliCommand`) with commands: `create`, `execute`, `status`, `list`, `compensate`, `help`
- `OutputFormatter` supporting JSON, CSV, and table output modes
- `SagaMessageTemplates` for consistent human-readable status messages
- `RequestContext` for per-request correlation ID propagation

### Changed
- `SagaOrchestrationService` now surfaces structured `SagaCommandResult` on every mutation, simplifying CLI and caller code

## [0.7.0] - 2025-07-14

### Added
- `MetricsService` with real-time counters: total sagas, success rate, P50/P95/P99 duration percentiles, compensation rate
- `HealthCheckService` reporting active saga count, worker liveness, and memory pressure
- `SagaDefinitionValidator` with fluent validation rules (duplicate step names, missing URLs, timeout sanity checks)
- `ValidationExtensions` helpers used throughout the application layer

### Fixed
- `SagaIdGenerator` now uses `Guid.NewGuid()` rather than a monotonic counter, preventing ID collisions across restarts

## [0.6.0] - 2025-06-23

### Added
- `SagaTimeoutWorker` background service that polls for and expires overdue sagas
- `CompensationWorker` background service for automatic compensation retry on transient failures
- `EventProcessingWorker` background service for async event fan-out
- `ServiceConfiguration` and `InfrastructureConfiguration` DI extension methods
- `SagaOptions` strongly-typed configuration class bound to `appsettings` / environment variables

### Changed
- Background workers run on dedicated `IHostedService` threads; they no longer contend with the main orchestration hot path

## [0.5.0] - 2025-06-02

### Added
- `CircuitBreaker` with configurable failure threshold and half-open recovery window
- `RateLimiter` using token-bucket algorithm; per-service limits configurable at runtime
- `CacheService` with TTL-based expiry and `CacheKeyBuilder` helpers
- `SagaJsonSerializer` wrapping `System.Text.Json` with saga-specific converters
- `ExceptionMapper` translating domain exceptions to HTTP status codes

### Fixed
- `HttpClientFactory` retry handler was not propagating `CancellationToken` to inner handlers

## [0.4.0] - 2025-05-12

### Added
- HTTP client factory (`HttpClientFactory`) with built-in retry and timeout resilience
- In-process event bus (`EventBus`, `EventObserver`) with typed pub/sub and wildcard subscriptions
- `WebhookHandler` for outbound webhook delivery with configurable retry
- `ServiceRegistry` for tracking external service endpoints and metadata
- `LoggingMiddleware` for structured per-request logging with correlation IDs
- `SagaEventPublisher` integrating the domain event model with the event bus

### Changed
- `SagaOrchestrationService` now publishes `SagaStarted`, `SagaCompleted`, `SagaFailed`, `SagaCompensated` events automatically

## [0.3.0] - 2025-04-21

### Added
- Five compensation strategies: `ReverseOrder` (LIFO), `ForwardOrder` (FIFO), `FromFailurePoint`, `Parallel`, `Manual`
- `CompensationService` orchestrating compensation execution per strategy
- `CompensationTransaction` domain model tracking per-step rollback status
- `InMemoryCompensationTransactionRepository` with full CRUD
- `CompensationStatus` and `CompensationStrategy` enums
- `SagaStepBuilder` fluent builder for step definition construction

### Fixed
- Parallel compensation strategy was awaiting tasks sequentially due to a missing `Task.WhenAll`

## [0.2.0] - 2025-04-01

### Added
- `RetryPolicy` with exponential backoff, configurable multiplier, jitter, and per-step max delay
- `TimeoutPolicy` for saga-level and step-level deadline enforcement
- `DateTimeExtensions` and `StringExtensions` utility methods
- `CollectionExtensions` with batch and safe-enumeration helpers
- `EnumExtensions` for display-name resolution
- `SagaConstants` centralising all default values (timeouts, retries, rate limits)

### Changed
- `SagaStep` now stores `AttemptCount` and `LastAttemptAt` for accurate retry accounting
- `SagaOrchestrationService.ExecuteNextStepAsync` honours the step-level retry policy before marking a step as failed

## [0.1.0] - 2025-03-10

### Added
- Initial release — core saga orchestration engine
- `Saga`, `SagaDefinition`, `SagaStep`, `SagaStepDefinition` domain models
- `SagaOrchestrationService` with create, start, execute-step, abort, and compensate operations
- `SagaDefinitionService` for definition CRUD and step management
- `SagaStatus` and `SagaStepStatus` enums covering the full state machine
- In-memory repository implementations: `InMemorySagaRepository`, `InMemorySagaStepRepository`, `InMemorySagaDefinitionRepository`
- `SagaIdGenerator` for unique saga and step IDs
- Exception hierarchy: `SagaException`, `SagaNotFoundException`, `SagaStepExecutionException`, `SagaTimeoutException`, `InvalidSagaDefinitionException`
- `SagaEvent` model for lifecycle event capture
- Basic DI registration via `ServiceConfiguration`
- Console logging with correlation ID support
- `SagaResponseMapper` and response DTOs (`SagaResponse`, `SagaCommandResult`, `CreateSagaRequest`)

---

## Version History

| Version | Date       | Highlights                                   |
|---------|------------|----------------------------------------------|
| 0.1.0   | 2025-03-10 | Core orchestration engine                    |
| 0.2.0   | 2025-04-01 | Retry & timeout policies                     |
| 0.3.0   | 2025-04-21 | Compensation strategies                      |
| 0.4.0   | 2025-05-12 | HTTP client, event bus, webhooks             |
| 0.5.0   | 2025-06-02 | Circuit breaker, rate limiter, caching       |
| 0.6.0   | 2025-06-23 | Background workers, configuration            |
| 0.7.0   | 2025-07-14 | Metrics, health checks, validation           |
| 0.8.0   | 2025-08-04 | CLI interface, output formatting             |
| 0.9.0   | 2025-08-25 | Test suite, visualization service            |
| 1.0.0   | 2025-09-22 | Stable release, docs, examples, packaging    |
| 2.0.0   | 2026-03-01 | Docker v2, port 8080, migration guide         |

---

## Support

- **Issues**: https://github.com/Sarmkadan/dotnet-saga-orchestrator/issues
- **Website**: https://sarmkadan.com

---

**Built by [Vladyslav Zaiets](https://sarmkadan.com)**
