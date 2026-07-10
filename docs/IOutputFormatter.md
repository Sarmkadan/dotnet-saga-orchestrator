# IOutputFormatter

The `IOutputFormatter` interface provides a consistent mechanism for serializing object data into structured text representations suitable for logging, console output, or file storage. It defines a contract for transforming data into common serialization formats, including JSON, human-readable tables, and comma-separated values (CSV). The `OutputFormatter` class provides the concrete implementation of these requirements.

## API

### OutputFormatter
Initializes a new instance of the `OutputFormatter` class.

*   **Parameters**: None.
*   **Return**: An instance of `OutputFormatter`.
*   **Throws**: N/A.

### FormatAsJson<T>
Serializes a generic object of type `T` into a formatted JSON string.

*   **Parameters**:
    *   `data` (`T`): The object instance to serialize.
*   **Return**: `string` - The JSON representation of the provided object.
*   **Throws**: `JsonException` if the object cannot be serialized.

### FormatAsJson
Serializes a non-generic object into a formatted JSON string.

*   **Parameters**:
    *   `data` (`object`): The object instance to serialize.
*   **Return**: `string` - The JSON representation of the provided object.
*   **Throws**: `JsonException` if the object cannot be serialized.

### FormatAsTable
Converts an `IEnumerable` collection of objects into a human-readable table string.

*   **Parameters**:
    *   `data` (`IEnumerable`): The collection of objects to format as a table.
*   **Return**: `string` - A string representation of the data formatted as a table.
*   **Throws**: `ArgumentNullException` if `data` is `null`.

### FormatAsCsv
Converts an `IEnumerable` collection of objects into a CSV (Comma-Separated Values) string.

*   **Parameters**:
    *   `data` (`IEnumerable`): The collection of objects to format as CSV.
*   **Return**: `string` - A string representation of the data in CSV format.
*   **Throws**: `ArgumentNullException` if `data` is `null`.

## Usage

```csharp
var formatter = new OutputFormatter();
var sagaData = new { SagaId = "saga-123-abc", Status = "InProgress" };

// Example 1: Serializing a single object to JSON
string jsonOutput = formatter.FormatAsJson(sagaData);
Console.WriteLine(jsonOutput);
```

```csharp
var formatter = new OutputFormatter();
var items = new List<object> {
    new { Id = 1, Operation = "Initialize" },
    new { Id = 2, Operation = "Process" }
};

// Example 2: Formatting a collection as a table
string tableOutput = formatter.FormatAsTable(items);
Console.WriteLine(tableOutput);
```

## Notes

*   **Null Values**: The `FormatAsTable` and `FormatAsCsv` methods will throw an `ArgumentNullException` if the provided `IEnumerable` collection is `null`. Empty collections will result in headers without rows or empty strings, depending on the implementation details.
*   **Circular References**: When utilizing `FormatAsJson` methods, ensure that the input objects do not contain circular references, as this may result in a `JsonException` depending on the underlying serializer configuration.
*   **Thread Safety**: The `OutputFormatter` implementation is designed to be stateless and thread-safe, permitting concurrent usage of its formatting methods across multiple threads without explicit synchronization.
