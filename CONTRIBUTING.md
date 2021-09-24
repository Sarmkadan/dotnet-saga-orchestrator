# Contributing to Saga Orchestrator

Thank you for your interest in contributing to the Saga Orchestrator! This document provides guidelines and instructions for contributing.

## Code of Conduct

- Be respectful and inclusive
- Assume good intent
- Focus on constructive feedback
- Welcome diverse perspectives

## How to Contribute

### Reporting Bugs

1. **Check existing issues** - Avoid duplicates
2. **Describe the bug clearly** - What happened and what was expected
3. **Provide steps to reproduce** - How to recreate the issue
4. **Include environment details** - OS, .NET version, etc.
5. **Share logs and stack traces** - If applicable

Example issue:

```
Title: Circuit breaker doesn't reset after timeout

Description:
When a service is temporarily unavailable, the circuit breaker opens correctly.
However, after the configured timeout (30s), the circuit breaker doesn't transition
to HalfOpen state and remains Open indefinitely.

Steps to reproduce:
1. Start application
2. Kill target service
3. Make request to that service
4. Wait 30+ seconds
5. Restart target service
6. Make request again

Expected: Request succeeds (circuit is HalfOpen)
Actual: Request fails (circuit still Open)

Environment:
- OS: Ubuntu 22.04
- .NET: 10.0.0
- Version: 1.2.0
```

### Suggesting Features

1. **Check existing issues** - Avoid duplicates
2. **Describe the feature** - What problem does it solve?
3. **Provide use cases** - Real-world scenarios
4. **Suggest implementation** - If you have ideas
5. **Discuss alternatives** - What else could work?

Example feature request:

```
Title: Add support for saga versioning and migration

Description:
Currently, saga definitions are immutable. This makes it difficult to update
workflows in production systems with running sagas.

Use Case:
We have an order processing saga with 100 active sagas. We need to add a
new validation step. With versioning, we could:
1. Create v2 of the saga definition
2. New sagas use v2
3. Existing sagas continue on v1
4. Support migration path for v1 → v2

Proposed Solution:
- Add version field to SagaDefinition
- Store saga version when created
- Support multiple versions concurrently
- Add migration utilities

Alternatives:
- Freeze all new sagas until old ones complete
- Manually update saga definitions in database
```

### Submitting Pull Requests

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Make your changes**
4. **Follow code style guidelines** (see below)
5. **Add/update tests**
6. **Update documentation**
7. **Commit with clear messages**
8. **Push to your fork**
9. **Create pull request with description**

#### Branch Naming

- Feature: `feature/feature-name`
- Bug fix: `fix/bug-name`
- Documentation: `docs/doc-name`
- Refactoring: `refactor/component-name`
- Performance: `perf/improvement-name`

#### Commit Message Format

Follow conventional commits:

```
<type>(<scope>): <subject>

<body>

<footer>
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `chore`

Examples:

```
feat(saga): add support for dynamic step delays

This allows saga steps to have variable execution delays based on
system load or business rules.

Closes #123
```

```
fix(compensation): handle concurrent compensation correctly

Previously, if compensation was triggered twice concurrently, it could
lead to duplicate compensations. Now we use atomic operations to ensure
only one compensation runs.

