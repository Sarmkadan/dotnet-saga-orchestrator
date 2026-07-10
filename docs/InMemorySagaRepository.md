# InMemorySagaRepository

`InMemorySagaRepository` is an in-memory implementation of a saga persistence store designed for development, testing, and lightweight scenarios where durable storage is not required. It stores saga instances in a thread-safe concurrent dictionary, keyed by both a unique saga identifier and an optional correlation identifier. All operations are asynchronous and return cloned copies of saga data to prevent unintended mutation of the internal store.

## API

### GetByIdAsync

```csharp
public async Task<Saga?> GetByIdAsync(Guid sagaId)
```

Retrieves a saga by its unique identifier.

**Parameters:**
- `sagaId` (`Guid`): The unique identifier of the saga to retrieve.

**Return Value:**
- A deep-cloned `Saga` instance if found; `null` otherwise.

**Exceptions:**
- `ArgumentNullException` is not thrown for `Guid` parameters, as they are value types and cannot be null.

---

### GetByCorrelationIdAsync

```csharp
public async Task<Saga?> GetByCorrelationIdAsync(string correlationId)
```

Retrieves a saga by its correlation identifier, typically used to locate a saga associated with a specific business entity or workflow instance.

**Parameters:**
- `correlationId` (`string`): The correlation identifier assigned during saga creation.

**Return Value:**
- A deep-cloned `Saga` instance if a saga with the given correlation identifier exists; `null` if no match is found or if the correlation identifier is null or empty.

**Exceptions:**
- Does not throw on null or empty input; returns `null` silently.

---

### CreateAsync

```csharp
public async Task<Saga?> CreateAsync(Saga saga)
```

Persists a new saga instance in the store. The saga must not already exist in the repository, and its correlation identifier must be unique among all stored sagas.

**Parameters:**
- `saga` (`Saga`): The saga instance to persist. Must have a non-empty `Id` and, if a `CorrelationId` is provided, it must not conflict with an existing entry.

**Return Value:**
- A deep-cloned `Saga` instance representing the newly stored saga; `null` if the saga already exists or if a correlation identifier conflict is detected.

**Exceptions:**
- `ArgumentNullException`: Thrown when `saga` is `null`.

---

### UpdateAsync

```csharp
public async Task<Saga?> UpdateAsync(Saga saga)
```

Updates an existing saga in the store. The saga must already be present in the repository.

**Parameters:**
- `saga` (`Saga`): The saga instance containing updated data. The `Id` must match an existing saga.

**Return Value:**
- A deep-cloned `Saga` instance reflecting the updated state; `null` if no saga with the given `Id` exists.

**Exceptions:**
- `ArgumentNullException`: Thrown when `saga` is `null`.

---

### DeleteAsync

```csharp
public async Task<bool> DeleteAsync(Guid sagaId)
```

Removes a saga from the store by its unique identifier.

**Parameters:**
- `sagaId` (`Guid`): The unique identifier of the saga to delete.

**Return Value:**
- `true` if the saga was found and successfully removed; `false` if no saga with the given identifier existed.

**Exceptions:**
- None.

---

### GetAllAsync

```csharp
public async Task<List<Saga>> GetAllAsync()
```

Returns all sagas currently stored in the repository.

**Parameters:**
- None.

**Return Value:**
- A `List<Saga>` containing deep-cloned copies of every stored saga. The list is empty if no sagas exist.

**Exceptions:**
- None.

---

### GetByStatusAsync

```csharp
public async Task<List<Saga>> GetByStatusAsync(SagaStatus status)
```

Retrieves all sagas whose current status matches the specified value.

**Parameters:**
- `status` (`SagaStatus`): The status to filter by.

**Return Value:**
- A `List<Saga>` containing deep-cloned copies of all sagas with the given status. The list is empty if no matches are found.

**Exceptions:**
- None.

---

### SearchAsync

```csharp
public async Task<List<Saga>> SearchAsync(string searchTerm)
```

Performs a basic text search across saga data and returns matching instances. The search typically examines saga type names, correlation identifiers, and other string fields.

**Parameters:**
- `searchTerm` (`string`): The term to search for. A null or empty string returns all sagas.

**Return Value:**
- A `List<Saga>` containing deep-cloned copies of sagas whose data contains the search term (case-insensitive). An empty list if no matches are found.

**Exceptions:**
- None.

## Usage

### Example 1: Creating and Retrieving a Saga by Correlation Identifier

```csharp
var repository = new InMemorySagaRepository();

var newSaga = new Saga
{
    Id = Guid.NewGuid(),
    SagaType = "OrderFulfillment",
    CorrelationId = "ORDER-12345",
    Status = SagaStatus.Pending,
    Data = new Dictionary<string, object> { ["OrderTotal"] = 149.99m }
};

Saga? created = await repository.CreateAsync(newSaga);
// created is a deep clone of newSaga

Saga? retrieved = await repository.GetByCorrelationIdAsync("ORDER-12345");
// retrieved matches the created saga; retrieved != created (different object references)
```

### Example 2: Updating Status and Searching

```csharp
var repository = new InMemorySagaRepository();

// Assume a saga with Id = sagaId already exists and is in Pending status
Saga? existing = await repository.GetByIdAsync(sagaId);
if (existing is not null)
{
    existing.Status = SagaStatus.Completed;
    existing.CompletedAt = DateTime.UtcNow;
    await repository.UpdateAsync(existing);
}

// Retrieve all completed sagas
List<Saga> completedSagas = await repository.GetByStatusAsync(SagaStatus.Completed);

// Search for sagas related to a specific order
List<Saga> orderSagas = await repository.SearchAsync("ORDER-12345");
```

## Notes

- **Thread Safety:** The underlying concurrent dictionary ensures that individual operations are atomic and safe for concurrent access. However, compound operations (e.g., check-then-update patterns like retrieving a saga, modifying it, and calling `UpdateAsync`) are not atomic as a unit. Callers must implement their own synchronization if consistency across multiple operations is required.
- **Deep Cloning:** All read operations return deep-cloned copies of stored saga instances. Modifications to returned objects do not affect the internal store unless explicitly persisted via `UpdateAsync`. This prevents accidental state corruption but incurs a performance cost proportional to saga data size.
- **Correlation Identifier Uniqueness:** `CreateAsync` enforces uniqueness of correlation identifiers. If a saga with the same correlation identifier already exists, the creation returns `null` rather than overwriting or throwing. Callers should handle this case explicitly when correlation identifiers are expected to be unique.
- **Null and Empty Handling:** Methods accepting reference-type parameters throw `ArgumentNullException` only where documented (`CreateAsync` and `UpdateAsync`). Search and retrieval methods treat null or empty inputs as non-matching conditions and return empty results or `null` without throwing.
- **Data Persistence:** As an in-memory implementation, all data is lost when the application process terminates. This repository is not suitable for production workloads requiring durability. It is intended for unit testing, integration testing, and rapid prototyping.
- **Status Enumeration:** `GetByStatusAsync` performs an exact match on the `SagaStatus` enum value. Sagas with undefined or custom status values not represented in the enum will not be returned unless the enum is extended to include them.
