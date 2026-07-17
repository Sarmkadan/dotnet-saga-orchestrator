# InMemorySagaRepositoryExtensions

Extension methods for querying and managing `Saga` entities in an in-memory repository. These methods provide convenience wrappers around common query patterns for saga orchestration scenarios, such as retrieving sagas by correlation ID, status, or definition, as well as counting, searching, and filtering based on execution state or timeouts.

## API

### `GetByCorrelationIdAsync`

Retrieves a single `Saga` by its unique correlation ID.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
  - `correlationId` (`string`): The unique identifier to search for.
- **Returns**: A `Task<Saga?>` representing the saga with the given correlation ID, or `null` if not found.
- **Throws**: `ArgumentNullException` if `correlationId` is `null`.

---

### `GetByStatusAsync`

Retrieves all sagas in a specific status.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
  - `status` (`SagaStatus`): The status to filter by.
- **Returns**: A `Task<IReadOnlyList<Saga>>` of sagas matching the status.
- **Throws**: `ArgumentNullException` if `status` is `null`.

---

### `SearchByDefinitionIdAsync`

Searches for sagas by their definition ID.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
  - `definitionId` (`string`): The definition ID to search for.
- **Returns**: A `Task<IReadOnlyList<Saga>>` of sagas matching the definition ID.
- **Throws**: `ArgumentNullException` if `definitionId` is `null`.

---
### `SearchByNameAsync`

Searches for sagas by their name.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
  - `name` (`string`): The name to search for.
- **Returns**: A `Task<IReadOnlyList<Saga>>` of sagas matching the name.
- **Throws**: `ArgumentNullException` if `name` is `null`.

---
### `GetTimedOutSagasAsync`

Retrieves all sagas that have timed out based on their timeout policy.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
- **Returns**: A `Task<IReadOnlyList<Saga>>` of sagas that have exceeded their timeout thresholds.

---
### `GetRetryableSagasAsync`

Retrieves all sagas that are eligible for retry based on retry policies and retry count.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
- **Returns**: A `Task<IReadOnlyList<Saga>>` of sagas that can be retried.

---
### `GetFailedSagasAfterAsync`

Retrieves sagas that failed after a specified timestamp.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
  - `timestamp` (`DateTime`): The cutoff timestamp for failure events.
- **Returns**: A `Task<IReadOnlyList<Saga>>` of sagas that failed after the given timestamp.

---
### `CountByStatusAsync`

Counts the number of sagas in a specific status.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
  - `status` (`SagaStatus`): The status to count.
- **Returns**: A `Task<int>` representing the count of sagas in the given status.
- **Throws**: `ArgumentNullException` if `status` is `null`.

---
### `CountAllAsync`

Counts the total number of sagas in the repository.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
- **Returns**: A `Task<int>` representing the total number of sagas.

---
### `ExistsByCorrelationIdAsync`

Checks whether a saga with the given correlation ID exists.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
  - `correlationId` (`string`): The correlation ID to check.
- **Returns**: A `Task<bool>` indicating whether a saga with the given correlation ID exists.
- **Throws**: `ArgumentNullException` if `correlationId` is `null`.

---
### `GetCompletedSagasAsync`

Retrieves all sagas that have completed successfully.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
- **Returns**: A `Task<IReadOnlyList<Saga>>` of completed sagas.

---
### `GetFailedSagasAsync`

Retrieves all sagas that have failed.

- **Parameters**:
  - `repository` (`IInMemorySagaRepository`): The in-memory saga repository.
- **Returns**: A `Task<IReadOnlyList<Saga>>` of failed sagas.

## Usage

### Example 1: Retrieve and process timed-out sagas
