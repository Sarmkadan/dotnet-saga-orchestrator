# dotnet-saga-orchestrator

A .NET library for managing distributed sagas with compensating transactions, retries and timeout handling.

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

## SagaIdGeneratorTests

The `SagaIdGeneratorTests` class contains unit tests for the `SagaIdGenerator` utility methods. These tests verify that ID generation methods produce correctly formatted identifiers with appropriate prefixes and that validation methods correctly identify valid and invalid IDs.

### Usage Example

```csharp
using Xunit;
using SagaOrchestrator.Core.Utilities;

public class ExampleTests
{
    [Fact]
    public void GenerateSagaId_ShouldHaveCorrectPrefix()
    {
        // Arrange & Act
        var id = SagaIdGenerator.GenerateSagaId();
        
        // Assert
        Assert.StartsWith("saga_", id);
    }

    [Fact]
    public void GenerateCorrelationId_ShouldHaveCorrectPrefix()
    {
        // Arrange & Act
        var id = SagaIdGenerator.GenerateCorrelationId();
        
        // Assert
        Assert.StartsWith("corr_", id);
    }

    [Fact]
    public void GenerateStepId_ShouldHaveCorrectPrefix()
    {
        // Arrange & Act
        var id = SagaIdGenerator.GenerateStepId();
        
        // Assert
        Assert.StartsWith("step_", id);
    }

    [Fact]
    public void GenerateTraceId_ShouldHaveCorrectPrefix()
    {
        // Arrange & Act
        var id = SagaIdGenerator.GenerateTraceId();
        
        // Assert
        Assert.StartsWith("trace_", id);
    }

    [Fact]
    public void GenerateRequestId_ShouldHaveCorrectPrefix()
    {
        // Arrange & Act
        var id = SagaIdGenerator.GenerateRequestId();
        
        // Assert
        Assert.StartsWith("req_", id);
    }

    [Fact]
    public void IsValidSagaId_ShouldValidateCorrectly()
    {
        // Arrange & Act
        bool isValid = SagaIdGenerator.IsValidSagaId("saga_1234567890abcdef1234567890abcdef");
        bool isInvalid = SagaIdGenerator.IsValidSagaId("corr_12345");
        
        // Assert
        Assert.True(isValid);
        Assert.False(isInvalid);
    }

    [Fact]
    public void IsValidCorrelationId_ShouldValidateCorrectly()
    {
        // Arrange & Act
        bool isValid = SagaIdGenerator.IsValidCorrelationId("corr_1234567890abcdef1234567890abcdef");
        bool isInvalid = SagaIdGenerator.IsValidCorrelationId("saga_123");
        
        // Assert
        Assert.True(isValid);
        Assert.False(isInvalid);
    }
}
```

## ISagaResponseMapper

The `ISagaResponseMapper` interface provides methods for converting saga domain models to response DTOs, enabling consistent API response formatting. It supports mapping individual sagas, collections of sagas, and individual saga steps to their corresponding response types.

## SagaResponse

The `SagaResponse` class is a response model that represents the state and metadata of a saga execution. It provides a structured view of saga operations including identification, timing, status tracking, and step-level details for API consumers. The response includes aggregated metrics like step counts, retry counts, and timing information to give a complete picture of saga progress and completion status.

### Usage Example

```csharp
using SagaOrchestrator.Application.DTOs;

// Create a completed saga response
var response = new SagaResponse
{
    Id = "saga_abc123",
    CorrelationId = "corr_xyz789",
    Status = "Completed",
    DefinitionId = "order_processing",
    DefinitionName = "Order Processing Saga",
    StartedAt = DateTime.UtcNow.AddMinutes(-5),
    CompletedAt = DateTime.UtcNow,
    FailureReason = null,
    StepCount = 3,
    CompletedSteps = 3,
    FailedSteps = 0,
    RetryCount = 0,
    Steps = new List<SagaStepResponse>
    {
        new SagaStepResponse
        {
            Id = "step_001",
            Name = "Validate Order",
            Order = 1,
            Status = "Completed",
            ServiceName = "https://order-service/api/validate",
            StartedAt = DateTime.UtcNow.AddMinutes(-5),
            CompletedAt = DateTime.UtcNow.AddMinutes(-4),
            ErrorMessage = null,
            RetryCount = 0
        },
        new SagaStepResponse
        {
            Id = "step_002",
            Name = "Process Payment",
            Order = 2,
            Status = "Completed",
            ServiceName = "https://payment-service/api/charge",
            StartedAt = DateTime.UtcNow.AddMinutes(-4),
            CompletedAt = DateTime.UtcNow.AddMinutes(-3),
            ErrorMessage = null,
            RetryCount = 0
        },
        new SagaStepResponse
        {
            Id = "step_003",
            Name = "Ship Order",
            Order = 3,
            Status = "Completed",
            ServiceName = "https://shipping-service/api/ship",
            StartedAt = DateTime.UtcNow.AddMinutes(-3),
            CompletedAt = DateTime.UtcNow.AddMinutes(-2),
            ErrorMessage = null,
            RetryCount = 0
        }
    }
};

Console.WriteLine($"Saga ID: {response.Id}");
Console.WriteLine($"Status: {response.Status}");
Console.WriteLine($"Definition: {response.DefinitionName}");
Console.WriteLine($"Progress: {response.CompletedSteps}/{response.StepCount} steps completed");
Console.WriteLine($"Duration: {(response.CompletedAt - response.StartedAt)?.TotalSeconds ?? 0} seconds");

// Create a failed saga response with retry information
var failedResponse = new SagaResponse
{
    Id = "saga_def456",
    CorrelationId = "corr_uvw123",
    Status = "Failed",
    DefinitionId = "payment_processing",
    DefinitionName = "Payment Processing Saga",
    StartedAt = DateTime.UtcNow.AddMinutes(-10),
    CompletedAt = DateTime.UtcNow,
    FailureReason = "Payment gateway timeout",
    StepCount = 2,
    CompletedSteps = 1,
    FailedSteps = 1,
    RetryCount = 2,
    Steps = new List<SagaStepResponse>
    {
        new SagaStepResponse
        {
            Id = "step_001",
            Name = "Validate Payment Method",
            Order = 1,
            Status = "Completed",
            ServiceName = "https://payment-service/api/validate",
            StartedAt = DateTime.UtcNow.AddMinutes(-10),
            CompletedAt = DateTime.UtcNow.AddMinutes(-9),
            ErrorMessage = null,
            RetryCount = 0
        },
        new SagaStepResponse
        {
            Id = "step_002",
            Name = "Charge Payment",
            Order = 2,
            Status = "Failed",
            ServiceName = "https://payment-service/api/charge",
            StartedAt = DateTime.UtcNow.AddMinutes(-9),
            CompletedAt = null,
            ErrorMessage = "Payment gateway timeout after 30 seconds",
            RetryCount = 2
        }
    }
};
```

## SagaCommandResult

The `SagaCommandResult` class is a standardized response DTO for all saga operations. It provides a consistent response format with success status, message, data payload, error collection, and metadata. This type is used throughout the application to return operation results from command handlers, services, and API endpoints.

### Usage Example

```csharp
using SagaOrchestrator.Application.DTOs;

// Create a successful command result with data
var successResult = SagaCommandResult.SuccessResult(
    "Order processed successfully",
    new { OrderId = "ord_12345", Status = "Completed" }
);

Console.WriteLine($"Success: {successResult.Success}");
Console.WriteLine($"Message: {successResult.Message}");
Console.WriteLine($"Data: {successResult.Data}");
Console.WriteLine($"Request ID: {successResult.RequestId}");
Console.WriteLine($"Timestamp: {successResult.Timestamp}");
Console.WriteLine($"Errors: {string.Join(", ", successResult.Errors)}");

// Create a failed command result with multiple errors
var failureResult = SagaCommandResult.FailureResult(
    "Failed to process payment",
    "Insufficient funds",
    "Payment gateway unavailable"
);

Console.WriteLine($"Success: {failureResult.Success}");
Console.WriteLine($"Message: {failureResult.Message}");
Console.WriteLine($"Errors: {string.Join(", ", failureResult.Errors)}");

// Create an exception command result
try
{
    // Some operation that might throw
}
catch (Exception ex)
{
    var exceptionResult = SagaCommandResult.ExceptionResult(ex);
    Console.WriteLine($"Exception captured: {exceptionResult.Message}");
    Console.WriteLine($"Error details: {exceptionResult.Errors[0]}");
}

// Generic version example
var genericSuccess = SagaCommandResult<string>.SuccessResult(
    "payment_processing_complete",
    "Payment processed successfully"
);

Console.WriteLine($"Generic Success: {genericSuccess.Success}");
Console.WriteLine($"Generic Data: {genericSuccess.Data}");
```

