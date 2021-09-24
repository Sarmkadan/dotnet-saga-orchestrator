# Getting Started with Saga Orchestrator

This guide will walk you through setting up and using the Saga Orchestrator for your first distributed transaction.

## Prerequisites

- .NET 10 SDK or later
- Basic knowledge of C# and async/await
- Understanding of the Saga pattern (recommended)
- Docker (optional, for containerized deployment)

## Installation

### Option 1: Clone from Repository

```bash
git clone https://github.com/Sarmkadan/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator
dotnet build
```

### Option 2: Create New Project

```bash
# Create a new console project
dotnet new console -n MyOrderSaga
cd MyOrderSaga

# Add project reference
dotnet add reference ../dotnet-saga-orchestrator/dotnet-saga-orchestrator.csproj
```

### Option 3: NuGet Package (Future Release)

```bash
dotnet package add SagaOrchestrator
```

## Your First Saga

### Step 1: Setup Dependency Injection

Create a console application and setup services:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Models;

// Configure services
var services = new ServiceCollection();
services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Add saga orchestrator with default configuration
services.AddSagaOrchestrator();

var provider = services.BuildServiceProvider();
```

### Step 2: Get Required Services

```csharp
var definitionService = provider.GetRequiredService<SagaDefinitionService>();
var orchestration = provider.GetRequiredService<SagaOrchestrationService>();
var logger = provider.GetRequiredService<ILogger<Program>>();
```

### Step 3: Create a Saga Definition

A saga definition describes the workflow - the steps that must be executed:

```csharp
// Create the definition
var definition = await definitionService.CreateDefinitionAsync(
    "Simple Order Processing",
    "Process a customer order");

logger.LogInformation($"Created definition: {definition.Name} ({definition.Id})");
```

### Step 4: Add Steps to the Definition

Each step represents a service call that can be compensated:

```csharp
// Step 1: Reserve inventory
var step1 = new SagaStepDefinition(
    "Reserve Inventory",                           // Step name
    "inventory-service",                           // Service name
    "http://inventory-service/api/reserve",        // Execute URL
    "http://inventory-service/api/release");       // Compensate URL

step1.SetTimeout(30);                              // 30 second timeout
step1.SetRetryPolicy(3, 1000);                     // 3 retries, 1 second delay

await definitionService.AddStepAsync(definition.Id, step1);
logger.LogInformation("Added: Reserve Inventory");

// Step 2: Process payment
var step2 = new SagaStepDefinition(
    "Process Payment",
    "payment-service",
    "http://payment-service/api/charge",
    "http://payment-service/api/refund");

step2.SetTimeout(30);
step2.SetRetryPolicy(2, 1500);

await definitionService.AddStepAsync(definition.Id, step2);
logger.LogInformation("Added: Process Payment");
```

### Step 5: Validate the Definition

Before executing, validate the definition:

```csharp
var validationResult = definitionService.ValidateDefinition(definition);

if (!validationResult.IsValid)
{
    logger.LogError("Definition validation failed:");
    foreach (var error in validationResult.Errors)
    {
        logger.LogError($"  - {error}");
    }
    return 1;
}

logger.LogInformation("Definition validation passed");
```

### Step 6: Create a Saga Instance

Once the definition is validated, create a saga instance:

```csharp
// Retrieve the definition with all steps
var retrievedDefinition = await definitionService.GetDefinitionAsync(definition.Id);

// Create saga instance
var saga = await orchestration.CreateSagaAsync(
    retrievedDefinition,
    maxRetries: 3,
    timeoutSeconds: 300);

logger.LogInformation($"Created saga: {saga.Id}");
logger.LogInformation($"Status: {saga.Status}");
logger.LogInformation($"Correlation ID: {saga.CorrelationId}");
```

### Step 7: Start Saga Execution

```csharp
// Start the saga
var startedSaga = await orchestration.StartSagaAsync(saga.Id);

logger.LogInformation($"Saga started");
logger.LogInformation($"Total steps: {startedSaga.Steps.Count}");
```

### Step 8: Execute Steps

Execute steps one by one:

```csharp
for (int i = 0; i < startedSaga.Steps.Count; i++)
{
    logger.LogInformation($"\nExecuting step {i + 1}...");
    
    var step = await orchestration.ExecuteNextStepAsync(saga.Id);
    
    if (step != null)
    {
        logger.LogInformation($"Step: {step.Name}");
        logger.LogInformation($"Status: {step.Status}");
        logger.LogInformation($"Started: {step.StartedAt:O}");
    }
}
```

### Step 9: Check Final Status

```csharp
// Get final saga state
var finalSaga = await orchestration.GetSagaAsync(saga.Id);

