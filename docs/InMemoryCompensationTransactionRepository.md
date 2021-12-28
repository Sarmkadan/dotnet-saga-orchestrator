# InMemoryCompensationTransactionRepository

An in-memory implementation of `ICompensationTransactionRepository` that stores `CompensationTransaction` objects in a concurrent dictionary. Designed for testing or lightweight scenarios where persistence is not required. All operations are thread-safe and support asynchronous execution.

## API

### `public async Task<CompensationTransaction?> GetByIdAsync(Guid id)`

Retrieves a compensation transaction by its unique identifier.

- **Parameters**
  - `id`: The unique identifier of the compensation transaction to retrieve.
- **Return value**
  - A `CompensationTransaction` instance if found; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `id` is `Guid.Empty`.

### `public async Task<CompensationTransaction?> CreateAsync(CompensationTransaction transaction)`

Adds a new compensation transaction to the repository.

- **Parameters**
  - `transaction`: The compensation transaction to create. Must not be `null`.
- **Return value**
  - The created `CompensationTransaction` instance, including any auto-generated identifiers.
- **Exceptions**
  - Throws `ArgumentNullException` if `transaction` is `null`.
  - Throws `InvalidOperationException` if a transaction with the same `Id` already exists.

### `public async Task<CompensationTransaction?> UpdateAsync(CompensationTransaction transaction)`

Updates an existing compensation transaction in the repository.

- **Parameters**
  - `transaction`: The updated compensation transaction. Must not be `null`.
- **Return value**
  - The updated `CompensationTransaction` instance if the update was successful; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `transaction` is `null`.
  - Throws `KeyNotFoundException` if no transaction with the specified `Id` exists.

### `public async Task<bool> DeleteAsync(Guid id)`

Removes a compensation transaction from the repository by its identifier.

- **Parameters**
  - `id`: The unique identifier of the compensation transaction to delete.
- **Return value**
  - `true` if the transaction was found and deleted; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `id` is `Guid.Empty`.

### `public async Task<List<CompensationTransaction>> GetBySagaIdAsync(Guid sagaId)`

Retrieves all compensation transactions associated with a specific saga identifier.

- **Parameters**
  - `sagaId`: The unique identifier of the saga whose transactions are to be retrieved.
- **Return value**
  - A list of `CompensationTransaction` instances matching the `sagaId`. Empty list if none found.
- **Exceptions**
  - Throws `ArgumentNullException` if `sagaId` is `Guid.Empty`.

### `public async Task<List<CompensationTransaction>> GetAllAsync()`

Retrieves all compensation transactions stored in the repository.

- **Return value**
  - A list of all `CompensationTransaction` instances. Empty list if none exist.
- **Exceptions**
  - None.

### `public async Task<List<CompensationTransaction>> GetByStatusAsync(CompensationStatus status)`

Retrieves all compensation transactions matching a specific status.

- **Parameters**
  - `status`: The `CompensationStatus` to filter by.
- **Return value**
  - A list of `CompensationTransaction` instances with the specified `status`. Empty list if none found.
- **Exceptions**
  - None.

## Usage

### Example 1: Creating and retrieving a compensation transaction
