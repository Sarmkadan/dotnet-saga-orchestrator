# dotnet-saga-orchestrator


## Architecture


See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the layer breakdown (Core / Application / Data / Infrastructure), the saga execution and compensation data flow, extension points, and the reasoning behind the bigger design decisions.


## SagaIdGenerator


The `SagaIdGenerator` class provides a set of utility methods for generating and validating unique identifiers used in saga workflows. These identifiers are essential for tracking sagas, steps, correlations, and requests.

### Usage Example


```csharp
using SagaOrchestrator.Core.Utilities;

// Generate various ID types 
string sagaId = SagaIdGenerator.GenerateSagaId();
Console.WriteLine($"Generated Saga ID: {sagaId}"); // Output: saga_<32-char-hex>

string correlationId = SagaIdGenerator.GenerateCorrelationId();
Console.WriteLine($"Generated Correlation ID: {correlationId}"); // Output: corr_<32-char-hex>

string stepId = SagaIdGenerator.GenerateStepId();
Console.WriteLine($"Generated Step ID: {stepId}"); // Output: step_<32-char-hex>

string traceId = SagaIdGenerator.GenerateTraceId();
Console.WriteLine($"Generated Trace ID: {traceId}"); // Output: trace_<32-char-hex>

string requestId = SagaIdGenerator.GenerateRequestId();
Console.WriteLine($"Generated Request ID: {requestId}"); // Output: req_<32-char-hex>

// Validate IDs
bool isValidSaga = SagaIdGenerator.IsValidSagaId("saga_1234567890abcdef1234567890abcdef");
Console.WriteLine($"Is valid saga ID: {isValidSaga}"); // Output: True

bool isValidCorrelation = SagaIdGenerator.IsValidCorrelationId("corr_1234567890abcdef1234567890abcdef");
Console.WriteLine($"Is valid correlation ID: {isValidCorrelation}"); // Output: True
```

## SagaCliCommand





The `SagaCliCommand` class represents a CLI command for saga operations with full argument parsing. It supports various commands including `create`, `execute`, `status`, `list`, `compensate`, and `help`. The class parses raw CLI arguments into structured command properties, validates them, and provides helpful error messages and usage text.

### Usage Example

```csharp
using SagaOrchestrator.Presentation.Cli.Commands;

// Parse CLI arguments into a structured command
var args1 = new[] { "create", "--definition", "OrderProcessing", "--data", "{\"orderId\": 123}" };
var command1 = SagaCliCommand.Parse(args1);

if (command1.IsValid)
{
    Console.WriteLine($"Command: {command1.CommandType}");
    Console.WriteLine($"Definition: {command1.Arguments["definition"]}");
    Console.WriteLine($"Data: {command1.Arguments["data"]}");
}
else
{
    Console.WriteLine("Validation errors:");
    foreach (var error in command1.ValidationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}

// Parse an execute command
var args2 = new[] { "execute", "--saga-id", "ddf3c1ec-5b23-4905-b78c-ecc897d15443", "--async" };
var command2 = SagaCliCommand.Parse(args2);

if (command2.IsValid)
{
    Console.WriteLine($"Executing saga: {command2.Arguments["saga-id"]}");
    Console.WriteLine($"Async mode: {command2.Options.Contains("async")}");
}

// Get help text
var helpText = command1.GetHelpText();
Console.WriteLine(helpText);
```

## SagaCliCommandExtensions





The `SagaCliCommandExtensions` class provides extension methods for the `SagaCliCommand` type that enable fluent and type-safe access to command arguments, options, and validation. These methods simplify working with saga CLI commands by providing convenient methods for retrieving and parsing arguments, checking for options, and validating required parameters for different command types.

### Usage Example

