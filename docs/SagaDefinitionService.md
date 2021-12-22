# SagaDefinitionService

Central service for managing saga definitions. It provides methods to create, retrieve, modify, activate/deactivate, clone, and validate saga definitions at runtime. The service ensures definitions are consistent and tracks validation errors without throwing exceptions for invalid operations.

## API

### `public SagaDefinitionService`

Constructor. Initializes a new instance of the service, typically with injected dependencies for persistence and validation.

### `public async Task<SagaDefinition> CreateDefinitionAsync`

Creates a new saga definition with the specified name and steps.

- **Parameters**
  - `name`: Unique name of the saga definition.
  - `steps`: Ordered list of step definitions to include in the saga.
- **Return value**
  - A `Task<SagaDefinition>` resolving to the newly created saga definition.
- **Throws**
  - `ArgumentException` if `name` is null or whitespace.
  - `ArgumentNullException` if `steps` is null.
  - `InvalidOperationException` if a definition with the same name already exists.

### `public async Task<SagaDefinition> AddStepAsync`

Adds a step to an existing saga definition.

- **Parameters**
  - `definitionId`: Identifier of the saga definition to modify.
  - `step`: Step definition to append to the saga.
- **Return value**
  - A `Task<SagaDefinition>` resolving to the updated saga definition.
- **Throws**
  - `ArgumentException` if `definitionId` is invalid.
  - `ArgumentNullException` if `step` is null.
  - `InvalidOperationException` if the definition is active or in an invalid state.

### `public async Task<SagaDefinition> RemoveStepAsync`

Removes a step from an existing saga definition by its index.

- **Parameters**
  - `definitionId`: Identifier of the saga definition to modify.
  - `stepIndex`: Zero-based index of the step to remove.
- **Return value**
  - A `Task<SagaDefinition>` resolving to the updated saga definition.
- **Throws**
  - `ArgumentException` if `definitionId` is invalid.
  - `ArgumentOutOfRangeException` if `stepIndex` is out of bounds.
  - `InvalidOperationException` if the definition is active or in an invalid state.

### `public ValidationResult ValidateDefinition`

Validates the current saga definition in memory.

- **Return value**
  - A `ValidationResult` containing any validation errors.
- **Remarks**
  - Does not modify the definition or throw exceptions.
  - Errors are also accessible via the `Errors` property.

### `public async Task<SagaDefinition> GetDefinitionAsync`

Retrieves a saga definition by its identifier.

- **Parameters**
  - `definitionId`: Identifier of the saga definition to retrieve.
- **Return value**
  - A `Task<SagaDefinition>` resolving to the requested saga definition, or `null` if not found.
- **Throws**
  - `ArgumentException` if `definitionId` is invalid.

### `public async Task<SagaDefinition?> GetDefinitionByNameAsync`

Retrieves a saga definition by its unique name.

- **Parameters**
  - `name`: Unique name of the saga definition to retrieve.
- **Return value**
  - A `Task<SagaDefinition?>` resolving to the requested saga definition, or `null` if not found.
- **Throws**
  - `ArgumentException` if `name` is null or whitespace.

### `public async Task<List<SagaDefinition>> ListDefinitionsAsync`

Lists all saga definitions.

- **Return value**
  - A `Task<List<SagaDefinition>>` resolving to a list of all saga definitions.
- **Remarks**
  - The list is not guaranteed to be ordered.

### `public async Task<SagaDefinition> ActivateDefinitionAsync`

Activates a saga definition, making it eligible for execution by the orchestrator.

- **Parameters**
  - `definitionId`: Identifier of the saga definition to activate.
- **Return value**
  - A `Task<SagaDefinition>` resolving to the activated saga definition.
- **Throws**
  - `ArgumentException` if `definitionId` is invalid.
  - `InvalidOperationException` if the definition is already active or invalid.

### `public async Task<SagaDefinition> DeactivateDefinitionAsync`

Deactivates a saga definition, preventing it from being executed by the orchestrator.

- **Parameters**
  - `definitionId`: Identifier of the saga definition to deactivate.
- **Return value**
  - A `Task<SagaDefinition>` resolving to the deactivated saga definition.
- **Throws**
  - `ArgumentException` if `definitionId` is invalid.
  - `InvalidOperationException` if the definition is already inactive or invalid.

### `public async Task<SagaDefinition> CloneDefinitionAsync`

Creates a deep copy of an existing saga definition with a new unique name.

- **Parameters**
  - `definitionId`: Identifier of the saga definition to clone.
  - `newName`: Unique name for the cloned definition.
- **Return value**
  - A `Task<SagaDefinition>` resolving to the newly cloned saga definition.
- **Throws**
  - `ArgumentException` if `definitionId` is invalid or `newName` is null or whitespace.
  - `InvalidOperationException` if a definition with `newName` already exists.

### `public bool IsValid`

Indicates whether the current saga definition is valid.

- **Return value**
  - `true` if the definition has no validation errors; otherwise, `false`.
- **Remarks**
  - Equivalent to `ValidateDefinition.Errors.Length == 0`.

### `public string[] Errors`

Gets the current validation errors for the saga definition.

- **Return value**
  - An array of error messages, or an empty array if no errors exist.

## Usage

### Creating and activating a saga definition