## CreateSagaRequest

The `CreateSagaRequest` class is a request model for creating a new saga instance. It allows you to specify the saga definition, configure retry behavior, set timeout constraints, and attach metadata or initial payload data. The request can be validated using the `IsValid()` method to ensure required fields are present and values are within acceptable ranges.

### Usage Example

```csharp
using SagaOrchestrator.Application.DTOs;

// Create a request to start a new order processing saga
var request = new CreateSagaRequest
{
    DefinitionId = "order_processing_v2",
    DefinitionName = "Order Processing Saga",
    MaxRetries = 3,
    TimeoutSeconds = 300,
    Metadata = new Dictionary<string, object>
    {
        { "customerId", "cust_12345" },
        { "orderId", "ord_67890" },
        { "priority", "high" }
    },
    Data = "{\"orderTotal\": 99.99, \"items\": [{\"sku\": \"WIDGET-001\", \"quantity\": 2}]}"
};

// Validate the request before sending
if (request.IsValid())
{
    Console.WriteLine("Saga request is valid");
    Console.WriteLine($"Definition: {request.DefinitionId}");
    Console.WriteLine($"Max Retries: {request.MaxRetries ?? 0}");
    Console.WriteLine($"Timeout: {request.TimeoutSeconds ?? 0} seconds");
    Console.WriteLine($"Metadata Count: {request.Metadata?.Count ?? 0}");
}

// Create a minimal request using only DefinitionId
var minimalRequest = new CreateSagaRequest
{
    DefinitionId = "simple_saga"
};

// Create a request with just DefinitionName (alternative to DefinitionId)
var namedRequest = new CreateSagaRequest
{
    DefinitionName = "Payment Processing",
    MaxRetries = 5,
    TimeoutSeconds = 600
};

// Create a request without optional fields
var basicRequest = new CreateSagaRequest
{
    DefinitionId = "inventory_update"
};
```

## TimeoutPolicy

The `TimeoutPolicy` class encapsulates timeout configuration for sagas and saga steps. It provides methods for checking timeout conditions, calculating remaining time, and determining if sufficient time remains for operations. Timeout policies can be created using predefined factory methods (`CreateLenient`, `CreateStandard`, `CreateStrict`) or customized with a specific timeout duration.

### Usage Example

```csharp
using SagaOrchestrator.Core.Utilities;

// Create a standard timeout policy (1 minute)
var standardTimeout = TimeoutPolicy.CreateStandard();
Console.WriteLine($"Standard timeout: {standardTimeout.TimeoutSeconds} seconds"); // 60
Console.WriteLine($"Is relaxed: {standardTimeout.IsRelaxed}"); // False

// Create a lenient timeout policy (5 minutes)
var lenientTimeout = TimeoutPolicy.CreateLenient();
Console.WriteLine($"Lenient timeout: {lenientTimeout.TimeoutSeconds} seconds"); // 300
Console.WriteLine($"Is relaxed: {lenientTimeout.IsRelaxed}"); // True

// Create a strict timeout policy (10 seconds)
var strictTimeout = TimeoutPolicy.CreateStrict();
Console.WriteLine($"Strict timeout: {strictTimeout.TimeoutSeconds} seconds"); // 10

// Create a custom timeout policy (2 minutes)
var customTimeout = TimeoutPolicy.Create(120);
Console.WriteLine($"Custom timeout: {customTimeout.TimeoutSeconds} seconds"); // 120

// Check if elapsed time has exceeded timeout
var startTime = DateTime.UtcNow;
var elapsed = TimeSpan.FromSeconds(45);
bool hasExceeded = customTimeout.HasExceeded(elapsed);
Console.WriteLine($"Has exceeded: {hasExceeded}"); // False

// Check if enough time remains with buffer
var buffer = TimeSpan.FromSeconds(10);
bool hasSufficientTime = customTimeout.HasSufficientTime(startTime, TimeSpan.FromSeconds(30));
Console.WriteLine($"Has sufficient time: {hasSufficientTime}"); // True

// Get remaining time
TimeSpan remaining = customTimeout.GetRemainingTime(startTime);
Console.WriteLine($"Remaining time: {remaining.TotalSeconds} seconds");

// Get elapsed percentage
double percentage = customTimeout.GetElapsedPercentage(startTime);
Console.WriteLine($"Elapsed percentage: {percentage:F2}%");
```

## TimeoutPolicyTests

The `TimeoutPolicyTests` record provides test data transfer objects for timeout policy testing scenarios. It encapsulates predefined timeout configurations with timeout duration in seconds and a relaxed flag, making it easy to create consistent test data for various timeout policy scenarios. This record is particularly useful for unit testing timeout-related functionality in saga orchestration.

### Usage Example

```csharp
using SagaOrchestrator.Tests;
using SagaOrchestrator.Core.Utilities;

// Create a standard timeout policy test configuration (60 seconds, not relaxed)
var standardTest = TimeoutPolicyTests.Standard;
Console.WriteLine($"Standard test timeout: {standardTest.TimeoutSeconds} seconds"); // 60
Console.WriteLine($"Standard test is relaxed: {standardTest.IsRelaxed}"); // False

// Convert test data to actual TimeoutPolicy
TimeoutPolicy standardPolicy = standardTest.ToPolicy();
Console.WriteLine($"Converted policy timeout: {standardPolicy.TimeoutSeconds} seconds"); // 60

// Create a lenient timeout policy test configuration (300 seconds, relaxed)
var lenientTest = TimeoutPolicyTests.Lenient;
Console.WriteLine($"Lenient test timeout: {lenientTest.TimeoutSeconds} seconds"); // 300
Console.WriteLine($"Lenient test is relaxed: {lenientTest.IsRelaxed}"); // True

// Create a strict timeout policy test configuration (10 seconds, not relaxed)
var strictTest = TimeoutPolicyTests.Strict;
Console.WriteLine($"Strict test timeout: {strictTest.TimeoutSeconds} seconds"); // 10
Console.WriteLine($"Strict test is relaxed: {strictTest.IsRelaxed}"); // False

// Create a custom timeout policy test configuration
var customTest = new TimeoutPolicyTests(120, false);
Console.WriteLine($"Custom test timeout: {customTest.TimeoutSeconds} seconds"); // 120
Console.WriteLine($"Custom test is relaxed: {customTest.IsRelaxed}"); // False

// Use test data in unit tests
public void TestTimeoutPolicy_StandardConfiguration()
{
    var test = TimeoutPolicyTests.Standard;
    var policy = test.ToPolicy();
    
    Assert.Equal(60, policy.TimeoutSeconds);
    Assert.False(policy.IsRelaxed);
}

public void TestTimeoutPolicy_LenientConfiguration()
{
    var test = TimeoutPolicyTests.Lenient;
    var policy = test.ToPolicy();
    
    Assert.Equal(300, policy.TimeoutSeconds);
    Assert.True(policy.IsRelaxed);
}
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

## CompensationService

The `CompensationService` handles the execution of compensating transactions when a saga fails. It manages the entire compensation workflow including initiating compensation, executing compensation steps in the appropriate order, retrying failed compensations, and checking for timeouts. The service supports different compensation strategies (reverse order, forward order, parallel) and automatically handles saga state transitions throughout the compensation process.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the compensation service with required dependencies
var compensationService = new CompensationService(
    compensationRepository,
    sagaRepository,
    stepRepository
);

// Example 1: Begin compensation for a failed saga
var failedSaga = await sagaRepository.GetByIdAsync("saga_abc123");
if (failedSaga.Status == SagaStatus.Failed)
{
    await compensationService.BeginCompensationAsync(failedSaga);
    
    // This creates compensation transactions for all completed steps
    var compensations = await compensationService.GetCompensationsAsync("saga_abc123");
    Console.WriteLine($"Created {compensations.Count} compensation transactions");
}

// Example 2: Execute the next compensation step
var nextCompensation = await compensationService.ExecuteNextCompensationAsync("saga_abc123");
if (nextCompensation != null)
{
    Console.WriteLine($"Executing compensation for step {nextCompensation.StepName} (order {nextCompensation.Order})");
    Console.WriteLine($"Status: {nextCompensation.Status}");
}
else
{
    Console.WriteLine("All compensations completed successfully");
}

// Example 3: Retry a failed compensation
var retrySuccess = await compensationService.RetryCompensationAsync("comp_789xyz");
if (retrySuccess)
{
    Console.WriteLine("Compensation marked for retry");
}

// Example 4: Check for timed out compensations
var timedOut = await compensationService.CheckTimeoutsAsync("saga_abc123");
if (timedOut.Count > 0)
{
    Console.WriteLine($"Found {timedOut.Count} timed out compensations");
    foreach (var timeout in timedOut)
    {
        Console.WriteLine($"- Compensation {timeout.Id} timed out: {timeout.ErrorMessage}");
    }
}

// Example 5: Get all compensations for a saga
var allCompensations = await compensationService.GetCompensationsAsync("saga_abc123");
foreach (var compensation in allCompensations)
{
    Console.WriteLine($"Compensation {compensation.Id}: {compensation.StepName} - {compensation.Status}");
}
```