```csharp
using SagaOrchestrator.Presentation.Cli.Commands;

// Parse a create command
var createArgs = new[] { "create", "--definition", "order_processing", "--max-retries", "3", "--timeout", "300", "--async" };
var createCommand = SagaCliCommand.Parse(createArgs);

if (createCommand.IsValid)
{
    // Get string arguments with and without defaults
    string? definition = createCommand.GetArgument("definition");
    string timeout = createCommand.GetArgument("timeout", "60"); // default value
    
    Console.WriteLine($"Definition: {definition}");
    Console.WriteLine($"Timeout (with default): {timeout}");
    
    // Get typed arguments
    int? maxRetries = createCommand.GetIntArgument("max-retries");
    bool? asyncMode = createCommand.GetBooleanArgument("async");
    
    Console.WriteLine($"Max retries: {maxRetries ?? 0}");
    Console.WriteLine($"Async mode: {asyncMode ?? false}");
    
    // Check for options
    bool hasAsyncOption = createCommand.HasOption("async");
    Console.WriteLine($"Has async option: {hasAsyncOption}");
    
    // Get all arguments and options
    var allArgs = createCommand.GetArguments();
    var allOptions = createCommand.GetOptions();
    
    Console.WriteLine($"Total arguments: {allArgs.Count}");
    Console.WriteLine($"Total options: {allOptions.Count}");
    
    // Validate required arguments for command type
    bool isValid = createCommand.ValidateRequiredArguments();
    Console.WriteLine($"Command validation passed: {isValid}");
    
    // Get formatted log string
    string logString = createCommand.ToLogString();
    Console.WriteLine($"Log string: {logString}");
}

// Parse an execute command
var executeArgs = new[] { "execute", "--saga-id", "saga_abc123", "--force" };
var executeCommand = SagaCliCommand.Parse(executeArgs);

if (executeCommand.IsValid)
{
    // Get saga ID
    string sagaId = executeCommand.GetArgument("saga-id")!;
    Console.WriteLine($"Executing saga: {sagaId}");
    
    // Check if force option is present
    bool forceExecute = executeCommand.HasOption("force");
    Console.WriteLine($"Force execute: {forceExecute}");
}

// Parse a status command
var statusArgs = new[] { "status", "--saga-id", "saga_xyz789" };
var statusCommand = SagaCliCommand.Parse(statusArgs);

if (statusCommand.IsValid)
{
    string sagaId = statusCommand.GetArgument("saga-id")!;
    Console.WriteLine($"Checking status for saga: {sagaId}");
    
    // Validate command-specific requirements
    bool isStatusValid = statusCommand.ValidateRequiredArguments();
    Console.WriteLine($"Status command validation: {isStatusValid}");
}
```

## InMemorySagaDefinitionRepositoryExtensions

The `InMemorySagaDefinitionRepositoryExtensions` class provides a set of convenience extension methods for querying and interacting with in-memory saga definition repositories. It simplifies common operations like retrieving saga definitions by name, version, or status, as well as performing counting and existence checks. These methods allow for more expressive and concise data retrieval in your application.

### Usage Example

```csharp
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Core.Domain.Models;

// Assuming repository is an instance of InMemorySagaDefinitionRepository
var repository = new InMemorySagaDefinitionRepository();

// Check if a definition exists
bool exists = await repository.ExistsByNameAsync("OrderProcessing");

// Retrieve specific definitions
var definition = await repository.GetByNameAsync("OrderProcessing");
var latestVersion = await repository.GetLatestVersionAsync("OrderProcessing");
var activeDefinitions = await repository.GetActiveAsync();

// Count definitions
int totalCount = await repository.CountAsync();
int activeCount = await repository.CountActiveAsync();

// Search and filter
var searchResults = await repository.SearchByNameAsync("Order");
var versionTwoDefinitions = await repository.GetByVersionAsync(2);
var recentDefinitions = await repository.GetCreatedAfterAsync(DateTime.UtcNow.AddDays(-7));
```

## InMemorySagaStepRepositoryExtensions

The `InMemorySagaStepRepositoryExtensions` class provides a set of extension methods for the `InMemorySagaStepRepository` that enable efficient querying, validation, and status tracking for saga steps directly from the repository. These methods simplify common operations such as retrieving steps by status, identifying retryable or timed-out steps, and checking the overall completion status of a saga.

### Usage Example

```csharp
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Data.Repositories;

// Assuming repository is an instance of InMemorySagaStepRepository
var repository = new InMemorySagaStepRepository(); 
string sagaId = "saga_1234567890abcdef";

// Get steps by status
var failedSteps = await repository.GetBySagaIdAndStatusAsync(sagaId, SagaStepStatus.Failed);

// Check for next pending step
var nextStep = await repository.GetNextPendingStepAsync(sagaId);

// Identify retryable failed steps
var retryableSteps = await repository.GetRetryableFailedStepsAsync(sagaId);

// Check for timed out steps
var timedOutSteps = await repository.GetTimedOutStepsAsync(sagaId);

// Get highest execution order
int maxOrder = await repository.GetMaxOrderForSagaAsync(sagaId);

// Check overall completion
bool isCompleted = await repository.AreAllStepsCompletedAsync(sagaId);

// Get active steps
var activeSteps = await repository.GetActiveStepsAsync(sagaId);
```