Fixes #456
```

## Code Style Guidelines

### C# Conventions

- **PascalCase** for public members
- **camelCase** for private members and parameters
- **CONSTANT_CASE** for constants
- **IPrefix** for interfaces
- **TPrefix** for type parameters

Example:

```csharp
public class OrderProcessor
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderProcessor> _logger;
    private const int MaxRetries = 3;

    public async Task<T> ProcessAsync<T>(string orderId)
    {
        var order = await _orderRepository.GetAsync(orderId);
        // ...
    }
}
```

### File Organization

1. Using statements (ordered)
2. Namespace declaration
3. Class/interface declaration
4. Constants and static fields
5. Fields
6. Constructors
7. Public methods
8. Internal/protected methods
9. Private methods
10. Nested types

### Comments

- Write meaningful comments explaining **why**, not **what**
- Keep comments close to code they describe
- Update comments when code changes
- Avoid obvious comments

Good:

```csharp
// Circuit breaker opens after 5 consecutive failures
// to prevent cascading failures in dependent services
if (failureCount >= FailureThreshold)
{
    state = CircuitBreakerState.Open;
}
```

Bad:

```csharp
// Increment failure count
failureCount++;
```

### Documentation

- Add XML documentation to public APIs
- Keep it concise and clear
- Include examples for complex methods
- Document exceptions

```csharp
/// <summary>
/// Executes the next pending step in the saga.
/// </summary>
/// <param name="sagaId">The saga ID to execute</param>
/// <returns>The executed step, or null if no pending steps</returns>
/// <exception cref="SagaNotFoundException">Thrown if saga doesn't exist</exception>
public async Task<SagaStep> ExecuteNextStepAsync(string sagaId)
{
    // Implementation
}
```

### Testing

- Write unit tests for all public methods
- Aim for >80% code coverage
- Use meaningful test names
- Follow AAA pattern (Arrange, Act, Assert)

```csharp
[Fact]
public async Task ExecuteNextStepAsync_WithValidSaga_ReturnsCompletedStep()
{
    // Arrange
    var saga = new Saga { Id = "saga-1", Status = SagaStatus.Running };
    var step = new SagaStep { Status = SagaStepStatus.Pending };
    _repository.Setup(r => r.GetAsync("saga-1")).ReturnsAsync(saga);

    // Act
    var result = await _orchestrator.ExecuteNextStepAsync("saga-1");

    // Assert
    Assert.NotNull(result);
    Assert.Equal(SagaStepStatus.Completed, result.Status);
}
```

## Development Setup

### Prerequisites

- .NET 10 SDK
- Git
- Text editor or IDE (VS Code, Visual Studio, Rider)

### Setup Steps

```bash
# Clone your fork
git clone https://github.com/YOUR_USERNAME/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator

# Setup development environment
make setup

# Run tests
make test

# Build project
make build

# Run examples
make examples
```

## Project Structure

```
dotnet-saga-orchestrator/
├── src/
│   ├── Presentation/      # CLI and user interfaces
│   ├── Application/       # Services and DTOs
│   ├── Infrastructure/    # HTTP, events, caching, etc.
│   ├── Core/             # Domain models and exceptions
│   └── Data/             # Repositories
├── examples/              # Example projects
├── docs/                 # Documentation
├── tests/                # Test projects (Phase 4)
├── Dockerfile            # Docker configuration
├── Makefile             # Build automation
└── README.md            # Project overview
```

## Testing

### Running Tests

```bash
# Run all tests
make test

# Run specific test class
dotnet test --filter "TestClass=SagaOrchestrationServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Writing Tests

- Use xUnit for test framework
- Use Moq for mocking
- One test per behavior
- Test public APIs, not private implementations
- Name tests clearly

## Documentation

### Updating Documentation

1. Update relevant `.md` files
2. Update code comments if behavior changes
3. Update examples if APIs change
4. Update API reference if methods change
5. Update CHANGELOG.md

### Building Documentation

```bash
# Generate XML documentation
dotnet build /p:GenerateDocumentationFile=true

# View generated docs
# Located in: bin/Release/net10.0/SagaOrchestrator.xml
```

## Code Review

### What to Expect

- Constructive feedback from maintainers
- Requests for tests or documentation
- Suggestions for improvements
- Time for revisions (typically 24-48 hours)

### Responding to Reviews

- Don't take feedback personally
- Ask for clarification if unclear
- Push updates to the same branch
- Mark conversations as resolved
- Re-request review after changes

## Versioning

This project follows [Semantic Versioning](https://semver.org/):

- **MAJOR** version for incompatible API changes
- **MINOR** version for backward-compatible feature additions
- **PATCH** version for backward-compatible bug fixes

## Release Process

1. Update version in `dotnet-saga-orchestrator.csproj`
2. Update `CHANGELOG.md` with changes
3. Commit: `chore: release v1.2.0`
4. Tag: `git tag -a v1.2.0 -m "Release 1.2.0"`
5. Push: `git push origin main --tags`
6. GitHub Actions builds and publishes

## Questions?

- **Issues**: https://github.com/Sarmkadan/dotnet-saga-orchestrator/issues
- **Email**: rutova2@gmail.com
- **Website**: https://sarmkadan.com

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

**Thank you for contributing to make Saga Orchestrator better!**

Built with ❤️ by [Vladyslav Zaiets](https://sarmkadan.com)
