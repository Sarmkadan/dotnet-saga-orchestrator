# SagaActivitySourceJsonExtensions

Provides JSON serialization and deserialization helpers for `SagaActivitySourceTelemetry` objects, along with a simple name property to identify the source.

## API

### `public static string ToJson(SagaActivitySourceTelemetry telemetry)`
Serializes the supplied `SagaActivitySourceTelemetry` instance to a JSON string.  
- **Parameters**  
  - `telemetry`: The telemetry object to serialize.  
- **Return value**  
  - A JSON‑encoded string representing the telemetry.  
- **Exceptions**  
  - `ArgumentNullException` if `telemetry` is `null`.  
  - `JsonSerializationException` if the object cannot be serialized (e.g., due to unsupported member types).

### `public static SagaActivitySourceTelemetry? FromJson(string json)`
Deserializes a JSON string into a `SagaActivitySourceTelemetry` instance.  
- **Parameters**  
  - `json`: The JSON string to parse.  
- **Return value**  
  - The deserialized `SagaActivitySourceTelemetry` object, or `null` if `json` is `null` or does not represent a valid telemetry payload.  
- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.  
  - `JsonSerializationException` if the JSON is malformed or cannot be mapped to the telemetry type.

### `public static bool TryFromJson(string json, out SagaActivitySourceTelemetry? result)`
Attempts to deserialize a JSON string into a `SagaActivitySourceTelemetry` instance without throwing exceptions on failure.  
- **Parameters**  
  - `json`: The JSON string to parse.  
  - `result`: When the method returns `true`, contains the deserialized telemetry; otherwise `null`.  
- **Return value**  
  - `true` if `json` was successfully parsed and mapped to a telemetry object; `false` otherwise.  
- **Exceptions**  
  - `ArgumentNullException` if `json` is `null`.

### `public string? Name { get; set; }`
Gets or sets an optional identifier for the activity source associated with the extension instance.  
- **Return value**  
  - The current name, or `null` if no name has been assigned.  
- **Exceptions**  
  - None.

## Usage

```csharp
using DotnetSagaOrchestrator.Telemetry;

// Create a telemetry object
var telemetry = new SagaActivitySourceTelemetry
{
    TraceId = "0af7651916cd43dd8448eb211c80319c",
    SpanId  = "b7ad6b7169203331",
    OperationName = "ProcessOrder"
};

// Serialize to JSON
string json = SagaActivitySourceJsonExtensions.ToJson(telemetry);
Console.WriteLine(json);
// Output: {"TraceId":"0af7651916cd43dd8448eb211c80319c","SpanId":"b7ad6b7169203331","OperationName":"ProcessOrder"}

// Deserialize using FromJson
SagaActivitySourceTelemetry? restored = SagaActivitySourceJsonExtensions.FromJson(json);
if (restored != null)
{
    Console.WriteLine(restored.OperationName); // ProcessOrder
}

// Deserialize safely with TryFromJson
if (SagaActivitySourceJsonExtensions.TryFromJson(json, out var maybeTelemetry) && maybeTelemetry != null)
{
    Console.WriteLine($"TraceId: {maybeTelemetry.TraceId}");
}
```

```csharp
// Using the Name property to label an extension instance
var extensions = new SagaActivitySourceJsonExtensions { Name = "OrderSagaSource" };
Console.WriteLine(extensions.Name); // OrderSagaSource

// The Name can be cleared or changed later
extensions.Name = null;
Console.WriteLine(extensions.Name); // (null output)
```

## Notes

- The static JSON methods do not retain any state; they are safe to call concurrently from multiple threads.  
- The `Name` property is instance‑specific; concurrent reads and writes to the same instance without external synchronization may lead to race conditions.  
- Passing `null` to any of the static methods results in an `ArgumentNullException`; the `TryFromJson` method follows the same rule for its input parameter but never throws for invalid JSON—instead it returns `false`.  
- If the JSON payload contains extra properties not defined on `SagaActivitySourceTelemetry`, they are ignored during deserialization; missing required properties cause deserialization to fail and produce a `null` result (or `false` for `TryFromJson`).  
- The `ToJson` method always produces UTF‑8 encodes the output; callers should treat the returned string as plain text suitable for storage or transmission.
