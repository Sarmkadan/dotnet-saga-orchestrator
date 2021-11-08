# API Reference

Complete API documentation for the Saga Orchestrator.

## Service Interfaces

### SagaOrchestrationService

Main orchestrator for saga lifecycle management.

```csharp
public interface ISagaOrchestrationService
{
    /// Creates a new saga instance from a definition
    /// Returns a Saga in Pending status
    Task<Saga> CreateSagaAsync(SagaDefinition definition, 
        int maxRetries = 3, int timeoutSeconds = 300);

    /// Starts saga execution, moving it to Running status
    /// Initializes all steps to Pending
    Task<Saga> StartSagaAsync(string sagaId);

    /// Executes the next pending step in the saga
    /// Returns null if all steps completed
    Task<SagaStep> ExecuteNextStepAsync(string sagaId);

    /// Retrieves saga by ID with all steps
    Task<Saga> GetSagaAsync(string sagaId);

    /// Lists all sagas, optionally filtered by status
    Task<List<Saga>> ListSagasAsync(SagaStatus? status = null);

    /// Initiates compensation for a saga
    /// Returns saga in Compensating status
    Task<Saga> CompensateSagaAsync(string sagaId, 
        CompensationStrategy strategy = CompensationStrategy.ReverseOrder);

    /// Aborts saga execution immediately
    Task<Saga> AbortSagaAsync(string sagaId, string reason = "");
}
```

### SagaDefinitionService

Manages saga definition lifecycle.

```csharp
public interface ISagaDefinitionService
{
    /// Creates a new saga definition
    /// Definition starts with no steps
    Task<SagaDefinition> CreateDefinitionAsync(string name, 
        string description = "");

    /// Retrieves definition by ID with all steps
    Task<SagaDefinition> GetDefinitionAsync(string definitionId);

    /// Adds a step to a definition
    /// Steps are executed in the order they were added
    Task<SagaStepDefinition> AddStepAsync(string definitionId, 
        SagaStepDefinition step);

    /// Lists all definitions in the system
    Task<List<SagaDefinition>> ListDefinitionsAsync();

    /// Validates definition structure and steps
    ValidationResult ValidateDefinition(SagaDefinition definition);

    /// Updates an existing definition
    Task<SagaDefinition> UpdateDefinitionAsync(string definitionId, 
        SagaDefinition definition);

    /// Removes a definition (if no active sagas use it)
    Task<bool> DeleteDefinitionAsync(string definitionId);
}
```

### CompensationService

Handles saga compensation and failure recovery.

```csharp
public interface ICompensationService
{
    /// Executes compensation for a failed saga
    /// Returns the compensation result
    Task<CompensationResult> CompensateSagaAsync(string sagaId, 
        CompensationStrategy strategy);

    /// Gets current compensation status for a saga
    Task<CompensationTransaction> GetCompensationStatusAsync(string sagaId);

    /// Lists all compensation transactions
    /// Optionally filtered by status
    Task<List<CompensationTransaction>> ListCompensationsAsync(
        CompensationStatus? status = null);

    /// Retries failed compensations
    Task<CompensationResult> RetryCompensationAsync(string sagaId);

    /// Manually marks compensation as complete
    Task<CompensationTransaction> CompleteCompensationAsync(string sagaId, 
        string notes = "");
}
```

### HealthCheckService

System health and status monitoring.

```csharp
public interface IHealthCheckService
{
    /// Gets current system health status
    Task<HealthStatus> GetHealthAsync();

    /// Checks individual service connectivity
    Task<ServiceHealth> CheckServiceAsync(string serviceName);

    /// Gets all active sagas count
    Task<int> GetActiveSagasCountAsync();

    /// Gets system uptime
    Task<TimeSpan> GetUptimeAsync();
}
```

### MetricsService

Saga execution statistics and performance data.

```csharp
public interface IMetricsService
{
    /// Gets aggregate metrics for all sagas
    SagaMetrics GetMetrics();

    /// Gets metrics for a specific saga
    Task<SagaMetrics> GetSagaMetricsAsync(string sagaId);

    /// Gets metrics for a specific step
    Task<StepMetrics> GetStepMetricsAsync(string stepName);

    /// Gets performance percentiles (P50, P95, P99)
    PercentileMetrics GetPercentiles();

    /// Resets all metrics
    void ResetMetrics();
}
```

## Model Classes

### Saga

Represents a saga instance.

