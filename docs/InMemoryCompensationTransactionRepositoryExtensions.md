# InMemoryCompensationTransactionRepositoryExtensions

Extension methods for `ICompensationTransactionRepository` that provide in-memory implementations of common compensation transaction queries. These methods are designed to simplify querying compensation transactions by saga ID, status, or activity state without requiring external storage operations.

## API

### `GetFirstBySagaIdAndStatusAsync`
Retrieves the first compensation transaction matching the specified saga ID and status. Returns `null` if no matching transaction exists.

- **Parameters**
  - `repository`: The `ICompensationTransactionRepository` instance.
  - `sagaId`: The saga identifier to match.
  - `status`: The compensation transaction status to filter by.
  - `cancellationToken`: Optional cancellation token.
- **Return value**: A `Task<CompensationTransaction?>` representing the first matching transaction or `null`.
- **Exceptions**: Throws `ArgumentNullException` if `repository` is `null`.

### `GetBySagaIdAndStatusAsync`
Retrieves all compensation transactions matching the specified saga ID and status.

- **Parameters**
  - `repository`: The `ICompensationTransactionRepository` instance.
  - `sagaId`: The saga identifier to match.
  - `status`: The compensation transaction status to filter by.
  - `cancellationToken`: Optional cancellation token.
- **Return value**: A `Task<List<CompensationTransaction>>` containing all matching transactions (empty list if none found).
- **Exceptions**: Throws `ArgumentNullException` if `repository` is `null`.

### `GetByStatusAsync`
Retrieves all compensation transactions with the specified status.

- **Parameters**
  - `repository`: The `ICompensationTransactionRepository` instance.
  - `status`: The compensation transaction status to filter by.
  - `cancellationToken`: Optional cancellation token.
- **Return value**: A `Task<List<CompensationTransaction>>` containing all transactions with the given status (empty list if none found).
- **Exceptions**: Throws `ArgumentNullException` if `repository` is `null`.

### `CountByStatusAsync`
Counts the number of compensation transactions with the specified status.

- **Parameters**
  - `repository`: The `ICompensationTransactionRepository` instance.
  - `status`: The compensation transaction status to filter by.
  - `cancellationToken`: Optional cancellation token.
- **Return value**: A `Task<int>` representing the count of matching transactions.
- **Exceptions**: Throws `ArgumentNullException` if `repository` is `null`.

### `GetTerminalTransactionsAsync`
Retrieves all compensation transactions that have reached a terminal state (e.g., completed or failed).

- **Parameters**
  - `repository`: The `ICompensationTransactionRepository` instance.
  - `cancellationToken`: Optional cancellation token.
- **Return value**: A `Task<List<CompensationTransaction>>` containing all terminal transactions (empty list if none found).
- **Exceptions**: Throws `ArgumentNullException` if `repository` is `null`.

### `GetActiveTransactionsAsync`
Retrieves all compensation transactions that are currently active (not in a terminal state).

- **Parameters**
  - `repository`: The `ICompensationTransactionRepository` instance.
  - `cancellationToken`: Optional cancellation token.
- **Return value**: A `Task<List<CompensationTransaction>>` containing all active transactions (empty list if none found).
- **Exceptions**: Throws `ArgumentNullException` if `repository` is `null`.

## Usage

### Example 1: Fetching all compensation transactions for a saga in a specific status
```csharp
var repository = new InMemoryCompensationTransactionRepository();
var sagaId = Guid.NewGuid();
var status = CompensationStatus.Pending;

// Add some test transactions
await repository.AddAsync(new CompensationTransaction(sagaId, status, "Step1"));
await repository.AddAsync(new CompensationTransaction(sagaId, CompensationStatus.Completed, "Step2"));

var transactions = await InMemoryCompensationTransactionRepositoryExtensions
    .GetBySagaIdAndStatusAsync(repository, sagaId, status);
```

### Example 2: Counting terminal transactions
```csharp
var repository = new InMemoryCompensationTransactionRepository();
var terminalCount = await InMemoryCompensationTransactionRepositoryExtensions
    .CountByStatusAsync(repository, CompensationStatus.Completed);

Console.WriteLine($"Terminal transactions: {terminalCount}");
```

## Notes

- All methods operate on an in-memory collection and are **not thread-safe** by default. If concurrent access is required, external synchronization (e.g., `lock`) must be applied around repository operations.
- The in-memory implementation assumes that the underlying repository (`ICompensationTransactionRepository`) is also in-memory. Using these extensions with a persistent repository may yield unexpected results.
- Cancellation tokens are passed through but are not used to interrupt in-memory operations; they only propagate cancellation requests to underlying async operations if supported.
- Methods returning lists (`GetBySagaIdAndStatusAsync`, `GetByStatusAsync`, etc.) return new collections and do not expose the internal repository state, ensuring immutability of results.