logger.LogInformation($"\n=== Saga Completed ===");
logger.LogInformation($"Status: {finalSaga.Status}");
logger.LogInformation($"Total steps: {finalSaga.Steps.Count}");
logger.LogInformation($"Completed steps: {finalSaga.Steps.Count(s => s.Status == SagaStepStatus.Completed)}");
logger.LogInformation($"Failed steps: {finalSaga.Steps.Count(s => s.Status == SagaStepStatus.Failed)}");

if (finalSaga.Status == SagaStatus.Completed)
{
    logger.LogInformation("✓ Order processed successfully!");
}
else if (finalSaga.Status == SagaStatus.Failed)
{
    logger.LogInformation("✗ Order processing failed");
    logger.LogInformation("Initiating compensation...");
    
    await orchestration.CompensateSagaAsync(saga.Id);
}
```

## Running the Example

Save this as `Program.cs` and run:

```bash
dotnet run
```

You should see output like:

```
iex [18:30:45] Created definition: Simple Order Processing (saga-def-123)
iex [18:30:46] Added: Reserve Inventory
iex [18:30:46] Added: Process Payment
iex [18:30:46] Definition validation passed
iex [18:30:47] Created saga: saga-123
iex [18:30:47] Status: Pending
iex [18:30:47] Correlation ID: corr-456
iex [18:30:47] Saga started
iex [18:30:47] Total steps: 2

iex [18:30:48] Executing step 1...
iex [18:30:48] Step: Reserve Inventory
iex [18:30:48] Status: Completed
iex [18:30:48] Started: 2026-05-04T18:30:48.000Z

iex [18:30:49] Executing step 2...
iex [18:30:49] Step: Process Payment
iex [18:30:49] Status: Completed
iex [18:30:49] Started: 2026-05-04T18:30:49.000Z

=== Saga Completed ===
Status: Completed
Total steps: 2
Completed steps: 2
Failed steps: 0
✓ Order processed successfully!
```

## Next Steps

Now that you've created your first saga, try:

1. **Add more steps** - Create a more complex workflow
2. **Handle failures** - Test compensation by injecting failures
3. **Configure retry policies** - Experiment with different retry strategies
4. **Use webhooks** - Subscribe to saga events
5. **Monitor metrics** - Check saga execution statistics

## Common Patterns

### Pattern 1: Sequential Processing

Steps execute one after another:

```csharp
// Step 1 → Step 2 → Step 3
var step1 = new SagaStepDefinition(...);
var step2 = new SagaStepDefinition(...);
var step3 = new SagaStepDefinition(...);

await definitionService.AddStepAsync(definition.Id, step1);
await definitionService.AddStepAsync(definition.Id, step2);
await definitionService.AddStepAsync(definition.Id, step3);
```

### Pattern 2: Conditional Steps

Execute steps based on conditions:

```csharp
var saga = await orchestration.GetSagaAsync(sagaId);

if (saga.Steps.All(s => s.Status == SagaStepStatus.Completed))
{
    // All steps done, execute final step
    await orchestration.ExecuteNextStepAsync(sagaId);
}
```

### Pattern 3: Parallel Processing

Use Task.WhenAll for concurrent execution:

```csharp
var tasks = new List<Task>();

foreach (var saga in sagas)
{
    tasks.Add(orchestration.StartSagaAsync(saga.Id));
}

await Task.WhenAll(tasks);
```

## Troubleshooting

### Issue: Saga Doesn't Execute Steps

**Check**: Is the saga in "Running" status?

```csharp
var saga = await orchestration.GetSagaAsync(sagaId);
logger.LogInformation($"Current status: {saga.Status}");

if (saga.Status != SagaStatus.Running)
{
    var started = await orchestration.StartSagaAsync(sagaId);
}
```

### Issue: Compensation Not Triggering

**Check**: Is the saga in "Failed" status?

```csharp
var saga = await orchestration.GetSagaAsync(sagaId);

if (saga.Status == SagaStatus.Failed)
{
    await orchestration.CompensateSagaAsync(sagaId);
}
```

### Issue: Timeout Errors

**Check**: Are step timeouts set appropriately?

```csharp
// Increase timeout
step.SetTimeout(60);  // 60 seconds instead of default 30

// Or increase saga timeout
var saga = await orchestration.CreateSagaAsync(
    definition,
    timeoutSeconds: 600);  // 10 minutes
```

## Best Practices

1. **Keep steps small** - Each step should represent a single service call
2. **Set appropriate timeouts** - Don't set too aggressive timeouts
3. **Use correlation IDs** - Track requests across services
4. **Plan compensation** - Think about how to undo each step
5. **Monitor health** - Check saga metrics regularly
6. **Log events** - Enable detailed logging during development
7. **Test failures** - Simulate failures to test compensation
8. **Use retries wisely** - Balance resilience with failure detection

## Resources

- [Architecture Guide](./architecture.md) - Deep dive into system architecture
- [API Reference](./api-reference.md) - Complete API documentation
- [Configuration Guide](../docs/configuration.md) - Detailed configuration options
- [Examples](../examples/) - Full working examples
