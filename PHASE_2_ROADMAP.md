# Phase 2 - Infrastructure & Integration (Planned Roadmap)

## Overview

Phase 2 of the dotnet-saga-orchestrator project is designed to extend the Phase 1 core orchestration engine with enterprise-grade infrastructure layers, integration components, and operational utilities.

**Status:** Roadmap (Planned for implementation)  
**Target Files:** 30+ new infrastructure files  
**Target LOC:** 4,000+ lines of production code  
**Framework:** .NET 10 with latest C# features

## Planned Components

### 1. Presentation Layer (CLI & API)

#### CLI Interface
- **SagaCliCommand.cs** - Full command parsing with validation
  - Commands: create, execute, status, list, compensate, help
  - Argument parsing and validation
  - Help text generation

- **CliHandler.cs** - Command routing and execution
  - Command dispatcher
  - Result formatting

#### API Controllers (Planned for Phase 3)
- REST API endpoints for all saga operations
- Request/response models with validation
- Error handling and status code mapping

### 2. Infrastructure Layer

#### HTTP Integration
- **HttpClientFactory.cs** - Resilient HTTP client
  - Retry policy (exponential backoff)
  - Circuit breaker pattern
  - Custom headers and authentication
  - Connection pooling with Polly integration

#### Caching
- **CacheService.cs** - In-memory cache with TTL
  - Thread-safe operations
  - Automatic expiration cleanup
  - Get/Set/Remove/Clear operations
  
- **CacheKeyBuilder.cs** - Standardized cache key generation
  - Type-safe key generation
  - Hierarchical key organization
  - Easy key pattern management

#### Event System (Pub/Sub)
- **EventBus.cs** - In-memory pub/sub implementation
  - Saga lifecycle events
  - Step execution events
  - Compensation events
  - Event history tracking

- **EventObserver.cs** - Event consumer pattern
  - Webhook trigger events
  - Domain event processing
  - Side-effect handlers

#### Logging
- **LoggingMiddleware.cs** - Structured logging
  - Saga lifecycle logging
  - Step execution tracking
  - Compensation event logging
  - Detailed error logging with context

#### Serialization
- **SagaJsonSerializer.cs** - Custom JSON serialization
  - Enum converters (camelCase)
  - DateTime ISO 8601 formatting
  - Polymorphic type handling
  - Indented and compact output

#### Output Formatting
- **OutputFormatter.cs** - Multi-format output
  - JSON (pretty and compact)
  - CSV export with headers
  - ASCII table for CLI
  - Extensible format system

#### Integration Modules
- **WebhookHandler.cs** - External system integration
  - Webhook subscriptions
  - Event delivery with retry
  - Subscription management
  - Delivery tracking

- **ServiceRegistry.cs** - Microservice tracking
  - Service registration
  - Health status monitoring
  - Service metadata
  - Query and filtering

#### Rate Limiting
- **RateLimiter.cs** - Token bucket rate limiter
  - Sliding window algorithm
  - Configurable throughput
  - Per-service rate limits
  - Status tracking

#### Resilience Patterns
- **CircuitBreaker.cs** - Fault tolerance
  - Three states: Closed, Open, HalfOpen
  - Configurable failure threshold
  - Automatic recovery
  - Per-resource state

#### Background Workers
- **SagaTimeoutWorker.cs** - Timeout monitoring
  - Saga timeout detection
  - Step-level timeout handling
  - Auto-abort expired sagas
  - Configurable intervals

- **CompensationWorker.cs** - Failure processing
  - Automatic compensation initiation
  - Failed saga detection
  - Compensation tracking
  - Completion monitoring

- **EventProcessingWorker.cs** - Event management
  - Event queue processing
  - History archival
  - Cleanup and maintenance
  - Event statistics

#### Request Context
- **RequestContext.cs** - Request tracking
  - Correlation IDs
  - User/tenant context
  - Metadata storage
  - Elapsed time tracking

- **PerformanceTracker.cs** - Execution timing
  - Operation-level timing
  - Aggregated metrics
  - Performance analysis

### 3. Core Extensions (130+ utilities)

#### String Extensions (30 methods)
- Case conversion (camelCase, snake_case, kebab-case, Title Case)
- String truncation with ellipsis
- Email/URL validation
- Slug generation for URLs
- Pattern matching utilities
- Character counting

#### DateTime Extensions (25 methods)
- Relative time formatting ("2 hours ago")
- Duration calculations
- Unix timestamp conversion
- Business day arithmetic
- Percentile calculations (P95, P99)
- Time range validation
- ISO 8601 formatting

#### Collection Extensions (25 methods)
- Batch/chunk operations
- Distinct by selector
- Group to dictionary
- Pagination utilities
- Flatten nested collections
- Window/sliding operations
- Min/Max by selector safe operations