## ISagaVisualizationService

The `ISagaVisualizationService` provides real-time visualization snapshots and streaming state updates for saga execution monitoring. It enables tracking saga progress, visualizing step execution graphs, and monitoring live state changes through polling-based streaming.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;
using System.Text.Json;

// Initialize the visualization service with required dependencies
var visualizationService = new SagaVisualizationService(
    sagaRepository,
    logger
);

// Example 1: Get a single saga snapshot
var snapshot = await visualizationService.GetSnapshotAsync("saga_abc123");
Console.WriteLine($"Saga: {snapshot.SagaName} ({snapshot.SagaId})");
Console.WriteLine($"Status: {snapshot.Status} ({snapshot.ProgressPercent:F1}% complete)");
Console.WriteLine($"Duration: {snapshot.ElapsedMs:F0}ms");
Console.WriteLine($"Steps: {snapshot.CompletedSteps}/{snapshot.TotalSteps}");
if (snapshot.FailureReason != null)
{
    Console.WriteLine($"Failed: {snapshot.FailureReason}");
}

// Example 2: Get all saga snapshots for dashboard display
var allSnapshots = await visualizationService.GetAllSnapshotsAsync();
Console.WriteLine($"Total sagas: {allSnapshots.Count}");
foreach (var s in allSnapshots.OrderByDescending(s => s.ElapsedMs))
{
    Console.WriteLine($"- {s.SagaName}: {s.Status} ({s.ProgressPercent:F1}%)");
}

// Example 3: Stream live state updates with custom polling interval
var cts = new CancellationTokenSource();
_ = Task.Run(async () =>
{
    await visualizationService.StreamLiveStateAsync(
        "saga_abc123",
        async snapshot =>
        {
            var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine($"Live Update: {DateTime.UtcNow:HH:mm:ss.fff}");
            Console.WriteLine(json);
            Console.WriteLine(new string('-', 60));
            
            if (snapshot.IsTerminal)
            {
                Console.WriteLine("Saga reached terminal state!");
                cts.Cancel();
            }
        },
        TimeSpan.FromSeconds(1),
        cts.Token
    );
});

// Run for 10 seconds then stop
await Task.Delay(TimeSpan.FromSeconds(10));
cts.Cancel();

// Example 4: Access step details from snapshot
var stepSnapshot = await visualizationService.GetSnapshotAsync("saga_xyz789");
foreach (var node in stepSnapshot.Nodes)
{
    Console.WriteLine($"Step {node.Index}: {node.Name} - {node.Status}");
    if (node.DurationMs.HasValue)
    {
        Console.WriteLine($"  Duration: {node.DurationMs.Value:F0}ms");
    }
    if (node.ErrorMessage != null)
    {
        Console.WriteLine($"  Error: {node.ErrorMessage}");
    }
}

// Example 5: Monitor progress over time
var progressSnapshots = new List<SagaVisualizationSnapshot>();
await visualizationService.StreamLiveStateAsync(
    "saga_progress_test",
    snapshot =>
    {
        progressSnapshots.Add(snapshot);
        return Task.CompletedTask;
    },
    TimeSpan.FromSeconds(2)
);

Console.WriteLine($"Captured {progressSnapshots.Count} snapshots");
Console.WriteLine($"Final status: {progressSnapshots.Last().Status}");
Console.WriteLine($"Final progress: {progressSnapshots.Last().ProgressPercent:F1}%");
```

## SagaDefinitionService

The `SagaDefinitionService` manages saga workflow definitions, enabling creation, modification, validation, and versioning of saga processes. It handles the lifecycle of saga definitions from creation through activation, including adding/removing steps, cloning for versioning, and comprehensive validation. The service ensures saga definitions are properly structured before being used to execute sagas.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the saga definition service
var sagaDefinitionService = new SagaDefinitionService(sagaDefinitionRepository);

// Example 1: Create a new saga definition
var definition = await sagaDefinitionService.CreateDefinitionAsync(
    "Order Processing Saga",
    "Handles the complete order processing workflow from validation to shipping"
);
Console.WriteLine($"Created definition: {definition.Name} (v{definition.Version})");

// Example 2: Add steps to the definition
var step1 = new SagaStepDefinition(
    "Validate Order",
    "https://order-service/api/validate",
    HttpMethod.Get,
    "Validate customer order details and inventory availability"
);
var step2 = new SagaStepDefinition(
    "Process Payment",
    "https://payment-service/api/charge",
    HttpMethod.Post,
    "Charge customer payment method"
);
var step3 = new SagaStepDefinition(
    "Ship Order",
    "https://shipping-service/api/ship",
    HttpMethod.Post,
    "Create shipping label and schedule delivery"
);

var updatedDefinition = await sagaDefinitionService.AddStepAsync(definition.Id, step1);
updatedDefinition = await sagaDefinitionService.AddStepAsync(definition.Id, step2);
updatedDefinition = await sagaDefinitionService.AddStepAsync(definition.Id, step3);

// Example 3: Validate the definition
var validationResult = sagaDefinitionService.ValidateDefinition(updatedDefinition);
if (!validationResult.IsValid)
{
    Console.WriteLine("Validation errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"- {error}");
    }
}
else
{
    Console.WriteLine("Definition is valid and ready for activation");
}

// Example 4: Activate the definition for use
var activatedDefinition = await sagaDefinitionService.ActivateDefinitionAsync(definition.Id);
Console.WriteLine($"Definition activated: {activatedDefinition.IsActive}");

// Example 5: List all active definitions
var activeDefinitions = await sagaDefinitionService.ListDefinitionsAsync(activeOnly: true);
Console.WriteLine($"Found {activeDefinitions.Count} active definitions");

// Example 6: Get a definition by name
var namedDefinition = await sagaDefinitionService.GetDefinitionByNameAsync("Order Processing Saga");
if (namedDefinition != null)
{
    Console.WriteLine($"Found definition: {namedDefinition.Name} (ID: {namedDefinition.Id})");
}

// Example 7: Clone a definition for versioning
var clonedDefinition = await sagaDefinitionService.CloneDefinitionAsync(definition.Id);
Console.WriteLine($"Cloned definition: {clonedDefinition.Name} v{clonedDefinition.Version}");

// Example 8: Remove a step
var definitionWithoutStep = await sagaDefinitionService.RemoveStepAsync(definition.Id, "Process Payment");
Console.WriteLine($"Removed step, definition now has {definitionWithoutStep.Steps.Count} steps");

// Example 9: Deactivate a definition
var deactivatedDefinition = await sagaDefinitionService.DeactivateDefinitionAsync(definition.Id);
Console.WriteLine($"Definition deactivated: {deactivatedDefinition.IsActive}");
```

