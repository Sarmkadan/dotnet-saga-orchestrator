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