## InMemorySagaRepositoryExtensions

The `InMemorySagaRepositoryExtensions` class provides a collection of extension methods for the `InMemorySagaRepository` that simplify common saga management operations. These methods enable querying sagas by correlation ID, status, definition ID, or name, as well as performing counting operations, existence checks, and retrieving sagas based on specific conditions like timeouts or retry eligibility.

### Usage Example

```csharp
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;

// Assuming repository is an instance of InMemorySagaRepository
var repository = new InMemorySagaRepository();

// Create a sample saga
var saga = new Saga
{
    Id = "saga_abc123",
    CorrelationId = "corr_order_456",
    DefinitionId = "order_processing",
    Name = "OrderProcessing",
    Status = SagaStatus.Running,
    CreatedAt = DateTime.UtcNow,
    TimeoutSeconds = 300,
    MaxRetries = 3,
    RetryCount = 0
};

// Add the saga to the repository
await repository.AddAsync(saga);

// Retrieve sagas by correlation ID
var foundSaga = await repository.GetByCorrelationIdAsync("corr_order_456");
Console.WriteLine(foundSaga?.Id); // Output: saga_abc123

// Check if saga exists by correlation ID
bool exists = await repository.ExistsByCorrelationIdAsync("corr_order_456");
Console.WriteLine(exists); // Output: True

// Get sagas by status
var runningSagas = await repository.GetByStatusAsync(SagaStatus.Running);
Console.WriteLine(runningSagas.Count);

// Search by definition ID
var orderSagas = await repository.SearchByDefinitionIdAsync("order_processing");
Console.WriteLine(orderSagas.Count);

// Search by name
var namedSagas = await repository.SearchByNameAsync("OrderProcessing");
Console.WriteLine(namedSagas.Count);

// Get timed out sagas
var timedOut = await repository.GetTimedOutSagasAsync();
Console.WriteLine(timedOut.Count);

// Get retryable sagas
var retryable = await repository.GetRetryableSagasAsync();
Console.WriteLine(retryable.Count);

// Get failed sagas after a specific date
var recentFailures = await repository.GetFailedSagasAfterAsync(DateTime.UtcNow.AddDays(-1));
Console.WriteLine(recentFailures.Count);

// Count sagas by status
int runningCount = await repository.CountByStatusAsync(SagaStatus.Running);
Console.WriteLine(runningCount);

// Count all sagas
int totalCount = await repository.CountAllAsync();
Console.WriteLine(totalCount);

// Get completed sagas
var completed = await repository.GetCompletedSagasAsync();
Console.WriteLine(completed.Count);

// Get failed sagas
var failed = await repository.GetFailedSagasAsync();
Console.WriteLine(failed.Count);
```

## CircuitBreakerRecoveryTests

The `CircuitBreakerRecoveryTests` class validates the asynchronous circuit breaker recovery lifecycle, ensuring that a breaker properly transitions from open → half-open → closed states based on probe success or failure. These tests verify the circuit breaker's resilience behavior with configurable failure thresholds and timeout windows, including per-identifier state isolation and explicit reset capabilities.

### Usage Example

