# InMemorySagaDefinitionRepository

An in-memory implementation of `ISagaDefinitionRepository` that stores `SagaDefinition` instances in a concurrent dictionary for testing and development scenarios where persistence is not required.

## API

### `public async Task<SagaDefinition?> GetByIdAsync(Guid id)`
Retrieves a saga definition by its unique identifier. Returns `null` if no matching definition exists. Throws `ArgumentNullException` if `id` is `Guid.Empty`.

### `public async Task<SagaDefinition?> GetByNameAsync(string name)`
Retrieves a saga definition by its name. Returns `null` if no matching definition exists. Throws `ArgumentNullException` if `name` is `null` or whitespace.

### `public async Task<SagaDefinition?> CreateAsync(SagaDefinition definition)`
Adds a new saga definition to the repository. Returns the created definition or `null` if the operation fails (e.g., duplicate name). Throws `ArgumentNullException` if `definition` is `null`.

### `public async Task<SagaDefinition?> UpdateAsync(SagaDefinition definition)`
Updates an existing saga definition. Returns the updated definition or `null` if no matching definition exists. Throws `ArgumentNullException` if `definition` is `null`.

### `public async Task<bool> DeleteAsync(Guid id)`
Removes a saga definition by its identifier. Returns `true` if the definition existed and was deleted, otherwise `false`. Throws `ArgumentNullException` if `id` is `Guid.Empty`.

### `public async Task<List<SagaDefinition>> GetAllAsync()`
Returns a list of all saga definitions stored in the repository. The list is a snapshot of the current state and may not reflect concurrent modifications.

### `public async Task<List<SagaDefinition>> GetActiveAsync()`
Returns a list of saga definitions marked as active. The list is a snapshot of the current state and may not reflect concurrent modifications.

### `public async Task<List<SagaDefinition>> SearchAsync(string query)`
Searches saga definitions by name or description using a case-insensitive substring match. Returns an empty list if no matches are found. Throws `ArgumentNullException` if `query` is `null`.

## Usage
