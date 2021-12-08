# ISagaSerializer

Interface defining serialization and deserialization operations for saga state and related data types within the dotnet-saga-orchestrator project. Implementations are responsible for converting saga state objects to and from JSON representations, supporting both compact and indented formatting, and handling custom types such as `SagaStatus`, `SagaStepStatus`, `CompensationStatus`, `CompensationStrategy`, and `DateTime`.

## API

### `public SagaJsonSerializer`

The default JSON serializer instance used internally by `ISagaSerializer` implementations for JSON serialization and deserialization. This field is exposed publicly to allow customization of serialization settings (e.g., converters, formatting) when implementing or extending serialization behavior.

### `public string Serialize<T>(T value)`

Serializes the provided saga state object of type `T` into a compact JSON string.

- **Parameters**:
  - `value`: The saga state object to serialize.
- **Return value**: A compact JSON string representation of the object.
- **Exceptions**: Throws `ArgumentNullException` if `value` is `null`.
- **Notes**: Uses the internal `SagaJsonSerializer` with default settings unless overridden.

### `public T? Deserialize<T>(string json)`

Deserializes a JSON string back into an instance of type `T`.

- **Parameters**:
  - `json`: The JSON string to deserialize.
- **Return value**: The deserialized object of type `T`, or `null` if the JSON represents a null value.
- **Exceptions**: Throws `ArgumentNullException` if `json` is `null`. Throws `JsonException` if the JSON is malformed or incompatible with type `T`.
- **Notes**: Uses the internal `SagaJsonSerializer` with default settings unless overridden.

### `public string SerializeIndented<T>(T value)`

Serializes the provided saga state object of type `T` into a human-readable, indented JSON string.

- **Parameters**:
  - `value`: The saga state object to serialize.
- **Return value**: A JSON string with indentation and formatting for readability.
- **Exceptions**: Throws `ArgumentNullException` if `value` is `null`.
- **Notes**: Uses the internal `SagaJsonSerializer` configured for indented output.

### `public override SagaStatus Read(ref Utf8JsonReader reader)`

Reads a JSON token stream and reconstructs a `SagaStatus` value.

- **Parameters**:
  - `reader`: The JSON reader positioned at the start of the `SagaStatus` value.
- **Return value**: The deserialized `SagaStatus` value.
- **Exceptions**: Throws `JsonException` if the JSON token is not a valid `SagaStatus` representation.
- **Notes**: Must be called when the reader is positioned at a valid JSON token representing a `SagaStatus`.

### `public override void Write(Utf8JsonWriter writer, SagaStatus value)`

Writes a `SagaStatus` value as a JSON token.

- **Parameters**:
  - `writer`: The JSON writer to write to.
  - `value`: The `SagaStatus` value to serialize.
- **Exceptions**: Throws `ArgumentNullException` if `writer` is `null`.
- **Notes**: Writes the value in a compact, standardized format.

### `public override SagaStepStatus Read(ref Utf8JsonReader reader)`

Reads a JSON token stream and reconstructs a `SagaStepStatus` value.

- **Parameters**:
  - `reader`: The JSON reader positioned at the start of the `SagaStepStatus` value.
- **Return value**: The deserialized `SagaStepStatus` value.
- **Exceptions**: Throws `JsonException` if the JSON token is not a valid `SagaStepStatus` representation.
- **Notes**: Must be called when the reader is positioned at a valid JSON token representing a `SagaStepStatus`.

### `public override void Write(Utf8JsonWriter writer, SagaStepStatus value)`

Writes a `SagaStepStatus` value as a JSON token.

- **Parameters**:
  - `writer`: The JSON writer to write to.
  - `value`: The `SagaStepStatus` value to serialize.
- **Exceptions**: Throws `ArgumentNullException` if `writer` is `null`.
- **Notes**: Writes the value in a compact, standardized format.

### `public override CompensationStatus Read(ref Utf8JsonReader reader)`

Reads a JSON token stream and reconstructs a `CompensationStatus` value.

- **Parameters**:
  - `reader`: The JSON reader positioned at the start of the `CompensationStatus` value.
- **Return value**: The deserialized `CompensationStatus` value.
- **Exceptions**: Throws `JsonException` if the JSON token is not a valid `CompensationStatus` representation.
- **Notes**: Must be called when the reader is positioned at a valid JSON token representing a `CompensationStatus`.

### `public override void Write(Utf8JsonWriter writer, CompensationStatus value)`

Writes a `CompensationStatus` value as a JSON token.

- **Parameters**:
  - `writer`: The JSON writer to write to.
  - `value`: The `CompensationStatus` value to serialize.
- **Exceptions**: Throws `ArgumentNullException` if `writer` is `null`.
- **Notes**: Writes the value in a compact, standardized format.

### `public override CompensationStrategy Read(ref Utf8JsonReader reader)`

Reads a JSON token stream and reconstructs a `CompensationStrategy` value.

- **Parameters**:
  - `reader`: The JSON reader positioned at the start of the `CompensationStrategy` value.
- **Return value**: The deserialized `CompensationStrategy` value.
- **Exceptions**: Throws `JsonException` if the JSON token is not a valid `CompensationStrategy` representation.
- **Notes**: Must be called when the reader is positioned at a valid JSON token representing a `CompensationStrategy`.

### `public override void Write(Utf8JsonWriter writer, CompensationStrategy value)`

Writes a `CompensationStrategy` value as a JSON token.

- **Parameters**:
  - `writer`: The JSON writer to write to.
  - `value`: The `CompensationStrategy` value to serialize.
- **Exceptions**: Throws `ArgumentNullException` if `writer` is `null`.
- **Notes**: Writes the value in a compact, standardized format.

### `public override DateTime Read(ref Utf8JsonReader reader)`

Reads a JSON token stream and reconstructs a `DateTime` value.

- **Parameters**:
  - `reader`: The JSON reader positioned at the start of the `DateTime` value.
- **Return value**: The deserialized `DateTime` value.
- **Exceptions**: Throws `JsonException` if the JSON token is not a valid `DateTime` representation.
- **Notes**: Expects ISO 8601 formatted string input unless custom formatting is applied via `SagaJsonSerializer`.

### `public override void Write(Utf8JsonWriter writer, DateTime value)`

Writes a `DateTime` value as a JSON token.

- **Parameters**:
  - `writer`: The JSON writer to write to.
  - `value`: The `DateTime` value to serialize.
- **Exceptions**: Throws `ArgumentNullException` if `writer` is `null`.
- **Notes**: Writes the value in ISO 8601 format by default.
