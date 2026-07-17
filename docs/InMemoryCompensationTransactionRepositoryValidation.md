# InMemoryCompensationTransactionRepositoryValidation

Provides static validation utilities for verifying the integrity and correctness of an in-memory compensation transaction repository. This type exposes methods to check whether the repository's internal state meets all required invariants, retrieve a list of validation failures, and assert validity with an exception on failure.

## API

### Validate

```csharp
public static IReadOnlyList<string> Validate(InMemoryCompensationTransactionRepository repository)
```

Runs all validation rules against the specified repository instance and returns a read-only list of error messages. Each entry describes a distinct invariant violation. Returns an empty list when the repository is fully valid.

**Parameters:**
- `repository` — The `InMemoryCompensationTransactionRepository` instance to inspect.

**Return value:**
- `IReadOnlyList<string>` — A list of validation error strings. Empty when no violations are found.

**Exceptions:**
- `ArgumentNullException` — Thrown when `repository` is `null`.

### IsValid

```csharp
public static bool IsValid(InMemoryCompensationTransactionRepository repository)
```

Convenience predicate that returns `true` if the repository passes all validation checks, or `false` if any invariant is violated. Equivalent to calling `Validate(repository)` and checking whether the resulting list is empty.

**Parameters:**
- `repository` — The `InMemoryCompensationTransactionRepository` instance to inspect.

**Return value:**
- `bool` — `true` when no validation errors exist; otherwise `false`.

**Exceptions:**
- `ArgumentNullException` — Thrown when `repository` is `null`.

### EnsureValid

```csharp
public static void EnsureValid(InMemoryCompensationTransactionRepository repository)
```

Performs the same validation as `Validate` but throws an aggregate exception containing all error messages if any violations are detected. Use this as a guard clause to enforce repository correctness at critical boundaries.

**Parameters:**
- `repository` — The `InMemoryCompensationTransactionRepository` instance to inspect.

**Exceptions:**
- `ArgumentNullException` — Thrown when `repository` is `null`.
- `ValidationException` (or an aggregate exception wrapping multiple error strings) — Thrown when one or more validation rules fail.

## Usage

### Example 1: Conditional logic based on validation

```csharp
var repository = new InMemoryCompensationTransactionRepository();
repository.Add(new CompensationTransaction { Id = "tx-1", Status = "Pending" });

if (InMemoryCompensationTransactionRepositoryValidation.IsValid(repository))
{
    Console.WriteLine("Repository is consistent; proceeding with orchestration.");
}
else
{
    var errors = InMemoryCompensationTransactionRepositoryValidation.Validate(repository);
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Example 2: Guarding a critical operation

```csharp
public void CommitCompensation(InMemoryCompensationTransactionRepository repository)
{
    InMemoryCompensationTransactionRepositoryValidation.EnsureValid(repository);

    // At this point the repository is guaranteed valid.
    var pending = repository.GetByStatus("Pending");
    foreach (var transaction in pending)
    {
        transaction.Status = "Committed";
        repository.Update(transaction);
    }
}
```

## Notes

- All methods are static and stateless; they are safe to call concurrently from multiple threads as long as the underlying repository instance is not mutated during validation.
- The validation rules themselves are determined by the internal implementation and may include checks for duplicate identifiers, missing required fields, inconsistent status transitions, or orphaned compensation records.
- `EnsureValid` throws on first invocation of failure; it does not short-circuit on the first error but collects all violations before throwing, giving the caller a complete picture of what is wrong.
- The returned `IReadOnlyList<string>` from `Validate` is a snapshot — subsequent mutations to the repository are not reflected in the list.
- Callers should treat an empty list from `Validate` and a `true` result from `IsValid` as equivalent guarantees at the moment of the call; there is no ongoing monitoring.
