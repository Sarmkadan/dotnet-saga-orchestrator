# SagaException

A specialized exception type used within the `dotnet-saga-orchestrator` project to represent failures that occur during saga execution. It extends the base `Exception` class to include saga-specific context such as the saga identifier and an optional error code, enabling more granular error handling and logging in distributed workflows.

## API

### `SagaException(string message)`
Constructs a new `SagaException` with the specified error message.

- **Parameters**:
  - `message` (string): A human-readable description of the error.
- **Return value**: A new instance of `SagaException`.
- **Throws**: Never throws directly; any exceptions during construction are propagated from the base class.

### `SagaException(string message, Exception? innerException)`
Constructs a new `SagaException` with the specified error message and an inner exception.

- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `innerException` (Exception?): The exception that is the cause of the current exception.
- **Return value**: A new instance of `SagaException`.
- **Throws**: Never throws directly; any exceptions during construction are propagated from the base class.

### `SagaException(string message, string sagaId)`
Constructs a new `SagaException` with the specified error message and saga identifier.

- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `sagaId` (string): The unique identifier of the saga associated with the error.
- **Return value**: A new instance of `SagaException`.
- **Throws**: Never throws directly; any exceptions during construction are propagated from the base class.

### `SagaException(string message, string sagaId, string errorCode)`
Constructs a new `SagaException` with the specified error message, saga identifier, and error code.

- **Parameters**:
  - `message` (string): A human-readable description of the error.
  - `sagaId` (string): The unique identifier of the saga associated with the error.
  - `errorCode` (string): A machine-readable code representing the type of error.
- **Return value**: A new instance of `SagaException`.
- **Throws**: Never throws directly; any exceptions during construction are propagated from the base class.

### Properties

#### `SagaId` (string?)
Gets the unique identifier of the saga associated with the exception.

- **Type**: `string?`
- **Description**: May be `null` if not provided during construction.

#### `ErrorCode` (string?)
Gets the machine-readable error code associated with the exception.

- **Type**: `string?`
- **Description**: May be `null` if not provided during construction.

## Usage

### Example 1: Basic Saga Failure