## SagaOrchestrationService

The `SagaOrchestrationService` is the main service responsible for managing saga execution workflows. It handles creating sagas, executing saga steps in sequence, handling retries and timeouts, and managing compensation workflows when sagas fail. The service coordinates between the saga definition, individual steps, and the compensation service to ensure reliable distributed transaction execution.

### Usage Example

```csharp
using SagaOrchestrationService = SagaOrchestrator.Application.Services.SagaOrchestrationService;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the saga orchestration service with required dependencies
var sagaOrchestrationService = new SagaOrchestrationService(
    sagaRepository,
    sagaStepRepository,
    compensationService,
    sagaLogger
);

// Example 1: Create a new saga instance
var definition = new SagaDefinition(
    "Order Processing Saga",
    "Handles complete order processing workflow"
);

// Add steps to the definition
var step1 = new SagaStepDefinition(
    "Validate Order",
    "https://order-service/api/validate",
    HttpMethod.Get,
    "Validate customer order details"
);
var step2 = new SagaStepDefinition(
    "Process Payment",
    "https://payment-service/api/charge",
    HttpMethod.Post,
    "Charge customer payment method"
);
var step3 = new SagaStepDefinition(
    "Ship Order",
    "https://shipping-service/api/ship",
    HttpMethod.Post,
    "Create shipping label and schedule delivery"
);
definition.AddStep(step1);
definition.AddStep(step2);
definition.AddStep(step3);

// Create the saga
var saga = await sagaOrchestrationService.CreateSagaAsync(
    definition,
    maxRetries: 3,
    timeoutSeconds: 300
);
Console.WriteLine($"Created saga: {saga.Id} with {saga.Steps.Count} steps");

// Example 2: Start the saga execution
var startedSaga = await sagaOrchestrationService.StartSagaAsync(saga.Id);
Console.WriteLine($"Saga started: {startedSaga.Status}");

// Example 3: Execute the next step (typically called by a background worker)
var executedStep = await sagaOrchestrationService.ExecuteNextStepAsync(saga.Id);
if (executedStep != null)
{
    Console.WriteLine($"Executed step {executedStep.Name} (order {executedStep.Order})");
    Console.WriteLine($"Step status: {executedStep.Status}");
}
else
{
    Console.WriteLine("All steps completed successfully");
}

// Example 4: Handle step timeout
var timeoutHandled = await sagaOrchestrationService.HandleTimeoutAsync(saga.Id, stepId);
if (timeoutHandled)
{
    Console.WriteLine("Timeout handled, step marked for retry or compensation");
}

// Example 5: Compensate a failed saga
var compensatedSaga = await sagaOrchestrationService.CompensateSagaAsync(saga.Id);
Console.WriteLine($"Compensation completed: {compensatedSaga.Status}");

// Example 6: Compensate with explicit strategy
var compensatedWithStrategy = await sagaOrchestrationService.CompensateSagaAsync(
    saga.Id,
    CompensationStrategy.ReverseOrder
);

// Example 7: Abort a running saga
await sagaOrchestrationService.AbortSagaAsync(saga.Id, "User requested cancellation");

// Example 8: Retrieve saga details
var retrievedSaga = await sagaOrchestrationService.GetSagaAsync(saga.Id);
Console.WriteLine($"Saga status: {retrievedSaga.Status}");
Console.WriteLine($"Failed at: {retrievedSaga.FailedAt}");

// Example 9: List sagas with filtering
var runningSagas = await sagaOrchestrationService.ListSagasAsync(SagaStatus.Running);
Console.WriteLine($"Found {runningSagas.Count} running sagas");

var failedSagas = await sagaOrchestrationService.ListSagasAsync(SagaStatus.Failed);
Console.WriteLine($"Found {failedSagas.Count} failed sagas");
```

## SagaEventPublisher

The `SagaEventPublisher` class provides event publishing and management capabilities for saga domain events. It maintains an audit trail of saga events and supports event subscription for real-time monitoring and integration with external systems. The publisher allows filtering and querying events by saga ID, event type, or severity level, and provides export functionality for compliance and debugging purposes.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the saga event publisher
var eventPublisher = new SagaEventPublisher();

// Subscribe to all saga events
Func<SagaEvent, Task> eventHandler = async (sagaEvent) => {
    Console.WriteLine($"Event received: {sagaEvent.EventType} for saga {sagaEvent.SagaId}");
    Console.WriteLine($"  - Severity: {sagaEvent.Severity}");
    Console.WriteLine($"  - Message: {sagaEvent.Message}");
    Console.WriteLine($"  - Timestamp: {sagaEvent.Timestamp}");
};

eventPublisher.Subscribe(eventHandler);

// Create and publish a saga event
var sagaEvent = new SagaEvent(
    sagaId: "saga_abc123",
    eventType: "SagaStarted",
    message: "Order processing saga has started",
    severity: EventSeverity.Information
);

await eventPublisher.PublishAsync(sagaEvent);

// Get all events for a specific saga
var sagaEvents = eventPublisher.GetSagaEvents("saga_abc123");
Console.WriteLine($"Found {sagaEvents.Count} events for saga saga_abc123");

// Get events by type
var startedEvents = eventPublisher.GetEventsByType("saga_abc123", "SagaStarted");
Console.WriteLine($"Found {startedEvents.Count} 'SagaStarted' events");

// Get all events with filtering
var allEvents = eventPublisher.GetAllEvents(
    sagaId: "saga_abc123",
    eventType: "SagaStepCompleted",
    severity: EventSeverity.Information
);

// Get event count
var eventCount = eventPublisher.GetEventCount("saga_abc123");
Console.WriteLine($"Total events for saga: {eventCount}");

// Export events to file
await eventPublisher.ExportEventsAsync("saga_events.json", "saga_abc123");
Console.WriteLine("Events exported to saga_events.json");

// Publish multiple events at once
var eventsToPublish = new SagaEvent[] {
    new SagaEvent("saga_abc123", "SagaStepStarted", "Validating order", EventSeverity.Debug),
    new SagaEvent("saga_abc123", "SagaStepCompleted", "Order validated successfully", EventSeverity.Information)
};

await eventPublisher.PublishAsync(eventsToPublish);
```

## IMetricsService

The `IMetricsService` interface provides methods for collecting and reporting metrics related to saga execution. It allows you to retrieve overall saga metrics, step-specific metrics, and performance statistics. The service can be used to monitor the health and efficiency of the saga system.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;

// Initialize the metrics service
var metricsService = new MetricsService(
    sagaRepository,
    stepRepository,
    logger
);

// Get overall saga metrics
var sagaMetrics = await metricsService.GetMetricsAsync();
Console.WriteLine($"Total Sagas: {sagaMetrics.TotalSagas}");
Console.WriteLine($"Completed Sagas: {sagaMetrics.CompletedSagas}");
Console.WriteLine($"Failed Sagas: {sagaMetrics.FailedSagas}");
Console.WriteLine($"Success Rate: {sagaMetrics.SuccessRate}%");
Console.WriteLine($"Average Duration: {sagaMetrics.AverageDurationSeconds} seconds");

// Get step-specific metrics
var stepMetrics = await metricsService.GetStepMetricsAsync("Validate Order");
Console.WriteLine($"Step Name: {stepMetrics.StepName}");
Console.WriteLine($"Total Executions: {stepMetrics.TotalExecutions}");
Console.WriteLine($"Successful Executions: {stepMetrics.SuccessfulExecutions}");
Console.WriteLine($"Failed Executions: {stepMetrics.FailedExecutions}");
Console.WriteLine($"Success Rate: {stepMetrics.SuccessRate}%");
Console.WriteLine($"Average Duration: {stepMetrics.AverageDurationMs} ms");

// Get performance statistics
var performanceStats = await metricsService.GetPerformanceStatsAsync();
Console.WriteLine($"Average Duration: {performanceStats.AverageDurationSeconds} seconds");
Console.WriteLine($"Min Duration: {performanceStats.MinDurationSeconds} seconds");
Console.WriteLine($"Max Duration: {performanceStats.MaxDurationSeconds} seconds");
Console.WriteLine($"Median Duration: {performanceStats.MedianDurationSeconds} seconds");
Console.WriteLine($"P95 Duration: {performanceStats.P95DurationSeconds} seconds");
Console.WriteLine($"P99 Duration: {performanceStats.P99DurationSeconds} seconds");
```