```csharp
using SagaOrchestrator.Infrastructure.Resilience;

// Create a circuit breaker with 3 failures required to open, and 5 second timeout
var breaker = new CircuitBreaker(failureThreshold: 3, timeoutSeconds: 5);

// Simulate failures to trip the breaker
for (int i = 0; i < 3; i++)
{
    try
    {
        await breaker.ExecuteAsync(() => throw new InvalidOperationException("Service unavailable"), 
                              "payment-service");
    }
    catch { /* expected */ }
}

// Breaker is now open - subsequent calls are rejected immediately
var state = breaker.GetState("payment-service"); // Returns CircuitBreakerState.Open

// Wait for timeout to elapse (5 seconds + buffer)
await Task.Delay(TimeSpan.FromSeconds(5.5));

// Breaker transitions to half-open state
state = breaker.GetState("payment-service"); // Returns CircuitBreakerState.HalfOpen

// Execute a probe request - success will close the breaker
try
{
    var result = await breaker.ExecuteAsync(() => Task.FromResult(42), "payment-service");
    state = breaker.GetState("payment-service"); // Returns CircuitBreakerState.Closed
}
catch
{
    // Probe failed - breaker reopens immediately
    state = breaker.GetState("payment-service"); // Returns CircuitBreakerState.Open
}

// Reset the breaker state explicitly (regardless of current state)
breaker.Reset("payment-service");
state = breaker.GetState("payment-service"); // Returns CircuitBreakerState.Closed

// Each identifier maintains independent breaker state
var inventoryBreakerState = breaker.GetState("inventory-service"); // Returns CircuitBreakerState.Closed
```

## InMemoryCompensationTransactionRepositoryValidation

The `InMemoryCompensationTransactionRepositoryValidation` class provides validation helpers for `InMemoryCompensationTransactionRepository` and `CompensationTransaction` instances. It offers methods to validate repository instances and compensation transactions, ensuring data integrity and proper state management during saga compensation workflows.

### Usage Example

```csharp
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Data.Repositories;

// Create a compensation transaction
var transaction = new CompensationTransaction
{
    Id = "comp_abc123",
    SagaId = "saga_xyz789",
    StepId = "step_123",
    StepName = "RefundPayment",
    Order = 1,
    Status = CompensationStatus.Pending,
    CompensationUrl = "https://api.example.com/compensate/refund",
    InitiatedAt = DateTime.UtcNow,
    TimeoutSeconds = 300,
    MaxRetries = 3,
    RetryCount = 0,
    RequestPayload = new Dictionary<string, object> { { "orderId", 123 } }
};

// Validate the transaction
var validationErrors = transaction.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Transaction is valid!");
}

// Check if transaction is valid
bool isValid = transaction.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Ensure transaction is valid (throws if invalid)
try
{
    transaction.EnsureValid();
    Console.WriteLine("Transaction passed validation!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate a repository instance
var repository = new InMemoryCompensationTransactionRepository();
var repositoryErrors = repository.Validate();
Console.WriteLine($"Repository validation errors: {repositoryErrors.Count}");

// Check repository validity
bool isRepositoryValid = repository.IsValid();
Console.WriteLine($"Repository is valid: {isRepositoryValid}");

// Ensure repository is valid
try
{
    repository.EnsureValid();
    Console.WriteLine("Repository passed validation!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Repository validation failed: {ex.Message}");
}
```

## SagaOptionsExtensions

The `SagaOptionsExtensions` class provides extension methods for `SagaOptions` that offer convenient ways to query and manipulate saga orchestrator configuration. These methods allow you to check caching status, calculate effective timeouts and retry limits based on configured policies, and create new option instances with overrides while preserving existing settings.

### Usage Example

