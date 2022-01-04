# CompensationServiceExtensions

Provides extension methods for managing and executing compensation transactions within the Saga Orchestrator pattern. These methods facilitate the tracking, querying, and execution of compensating actions to roll back distributed transactions when failures occur.

## API

### `ExecuteAllCompensationsAsync`

Executes all pending compensation transactions asynchronously. This method retrieves all transactions with a pending status and attempts to execute their compensation logic in sequence.

**Parameters**
- `serviceProvider` (IServiceProvider): The dependency injection container used to resolve compensation handlers.
- `cancellationToken` (CancellationToken): A token to monitor for cancellation requests.

**Return Value**
- A `Task<List<CompensationTransaction>>` that resolves to the list of compensation transactions that were executed, including any that failed during execution.

**Exceptions**
- Throws `ArgumentNullException` if `serviceProvider` is null.
- Throws `InvalidOperationException` if a compensation handler cannot be resolved for a transaction.

---

### `GetCompensationsByStatusAsync`

Retrieves all compensation transactions filtered by a specific status asynchronously.

**Parameters**
- `serviceProvider` (IServiceProvider): The dependency injection container used to resolve the compensation repository.
- `status` (CompensationStatus): The status of transactions to retrieve.
- `cancellationToken` (CancellationToken): A token to monitor for cancellation requests.

**Return Value**
- A `Task<List<CompensationTransaction>>` that resolves to the list of transactions matching the specified status.

**Exceptions**
- Throws `ArgumentNullException` if `serviceProvider` is null.
- Throws `ArgumentOutOfRangeException` if an invalid `CompensationStatus` value is provided.

---

### `HasPendingCompensationsAsync`

Determines whether any compensation transactions are currently in a pending state asynchronously.

**Parameters**
- `serviceProvider` (IServiceProvider): The dependency injection container used to resolve the compensation repository.
- `cancellationToken` (CancellationToken): A token to monitor for cancellation requests.

**Return Value**
- A `Task<bool>` that resolves to `true` if pending transactions exist; otherwise, `false`.

**Exceptions**
- Throws `ArgumentNullException` if `serviceProvider` is null.

---
### `GetCompensationCountAsync`

Retrieves the total number of compensation transactions matching a specified status asynchronously.

**Parameters**
- `serviceProvider` (IServiceProvider): The dependency injection container used to resolve the compensation repository.
- `status` (CompensationStatus): The status of transactions to count.
- `cancellationToken` (CancellationToken): A token to monitor for cancellation requests.

**Return Value**
- A `Task<int>` that resolves to the count of transactions matching the specified status.

**Exceptions**
- Throws `ArgumentNullException` if `serviceProvider` is null.
- Throws `ArgumentOutOfRangeException` if an invalid `CompensationStatus` value is provided.

## Usage

### Example 1: Executing All Pending Compensations
