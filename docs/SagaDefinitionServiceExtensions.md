# SagaDefinitionServiceExtensions

Provides a set of static extension methods for managing saga definitions in the `dotnet-saga-orchestrator` framework. These methods encapsulate common operations such as creating, activating, modifying, validating, and querying saga definitions, and are designed to be used against an implementation of the underlying saga definition service.

## API

### `CreateAndActivateDefinitionAsync`

Creates a new saga definition and immediately activates it.

- **Parameters**  
  - `this ISagaDefinitionService service` – The service instance being extended.  
  - `string name` – The name of the saga definition.  
  - `string description` – A description of the saga definition.  
  - `CancellationToken cancellationToken = default` – Optional cancellation token.

- **Returns**  
  `Task<SagaDefinition>` – The newly created and activated saga definition.

- **Throws**  
  - `ArgumentNullException` – If `service` is `null`.  
  - `ArgumentException` – If `name` is `null` or empty.  
  - `InvalidOperationException` – If a definition with the same name already exists and is active.

### `AddStepsAsync`

Adds one or more steps to an existing saga definition.

- **Parameters**  
  - `this ISagaDefinitionService service` – The service instance.  
  - `Guid definitionId` – The identifier of the saga definition.  
  - `IEnumerable<SagaStep> steps` – The steps to add.  
  - `CancellationToken cancellationToken = default` – Optional cancellation token.

- **Returns**  
  `Task<SagaDefinition>` – The updated saga definition with the new steps included.

- **Throws**  
  - `ArgumentNullException` – If `service` or `steps` is `null`.  
  - `KeyNotFoundException` – If no definition with the given `definitionId` exists.  
  - `InvalidOperationException` – If the definition is inactive or if any step conflicts with existing steps.

### `RemoveStepsAsync`

Removes specified steps from a saga definition.

- **Parameters**  
  - `this ISagaDefinitionService service` – The service instance.  
  - `Guid definitionId` – The identifier of the saga definition.  
  - `IEnumerable<Guid> stepIds` – The identifiers of the steps to remove.  
  - `CancellationToken cancellationToken = default` – Optional cancellation token.

- **Returns**  
  `Task<SagaDefinition>` – The updated saga definition after removal.

- **Throws**  
  - `ArgumentNullException` – If `service` or `stepIds` is `null`.  
  - `KeyNotFoundException` – If no definition with the given `definitionId` exists, or if any step ID is not found.  
  - `InvalidOperationException` – If the definition is inactive.

### `DefinitionExistsAsync`

Checks whether a saga definition with the specified name exists.

- **Parameters**  
  - `this ISagaDefinitionService service` – The service instance.  
  - `string name` – The name to check.  
  - `CancellationToken cancellationToken = default` – Optional cancellation token.

- **Returns**  
  `Task<bool>` – `true` if a definition with the given name exists; otherwise `false`.

- **Throws**  
  - `ArgumentNullException` – If `service` is `null`.  
  - `ArgumentException` – If `name` is `null` or empty.

### `GetOrCreateDefinitionAsync`

Retrieves an existing saga definition by name, or creates a new one if it does not exist.

- **Parameters**  
  - `this ISagaDefinitionService service` – The service instance.  
  - `string name` – The name of the definition.  
  - `string description` – The description to use if a new definition is created.  
  - `CancellationToken cancellationToken = default` – Optional cancellation token.

- **Returns**  
  `Task<SagaDefinition>` – The existing or newly created saga definition.

- **Throws**  
  - `ArgumentNullException` – If `service` is `null`.  
  - `ArgumentException` – If `name` is `null` or empty.

### `ValidateOrThrow`

Synchronously validates a saga definition and throws if validation fails.

- **Parameters**  
  - `SagaDefinition definition` – The saga definition to validate.

- **Returns**  
  `void`

- **Throws**  
  - `ArgumentNullException` – If `definition` is `null`.  
  - `ValidationException` – If the definition fails validation (e.g., missing required fields, circular step dependencies).

### `GetActiveDefinitionsAsync`

Retrieves all currently active saga definitions.

- **Parameters**  
  - `this ISagaDefinitionService service` – The service instance.  
  - `CancellationToken cancellationToken = default` – Optional cancellation token.

- **Returns**  
  `Task<List<SagaDefinition>>` – A list of active saga definitions.

- **Throws**  
  - `ArgumentNullException` – If `service` is `null`.

### `GetInactiveDefinitionsAsync`

Retrieves all currently inactive saga definitions.

- **Parameters**  
  - `this ISagaDefinitionService service` – The service instance.  
  - `CancellationToken cancellationToken = default` – Optional cancellation token.