```csharp
using SagaOrchestrator.Configuration;

// Create base saga options with configuration
var baseOptions = new SagaOptions
{
    TimeoutPolicies = new TimeoutPolicies
    {
        DefaultStepTimeoutSeconds = 30,
        DefaultSagaTimeoutSeconds = 300,
        MaxStepTimeoutSeconds = 60,
        MaxSagaTimeoutSeconds = 600,
        CompensationTimeoutSeconds = 30
    },
    RetryPolicies = new RetryPolicies
    {
        DefaultMaxRetries = 3,
        MaxRetries = 5,
        DefaultRetryDelayMs = 1000,
        MaxBackoffDelayMs = 30000,
        BackoffMultiplier = 2.0,
        UseExponentialBackoff = true
    },
    CachePolicies = new CachePolicies
    {
        EnableCaching = true,
        SagaCacheExpirationMinutes = 60,
        DefinitionCacheExpirationMinutes = 120,
        HealthCheckCacheExpirationSeconds = 30,
        MaxCacheSize = 1000
    }
};

// Check if caching is enabled
bool cachingEnabled = baseOptions.IsCachingEnabled();
Console.WriteLine($"Caching enabled: {cachingEnabled}");

// Calculate effective timeouts based on requested values and policy constraints
int requestedStepTimeout = 45;
int effectiveStepTimeout = baseOptions.GetEffectiveStepTimeout(requestedStepTimeout);
Console.WriteLine($"Effective step timeout: {effectiveStepTimeout} seconds");

int requestedSagaTimeout = 250;
int effectiveSagaTimeout = baseOptions.GetEffectiveSagaTimeout(requestedSagaTimeout);
Console.WriteLine($"Effective saga timeout: {effectiveSagaTimeout} seconds");

// Calculate effective retry limit based on requested value and policy constraints
int requestedMaxRetries = 4;
int effectiveMaxRetries = baseOptions.GetEffectiveMaxRetries(requestedMaxRetries);
Console.WriteLine($"Effective max retries: {effectiveMaxRetries}");

// Create new options with overrides while preserving existing settings
var customizedOptions = baseOptions.WithOverrides(options =>
{
    options.TimeoutPolicies.DefaultStepTimeoutSeconds = 45;
    options.RetryPolicies.DefaultMaxRetries = 5;
    options.CachePolicies.SagaCacheExpirationMinutes = 120;
});

Console.WriteLine($"Customized step timeout: {customizedOptions.TimeoutPolicies.DefaultStepTimeoutSeconds}");
Console.WriteLine($"Customized max retries: {customizedOptions.RetryPolicies.DefaultMaxRetries}");
Console.WriteLine($"Customized cache expiration: {customizedOptions.CachePolicies.SagaCacheExpirationMinutes} minutes");
```

## SagaOptionsValidation

The `SagaOptionsValidation` class provides validation helpers for `SagaOptions` configuration.

## InMemorySagaStepRepositoryValidation

The `InMemorySagaStepRepositoryValidation` class provides validation helpers for `InMemorySagaStepRepository` instances and `SagaStep` entities. It offers methods to validate repository instances and saga steps, ensuring data integrity and proper state management during saga execution workflows.

### Usage Example

```csharp
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;

// Create a valid saga step
var step = new SagaStep
{
    Id = "step_abc123",
    SagaId = "saga_xyz789",
    Name = "ProcessPayment",
    Order = 1,
    Status = SagaStepStatus.Pending,
    ServiceUrl = "https://api.example.com/process",
    MaxRetries = 3,
    TimeoutSeconds = 60,
    Payload = new Dictionary<string, object> { { "amount", 100.50 } }
};

// Validate the step
var validationErrors = step.Validate();
if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("SagaStep is valid!");
}

// Check if step is valid
bool isValid = step.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Ensure step is valid (throws if invalid)
try
{
    step.EnsureValid();
    Console.WriteLine("SagaStep passed validation!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}

// Validate a repository instance
var repository = new InMemorySagaStepRepository();
var repositoryErrors = repository.Validate();
Console.WriteLine($"Repository validation errors: {repositoryErrors.Count}");

// Check repository validity
bool isRepositoryValid = repository.IsValid();
Console.WriteLine($"Repository is valid: {isRepositoryValid}");

// Ensure repository is valid
try
{
    repository.EnsureValid();
    Console.WriteLine("Repository passed validation!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Repository validation failed: {ex.Message}");
}
```

## ISagaStateRenderer

The `ISagaStateRenderer` interface defines methods for visualizing various aspects of a saga's state, providing textual representations of progress, state diagrams, full reports, and DOT-formatted graphs. It enables consistent rendering of saga workflow information across different visualization tools and interfaces.

### Usage Example

```csharp
public class SagaStateRendererExample : ISagaStateRenderer
{
    public string RenderProgressBar() => "[=====>] 75% complete";
    public string RenderStateDiagram() => "State A --> State B --> State C";
    public string RenderFullReport() => "Saga ID: 123 | Status: InProgress | Created: 2026-08-25";
    public string RenderDot() => "digraph { A --> B --> C }";
}

// Usage example
var renderer = new SagaStateRendererExample();
Console.WriteLine(renderer.RenderProgressBar());  // Outputs: [=====>] 75% complete
Console.WriteLine(renderer.RenderStateDiagram());  // Outputs: State A --> State B --> State C
```

This interface is typically used by visualization tools, monitoring systems, or command-line interfaces to display saga execution details in a human-readable format.


## MetricsSummary