## InMemorySagaStepRepository

The `InMemorySagaStepRepository` provides an in-memory implementation of `ISagaStepRepository` for storing and retrieving saga steps during execution. It maintains all saga steps in a thread-safe dictionary, enabling fast CRUD operations without external dependencies. This implementation is ideal for testing, development environments, or scenarios where persistence is not required.

### Usage Example

```csharp
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the in-memory saga step repository
var stepRepository = new InMemorySagaStepRepository();

// Example 1: Create a new saga step
var step1 = new SagaStep
{
    Id = "step_001",
    SagaId = "saga_order_123",
    Name = "Validate Order",
    Order = 1,
    Status = SagaStepStatus.Pending,
    ServiceName = "https://order-service/api/validate",
    HttpMethod = System.Net.Http.HttpMethod.Get,
    RetryCount = 0,
    MaxRetries = 3,
    TimeoutSeconds = 30,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

var createdStep = await stepRepository.CreateAsync(step1);
Console.WriteLine($"Created step: {createdStep?.Id} - {createdStep?.Name}");

// Example 2: Retrieve a step by ID
var retrievedStep = await stepRepository.GetByIdAsync("step_001");
if (retrievedStep != null)
{
    Console.WriteLine($"Retrieved step: {retrievedStep.Name} (Status: {retrievedStep.Status})");
}

// Example 3: Update a step's status
retrievedStep.Status = SagaStepStatus.InProgress;
retrievedStep.UpdatedAt = DateTime.UtcNow;
var updatedStep = await stepRepository.UpdateAsync(retrievedStep);
Console.WriteLine($"Updated step status to: {updatedStep?.Status}");

// Example 4: Get all steps for a specific saga
var sagaSteps = await stepRepository.GetBySagaIdAsync("saga_order_123");
Console.WriteLine($"Found {sagaSteps.Count} steps for saga saga_order_123");
foreach (var step in sagaSteps)
{
    Console.WriteLine($"- Step {step.Order}: {step.Name} ({step.Status})");
}

// Example 5: Get step by order within a saga
var firstStep = await stepRepository.GetByOrderAsync("saga_order_123", 1);
Console.WriteLine($"First step: {firstStep?.Name}");

// Example 6: Get all steps with a specific status
var pendingSteps = await stepRepository.GetByStatusAsync(SagaStepStatus.Pending);
Console.WriteLine($"Found {pendingSteps.Count} pending steps across all sagas");

// Example 7: Get all steps in the repository
var allSteps = await stepRepository.GetAllAsync();
Console.WriteLine($"Total steps in repository: {allSteps.Count}");

// Example 8: Delete a step
var deleted = await stepRepository.DeleteAsync("step_001");
Console.WriteLine($"Step deleted: {deleted}");

// Example 9: Handle duplicate creation (throws exception)
try
{
    await stepRepository.CreateAsync(step1);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Expected error: {ex.Message}");
}

// Example 10: Attempt to update non-existent step
var nonExistentStep = new SagaStep { Id = "step_nonexistent", Name = "Non-existent" };
var updateResult = await stepRepository.UpdateAsync(nonExistentStep);
Console.WriteLine($"Update non-existent step result: {updateResult?.Id ?? "null (step not found)}");
```

## InMemorySagaRepository

The `InMemorySagaRepository` provides an in-memory implementation of `ISagaRepository` for storing and retrieving sagas during execution. It maintains all sagas in a thread-safe dictionary, enabling fast CRUD operations without external dependencies. This implementation is ideal for testing, development environments, or scenarios where persistence is not required.

### Usage Example

```csharp
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the in-memory saga repository
var sagaRepository = new InMemorySagaRepository();

// Example 1: Create a new saga
var saga = new Saga
{
    Id = "saga_order_123",
    CorrelationId = "corr_order_123",
    Definition = new SagaDefinition("Order Processing Saga", "Handles complete order processing workflow"),
    Status = SagaStatus.Pending,
    MaxRetries = 3,
    TimeoutSeconds = 300,
    StartedAt = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

var createdSaga = await sagaRepository.CreateAsync(saga);
Console.WriteLine($"Created saga: {createdSaga?.Id} - {createdSaga?.Definition.Name}");

// Example 2: Retrieve a saga by ID
var retrievedSaga = await sagaRepository.GetByIdAsync("saga_order_123");
if (retrievedSaga != null)
{
    Console.WriteLine($"Retrieved saga: {retrievedSaga.Definition.Name} (Status: {retrievedSaga.Status}");
}

// Example 3: Retrieve a saga by correlation ID
var sagaByCorrelation = await sagaRepository.GetByCorrelationIdAsync("corr_order_123");
Console.WriteLine($"Found saga by correlation ID: {sagaByCorrelation?.Id}");

// Example 4: Update a saga's status
retrievedSaga.Status = SagaStatus.Running;
retrievedSaga.UpdatedAt = DateTime.UtcNow;
var updatedSaga = await sagaRepository.UpdateAsync(retrievedSaga);
Console.WriteLine($"Updated saga status to: {updatedSaga?.Status}");

// Example 5: Get all sagas in the repository
var allSagas = await sagaRepository.GetAllAsync();
Console.WriteLine($"Total sagas in repository: {allSagas.Count}");

// Example 6: Get all sagas with a specific status
var pendingSagas = await sagaRepository.GetByStatusAsync(SagaStatus.Pending);
Console.WriteLine($"Found {pendingSagas.Count} pending sagas");

// Example 7: Search sagas by criteria
var searchCriteria = new Dictionary<string, object>
{
    { "status", SagaStatus.Pending },
    { "definitionId", "order_processing" }
};
var searchedSagas = await sagaRepository.SearchAsync(searchCriteria);
Console.WriteLine($"Found {searchedSagas.Count} sagas matching criteria");

// Example 8: Delete a saga
var deleted = await sagaRepository.DeleteAsync("saga_order_123");
Console.WriteLine($"Saga deleted: {deleted}");

// Example 9: Handle duplicate creation (throws exception)
try
{
    await sagaRepository.CreateAsync(saga);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Expected error: {ex.Message}");
}

// Example 10: Attempt to update non-existent saga
var nonExistentSaga = new Saga { Id = "saga_nonexistent", Definition = new SagaDefinition("Test", "Test") };
var updateResult = await sagaRepository.UpdateAsync(nonExistentSaga);
Console.WriteLine($"Update non-existent saga result: {updateResult?.Id ?? "null (saga not found)}");
```

## InMemoryCompensationTransactionRepository

The `InMemoryCompensationTransactionRepository` provides an in-memory implementation of `ICompensationTransactionRepository` for storing and retrieving compensation transactions during saga execution. It maintains all compensation transactions in a thread-safe dictionary, enabling fast CRUD operations without external dependencies. This implementation is ideal for testing, development environments, or scenarios where persistence is not required.

## DebuggerOptions

The `DebuggerOptions` class provides configuration for the distributed saga debugger, controlling snapshot capture behavior, breakpoint limits, and data inclusion policies. It can be loaded from `appsettings.json` under the `SagaDebugger` section or configured programmatically using the `DebuggerOptionsBuilder` fluent API. The debugger adds zero overhead when disabled, making it safe for production use.

