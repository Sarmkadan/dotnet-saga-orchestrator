# CompensationServiceTests

Unit test suite for the `CompensationService` class, verifying correct behavior across all public methods including constructor validation, compensation lifecycle transitions, transaction retrieval, and status management. The tests cover both success paths and error conditions, ensuring the service enforces its contracts for null arguments, invalid saga states, and missing entities.

## API

### `Constructor_WithNullRepository_ThrowsArgumentNullException`

Validates that the `CompensationService` constructor throws an `ArgumentNullException` when a null repository dependency is supplied. This test enforces the defensive programming guard at instantiation time.

- **Parameters:** None (parameterless test method)
- **Returns:** void
- **Throws:** Asserts that `ArgumentNullException` is thrown by the constructor under test

### `BeginCompensationAsync_WithNullSaga_ThrowsArgumentNullException`

Ensures that `BeginCompensationAsync` throws an `ArgumentNullException` when invoked with a null saga reference. Protects against null propagation into the compensation workflow.

- **Parameters:** None
- **Returns:** void
- **Throws:** Asserts that `ArgumentNullException` is thrown

### `BeginCompensationAsync_WithRunningStatus_ThrowsSagaException`

Verifies that attempting to begin compensation on a saga that is still in a running state results in a `SagaException`. Compensation should only be initiated from terminal or failed states.

- **Parameters:** None
- **Returns:** void
- **Throws:** Asserts that `SagaException` is thrown

### `BeginCompensationAsync_WithFailedStatus_TransitionsToCompensating`

Confirms that a saga in a failed status correctly transitions to the compensating state when `BeginCompensationAsync` is called. This is the primary entry point for the compensation workflow.

- **Parameters:** None
- **Returns:** void
- **Throws:** No exception expected

### `BeginCompensationAsync_CreatesCompensationTransactionsForCompleted`

Validates that compensation transactions are generated only for steps that have completed successfully. Failed or pending steps should not produce compensation entries.

- **Parameters:** None
- **Returns:** void
- **Throws:** No exception expected

### `BeginCompensationAsync_IgnoresPendingSteps`

Ensures that steps still in a pending state are excluded from compensation transaction creation. Only steps that have executed and require reversal are considered.

- **Parameters:** None
- **Returns:** void
- **Throws:** No exception expected

### `ExecuteNextCompensationAsync_WithNonexistentSaga_ThrowsSagaNotFoundException`

Verifies that requesting the next compensation step for a saga ID that does not exist in the repository throws a `SagaNotFoundException`.

- **Parameters:** None
- **Returns:** void
- **Throws:** Asserts that `SagaNotFoundException` is thrown

### `ExecuteNextCompensationAsync_WithNonCompensatingStatus_ThrowsSagaException`

Ensures that calling `ExecuteNextCompensationAsync` on a saga that is not in the compensating state throws a `SagaException`. The method requires the saga to be actively undergoing compensation.

- **Parameters:** None
- **Returns:** void
- **Throws:** Asserts that `SagaException` is thrown

### `ExecuteNextCompensationAsync_WithNoCompensations_ReturnsNull`

Tests that when a saga has no compensation transactions defined, the method returns null rather than throwing, signaling completion of the compensation process.

- **Parameters:** None
- **Returns:** Asserts that the result is null
- **Throws:** No exception expected

### `ExecuteNextCompensationAsync_WithPendingCompensation_ReturnsThat`

Confirms that the next pending (unexecuted) compensation transaction is returned when one exists. The method respects execution order of the compensation queue.

- **Parameters:** None
- **Returns:** Asserts that a non-null compensation transaction is returned
- **Throws:** No exception expected

### `ExecuteNextCompensationAsync_SkipsPreviouslyExecuted`

Verifies that compensation transactions already marked as executed are skipped when determining the next compensation to process. Only unexecuted compensations are candidates.

- **Parameters:** None
- **Returns:** void
- **Throws:** No exception expected

### `CompleteCompensationAsync_WithValidTransaction_MarksSagaCompensated`

Validates that completing a compensation transaction with valid data results in the saga being marked as fully compensated, representing the terminal state of the compensation workflow.

- **Parameters:** None
- **Returns:** void
- **Throws:** No exception expected

## Usage

### Example 1: Testing the full compensation lifecycle

```csharp
[Test]
public async Task FullCompensationLifecycle_FromFailureToCompensated()
{
    // Arrange
    var saga = new Saga { Id = Guid.NewGuid(), Status = SagaStatus.Failed };
    var completedStep = new SagaStep { Status = StepStatus.Completed };
    saga.Steps.Add(completedStep);
    
    mockRepository.Setup(r => r.GetSagaAsync(saga.Id)).ReturnsAsync(saga);
    var service = new CompensationService(mockRepository.Object);

    // Act - Begin compensation
    await service.BeginCompensationAsync(saga);
    
    // Assert - Status transitioned
    Assert.That(saga.Status, Is.EqualTo(SagaStatus.Compensating));
    Assert.That(saga.CompensationTransactions, Has.Count.EqualTo(1));

    // Act - Execute next compensation
    var nextCompensation = await service.ExecuteNextCompensationAsync(saga.Id);
    
    // Assert - Pending compensation returned
    Assert.That(nextCompensation, Is.Not.Null);
    Assert.That(nextCompensation.Status, Is.EqualTo(CompensationStatus.Pending));

    // Act - Complete compensation
    await service.CompleteCompensationAsync(nextCompensation);
    
    // Assert - Saga marked compensated
    Assert.That(saga.Status, Is.EqualTo(SagaStatus.Compensated));
}
```

### Example 2: Verifying guard clauses in sequence

```csharp
[Test]
public void CompensationService_GuardClauses_ThrowAppropriateExceptions()
{
    // Constructor null guard
    Assert.Throws<ArgumentNullException>(() => new CompensationService(null));

    var service = new CompensationService(mockRepository.Object);

    // BeginCompensationAsync null guard
    Assert.ThrowsAsync<ArgumentNullException>(
        async () => await service.BeginCompensationAsync(null));

    // BeginCompensationAsync invalid status guard
    var runningSaga = new Saga { Status = SagaStatus.Running };
    Assert.ThrowsAsync<SagaException>(
        async () => await service.BeginCompensationAsync(runningSaga));

    // ExecuteNextCompensationAsync nonexistent saga guard
    mockRepository.Setup(r => r.GetSagaAsync(It.IsAny<Guid>())).ReturnsAsync((Saga)null);
    Assert.ThrowsAsync<SagaNotFoundException>(
        async () => await service.ExecuteNextCompensationAsync(Guid.NewGuid()));
}
```

## Notes

- **Status machine enforcement:** The tests collectively verify a strict state machine where compensation can only begin from a failed state, execution of compensation steps requires the compensating state, and completion transitions to compensated. Any deviation from this sequence is rejected with `SagaException`.
- **Null safety:** Both the constructor and `BeginCompensationAsync` enforce null checks. The constructor guard prevents the service from being instantiated in an invalid state, while the method guard protects against null arguments at runtime.
- **Compensation transaction filtering:** Tests confirm that only completed steps generate compensation transactions. Pending steps are explicitly ignored, and previously executed compensations are skipped during retrieval, preventing duplicate reversal operations.
- **Empty compensation handling:** When no compensations exist, `ExecuteNextCompensationAsync` returns null rather than throwing, allowing callers to distinguish between "no work to do" and error conditions without exception overhead.
- **Thread safety:** These tests do not explicitly cover concurrent access scenarios. The service's thread safety characteristics depend on the underlying repository implementation and its transactional guarantees.
