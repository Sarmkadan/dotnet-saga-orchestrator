// existing content ...

## SagaDefinitionServiceExtensions

The `SagaDefinitionServiceExtensions` class provides a set of convenience extension methods for the `SagaDefinitionService`, making it easier to work with saga definitions. These extensions allow you to create and manage saga definitions, including adding and removing steps, checking for existing definitions, and retrieving active and inactive definitions.

### Usage Example

```csharp
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;

// Create a saga definition service
var sagaDefinitionService = new SagaDefinitionService();

// Create and activate a new saga definition
var definition = await sagaDefinitionService.CreateAndActivateDefinitionAsync(
    "Order Processing Saga",
    "Process orders asynchronously");

// Add multiple steps to the saga definition
var steps = new[]
{
    new SagaStepDefinition("Reserve Inventory", "inventory-service", "http://inventory/reserve", "http://inventory/release"),
    new SagaStepDefinition("Process Payment", "payment-service", "http://payment/charge", "http://payment/refund")
};

definition = await sagaDefinitionService.AddStepsAsync(
    definition.Id,
    steps);

// Check if a saga definition exists
bool exists = await sagaDefinitionService.DefinitionExistsAsync("Order Processing Saga");

// Get or create a saga definition
definition = await sagaDefinitionService.GetOrCreateDefinitionAsync(
    "Order Processing Saga",
    "Process orders asynchronously",
    activateIfCreated: true);

// Validate a saga definition
sagaDefinitionService.ValidateOrThrow(definition);

// Get active and inactive saga definitions
var activeDefinitions = await sagaDefinitionService.GetActiveDefinitionsAsync();
var inactiveDefinitions = await sagaDefinitionService.GetInactiveDefinitionsAsync();

// Get a saga definition by name and validate it
definition = await sagaDefinitionService.GetAndValidateDefinitionAsync("Order Processing Saga");

// Create a new version of an existing saga definition
var newDefinition = await sagaDefinitionService.CreateNewVersionAsync(
    definition.Id,
    activateNewVersion: true);
```