- **Returns**  
  `Task<List<SagaDefinition>>` – A list of inactive saga definitions.

- **Throws**  
  - `ArgumentNullException` – If `service` is `null`.

### `GetAndValidateDefinitionAsync`

Retrieves a saga definition by identifier and validates it.

- **Parameters**  
  - `this ISagaDefinitionService service` – The service instance.  
  - `Guid definitionId` – The identifier of the definition.  
  - `CancellationToken cancellationToken = default` – Optional cancellation token.

- **Returns**  
  `Task<SagaDefinition>` – The validated saga definition.

- **Throws**  
  - `ArgumentNullException` – If `service` is `null`.  
  - `KeyNotFoundException` – If no definition with the given `definitionId` exists.  
  - `ValidationException` – If the definition fails validation.

### `CreateNewVersionAsync`

Creates a new version of an existing saga definition, preserving the original as inactive.

- **Parameters**  
  - `this ISagaDefinitionService service` – The service instance.  
  - `Guid definitionId` – The identifier of the definition to version.  
  - `CancellationToken cancellationToken = default` – Optional cancellation token.

- **Returns**  
  `Task<SagaDefinition>` – The newly created version (active by default).

- **Throws**  
  - `ArgumentNullException` – If `service` is `null`.  
  - `KeyNotFoundException` – If no definition with the given `definitionId` exists.  
  - `InvalidOperationException` – If the definition is already inactive.

## Usage

### Example 1: Creating and Activating a Definition with Steps

```csharp
using SagaOrchestrator;
using static SagaOrchestrator.SagaDefinitionServiceExtensions;

public async Task CreateOrderSagaAsync(ISagaDefinitionService sagaService)
{
    // Create and activate a new saga definition
    var definition = await sagaService.CreateAndActivateDefinitionAsync(
        "OrderProcessing",
        "Handles order placement and fulfillment");

    // Add steps to the definition
    var steps = new List<SagaStep>
    {
        new SagaStep { Name = "ValidateOrder", Order = 1 },
        new SagaStep { Name = "ReserveInventory", Order = 2 },
        new SagaStep { Name = "ProcessPayment", Order = 3 }
    };

    var updatedDefinition = await sagaService.AddStepsAsync(
        definition.Id,
        steps);

    // Validate the definition
    SagaDefinitionServiceExtensions.ValidateOrThrow(updatedDefinition);
}
```

### Example 2: Retrieving Active Definitions and Creating a New Version

```csharp
using SagaOrchestrator;
using static SagaOrchestrator.SagaDefinitionServiceExtensions;

public async Task VersionActiveDefinitionAsync(ISagaDefinitionService sagaService)
{
    // Get all active definitions
    var activeDefinitions = await sagaService.GetActiveDefinitionsAsync();

    // Find a specific definition by name
    var target = activeDefinitions.FirstOrDefault(d => d.Name == "OrderProcessing");
    if (target == null)
    {
        Console.WriteLine("Definition not found.");
        return;
    }

    // Create a new version of the definition
    var newVersion = await sagaService.CreateNewVersionAsync(target.Id);

    // The original definition is now inactive; the new version is active
    Console.WriteLine($"New version created with ID: {newVersion.Id}");
}
```

## Notes

- **Thread Safety**  
  The extension methods are static and do not maintain internal state. However, the underlying `ISagaDefinitionService` implementation may not be thread-safe. Concurrent calls to methods that modify definitions (e.g., `AddStepsAsync`, `RemoveStepsAsync`, `CreateNewVersionAsync`) should be serialized or use appropriate locking mechanisms to avoid race conditions.

- **Validation**  
  `ValidateOrThrow` is a synchronous method and performs no I/O. It can be called on any thread without concern for async context. All other methods are asynchronous and should be awaited.

- **Definition State**  
  Methods that modify a definition (add/remove steps, create new version) typically require the definition to be in an active state. Attempting to modify an inactive definition will result in an `InvalidOperationException`. Use `GetActiveDefinitionsAsync` or `GetAndValidateDefinitionAsync` to confirm the state before modification.

- **Duplicate Names**  
  `CreateAndActivateDefinitionAsync` will throw if a definition with the same name already exists and is active. Use `DefinitionExistsAsync` or `GetOrCreateDefinitionAsync` to handle this case gracefully.

- **Versioning**  
  `CreateNewVersionAsync` marks the original definition as inactive and creates a new active copy with the same name and steps. The new definition receives a new unique identifier. Steps are deep-copied; modifications to the new version do not affect the original.