The `MetricsSummary` class is a data transfer object (DTO) that aggregates high-level operational metrics about saga execution into a single snapshot. It reports the total number of sagas, a breakdown of saga counts by status, the average saga duration in seconds, and the compensation rate, along with the timestamp at which the metrics were captured. This makes it well suited for dashboards, health checks, and monitoring endpoints that need a compact view of orchestrator health.

### Usage Example

```csharp
using SagaOrchestrator.Application.DTOs;

// Build a metrics summary from collected telemetry
var summary = new MetricsSummary
{
    TotalSagas = 150,
    ByStatus = new Dictionary<string, int>
    {
        { "Completed", 120 },
        { "Running", 20 },
        { "Failed", 10 }
    },
    AverageDurationSeconds = 42.7,
    CompensationRate = 0.066,
    Timestamp = DateTime.UtcNow
};

Console.WriteLine($"Total sagas: {summary.TotalSagas}");
Console.WriteLine($"Average duration: {summary.AverageDurationSeconds:F1} seconds");
Console.WriteLine($"Compensation rate: {summary.CompensationRate:P1}");
Console.WriteLine($"Captured at: {summary.Timestamp:u}");

foreach (var statusCount in summary.ByStatus)
{
    Console.WriteLine($"  {statusCount.Key}: {statusCount.Value}");
}
```

## SagaOrchestrationServiceHappyPathTests


The `SagaOrchestrationServiceHappyPathTests` class contains happy-path unit tests for the saga orchestration service, exercising the full lifecycle where every step succeeds: creating a saga from a valid definition with an initialized status, starting it so its steps are created, executing each step in order until the saga completes, and retrieving sagas by ID. It also verifies idempotent step execution (re-executing a completed step returns the same step) and the failure contracts, where null or empty arguments throw `ArgumentNullException`/`ArgumentException`, unknown saga IDs raise `SagaNotFoundException`, starting an already-running saga throws `SagaException`, and invalid definitions are rejected with `InvalidSagaDefinitionException`.

### Usage Example

```csharp
using System;
using System.Threading.Tasks;
using SagaOrchestrator.Tests;

// Instantiate the happy-path test suite
var tests = new SagaOrchestrationServiceHappyPathTests();

// Creating a saga from a valid definition yields an initialized saga
await tests.CreateSagaAsync_WithValidDefinition_CreatesSagaWithInitializedStatus();

// Starting an initialized saga transitions it to Running and creates its steps
await tests.StartSagaAsync_WithInitializedSaga_StartsSagaAndCreatesSteps();

// Executing next steps in order completes every step and the saga itself
await tests.ExecuteNextStepAsync_ExecutesAllStepsInOrder_CompletesSagaSuccessfully();

// Re-executing a completed step is idempotent and returns the same step
await tests.ExecuteNextStepAsync_Idempotency_ExecutingCompletedStepReturnsSameStep();

// Sagas can be retrieved by their identifier
await tests.GetSagaAsync_WithValidId_ReturnsSaga();

// Unknown saga IDs fail fast with SagaNotFoundException
try
{
    await tests.StartSagaAsync_WithNonExistentSagaId_ThrowsSagaNotFoundException();
}
catch (Exception ex)
{
    Console.WriteLine($"Expected failure: {ex.GetType().Name}");
}

// Null arguments are rejected with ArgumentNullException
try
{
    await tests.CreateSagaAsync_WithNullDefinition_ThrowsArgumentNullException();
}
catch (ArgumentNullException)
{
    Console.WriteLine("A null saga definition is rejected.");
}
```

## OutputFormatterTests

The `OutputFormatterTests` class contains unit tests for the `OutputFormatter` class, verifying that it correctly formats saga data as JSON, table, and CSV, including handling of special characters and empty lists.

### Usage Example

```csharp
using SagaOrchestrator.Tests.Infrastructure.Formatting;

// Instantiate the test class
var tests = new OutputFormatterTests();

// Test JSON formatting for a generic object
tests.FormatAsJson_Generic_ReturnsSerializedString();

// Test JSON formatting for a Saga object
tests.FormatAsJson_Saga_ReturnsSerializedString();

// Test table formatting for an empty list
tests.FormatAsTable_EmptyList_ReturnsNoSagasFoundMessage();

// Test table formatting for a normal list
var sagas = new List<Saga> { new Saga { Id = "s1", Status = SagaStatus.Running } };
tests.FormatAsTable_NormalList_ReturnsFormattedTable();

// Test CSV formatting for a normal list
tests.FormatAsCsv_NormalList_ReturnsFormattedCsv();

// Test CSV formatting with special characters
var specialSagas = new List<Saga> { new Saga { Id = "s\n1", Status = SagaStatus.Running } };
tests.FormatAsCsv_SpecialCharacters_EscapesCorrectly();
```