### Usage Example

```csharp
using SagaOrchestrator.Configuration;

// Example 1: Configure via builder API
var debuggerOptions = new DebuggerOptionsBuilder()
    .Enable()
    .WithMaxSnapshotsPerSaga(100)
    .WithAutoCapture(
        onStepTransition: true,
        onCompensation: true,
        onTerminalState: true
    )
    .WithMaxBreakpointsPerSaga(25)
    .WithDataInclusion(
        includePayloads: true,
        includeMetadata: true
    )
    .WithTimeTravel(enabled: true)
    .Build();

Console.WriteLine($"Debugger enabled: {debuggerOptions.IsEnabled}");
Console.WriteLine($"Max snapshots: {debuggerOptions.MaxSnapshotsPerSaga}");
Console.WriteLine($"Auto capture on step transition: {debuggerOptions.AutoCaptureOnStepTransition}");
Console.WriteLine($"Auto capture on compensation: {debuggerOptions.AutoCaptureOnCompensation}");
Console.WriteLine($"Auto capture on terminal state: {debuggerOptions.AutoCaptureOnTerminalState}");
Console.WriteLine($"Max breakpoints: {debuggerOptions.MaxBreakpointsPerSaga}");
Console.WriteLine($"Include step payloads: {debuggerOptions.IncludeStepPayloads}");
Console.WriteLine($"Include saga metadata: {debuggerOptions.IncludeSagaMetadata}");
Console.WriteLine($"Time travel enabled: {debuggerOptions.EnableTimeTravel}");

// Example 2: Load from appsettings.json
// In appsettings.json:
// {
//   "SagaDebugger": {
//     "IsEnabled": true,
//     "MaxSnapshotsPerSaga": 100,
//     "AutoCaptureOnStepTransition": true,
//     "AutoCaptureOnCompensation": true,
//     "AutoCaptureOnTerminalState": true,
//     "MaxBreakpointsPerSaga": 25,
//     "IncludeStepPayloads": true,
//     "IncludeSagaMetadata": true,
//     "EnableTimeTravel": true
//   }
// }

// Then bind to configuration:
// services.Configure<DebuggerOptions>(configuration.GetSection(DebuggerOptions.SectionName));

// Example 3: Minimal configuration with defaults
var minimalOptions = new DebuggerOptionsBuilder()
    .Enable()
    .Build();

Console.WriteLine($"Minimal debugger enabled: {minimalOptions.IsEnabled}");
Console.WriteLine($"Default max snapshots: {minimalOptions.MaxSnapshotsPerSaga}");
Console.WriteLine($"Default max breakpoints: {minimalOptions.MaxBreakpointsPerSaga}");
```

### Usage Example

```csharp
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the in-memory compensation transaction repository
var compensationRepository = new InMemoryCompensationTransactionRepository();

// Example 1: Create a new compensation transaction
var compensation = new CompensationTransaction
{
  Id = "comp_001",
  SagaId = "saga_order_123",
  StepName = "Process Payment",
  Order = 2,
  Status = CompensationStatus.Pending,
  ServiceName = "https://payment-service/api/refund",
  HttpMethod = System.Net.Http.HttpMethod.Post,
  RetryCount = 0,
  MaxRetries = 3,
  TimeoutSeconds = 30,
  CreatedAt = DateTime.UtcNow,
  UpdatedAt = DateTime.UtcNow
};

var createdCompensation = await compensationRepository.CreateAsync(compensation);
Console.WriteLine($"Created compensation: {createdCompensation?.Id} - {createdCompensation?.StepName}");

// Example 2: Retrieve a compensation transaction by ID
var retrievedCompensation = await compensationRepository.GetByIdAsync("comp_001");
if (retrievedCompensation != null)
{
  Console.WriteLine($"Retrieved compensation: {retrievedCompensation.StepName} (Status: {retrievedCompensation.Status}");
}

// Example 3: Update a compensation transaction's status
retrievedCompensation.Status = CompensationStatus.InProgress;
retrievedCompensation.UpdatedAt = DateTime.UtcNow;
var updatedCompensation = await compensationRepository.UpdateAsync(retrievedCompensation);
Console.WriteLine($"Updated compensation status to: {updatedCompensation?.Status}");

// Example 4: Get all compensation transactions for a specific saga
var sagaCompensations = await compensationRepository.GetBySagaIdAsync("saga_order_123");
Console.WriteLine($"Found {sagaCompensations.Count} compensation transactions for saga saga_order_123");
foreach (var comp in sagaCompensations)
{
  Console.WriteLine($"- Compensation {comp.Order}: {comp.StepName} ({comp.Status})");
}

// Example 5: Get all compensation transactions with a specific status
var pendingCompensations = await compensationRepository.GetByStatusAsync(CompensationStatus.Pending);
Console.WriteLine($"Found {pendingCompensations.Count} pending compensations across all sagas");

// Example 6: Get all compensation transactions in the repository
var allCompensations = await compensationRepository.GetAllAsync();
Console.WriteLine($"Total compensations in repository: {allCompensations.Count}");

// Example 7: Delete a compensation transaction
var deleted = await compensationRepository.DeleteAsync("comp_001");
Console.WriteLine($"Compensation deleted: {deleted}");

// Example 8: Handle duplicate creation (throws exception)
try
{
  await compensationRepository.CreateAsync(compensation);
}
catch (InvalidOperationException ex)
{
  Console.WriteLine($"Expected error: {ex.Message}");
}

// Example 9: Attempt to update non-existent compensation
var nonExistentCompensation = new CompensationTransaction { Id = "comp_nonexistent", SagaId = "saga_test" };
var updateResult = await compensationRepository.UpdateAsync(nonExistentCompensation);
Console.WriteLine($"Update non-existent compensation result: {updateResult?.Id ?? "null (compensation not found)}");
```

## SagaOptions

The `SagaOptions` class provides centralized configuration for saga orchestrator behavior, including timeout policies, retry strategies, caching settings, worker configurations, and webhook policies. These options can be loaded from `appsettings.json` under the `SagaOrchestrator` section or configured programmatically using the `SagaOptionsBuilder` fluent API. The configuration controls saga execution characteristics like timeouts, retry behavior, caching duration, background worker intervals, and webhook delivery settings.

## InfrastructureConfiguration

The `InfrastructureConfiguration` record defines infrastructure-level configuration for the saga orchestrator, controlling which infrastructure services are registered in the dependency injection container. It enables or disables caching, HTTP clients, event bus, formatting, logging, integration services, rate limiting, and background workers. This configuration is typically loaded from `appsettings.json` under the `Infrastructure` section or configured programmatically using the `InfrastructureConfiguration` constructor.

### Usage Example

```csharp
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Configuration;

// Example 1: Configure infrastructure services via InfrastructureConfiguration
var services = new ServiceCollection();

var infrastructureConfig = new InfrastructureConfiguration(
    EnableCaching: true,
    EnableHttpClients: true,
    EnableEventBus: true,
    EnableFormatting: true,
    EnableLogging: true,
    EnableIntegration: true,
    EnableRateLimiting: true,
    EnableBackgroundWorkers: true
);

// Configure all enabled infrastructure services
services = infrastructureConfig.ConfigureServices(services);

// Example 2: Minimal configuration with defaults (all features enabled)
var defaultConfig = InfrastructureConfiguration.Default;
services = defaultConfig.ConfigureServices(services);

// Example 3: Disable specific infrastructure features for testing
var testConfig = new InfrastructureConfiguration(
    EnableCaching: false,      // Disable caching for testing
    EnableHttpClients: true,    // Keep HTTP clients enabled
    EnableEventBus: false,      // Disable event bus
    EnableFormatting: true,    // Keep formatting
    EnableLogging: true,        // Keep logging
    EnableIntegration: false,    // Disable integration services
    EnableRateLimiting: false,  // Disable rate limiting
    EnableBackgroundWorkers: false // Disable background workers
);

services = testConfig.ConfigureServices(services);

// Example 4: Load from appsettings.json
// In appsettings.json:
// {
//   "Infrastructure": {
//     "EnableCaching": true,
//     "EnableHttpClients": true,
//     "EnableEventBus": true,
//     "EnableFormatting": true,
//     "EnableLogging": true,
//     "EnableIntegration": true,
//     "EnableRateLimiting": true,
//     "EnableBackgroundWorkers": true
//   }
// }
//
// Then bind to configuration:
// services.Configure<InfrastructureConfiguration>(configuration.GetSection("Infrastructure"));
```

