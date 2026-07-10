# SagaIdGeneratorTests

The `SagaIdGeneratorTests` class contains unit tests for the `SagaIdGenerator` component. Each test verifies that generated identifiers (saga IDs, correlation IDs, step IDs, trace IDs, request IDs) conform to the expected prefix format, and that the corresponding validation methods correctly accept or reject identifiers based on those prefixes.

## API

All test methods are parameterless, return `void`, and do not throw exceptions under normal operation. Assertion failures (e.g., from `Assert.True` or `Assert.StartsWith`) will cause the test to fail.

### `GenerateSagaId_ShouldHaveCorrectPrefix`
Verifies that `SagaIdGenerator.GenerateSagaId()` returns a string starting with the expected prefix (e.g., `"saga-"`).

### `GenerateCorrelationId_ShouldHaveCorrectPrefix`
Verifies that `SagaIdGenerator.GenerateCorrelationId()` returns a string starting with the expected prefix (e.g., `"corr-"`).

### `GenerateStepId_ShouldHaveCorrectPrefix`
Verifies that `SagaIdGenerator.GenerateStepId()` returns a string starting with the expected prefix (e.g., `"step-"`).

### `GenerateTraceId_ShouldHaveCorrectPrefix`
Verifies that `SagaIdGenerator.GenerateTraceId()` returns a string starting with the expected prefix (e.g., `"trace-"`).

### `GenerateRequestId_ShouldHaveCorrectPrefix`
Verifies that `SagaIdGenerator.GenerateRequestId()` returns a string starting with the expected prefix (e.g., `"req-"`).

### `IsValidSagaId_ShouldValidateCorrectly`
Verifies that `SagaIdGenerator.IsValidSagaId(string)` returns `true` for well-formed saga IDs and `false` for malformed or null/empty inputs.

### `IsValidCorrelationId_ShouldValidateCorrectly`
Verifies that `SagaIdGenerator.IsValidCorrelationId(string)` returns `true` for well-formed correlation IDs and `false` for malformed or null/empty inputs.

## Usage

The following examples demonstrate how to run these tests using a typical test runner (e.g., `dotnet test`). The tests are intended to be executed as part of a continuous integration pipeline to ensure identifier generation and validation remain consistent.

### Example 1: Running all prefix tests

```csharp
using Xunit;

public class SagaIdGeneratorTests
{
    [Fact]
    public void GenerateSagaId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateSagaId();
        Assert.StartsWith("saga-", id);
    }

    [Fact]
    public void GenerateCorrelationId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateCorrelationId();
        Assert.StartsWith("corr-", id);
    }

    // Additional prefix tests follow the same pattern...
}
```

### Example 2: Running validation tests

```csharp
[Fact]
public void IsValidSagaId_ShouldValidateCorrectly()
{
    Assert.True(SagaIdGenerator.IsValidSagaId("saga-abc123"));
    Assert.False(SagaIdGenerator.IsValidSagaId("invalid"));
    Assert.False(SagaIdGenerator.IsValidSagaId(null));
    Assert.False(SagaIdGenerator.IsValidSagaId(""));
}

[Fact]
public void IsValidCorrelationId_ShouldValidateCorrectly()
{
    Assert.True(SagaIdGenerator.IsValidCorrelationId("corr-xyz789"));
    Assert.False(SagaIdGenerator.IsValidCorrelationId("bad-prefix"));
    Assert.False(SagaIdGenerator.IsValidCorrelationId(null));
    Assert.False(SagaIdGenerator.IsValidCorrelationId(""));
}
```

## Notes

- **Edge cases**: Validation tests should cover `null`, empty strings, and strings with incorrect prefixes. The prefix tests assume that generated IDs are never `null` or empty; if the generator can return such values, additional tests should be added.
- **Thread safety**: The `SagaIdGenerator` methods are expected to be thread-safe (e.g., using `Guid.NewGuid()` or similar stateless generation). The tests themselves are not thread-safe and should be run sequentially per test class.
- **Prefix constants**: The exact prefix strings are defined within `SagaIdGenerator` and may change over time. These tests serve as a contract to detect unintended prefix modifications.
- **Test isolation**: Each test method is independent and does not rely on shared state. No cleanup is required.
