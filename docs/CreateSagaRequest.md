# CreateSagaRequest

Represents the input model for initiating a new saga orchestration. This type encapsulates the identifier of the saga definition to execute, optional overrides for retry and timeout policies, arbitrary metadata for correlation or tracing, and an optional serialized payload that will be passed to the saga’s initial step. The `IsValid` property provides a quick, client-side validation check before the request is submitted to the orchestrator.

## API

### `public string DefinitionId`

Gets or sets the unique identifier of the saga definition that should be executed. This value is required; an empty or null `DefinitionId` will cause `IsValid` to return `false`. The orchestrator uses this identifier to locate the corresponding saga blueprint and its associated step graph.

### `public string? DefinitionName`

Gets or sets an optional human-readable name for the saga definition. When provided, it can be used as a secondary lookup key or for diagnostic logging. A null value is permitted and simply means no name-based resolution will be attempted.

### `public int? MaxRetries`

Gets or sets an optional override for the maximum number of retries allowed across the entire saga execution. When null, the orchestrator falls back to the default retry policy defined in the saga definition. A non-null, non-negative value replaces that default for this specific saga instance. Setting a negative value will cause `IsValid` to return `false`.

### `public int? TimeoutSeconds`

Gets or sets an optional override for the total execution timeout in seconds. When null, the timeout defaults to the value configured in the saga definition. A non-null, positive value overrides that default. A value of zero or a negative value will cause `IsValid` to return `false`.

### `public Dictionary<string, object>? Metadata`

Gets or sets an optional dictionary of key-value pairs for attaching arbitrary metadata to the saga instance. Common uses include storing correlation IDs, tenant identifiers, or custom tags for observability platforms. The dictionary itself can be null; if non-null, it may be empty. The keys are case-sensitive strings, and the values can be any object that the serialization layer supports.

### `public string? Data`

Gets or sets an optional serialized payload, typically a JSON string, that represents the input data for the saga’s first step. A null value indicates that the saga expects no initial data or that the data will be fetched by the first step itself. The orchestrator does not interpret this string; it is passed through to the saga execution context as-is.

### `public bool IsValid`

Gets a value indicating whether the request passes basic client-side validation. This property returns `true` only when `DefinitionId` is not null or empty, `MaxRetries` (if set) is non-negative, and `TimeoutSeconds` (if set) is greater than zero. It does not validate the contents of `Metadata` or `Data`, nor does it guarantee that the `DefinitionId` actually exists in the orchestrator’s registry.

## Usage

### Example 1: Minimal valid request with metadata

```csharp
var request = new CreateSagaRequest
{
    DefinitionId = "order-fulfillment-v2",
    Metadata = new Dictionary<string, object>
    {
        ["correlationId"] = Guid.NewGuid().ToString(),
        ["tenant"] = "acme-west"
    }
};

if (request.IsValid)
{
    await sagaOrchestrator.SubmitAsync(request);
}
else
{
    Console.WriteLine("Request validation failed.");
}
```

### Example 2: Full override with initial payload

```csharp
var request = new CreateSagaRequest
{
    DefinitionId = "customer-onboarding",
    DefinitionName = "Customer Onboarding (High Priority)",
    MaxRetries = 5,
    TimeoutSeconds = 3600,
    Data = JsonSerializer.Serialize(new { CustomerId = "CUST-1234", Plan = "enterprise" }),
    Metadata = new Dictionary<string, object>
    {
        ["initiatedBy"] = "admin-portal",
        ["priority"] = "high"
    }
};

if (!request.IsValid)
{
    throw new InvalidOperationException(
        "Saga request is invalid. Check DefinitionId, MaxRetries, and TimeoutSeconds.");
}

var sagaId = await sagaOrchestrator.SubmitAsync(request);
Console.WriteLine($"Saga created with ID: {sagaId}");
```

## Notes

- `IsValid` performs only structural checks on the request itself. It does not verify that `DefinitionId` corresponds to a registered saga definition, nor does it validate that `Data` can be deserialized by the target saga’s initial step. Those checks occur server-side during submission.
- The `Metadata` dictionary, when provided, is typically serialized alongside the saga instance. Values must be of types supported by the configured serialization layer (e.g., strings, numbers, booleans, or nested dictionaries). Complex objects that require custom converters may cause serialization failures at submission time.
- `MaxRetries` and `TimeoutSeconds` override saga-level defaults only when they are non-null. Setting `MaxRetries` to `0` is valid and means no retries will be attempted. Setting `TimeoutSeconds` to a very large value may cause the saga to remain in a running state indefinitely if a step hangs; ensure the orchestrator has infrastructure-level timeout enforcement if this is a concern.
- This type is not thread-safe. It is designed to be constructed, validated, and submitted on a single thread. Concurrent reads and writes to the same instance without external synchronization may produce inconsistent state, particularly for the `Metadata` dictionary if it is mutated after being assigned.
- Once submitted to the orchestrator, changes to the request instance have no effect on the already-created saga. The orchestrator captures a snapshot of the relevant values at submission time.
