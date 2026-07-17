# InMemorySagaDefinitionRepositoryExtensions

Provides a set of static extension methods for querying an in-memory saga definition repository. These methods encapsulate common retrieval and inspection operations, returning `Task`-based results for asynchronous consumption. They are designed to work with any implementation that exposes the underlying collection of `SagaDefinition` objects.

## API

### `GetByNameAsync`

```csharp
public static async Task<SagaDefinition?> GetByNameAsync(this IInMemorySagaDefinitionRepository repository, string name)
```

Retrieves a single saga definition whose `Name` property exactly matches the specified `name`.

- **Parameters**  
  - `repository` – The in-memory repository instance.  
  - `name` – The exact name of the saga definition to locate.

- **Returns**  
  A `Task<SagaDefinition?>` that resolves to the matching definition, or `null` if no definition with that name exists.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.  
  - `ArgumentException` – if `name` is `null` or empty.

---

### `GetAllAsync`

```csharp
public static async Task<IReadOnlyList<SagaDefinition>> GetAllAsync(this IInMemorySagaDefinitionRepository repository)
```

Returns all saga definitions currently stored in the repository.

- **Parameters**  
  - `repository` – The in-memory repository instance.

- **Returns**  
  A `Task<IReadOnlyList<SagaDefinition>>` containing every definition in the repository. The list is empty if no definitions exist.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.

---

### `GetActiveAsync`

```csharp
public static async Task<IReadOnlyList<SagaDefinition>> GetActiveAsync(this IInMemorySagaDefinitionRepository repository)
```

Returns only those saga definitions whose status is considered active (e.g., not disabled, deprecated, or completed).

- **Parameters**  
  - `repository` – The in-memory repository instance.

- **Returns**  
  A `Task<IReadOnlyList<SagaDefinition>>` containing the active definitions. The list is empty if no active definitions exist.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.

---

### `SearchByNameAsync`

```csharp
public static async Task<IReadOnlyList<SagaDefinition>> SearchByNameAsync(this IInMemorySagaDefinitionRepository repository, string searchTerm)
```

Performs a case-insensitive substring search on saga definition names.

- **Parameters**  
  - `repository` – The in-memory repository instance.  
  - `searchTerm` – The substring to search for within definition names.

- **Returns**  
  A `Task<IReadOnlyList<SagaDefinition>>` containing all definitions whose name contains the `searchTerm`. The list is empty if no matches are found.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.  
  - `ArgumentException` – if `searchTerm` is `null` or empty.

---

### `GetByVersionAsync`

```csharp
public static async Task<IReadOnlyList<SagaDefinition>> GetByVersionAsync(this IInMemorySagaDefinitionRepository repository, string name, int version)
```

Retrieves all saga definitions that have the specified `name` and exactly the given `version`.

- **Parameters**  
  - `repository` – The in-memory repository instance.  
  - `name` – The exact name of the saga definition.  
  - `version` – The version number to match.

- **Returns**  
  A `Task<IReadOnlyList<SagaDefinition>>` containing the matching definitions. The list is empty if no definitions match.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.  
  - `ArgumentException` – if `name` is `null` or empty.  
  - `ArgumentOutOfRangeException` – if `version` is less than 1.

---

### `GetLatestVersionAsync`

```csharp
public static async Task<SagaDefinition?> GetLatestVersionAsync(this IInMemorySagaDefinitionRepository repository, string name)
```

Finds the saga definition with the highest version number for the given `name`.

- **Parameters**  
  - `repository` – The in-memory repository instance.  
  - `name` – The exact name of the saga definition.

- **Returns**  
  A `Task<SagaDefinition?>` that resolves to the definition with the largest version, or `null` if no definition with that name exists.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.  
  - `ArgumentException` – if `name` is `null` or empty.

---

### `ExistsByNameAsync`

```csharp
public static async Task<bool> ExistsByNameAsync(this IInMemorySagaDefinitionRepository repository, string name)
```