## SagaMessageTemplatesValidationTests

The `SagaMessageTemplatesValidationTests` class contains unit tests for the `SagaMessageTemplatesValidation` class, which validates parameters for various saga messaging templates. These tests verify that validation methods correctly return empty lists for valid inputs, throw `ArgumentNullException` for null parameters, and return appropriate error messages for invalid inputs such as exceeding maximum lengths or negative values.

### Usage Example

```csharp
using SagaOrchestrator.Tests;
using SagaOrchestrator.Infrastructure.Messaging;

// Instantiate the validation tester
var tests = new SagaMessageTemplatesValidationTests();

// Test that valid saga creation parameters return no validation errors
tests.ValidateSagaCreated_ShouldReturnEmptyList_WhenAllParametersAreValid();

// Test that null saga ID throws ArgumentNullException
try
{
    tests.ValidateSagaCreated_ShouldThrowArgumentNullException_WhenSagaIdIsNull();
}
catch (ArgumentNullException)
{
    // Expected exception
}

// Test that excessively long saga ID returns validation error
tests.ValidateSagaCreated_ShouldReturnError_WhenSagaIdExceedsMaxLength();
```

## SagaMessageTemplatesJsonExtensionsTests

The `SagaMessageTemplatesJsonExtensionsTests` class verifies JSON serialization, deserialization, and try-pattern parsing for saga message strings. It covers empty values, special characters, quotes, Unicode text, optional indentation, invalid or malformed JSON, and null-argument validation.

### Usage Example

```csharp
using SagaOrchestrator.Tests;

var tests = new SagaMessageTemplatesJsonExtensionsTests();

// Verify serialization behavior for representative message content.
tests.ToJson_ShouldSerializeSimpleString();
tests.ToJson_ShouldSerializeStringWithSpecialCharacters();
tests.ToJson_WithIndentedTrue_ShouldFormatWithIndentation();
tests.ToJson_WithUnicodeCharacters_ShouldPreserveCharacters();

// Verify deserialization and try-pattern behavior.
tests.FromJson_ShouldDeserializeStringWithQuotes();
tests.FromJson_ShouldReturnNull_WhenJsonIsMalformed();
tests.TryFromJson_ShouldReturnTrueAndValue_WhenJsonIsValid();
tests.TryFromJson_ShouldReturnFalseAndNull_WhenJsonIsInvalid();

// Verify null inputs are rejected.
tests.ToJson_ShouldThrowArgumentNullException_WhenMessageIsNull();
tests.FromJson_ShouldThrowArgumentNullException_WhenJsonIsNull();
tests.TryFromJson_ShouldThrowArgumentNullException_WhenJsonIsNull();
```

## SagaMessageTemplatesTests

The `SagaMessageTemplatesTests` class verifies the formatted and detailed messages produced for saga creation, step starts, step completion, step failure, retries, and saga completion. It also covers edge cases such as null or empty values, zero and negative numbers, boundary retry attempts, and partial completion.

### Usage Example

```csharp
using SagaOrchestrator.Tests;

var tests = new SagaMessageTemplatesTests();

// Verify the standard message generated for each stage of a saga.
tests.SagaCreated_Format_ShouldReturnCorrectMessage();
tests.StepStarted_Format_ShouldReturnCorrectMessage();
tests.StepCompleted_Format_ShouldReturnCorrectMessage();
tests.StepFailed_Format_ShouldReturnCorrectMessage();
tests.SagaCompleted_Format_ShouldReturnCorrectMessage();

// Exercise detailed messages and representative edge cases.
tests.SagaCreated_Detailed_ShouldHandleNullValues();
tests.StepStarted_Detailed_ShouldHandleBoundaryValues();
tests.StepCompleted_Detailed_ShouldHandleEmptyResult();
tests.StepFailed_WithRetry_ShouldHandleFirstAndLastAttempt();
tests.SagaCompleted_Format_ShouldHandlePartialCompletion();
```

