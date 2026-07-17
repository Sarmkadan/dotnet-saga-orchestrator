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

## SagaOptionsValidation

The `SagaOptionsValidation` class provides validation helpers for `SagaOptions` configuration. It validates timeout policies, retry policies, cache policies, worker policies, and webhook policies within the saga configuration, ensuring all timeout and retry values are positive and properly configured.

### Usage Example

```csharp
using SagaOrchestrator.Configuration;

// Create valid saga options with proper configuration
var options = new SagaOptions
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
        SagaCacheExpirationMinutes = 60,
        DefinitionCacheExpirationMinutes = 120,
        HealthCheckCacheExpirationSeconds = 30,
        MaxCacheSize = 1000
    },
    WorkerPolicies = new WorkerPolicies
    {
        TimeoutWorkerIntervalSeconds = 5,
        CompensationWorkerIntervalSeconds = 10,
        EventProcessingWorkerIntervalSeconds = 2,
        MaxEventsToKeep = 1000
    },
    WebhookPolicies = new WebhookPolicies
    {
        WebhookTimeoutSeconds = 30,
        MaxWebhookRetries = 3,
        WebhookRetryDelayMs = 2000,
        MaxWebhookPayloadBytes = 1024 * 1024 // 1MB
    }
};

// Validate the options and get error messages
var validationErrors = options.Validate();
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
    Console.WriteLine("SagaOptions are valid!");
}

// Check if options are valid
bool isValid = options.IsValid();
Console.WriteLine($"Is valid: {isValid}");

// Ensure options are valid (throws if invalid)
try
{
    options.EnsureValid();
    Console.WriteLine("SagaOptions passed validation!");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Validation failed: {ex.Message}");
}
```

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