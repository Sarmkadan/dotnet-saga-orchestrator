# SagaDefinition
Represents the definition of a saga workflow, encapsulating its metadata, steps, and execution settings within the dotnet-saga-orchestrator library.

## API
| Member | Description |
|--------|-------------|
| **Id** | Unique identifier for the saga definition. |
| **Name** | Human‑readable name of the saga. |
| **Description** | Optional detailed description of the saga’s purpose. |
| **Version** | Incremental version number used for change tracking. |
| **Steps** | Collection of `SagaStepDefinition` objects that define the saga’s execution order. |
| **CreatedAt** | Timestamp indicating when the definition was instantiated. |
| **IsActive** | Flag indicating whether the saga definition is currently active and eligible for execution. |
| **CompensationStrategy** | Specifies how compensating actions are applied when a step fails. |
| **SagaDefinition()** | Parameterless constructor that creates a new, empty saga definition. |
| **SagaDefinition(...)** | Overloaded constructor that allows initializing the definition with values for its properties (e.g., Id, Name, Version, etc.). |
| **AddStep(SagaStepDefinition step)** | Adds the supplied step to the end of the `Steps` collection. Throws an exception if `step` is null. |
| **Validate()** | Performs validation of the saga definition (e.g., ensures required fields are present and steps are correctly ordered). Returns `true` if the definition is valid; otherwise `false`. |
| **GetStepByName(string name)** | Returns the `SagaStepDefinition` with the matching `Name`, or `null` if no step matches. Throws an exception if `name` is null or empty. |
| **GetStepByOrder(int order)** | Returns the `SagaStepDefinition` at the zero‑based position `order` within the `Steps` collection, or `null` if the index is out of range. Throws an exception if `order` is negative. |

## Usage
```csharp
// Create a new saga definition and configure its basic properties
var saga = new SagaDefinition
{
    Id = "order-processing",
    Name = "Order Processing Saga",
    Description = "Handles order creation, payment, and shipment.",
    Version = 1,
    IsActive = true,
    CompensationStrategy = CompensationStrategy.Backward
};

// Add steps to the saga
saga.AddStep(new SagaStepDefinition { Name = "ValidateOrder", Order = 0 });
saga.AddStep(new SagaStepDefinition { Name = "ProcessPayment", Order = 1 });
saga.AddStep(new SagaStepDefinition { Name = "ShipOrder", Order = 2 });

// Validate the definition before use
if (!saga.Validate())
{
    throw new InvalidOperationException("Saga definition is invalid.");
}

// Retrieve a step by name
var paymentStep = saga.GetStepByName("ProcessPayment");
```
```csharp
// Example using the overloaded constructor to initialize a definition
var saga = new SagaDefinition(
    id: "inventory-sync",
    name: "Inventory Synchronization",
    description: "Keeps inventory levels consistent across services.",
    version: 2,
    isActive: true,
    compensationStrategy: CompensationStrategy.Forward,
    createdAt: DateTime.UtcNow);

// Steps can be added after construction
saga.Steps.Add(new SagaStepDefinition { Name = "ReserveStock", Order = 0 });
saga.Steps.Add(new SagaStepDefinition { Name = "UpdateCatalog", Order = 1 });

// Fetch the second step by its order
var secondStep = saga.GetStepByOrder(1);
```

## Notes
- The `Steps` list is mutable; modifying it after the saga has started executing may lead to undefined behavior. It is recommended to treat the definition as immutable once deployed.
- Validation does not guarantee runtime success; it only checks structural correctness (e.g., non‑null required fields, unique step names, sequential ordering).
- Thread safety: The type itself does not provide synchronization. Concurrent reads are safe, but concurrent modifications to `Steps` or property values should be guarded by the caller.
- If `CompensationStrategy` is set to `null`, the saga will not execute any compensating actions on failure.
- The `CreatedAt` property is set automatically by the parameterless constructor to the current UTC time; the overloaded constructor allows overriding this value if needed.