```csharp
public class Saga
{
    /// Unique identifier for this saga
    public string Id { get; set; }

    /// Reference to the definition this saga is based on
    public string DefinitionId { get; set; }

    /// Current execution status
    public SagaStatus Status { get; set; }

    /// All steps in this saga
    public List<SagaStep> Steps { get; set; }

    /// Correlation ID for distributed tracing
    public string CorrelationId { get; set; }

    /// When was this saga created
    public DateTime CreatedAt { get; set; }

    /// When did execution start
    public DateTime? StartedAt { get; set; }

    /// When did execution complete
    public DateTime? CompletedAt { get; set; }

    /// Custom metadata
    public string Metadata { get; set; }

    /// Maximum retry attempts
    public int MaxRetries { get; set; }

    /// Total saga timeout in seconds
    public int TimeoutSeconds { get; set; }

    /// Current retry attempt
    public int CurrentRetryCount { get; set; }
}
```

### SagaStep

Represents a single step execution.

```csharp
public class SagaStep
{
    /// Unique step identifier
    public string Id { get; set; }

    /// Parent saga ID
    public string SagaId { get; set; }

    /// Reference to step definition
    public string DefinitionId { get; set; }

    /// Step display name
    public string Name { get; set; }

    /// Current step status
    public SagaStepStatus Status { get; set; }

    /// When execution started
    public DateTime? StartedAt { get; set; }

    /// When execution completed
    public DateTime? CompletedAt { get; set; }

    /// Step response from service
    public string Response { get; set; }

    /// Error details if step failed
    public string ErrorMessage { get; set; }

    /// Number of retry attempts
    public int RetryCount { get; set; }
}
```

### SagaDefinition

Defines a saga workflow.

```csharp
public class SagaDefinition
{
    /// Unique definition identifier
    public string Id { get; set; }

    /// Workflow name
    public string Name { get; set; }

    /// Workflow description
    public string Description { get; set; }

    /// Steps to execute
    public List<SagaStepDefinition> Steps { get; set; }

    /// When definition was created
    public DateTime CreatedAt { get; set; }

    /// Custom metadata
    public string Metadata { get; set; }
}
```

### SagaStepDefinition

Defines a single step in a workflow.

```csharp
public class SagaStepDefinition
{
    /// Step name
    public string Name { get; set; }

    /// Service this step targets
    public string ServiceName { get; set; }

    /// URL to execute for this step
    public string ExecutionUrl { get; set; }

    /// URL to execute for compensation
    public string CompensationUrl { get; set; }

    /// Step timeout in seconds
    public int TimeoutSeconds { get; set; }

    /// Retry policy for this step
    public RetryPolicy RetryPolicy { get; set; }

    /// Circuit breaker settings
    public CircuitBreakerSettings CircuitBreakerSettings { get; set; }

    /// Custom metadata
    public Dictionary<string, object> Metadata { get; set; }

    /// Sets timeout for this step
    public void SetTimeout(int seconds);

    /// Sets retry policy
    public void SetRetryPolicy(int maxRetries, int delayMs);
}
```

## Enums

### SagaStatus

```csharp
public enum SagaStatus
{
    Pending = 0,           // Created but not started
    Initialized = 1,       // Definition loaded
    Running = 2,           // Currently executing
    Completed = 3,         // All steps succeeded
    Failed = 4,            // One or more steps failed
    Compensating = 5,      // Compensation in progress
    Compensated = 6,       // Compensation complete
    Aborted = 7,           // Manually aborted
    TimedOut = 8           // Exceeded timeout
}
```

### SagaStepStatus

```csharp
public enum SagaStepStatus
{
    Pending = 0,           // Not yet executed
    Running = 1,           // Currently executing
    Completed = 2,         // Succeeded
    Failed = 3,            // Failed
    Compensating = 4,      // Compensation in progress
    Compensated = 5,       // Compensation complete
    TimedOut = 6,          // Exceeded timeout
    Skipped = 7            // Skipped in compensation
}
```

### CompensationStatus

```csharp
public enum CompensationStatus
{
    NotStarted = 0,        // Compensation not initiated
    InProgress = 1,        // Compensation running
    Completed = 2,         // Compensation finished
    PartiallyFailed = 3,   // Some compensations failed
    ManualIntervention = 4 // Waiting for manual action
}
```

### CompensationStrategy

```csharp
public enum CompensationStrategy
{
    ReverseOrder = 0,      // LIFO - reverse order of completion
    ForwardOrder = 1,      // FIFO - execution order
    FromFailurePoint = 2,  // Only from failure point
    Parallel = 3,          // All concurrent
    Manual = 4             // Requires external intervention
}
```

## Exceptions

All exceptions inherit from `SagaException`.

### SagaException

