# SagaCommandResult

A result container used in the Saga Orchestrator pattern to encapsulate the outcome of a saga command execution. It provides a structured way to communicate success, failure, or exceptions along with associated data and error details. The type is available in both non-generic and generic forms to support scenarios where typed data payloads are required.

## API

### Non-Generic Members

#### `Success`
- **Type**: `bool`
- **Purpose**: Indicates whether the saga command executed successfully.
- **Remarks**: `true` when the operation completed without errors; otherwise `false`.

#### `Message`
- **Type**: `string`
- **Purpose**: A human-readable message describing the result of the command execution.
- **Remarks**: May be `null` or empty when no message is provided.

#### `Data`
- **Type**: `object?`
- **Purpose**: An optional untyped data payload associated with the result.
- **Remarks**: Can be `null` if no data is relevant.

#### `Errors`
- **Type**: `List<string>`
- **Purpose**: A collection of error messages encountered during execution.
- **Remarks**: Empty if no errors occurred.

#### `Timestamp`
- **Type**: `DateTime`
- **Purpose**: The UTC timestamp when the result was created.
- **Remarks**: Set automatically on construction.

#### `RequestId`
- **Type**: `string`
- **Purpose**: A unique identifier for the request that produced this result.
- **Remarks**: Useful for correlating logs and diagnostics.

#### `SuccessResult`
- **Type**: `static SagaCommandResult`
- **Purpose**: A pre-constructed successful result with default values.
- **Remarks**: `Success = true`, `Errors = empty`, `Message = null`, `Data = null`.

#### `FailureResult`
- **Type**: `static SagaCommandResult`
- **Purpose**: A pre-constructed failed result indicating a business-level failure.
- **Remarks**: `Success = false`, `Errors = empty`, `Message = "Command failed"`, `Data = null`.

#### `ExceptionResult`
- **Type**: `static SagaCommandResult`
- **Purpose**: A pre-constructed result indicating an unhandled exception occurred.
- **Remarks**: `Success = false`, `Errors = empty`, `Message = "Command failed due to exception"`, `Data = null`.

---

### Generic Members (`SagaCommandResult<T>`)

#### `Success`
- **Type**: `bool`
- **Purpose**: Indicates whether the saga command executed successfully.
- **Remarks**: Same semantics as the non-generic version.

#### `Message`
- **Type**: `string`
- **Purpose**: A human-readable message describing the result.
- **Remarks**: May be `null` or empty.

#### `Data`
- **Type**: `T?`
- **Purpose**: A strongly-typed data payload associated with the result.
- **Remarks**: Can be `null` if no data is relevant.

#### `Errors`
- **Type**: `List<string>`
- **Purpose**: A collection of error messages encountered during execution.
- **Remarks**: Empty if no errors occurred.

#### `Timestamp`
- **Type**: `DateTime`
- **Purpose**: The UTC timestamp when the result was created.
- **Remarks**: Set automatically on construction.

#### `RequestId`
- **Type**: `string`
- **Purpose**: A unique identifier for the request that produced this result.
- **Remarks**: Useful for correlating logs and diagnostics.

#### `Items`
- **Type**: `List<T>`
- **Purpose**: A collection of typed items, typically used for paginated or batched results.
- **Remarks**: Empty if no items are present.

#### `PageNumber`
- **Type**: `int`
- **Purpose**: The page number in a paginated result set.
- **Remarks**: Defaults to `1` if not specified.

#### `SuccessResult`
- **Type**: `static SagaCommandResult<T>`
- **Purpose**: A pre-constructed successful result with default values.
- **Remarks**: `Success = true`, `Errors = empty`, `Message = null`, `Data = default`, `Items = empty`, `PageNumber = 1`.

#### `FailureResult`
- **Type**: `static SagaCommandResult<T>`
- **Purpose**: A pre-constructed failed result indicating a business-level failure.
- **Remarks**: `Success = false`, `Errors = empty`, `Message = "Command failed"`, `Data = default`, `Items = empty`, `PageNumber = 1`.

#### `ExceptionResult`
- **Type**: `static SagaCommandResult<T>`
- **Purpose**: A pre-constructed result indicating an unhandled exception occurred.
- **Remarks**: `Success = false`, `Errors = empty`, `Message = "Command failed due to exception"`, `Data = default`, `Items = empty`, `PageNumber = 1`.

## Usage

### Example 1: Basic Success with Untyped Data
