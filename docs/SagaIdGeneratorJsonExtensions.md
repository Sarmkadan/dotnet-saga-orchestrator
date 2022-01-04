# SagaIdGeneratorJsonExtensions

Provides JSON serialization helpers for `SagaIdGenerator` instances, allowing conversion to and from JSON format and direct access to the underlying `SagaId`.

## API

### `public static string ToJson(this SagaIdGenerator generator)`

Serializes the specified `SagaIdGenerator` to a JSON string.

- **Parameters**  
  - `generator`: The `SagaIdGenerator` instance to serialize.  
- **Return value**  
  - A JSON string representing the generator's state.  
- **Exceptions**  
  - `ArgumentNullException` if `generator` is `null`.  
  - `JsonSerializationException` if the serialization process fails.

### `public static string? FromJson(this string json)`

Attempts to extract the `SagaId` value from a JSON string produced by `ToJson`.

- **Parameters**  
  - `json`: The JSON string to parse.  
- **Return value**  
  - The extracted `SagaId` as a string, or `null` if the JSON does not contain a valid `SagaId` or parsing fails.  
- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.  
  - `JsonException` if the JSON is malformed.

### `public static bool TryFromJson(this string json, out SagaIdGenerator? generator)`

Attempts to deserialize a JSON string into a `SagaIdGenerator` instance.

- **Parameters**  
  - `json`: The JSON string to parse.  
  - `generator`: When the method returns `true`, contains the deserialized `SagaIdGenerator`; otherwise `null`.  
- **Return value**  
  - `true` if `json` was successfully deserialized; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.

### `public string? SagaId { get; set; }`

Gets or sets the `SagaId` associated with the current `SagaIdGenerator` instance.

- **Return value**  
  - The `SagaId` string, or `null` if no identifier has been assigned.  
- **Exceptions**  
  - None.

## Usage

```csharp
var generator = new SagaIdGenerator { SagaId = "abc-123-def" };
string json = generator.ToJson();
// json now contains something like: {"SagaId":"abc-123-def"}

// Later, retrieve the identifier from JSON
string? id = json.FromJson();
// id == "abc-123-def"
```

```csharp
string json = "{\"SagaId\":\"xyz-789\"}";
if (json.TryFromJson(out var gen) && gen != null)
{
    Console.WriteLine($"Deserialized SagaId: {gen.SagaId}");
}
else
{
    Console.WriteLine("Failed to deserialize SagaIdGenerator.");
}
```

## Notes

- Passing `null` to any of the static methods results in an `ArgumentNullException`.  
- The `FromJson` method returns `null` when the JSON lacks a `SagaId` property or when the value cannot be parsed as a string; it does not throw for missing data.  
- `TryFromJson` never throws for malformed JSON; it simply returns `false` and sets the output to `null`.  
- The static methods do not retain state and are safe to call concurrently from multiple threads.  
- The `SagaId` property is instance‑specific; concurrent read/write access to the same instance requires external synchronization to avoid race conditions.