```csharp
public class SagaException : Exception
{
    public string ErrorCode { get; set; }
    public string SagaId { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### SagaNotFoundException

Thrown when saga doesn't exist.

```csharp
public class SagaNotFoundException : SagaException
{
    // Example: throw new SagaNotFoundException("saga-123");
}
```

### SagaTimeoutException

Thrown when saga exceeds timeout.

```csharp
public class SagaTimeoutException : SagaException
{
    public int TimeoutSeconds { get; set; }
    public TimeSpan ElapsedTime { get; set; }
}
```

### SagaStepExecutionException

Thrown when step execution fails.

```csharp
public class SagaStepExecutionException : SagaException
{
    public string StepName { get; set; }
    public int RetryCount { get; set; }
    public string ServiceName { get; set; }
}
```

### InvalidSagaDefinitionException

Thrown when definition validation fails.

```csharp
public class InvalidSagaDefinitionException : SagaException
{
    public List<string> ValidationErrors { get; set; }
}
```

## DTOs

### SagaResponse

API response model for saga.

```csharp
public class SagaResponse
{
    public string Id { get; set; }
    public string Status { get; set; }
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<StepResponse> Steps { get; set; }
}
```

### CreateSagaRequest

API request model.

```csharp
public class CreateSagaRequest
{
    [Required]
    public string DefinitionId { get; set; }

    public int? MaxRetries { get; set; }
    
    public int? TimeoutSeconds { get; set; }
    
    public string CorrelationId { get; set; }
}
```

### SagaCommandResult

Generic operation result.

```csharp
public class SagaCommandResult<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
    public List<string> Errors { get; set; }
}
```

## Common Usage Patterns

### Creating a Saga

```csharp
var definition = await definitionService.GetDefinitionAsync(defId);
var saga = await orchestration.CreateSagaAsync(
    definition,
    maxRetries: 3,
    timeoutSeconds: 300);
```

### Executing Steps

```csharp
await orchestration.StartSagaAsync(saga.Id);

while (true)
{
    var step = await orchestration.ExecuteNextStepAsync(saga.Id);
    if (step == null) break;
}
```

### Handling Failures

```csharp
var saga = await orchestration.GetSagaAsync(sagaId);

if (saga.Status == SagaStatus.Failed)
{
    await orchestration.CompensateSagaAsync(
        sagaId,
        CompensationStrategy.ReverseOrder);
}
```

### Monitoring Progress

```csharp
var metrics = metricsService.GetMetrics();
Console.WriteLine($"Success Rate: {metrics.SuccessRate:P2}");
Console.WriteLine($"Average Duration: {metrics.AverageDurationMs}ms");
Console.WriteLine($"P95 Duration: {metrics.P95DurationMs}ms");
```

## Async/Await Convention

All I/O operations use async/await:

```csharp
// ✓ Correct
await orchestration.StartSagaAsync(sagaId);
var saga = await orchestration.GetSagaAsync(sagaId);

// ✗ Incorrect - Don't block
var saga = orchestration.GetSagaAsync(sagaId).Result;
```

## Dependency Injection

Register services in DI container:

```csharp
services.AddSagaOrchestrator();

// Or individual registration
services.AddScoped<SagaOrchestrationService>();
services.AddScoped<SagaDefinitionService>();
services.AddScoped<CompensationService>();
services.AddScoped<HealthCheckService>();
services.AddScoped<MetricsService>();
```

## Return Types

### Task<T>

Async operations returning a single result:

```csharp
Task<Saga> CreateSagaAsync(...);
Task<SagaStep> ExecuteNextStepAsync(...);
```

### Task<List<T>>

Async operations returning collections:

```csharp
Task<List<Saga>> ListSagasAsync();
Task<List<SagaDefinition>> ListDefinitionsAsync();
```

### Task

Async operations with no return value:

```csharp
Task StartSagaAsync(...);
Task AbortSagaAsync(...);
```

## Validation

Input validation happens at service boundaries:

```csharp
var definition = new SagaDefinition
{
    Name = "My Saga",
    Steps = new List<SagaStepDefinition> { ... }
};

var result = definitionService.ValidateDefinition(definition);
if (!result.IsValid)
{
    // Handle errors
    foreach (var error in result.Errors)
    {
        Console.WriteLine(error);
    }
}
```

## Error Handling

All operations throw `SagaException` or subclasses:

```csharp
try
{
    var saga = await orchestration.GetSagaAsync("invalid-id");
}
catch (SagaNotFoundException ex)
{
    Console.WriteLine($"Saga not found: {ex.SagaId}");
}
catch (SagaException ex)
{
    Console.WriteLine($"Error {ex.ErrorCode}: {ex.Message}");
}
```

## Rate Limits

The HTTP client respects rate limits:

```csharp
// Returns false if rate limited
var allowed = await rateLimiter.AllowRequestAsync("service", rps: 100);
if (!allowed)
{
    // Implement backoff or fallback
}
```