### Usage Example

```csharp
using SagaOrchestrator.Configuration;

// Example 1: Configure via builder API for development environment
var devOptions = new SagaOptionsBuilder()
    .WithDefaultStepTimeout(60)  // 1 minute for local development
    .WithDefaultSagaTimeout(600)  // 10 minutes
    .WithDefaultMaxRetries(5)  // More retries for local testing
    .WithCachingEnabled(false)  // Disable caching for easier debugging
    .WithWebhooksEnabled(false)  // Disable webhooks locally
    .WithTimeoutWorker(true, 10)  // Faster timeout checks
    .WithCompensationWorker(true, 5)  // Faster compensation
    .WithExponentialBackoff(true, 1.5)  // Custom backoff multiplier
    .Build();

Console.WriteLine($"Development Configuration:");
Console.WriteLine($"- Default Step Timeout: {devOptions.TimeoutPolicies.DefaultStepTimeoutSeconds}s");
Console.WriteLine($"- Default Saga Timeout: {devOptions.TimeoutPolicies.DefaultSagaTimeoutSeconds}s");
Console.WriteLine($"- Default Max Retries: {devOptions.RetryPolicies.DefaultMaxRetries}");
Console.WriteLine($"- Caching Enabled: {devOptions.CachePolicies.EnableCaching}");
Console.WriteLine($"- Webhooks Enabled: {devOptions.WebhookPolicies.EnableWebhooks}");

// Example 2: Configure for production environment
var prodOptions = new SagaOptionsBuilder()
    .WithDefaultStepTimeout(30)  // 30 seconds for production
    .WithDefaultSagaTimeout(300)  // 5 minutes
    .WithDefaultMaxRetries(3)  // Standard retry count
    .WithCachingEnabled(true)  // Enable caching in production
    .WithSagaCacheExpiration(5)  // 5 minute cache for sagas
    .WithWebhooksEnabled(true)  // Enable webhooks in production
    .WithTimeoutWorker(true, 30)  // Standard interval
    .WithCompensationWorker(true, 15)
    .WithExponentialBackoff(true)
    .Build();

Console.WriteLine($"\nProduction Configuration:");
Console.WriteLine($"- Default Step Timeout: {prodOptions.TimeoutPolicies.DefaultStepTimeoutSeconds}s");
Console.WriteLine($"- Default Saga Timeout: {prodOptions.TimeoutPolicies.DefaultSagaTimeoutSeconds}s");
Console.WriteLine($"- Saga Cache Expiration: {prodOptions.CachePolicies.SagaCacheExpirationMinutes} minutes");
Console.WriteLine($"- Max Cache Size: {prodOptions.CachePolicies.MaxCacheSize} items");

// Example 3: Load from appsettings.json
// In appsettings.json:
// {
//   "SagaOrchestrator": {
//     "TimeoutPolicies": {
//       "DefaultStepTimeoutSeconds": 30,
//       "DefaultSagaTimeoutSeconds": 300,
//       "MaxStepTimeoutSeconds": 3600,
//       "MaxSagaTimeoutSeconds": 86400,
//       "CompensationTimeoutSeconds": 120
//     },
//     "RetryPolicies": {
//       "DefaultMaxRetries": 3,
//       "DefaultRetryDelayMs": 1000,
//       "MaxRetries": 10,
//       "UseExponentialBackoff": true,
//       "BackoffMultiplier": 2.0,
//       "MaxBackoffDelayMs": 30000
//     },
//     "CachePolicies": {
//       "EnableCaching": true,
//       "SagaCacheExpirationMinutes": 15,
//       "DefinitionCacheExpirationMinutes": 60,
//       "HealthCheckCacheExpirationSeconds": 30,
//       "MaxCacheSize": 10000
//     },
//     "WorkerPolicies": {
//       "EnableTimeoutWorker": true,
//       "TimeoutWorkerIntervalSeconds": 30,
//       "EnableCompensationWorker": true,
//       "CompensationWorkerIntervalSeconds": 15,
//       "EnableEventProcessingWorker": true,
//       "EventProcessingWorkerIntervalSeconds": 10,
//       "MaxEventsToKeep": 10000
//     },
//     "WebhookPolicies": {
//       "EnableWebhooks": true,
//       "WebhookTimeoutSeconds": 10,
//       "MaxWebhookRetries": 3,
//       "WebhookRetryDelayMs": 1000,
//       "MaxWebhookPayloadBytes": 1024000
//     }
//   }
// }
//
// Then bind to configuration:
// services.Configure<SagaOptions>(configuration.GetSection(SagaOptions.SectionName));
```

## SagaIntegrationTests

The `SagaIntegrationTests` class provides comprehensive integration tests for the Saga Orchestrator system, validating end-to-end workflows, concurrent operations, and various configuration scenarios. These tests exercise the complete saga lifecycle including definition creation, saga instantiation, step execution, timeout handling, retry policies, compensation workflows, and status-based queries. The test suite ensures thread safety, proper configuration application, and system reliability across different scenarios.

### Usage Example

```csharp
using SagaOrchestrator.Tests;
using Xunit;

public class ExampleIntegrationTests
{
    [Fact]
    public async Task TestCompleteWorkflow()
    {
        // Create a test instance
        var tests = new SagaIntegrationTests();
        
        // Test end-to-end saga workflow
        await tests.EndToEnd_CreateDefinition_CreateSaga_ExecuteSteps_CompletesSuccessfully();
        
        // Test money transfer scenario with three steps
        await tests.MoneyTransferScenario_DefinitionWithThreeSteps_ValidatesAndCreates();
        
        // Test concurrent operations
        await tests.ConcurrentSagaCreation_MultipleThreads_AllSagasCreatedSuccessfully();
        await tests.ConcurrentSagaExecution_MultipleThreads_AllProcessWithoutErrors();
        
        // Test different timeout and retry configurations
        await tests.SagaWithDifferentTimeouts_CreatesCorrectPolicies();
        await tests.SagaWithDifferentRetryPolicies_CreatesCorrectConfigs();
        
        // Test saga lifecycle and compensation
        await tests.SagaLifecycle_Create_Start_Fail_BeginCompensation_Workflow();
        
        // Test status filtering
        await tests.GetSagasByStatus_ReturnsOnlyMatchingStatus();
        
        // Test edge cases
        await tests.SagaWithManySteps_Handles100Steps();
        await tests.CreateMultipleDefinitions_TracksThemIndependently();
    }
}
```

## CompensationServiceTests

The `CompensationServiceTests` class contains unit tests for the `CompensationService` that validate compensation workflows, transaction creation, and error handling scenarios. These tests verify that the compensation service properly handles saga failures by creating compensation transactions for completed steps, executing them in the correct order, and managing state transitions throughout the compensation process. The test suite ensures thread safety and proper error handling across different compensation scenarios.

### Usage Example