Checks whether at least one saga definition with the specified `name` exists in the repository.

- **Parameters**  
  - `repository` – The in-memory repository instance.  
  - `name` – The exact name to check.

- **Returns**  
  A `Task<bool>` that resolves to `true` if a definition with that name exists; otherwise `false`.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.  
  - `ArgumentException` – if `name` is `null` or empty.

---

### `GetCreatedAfterAsync`

```csharp
public static async Task<IReadOnlyList<SagaDefinition>> GetCreatedAfterAsync(this IInMemorySagaDefinitionRepository repository, DateTime after)
```

Returns all saga definitions whose creation timestamp is later than the specified `after` date and time.

- **Parameters**  
  - `repository` – The in-memory repository instance.  
  - `after` – The cutoff date/time; definitions created strictly after this value are returned.

- **Returns**  
  A `Task<IReadOnlyList<SagaDefinition>>` containing the matching definitions. The list is empty if no definitions were created after the given time.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.

---

### `CountAsync`

```csharp
public static async Task<int> CountAsync(this IInMemorySagaDefinitionRepository repository)
```

Returns the total number of saga definitions in the repository.

- **Parameters**  
  - `repository` – The in-memory repository instance.

- **Returns**  
  A `Task<int>` that resolves to the total count.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.

---

### `CountActiveAsync`

```csharp
public static async Task<int> CountActiveAsync(this IInMemorySagaDefinitionRepository repository)
```

Returns the number of saga definitions that are currently active.

- **Parameters**  
  - `repository` – The in-memory repository instance.

- **Returns**  
  A `Task<int>` that resolves to the count of active definitions.

- **Exceptions**  
  - `ArgumentNullException` – if `repository` is `null`.

---

## Usage

The following examples assume that an implementation of `IInMemorySagaDefinitionRepository` has been registered in the dependency injection container and is injected as `_repository`.

### Example 1: Retrieving and checking existence

```csharp
public async Task<SagaDefinition?> GetOrCreateDefinitionAsync(string name)
{
    // Check if a definition with this name already exists
    bool exists = await InMemorySagaDefinitionRepositoryExtensions.ExistsByNameAsync(_repository, name);
    if (exists)
    {
        // Retrieve the latest version
        return await InMemorySagaDefinitionRepositoryExtensions.GetLatestVersionAsync(_repository, name);
    }

    // Create a new definition (omitted for brevity)
    return null;
}
```

### Example 2: Searching and filtering by creation date

```csharp
public async Task<IReadOnlyList<SagaDefinition>> GetRecentDefinitionsAsync(string searchTerm, DateTime since)
{
    // Search by name substring
    var searchResults = await InMemorySagaDefinitionRepositoryExtensions.SearchByNameAsync(_repository, searchTerm);

    // Further filter by creation date
    var recentResults = searchResults
        .Where(d => d.CreatedAt > since)
        .ToList();

    return recentResults.AsReadOnly();
}
```

---

## Notes

- **Thread safety** – These extension methods do not introduce any synchronization themselves. If the underlying `IInMemorySagaDefinitionRepository` implementation is not thread-safe, concurrent calls from multiple threads may lead to inconsistent results or exceptions. Consider using a thread-safe wrapper or external locking when accessing the repository from parallel contexts.
- **Null and empty parameters** – Methods that accept a `name` or `searchTerm` throw `ArgumentException` when the value is `null` or empty. Always validate input before calling.
- **Version semantics** – `GetByVersionAsync` expects a positive version number (≥1). Passing zero or a negative value will throw `ArgumentOutOfRangeException`.
- **Active status** – The definition of “active” is implementation-specific. `GetActiveAsync` and `CountActiveAsync` rely on a property or predicate defined by the repository; ensure the repository’s active filter matches your domain logic.
- **Asynchronous behavior** – All methods are `async` and return `Task`. Even though the underlying data is in-memory, the async pattern is preserved for consistency with other repository abstractions and to allow future I/O-bound implementations.
