# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- Comprehensive documentation and API reference
- Five complete example projects (OrderProcessing, MoneyTransfer, TravelBooking, etc.)
- Metrics and health monitoring example
- Advanced retry policies documentation
- Compensation strategies comprehensive guide
- Docker support with docker-compose.yml
- GitHub Actions CI/CD workflow
- .editorconfig for code consistency
- Makefile for build automation
- FAQ section with 50+ questions and answers
- Deployment guides (local, Docker, Kubernetes, AWS, Azure, Google Cloud)
- Configuration reference documentation

### Changed
- Expanded README with 10+ usage examples
- Improved error messages for better debugging
- Enhanced logging with more contextual information
- Better timeout handling with detailed status tracking

### Fixed
- Improved compensation transaction tracking
- Fixed concurrent access issues in cache service
- Enhanced retry policy validation
- Better handling of timeout edge cases

## [1.1.0] - 2026-04-15

### Added
- Full infrastructure layer (Phase 2 implementation)
- HTTP client factory with resilience patterns
- In-memory event bus with pub/sub support
- Caching service with TTL
- Webhook integration framework
- Circuit breaker pattern implementation
- Rate limiting with token bucket algorithm
- Background workers for timeouts, compensation, and events
- Metrics service with performance tracking
- Health check service
- Request context tracking with correlation IDs
- 130+ extension methods for common operations
- Fluent builder APIs for saga configuration
- Exception mapper for HTTP error codes
- Output formatting (JSON, CSV, Table)
- Service registry for external service tracking
- Comprehensive validation framework

### Changed
- Refactored dependency injection configuration
- Improved service interfaces with cleaner contracts
- Enhanced logging throughout infrastructure
- Better separation of concerns

### Fixed
- Thread safety in background workers
- Memory management in cache service
- Proper async/await patterns throughout
- Event bus subscriber cleanup

## [1.0.0] - 2026-03-01

### Added
- Initial release (Phase 1 - Core Architecture)
- Complete saga orchestration engine
- Compensating transaction support
- Automatic retry logic with exponential backoff
- Timeout handling and detection
- Compensation strategy patterns:
  - Reverse order (LIFO)
  - Forward order (FIFO)
  - From failure point
  - Parallel
  - Manual
- In-memory repository implementations
- Saga state management
- Step execution tracking
- Exception hierarchy and custom exceptions
- Domain model layer with validation
- Service layer with core business logic
- CLI interface for saga management
- Dependency injection configuration
- Console logging
- Basic health checks
- Saga persistence in memory
- Correlation ID support
- Comprehensive test coverage ready architecture

### Features
- Saga pattern implementation
- Compensating transactions
- Retry policies per step
- Timeout detection and handling
- Event-driven architecture ready
- Extensible design for future layers
- 25+ domain classes
- 8+ service classes
- 5+ enums with full statuses
- Exception hierarchy with mappings

## [Unreleased]

### Planned for v2.0.0
- Database persistence (SQL Server, PostgreSQL)
- Entity Framework Core integration
- REST API endpoints (ASP.NET Core)
- gRPC service definitions
- Distributed tracing (OpenTelemetry)
- Message queue integration (RabbitMQ, Kafka)
- Web dashboard for saga monitoring
- Advanced diagnostics and profiling
- Performance optimization for large-scale deployments

### Planned for v2.1.0
- Saga migration and versioning
- Batch operation support
- Saga grouping and hierarchies
- Custom compensation callbacks
- Saga replay capability
- Dead letter queues
- Retry scheduling improvements

### Planned for v3.0.0
- Multi-cloud support
- Kubernetes operators
- Service mesh integration (Istio)
- Policy as code
- Cost analysis tools
- Advanced analytics
- Machine learning-based failure prediction
- GraphQL API support

## Version History Summary

| Version | Date | Focus |
|---------|------|-------|
| 0.1.0 | Jan 2026 | Project initialization |
| 1.0.0 | Mar 2026 | Core architecture (Phase 1) |
| 1.1.0 | Apr 2026 | Infrastructure layer (Phase 2) |
| 1.2.0 | May 2026 | Documentation & Examples (Phase 3) |
| 2.0.0 | TBD | Database & API (Phase 4) |

## Contributors

- **Vladyslav Zaiets** - Initial architecture and implementation
- Community contributions welcome!

## License

Copyright (c) 2026 Vladyslav Zaiets

Licensed under the MIT License - see LICENSE file for details.

---

## Breaking Changes

### 1.0.0 → 1.1.0
- No breaking changes (additive release)

### 1.1.0 → 1.2.0
- No breaking changes (documentation and examples only)

---

## Upgrade Guide

### Upgrading to 1.1.0 from 1.0.0

No code changes required. New features are:
- Update package reference
- New services available in DI container
- New extension methods accessible

```csharp
// Before
services.AddSagaOrchestrator();

// After - still works, new features optional
services.AddSagaOrchestrator()
    .WithCachingEnabled(true)
    .WithWebhooksEnabled(true);
```

### Upgrading to 1.2.0 from 1.1.0

No code changes required. Documentation and examples added.

---

## Support

- **Issues**: https://github.com/Sarmkadan/dotnet-saga-orchestrator/issues
- **Email**: rutova2@gmail.com
- **Website**: https://sarmkadan.com

---

**Built with ❤️ by [Vladyslav Zaiets](https://sarmkadan.com)**