```csharp
using SagaOrchestrator.Tests;
using Xunit;

public class ExampleCompensationTests
{
[Fact]
public async Task TestCompensationWorkflow()
{
// Create a test instance
var tests = new CompensationServiceTests();

// Test constructor validation
var act1 = () => new CompensationService(null!, sagaRepoMock.Object, stepRepoMock.Object);
await act1.Should().Throw<ArgumentNullException>();

// Test saga compensation workflows
await tests.BeginCompensationAsync_WithFailedStatus_TransitionsToCompensating();
await tests.BeginCompensationAsync_CreatesCompensationTransactionsForCompleted();
await tests.BeginCompensationAsync_IgnoresPendingSteps();

// Test compensation execution
await tests.ExecuteNextCompensationAsync_WithPendingCompensation_ReturnsThat();
await tests.ExecuteNextCompensationAsync_SkipsPreviouslyExecuted();
await tests.CompleteCompensationAsync_WithValidTransaction_MarksSagaCompensated();

// Test error scenarios
await tests.BeginCompensationAsync_WithNullSaga_ThrowsArgumentNullException();
await tests.BeginCompensationAsync_WithRunningStatus_ThrowsSagaException();
await tests.ExecuteNextCompensationAsync_WithNonexistentSaga_ThrowsSagaNotFoundException();
}
}
```

## SagaDefinitionValidatorTests

The `SagaDefinitionValidatorTests` class contains unit tests for the `SagaDefinitionValidator` that validate saga definition structure, step configurations, and business rule compliance. These tests verify that saga definitions meet all structural requirements including name validation, step count limits, step ordering, service URL formats, timeout constraints, and retry policy validation. The test suite ensures that invalid definitions are properly rejected with appropriate error messages.

### Usage Example

```csharp
using SagaOrchestrator.Tests;
using Xunit;

public class ExampleValidatorTests
{
    [Fact]
    public async Task ValidateDefinition_WithValidDefinition_DoesNotThrow()
    {
        // Arrange
        var tests = new SagaDefinitionValidatorTests();
        var definition = new SagaDefinition(
            "Order Processing Saga",
            "Handles complete order processing workflow"
        );
        
        definition.AddStep(new SagaStepDefinition(
            "Validate Order",
            "https://order-service/api/validate",
            HttpMethod.Get,
            "Validate customer order details"
        ));
        
        definition.AddStep(new SagaStepDefinition(
            "Process Payment",
            "https://payment-service/api/charge",
            HttpMethod.Post,
            "Charge customer payment method"
        ));
        
        // Act & Assert
        await tests.ValidateAsync_WithValidDefinition_DoesNotThrow(definition);
    }

    [Fact]
    public async Task ValidateDefinition_WithInvalidDefinition_Throws()
    {
        // Arrange
        var tests = new SagaDefinitionValidatorTests();
        var invalidDefinition = new SagaDefinition(
            "", // Empty name
            ""
        );
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => tests.ValidateAsync_WithInvalidDefinition_Throws(invalidDefinition)
        );
    }

    [Fact]
    public async Task ValidateDefinition_WithTooManySteps_Throws()
    {
        // Arrange
        var tests = new SagaDefinitionValidatorTests();
        var definition = new SagaDefinition("Test Saga", "Test");
        
        // Add 101 steps (exceeds maximum of 100)
        for (int i = 0; i <= 100; i++)
        {
            definition.AddStep(new SagaStepDefinition(
                $"Step {i}",
                $"https://service-{i}.com/api",
                HttpMethod.Get,
                "Test step"
            ));
        }
        
        // Act & Assert
        var errors = await tests.ValidateAndGetErrorsAsync_TooManySteps_ReturnsError(definition);
        Assert.Contains(errors, e => e.Contains("Maximum 100 steps"));
    }

    [Fact]
    public async Task ValidateDefinition_WithDuplicateStepOrder_Throws()
    {
        // Arrange
        var tests = new SagaDefinitionValidatorTests();
        var definition = new SagaDefinition("Test Saga", "Test");
        
        var step1 = new SagaStepDefinition(
            "First Step",
            "https://service1.com/api",
            HttpMethod.Get,
            "First step"
        );
        var step2 = new SagaStepDefinition(
            "Second Step",
            "https://service2.com/api",
            HttpMethod.Post,
            "Second step"
        );
        
        definition.AddStep(step1);
        definition.AddStep(step2);
        
        // Try to add another step with order 1 (duplicate)
        var step3 = new SagaStepDefinition(
            "Third Step",
            "https://service3.com/api",
            HttpMethod.Get,
            "Third step"
        );
        step3.Order = 1;
        definition.AddStep(step3);
        
        // Act & Assert
        var errors = await tests.ValidateAndGetErrorsAsync_DuplicateStepOrder_ReturnsError(definition);
        Assert.Contains(errors, e => e.Contains("Duplicate step order"));
    }
}
```

## InMemorySagaDefinitionRepository

The `InMemorySagaDefinitionRepository` provides an in-memory implementation of `ISagaDefinitionRepository` for storing and retrieving saga workflow definitions. It maintains all saga definitions in a thread-safe dictionary, enabling fast CRUD operations without external dependencies. This implementation is ideal for testing, development environments, or scenarios where persistence is not required.

### Usage Example

```csharp
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

// Initialize the in-memory saga definition repository
var sagaDefinitionRepository = new InMemorySagaDefinitionRepository();

// Example 1: Create a new saga definition
var definition = new SagaDefinition(
  "Order Processing Saga",
  "Handles the complete order processing workflow from validation to shipping"
);

definition.AddStep(new SagaStepDefinition(
  "Validate Order",
  "https://order-service/api/validate",
  HttpMethod.Get,
  "Validate customer order details and inventory availability"
));

definition.AddStep(new SagaStepDefinition(
  "Process Payment",
  "https://payment-service/api/charge",
  HttpMethod.Post,
  "Charge customer payment method"
));

definition.AddStep(new SagaStepDefinition(
  "Ship Order",
  "https://shipping-service/api/ship",
  HttpMethod.Post,
  "Create shipping label and schedule delivery"
));

var createdDefinition = await sagaDefinitionRepository.CreateAsync(definition);
Console.WriteLine($"Created definition: {createdDefinition?.Name} (ID: {createdDefinition?.Id})");

// Example 2: Retrieve a definition by ID
var retrievedDefinition = await sagaDefinitionRepository.GetByIdAsync(createdDefinition.Id);
if (retrievedDefinition != null)
{
  Console.WriteLine($"Retrieved definition: {retrievedDefinition.Name} (Version: {retrievedDefinition.Version})");
}

// Example 3: Retrieve a definition by name
var definitionByName = await sagaDefinitionRepository.GetByNameAsync("Order Processing Saga");
Console.WriteLine($"Found definition by name: {definitionByName?.Id}");

// Example 4: Update a definition's active status
retrievedDefinition.IsActive = true;
retrievedDefinition.UpdatedAt = DateTime.UtcNow;
var updatedDefinition = await sagaDefinitionRepository.UpdateAsync(retrievedDefinition);
Console.WriteLine($"Updated definition active status to: {updatedDefinition?.IsActive}");

// Example 5: Get all saga definitions
var allDefinitions = await sagaDefinitionRepository.GetAllAsync();
Console.WriteLine($"Total definitions in repository: {allDefinitions.Count}");

// Example 6: Get all active saga definitions
var activeDefinitions = await sagaDefinitionRepository.GetActiveAsync();
Console.WriteLine($"Active definitions: {activeDefinitions.Count}");

// Example 7: Search definitions by criteria
var searchCriteria = new Dictionary<string, object>
{
  { "name", "Order" },
  { "activeOnly", true }
};
var searchResults = await sagaDefinitionRepository.SearchAsync(searchCriteria);
Console.WriteLine($"Found {searchResults.Count} definitions matching criteria");

// Example 8: Delete a definition
var deleted = await sagaDefinitionRepository.DeleteAsync(createdDefinition.Id);
Console.WriteLine($"Definition deleted: {deleted}");

// Example 9: Handle duplicate creation (throws exception)
try
{
  await sagaDefinitionRepository.CreateAsync(definition);
}
catch (InvalidOperationException ex)
{
  Console.WriteLine($"Expected error: {ex.Message}");
}

// Example 10: Attempt to update non-existent definition
var nonExistentDefinition = new SagaDefinition("Non-existent", "Test");
nonExistentDefinition.Id = "non_existent_id";
var updateResult = await sagaDefinitionRepository.UpdateAsync(nonExistentDefinition);
Console.WriteLine($"Update non-existent definition result: {updateResult?.Id ?? "null (definition not found)}");
```