## SagaDebuggerServiceJsonExtensionsJsonExtensionsTests

`SagaDebuggerServiceJsonExtensionsJsonExtensionsTests` verifies the JSON serialization and deserialization behavior of the saga debugger service, including valid, formatted, compact, and camel-cased output. It also covers argument validation and confirms that the try-pattern returns `false` with a null result when JSON is invalid or cannot be deserialized.

### Usage Example

```csharp
using SagaOrchestrator.Tests.Infrastructure.Debugging;

var tests = new SagaDebuggerServiceJsonExtensionsJsonExtensionsTests();

tests.ToJson_ShouldSerializeValidJson();
tests.ToJson_WithIndentedTrue_ShouldProduceFormattedJson();
tests.ToJson_WithIndentedFalse_ShouldProduceCompactJson();
tests.ToJson_WithNullValue_ShouldThrowArgumentNullException();
tests.FromJson_WithNullOrWhitespaceJson_ShouldThrowArgumentException();
tests.TryFromJson_WithNullOrWhitespaceJson_ShouldThrowArgumentException();
tests.TryFromJson_WithInvalidJson_ShouldReturnFalseAndSetNull();
tests.TryFromJson_WithUnDeserializableJson_ShouldReturnFalseAndSetNull();
tests.ToJson_ShouldUseCamelCaseNamingPolicy();
tests.ToJson_ShouldProduceNonEmptyOutput();
```

## SagaDebuggerServiceJsonExtensionsTests

`SagaDebuggerServiceJsonExtensionsTests` verifies the JSON serialization helpers for `SagaDebuggerService`, including indented and compact output and the resulting JSON structure. It also checks the validation and failure behavior of `FromJson` and `TryFromJson` for null, empty, whitespace, invalid, and valid serialized input.

### Usage Example

```csharp
using SagaOrchestrator.Tests.Infrastructure.Debugging;

var tests = new SagaDebuggerServiceJsonExtensionsTests();

// Verify serialization output and formatting.
tests.ToJson_WithValidService_ReturnsJsonString();
tests.ToJson_WithIndentedTrue_ReturnsFormattedJson();
tests.ToJson_WithIndentedFalse_ReturnsCompactJson();
tests.JsonOptions_ProducesValidJson();
tests.ToJson_ProducesValidJsonStructure();

// Verify argument validation and deserialization failure handling.
tests.ToJson_WithNullService_ThrowsArgumentNullException();
tests.FromJson_WithEmptyJson_ThrowsArgumentException();
tests.FromJson_WithInvalidJson_ThrowsFormatException();
tests.TryFromJson_WithInvalidJson_ReturnsFalseAndNull();
tests.TryFromJson_WithValidJson_ReturnsTrue();
```

## RetryPolicyTestsBehavior

`RetryPolicyTestsBehavior` is an xUnit behavior suite for retry-policy delay calculation, retry limits, factory methods, and constructor defaults. It verifies exponential and linear delays, maximum-delay capping, jitter variation, and the behavior at or beyond the configured retry limit.

### Usage Example

```csharp
using SagaOrchestrator.Tests;

var tests = new RetryPolicyTestsBehavior();

// Run representative delay and retry-limit behavior checks.
tests.CalculateDelay_SuccessFirstTry_NoRetryLogicApplied();
tests.CalculateDelay_DelayGrowsExponentially();
tests.CalculateDelay_DelayCappedAtMaxDelay();
tests.CalculateDelay_WithJitter_AppliesRandomVariation();
tests.CanRetry_WithinMaxRetries_ReturnsTrue(1);
tests.CanRetry_AtMaxRetries_ReturnsFalse();
tests.CanRetry_BeyondMaxRetries_ReturnsFalse();

// Verify the supplied policy factories and constructor behavior.
tests.CreateExponentialWithJitter_JitterEnabled();
tests.CreateLinear_DelaysGrowLinearly();
tests.DefaultConstructor_UsesExpectedDefaultValues();
tests.CustomConstructor_SetsAllPropertiesCorrectly();

// Confirm that calculating a delay after retry exhaustion is rejected.
tests.CalculateDelay_RetriesExhausted_ThrowsInvalidOperationException();
```
