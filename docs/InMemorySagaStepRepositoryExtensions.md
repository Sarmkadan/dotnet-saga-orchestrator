# InMemorySagaStepRepositoryExtensions

Provides extension methods for querying and managing `SagaStep` instances stored in an in-memory repository. These methods facilitate common saga orchestration operations such as retrieving steps by status, checking completion, and managing retries or timeouts.

## API

### `GetBySagaIdAndStatusAsync`

Retrieves all saga steps associated with a given saga identifier that match the specified status.

- **Parameters**
  - `repository` (`IInMemorySagaStepRepository`): The in-memory repository instance.
  - `sagaId` (`Guid`): The unique identifier of the saga.
  - `status` (`SagaStepStatus`): The status to filter steps by.
- **Returns**
  - `Task<IReadOnlyList<SagaStep>>`: A read-only list of matching saga steps.
- **Throws**
  - `ArgumentNullException`: If `repository` is `null`.

### `GetNextPendingStepAsync`

Retrieves the next pending saga step for a given saga, ordered by ascending `Order`. Returns `null` if no pending steps exist.

- **Parameters**
  - `repository` (`IInMemorySagaStepRepository`): The in-memory repository instance.
  - `sagaId` (`Guid`): The unique identifier of the saga.
- **Returns**
  - `Task<SagaStep?>`: The next pending step, or `null` if none exists.
- **Throws**
  - `ArgumentNullException`: If `repository` is `null`.

### `GetRetryableFailedStepsAsync`

Retrieves all saga steps that have failed and are eligible for retry, based on the saga's retry policy.

- **Parameters**
  - `repository` (`IInMemorySagaStepRepository`): The in-memory repository instance.
  - `sagaId` (`Guid`): The unique identifier of the saga.
- **Returns**
  - `Task<IReadOnlyList<SagaStep>>`: A read-only list of retryable failed steps.
- **Throws**
  - `ArgumentNullException`: If `repository` is `null`.

### `GetTimedOutStepsAsync`

Retrieves all saga steps that have exceeded their timeout threshold and are considered timed out.

- **Parameters**
  - `repository` (`IInMemorySagaStepRepository`): The in-memory repository instance.
  - `sagaId` (`Guid`): The unique identifier of the saga.
- **Returns**
  - `Task<IReadOnlyList<SagaStep>>`: A read-only list of timed out steps.
- **Throws**
  - `ArgumentNullException`: If `repository` is `null`.

### `GetMaxOrderForSagaAsync`

Retrieves the highest `Order` value among all saga steps associated with the given saga identifier.

- **Parameters**
  - `repository` (`IInMemorySagaStepRepository`): The in-memory repository instance.
  - `sagaId` (`Guid`): The unique identifier of the saga.
- **Returns**
  - `Task<int>`: The maximum order value, or `0` if no steps exist.
- **Throws**
  - `ArgumentNullException`: If `repository` is `null`.

### `AreAllStepsCompletedAsync`

Determines whether all saga steps associated with the given saga identifier have reached a terminal status (`Completed`, `Failed`, or `Skipped`).

- **Parameters**
  - `repository` (`IInMemorySagaStepRepository`): The in-memory repository instance.
  - `sagaId` (`Guid`): The unique identifier of the saga.
- **Returns**
  - `Task<bool>`: `true` if all steps are completed; otherwise, `false`.
- **Throws**
  - `ArgumentNullException`: If `repository` is `null`.

### `GetActiveStepsAsync`

Retrieves all saga steps associated with the given saga identifier that are currently in progress or pending.

- **Parameters**
  - `repository` (`IInMemorySagaStepRepository`): The in-memory repository instance.
  - `sagaId` (`Guid`): The unique identifier of the saga.
- **Returns**
  - `Task<IReadOnlyList<SagaStep>>`: A read-only list of active steps.
- **Throws**
  - `ArgumentNullException`: If `repository` is `null`.

## Usage

### Example: Processing pending steps in a saga
