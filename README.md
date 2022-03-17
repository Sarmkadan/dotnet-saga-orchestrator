// existing content ...

## InMemoryCompensationTransactionRepositoryExtensions

The `InMemoryCompensationTransactionRepositoryExtensions` class provides a set of convenience extension methods for the `ICompensationTransactionRepository`, making it easier to work with compensation transactions in memory. These extensions allow you to retrieve compensation transactions by saga ID and status, get transactions by status, count transactions by status, and filter transactions by terminal or active states.

### Usage Example

```csharp
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

// Create an instance of ICompensationTransactionRepository
var repository = new InMemoryCompensationTransactionRepository();

// Get the first compensation transaction by saga ID and status
var transaction = await repository.GetFirstBySagaIdAndStatusAsync(
    "Order-123",
    CompensationStatus.Pending);

// Get all compensation transactions by saga ID and status
var transactions = await repository.GetBySagaIdAndStatusAsync(
    "Order-123",
    CompensationStatus.Pending);

// Get all compensation transactions by status
var pendingTransactions = await repository.GetByStatusAsync(
    CompensationStatus.Pending);

// Count compensation transactions by status
var pendingCount = await repository.CountByStatusAsync(
    CompensationStatus.Pending);

// Get terminal compensation transactions
var terminalTransactions = await repository.GetTerminalTransactionsAsync();

// Get active compensation transactions
var activeTransactions = await repository.GetActiveTransactionsAsync();
```