#### Enum Extensions (20 methods)
- Description extraction
- Display name generation
- Enum parsing (case-insensitive)
- Value conversion
- Flags operations
- Enum member enumeration
- Dictionary generation

#### Validation Extensions (20 methods)
- Null checks with parameter names
- Range validation
- Custom validators with fluent API
- String format validation
- Collection validation
- Enum value validation
- Email/URL validation

### 4. Application Services

#### Metrics Service
- **MetricsService.cs** - Execution statistics
  - Saga completion metrics
  - Success/failure rates
  - Performance percentiles (P95, P99)
  - Step-level metrics
  - Duration analysis

#### Health Check Service
- **HealthCheckService.cs** - System monitoring
  - Service health status
  - Active saga count
  - Uptime tracking
  - Metric aggregation
  - System diagnostics

#### Validation Services
- **SagaDefinitionValidator.cs** - Definition validation
  - Structure validation
  - Step validation
  - Order integrity checks
  - Size limits
  - URL validation

- **SagaRequestValidator.cs** - Request validation
  - Input sanitization
  - Length checks
  - Format validation

### 5. Utility Components

#### Response Models
- **SagaCommandResult.cs** - Standardized responses
  - Success/failure results
  - Generic result wrappers
  - Error details
  - Request tracking

- **HealthCheckResponse.cs** - Health check data
  - Status information
  - Uptime metrics
  - Saga counts

#### Mappers
- **SagaResponseMapper.cs** - DTO mapping
  - Domain model to response conversion
  - Step response mapping
  - Safe null handling

#### Exception Handling
- **ExceptionMapper.cs** - Exception to HTTP mapping
  - Status code assignment
  - Error code generation
  - Message mapping
  - Error response formatting

#### Message Templates
- **SagaMessageTemplates.cs** - Event message formatting
  - Saga creation messages
  - Step execution messages
  - Compensation messages
  - Timeout messages
  - Health status messages
  - Service registry messages

#### Configuration
- **InfrastructureConfiguration.cs** - DI registration
  - Service registration
  - Selective feature enablement
  - Dependency injection setup

- **SagaOptions.cs** - Configuration options
  - Timeout policies
  - Retry strategies
  - Cache policies
  - Worker settings
  - Webhook policies
  - Fluent builder API

#### Fluent Builders
- **SagaStepBuilder.cs** - Saga step creation
  - Fluent API for configuration
  - Order and timeout setup
  - Retry policy configuration
  - Compensation URL setup
  - Circuit breaker settings

- **SagaDefinitionBuilder.cs** - Definition creation
  - Progressive step addition
  - Validation during build
  - Chainable API

## Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│           Presentation Layer (CLI)                  │
│  ┌──────────────────────────────────────────────┐  │
│  │  CliHandler    │  CliCommand    │  Results  │  │
│  └──────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│         Application Services Layer                   │
│  ┌─────────────┬──────────────┬──────────────────┐  │
│  │  Orchestr.  │  Definition  │  Compensation   │  │
│  │  Services   │  Services    │  Services       │  │
│  └─────────────┴──────────────┴──────────────────┘  │
│  ┌──────────────┬──────────────┬──────────────────┐ │
│  │  Validators  │  Mappers     │  Metrics        │ │
│  └──────────────┴──────────────┴──────────────────┘ │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│        Infrastructure Layer                          │
│  ┌──────────┬──────────┬──────────┬──────────────┐  │
│  │  HTTP    │  Events  │  Cache   │  Logging    │  │
│  │  Client  │  Bus     │  Layer   │  Middleware │  │
│  └──────────┴──────────┴──────────┴──────────────┘  │
│  ┌──────────┬──────────┬──────────┬──────────────┐  │
│  │  Webhooks│  Service │  Rate    │  Circuit    │  │
│  │  Handler │  Registry│  Limiter │  Breaker    │  │
│  └──────────┴──────────┴──────────┴──────────────┘  │
│  ┌──────────┬──────────┬──────────┬──────────────┐  │
│  │  Timeout │  Compens │  Event   │  Serialization
│  │  Worker  │  Worker  │  Processor            │  │
│  └──────────┴──────────┴──────────┴──────────────┘  │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│         Core Extensions (130+ methods)               │
│  String • DateTime • Collections • Enums • Validation
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│        Phase 1: Core Domain & Services              │
│        (Saga, Step, Compensation, Event models)    │
└─────────────────────────────────────────────────────┘
```

## Implementation Priorities

### Priority 1 (Critical)
- HTTP client factory with resilience
- Event bus and pub/sub
- Caching layer
- Logging infrastructure
- Background workers

### Priority 2 (Important)
- Service registry
- Rate limiting
- Circuit breaker
- Metrics service
- Validation services

### Priority 3 (Enhanced)
- CLI interface
- Output formatters
- Webhook handler
- Configuration builder
- Extension methods

## Expected File Structure

```
src/
├── Presentation/
│   └── Cli/
│       ├── Commands/
│       │   └── SagaCliCommand.cs
│       └── CliHandler.cs
├── Infrastructure/
│   ├── Http/
│   │   └── HttpClientFactory.cs
│   ├── Caching/
│   │   ├── CacheService.cs
│   │   └── CacheKeyBuilder.cs
│   ├── Events/
│   │   ├── EventBus.cs
│   │   └── EventObserver.cs
│   ├── Logging/
│   │   └── LoggingMiddleware.cs
│   ├── Serialization/
│   │   └── SagaJsonSerializer.cs
│   ├── Formatting/
│   │   └── OutputFormatter.cs
│   ├── Integration/
│   │   ├── WebhookHandler.cs
│   │   └── ServiceRegistry.cs
│   ├── RateLimiting/
│   │   └── RateLimiter.cs
│   ├── Resilience/
│   │   └── CircuitBreaker.cs
│   ├── Context/
│   │   └── RequestContext.cs
│   ├── BackgroundWorkers/
│   │   ├── SagaTimeoutWorker.cs
│   │   ├── CompensationWorker.cs
│   │   └── EventProcessingWorker.cs
│   └── Messaging/
│       └── SagaMessageTemplates.cs
├── Core/
│   ├── Extensions/
│   │   ├── StringExtensions.cs
│   │   ├── DateTimeExtensions.cs
│   │   ├── CollectionExtensions.cs
│   │   ├── EnumExtensions.cs
│   │   └── ValidationExtensions.cs
│   ├── Builders/
│   │   └── SagaStepBuilder.cs
│   └── Exceptions/
│       └── ExceptionMapper.cs
└── Application/
    ├── Services/
    │   ├── HealthCheckService.cs
    │   └── MetricsService.cs
    ├── Validators/
    │   └── SagaDefinitionValidator.cs
    ├── Mappers/
    │   └── SagaResponseMapper.cs
    └── DTOs/
        └── SagaCommandResult.cs
```

## Key Metrics

- **Planned Files:** 30+
- **Planned LOC:** 4,000+
- **Extension Methods:** 130+
- **Interfaces:** 15+
- **Background Workers:** 3
- **Domain Events:** 8+

## Design Principles

1. **Separation of Concerns** - Clear layering
2. **Dependency Injection** - Testable, loose coupling
3. **Extension Methods** - Fluent, readable APIs
4. **Async/Await** - All I/O operations async
5. **Thread Safety** - Lock-based synchronization
6. **Type Safety** - Leverage C# 10+ features
7. **Error Handling** - Comprehensive exception mapping
8. **Performance** - Optimized algorithms and caching

## Future Enhancements

### Database Persistence (Phase 3)
- EF Core mapping
- SQL Server/PostgreSQL support
- Migration framework
- Query optimization

### REST API (Phase 3)
- ASP.NET Core integration
- Swagger/OpenAPI documentation
- Authentication/Authorization
- CORS support

### Message Queue Integration (Phase 4)
- RabbitMQ support
- Kafka integration
- Dead letter queues
- Message routing

### Distributed Tracing (Phase 4)
- OpenTelemetry integration
- Jaeger exporter
- Custom instrumentation
- Performance profiling

## Testing Strategy

The Phase 2 infrastructure is designed for testability:

- Unit tests for all utility methods
- Integration tests for services
- Mock-friendly interfaces
- In-memory implementations for testing
- Event-based verification

## Performance Characteristics

- **Cache Lookups:** O(1)
- **Rate Limiting:** O(1) per request
- **Circuit Breaker:** O(1) state check
- **Event Pub/Sub:** O(n) subscribers
- **Serialization:** Optimized JSON with reusable converters

## Conclusion

Phase 2 represents a significant expansion of the saga orchestrator with production-grade infrastructure. It provides:

- ✅ Enterprise integration capabilities
- ✅ Operational monitoring and metrics
- ✅ Resilience patterns (circuit breaker, rate limiting)
- ✅ Extensible validation and transformation
- ✅ CLI and API foundations
- ✅ 130+ utility methods for common operations
- ✅ Event-driven architecture support
- ✅ Comprehensive logging and tracking

This roadmap ensures the saga orchestrator is production-ready for enterprise microservice environments.

## Author

**Vladyslav Zaiets**
- Website: https://sarmkadan.com
- Email: rutova2@gmail.com
- Role: CTO & Software Architect

## License

MIT License - Copyright (c) 2026 Vladyslav Zaiets
