# ISagaLogger

`ISagaLogger` defines the contract for logging the lifecycle and state transitions of saga orchestrations within the `dotnet-saga-orchestrator` framework. It provides a structured mechanism to record key events, including saga initiation, individual step execution, compensation processes, and final outcomes, ensuring robust observability and auditability for distributed transactions.

## API

### SagaLogger
The constructor for the default implementation of `ISagaLogger`.
*   **Purpose:** Initializes a new instance of the saga logger.

### LogSagaCreated
Records the initiation of a new saga orchestration.
*   **Parameters:**
    *   `string sagaId`: The unique identifier for the saga.
*   **Returns:** `void`
*   **Throws:** `ArgumentNullException` if `sagaId` is null or empty.

### LogStepStarted
Records that a specific step within a saga has begun execution.
*   **Parameters:**
    *   `string sagaId`: The unique identifier for the saga.
    *   `string stepName`: The name of the step being started.
*   **Returns:** `void`
*   **Throws:** `ArgumentNullException` if `sagaId` or `stepName` is null or empty.

### LogStepCompleted
Records that a specific step within a saga has completed successfully.
*   **Parameters:**
    *   `string sagaId`: The unique identifier for the saga.
    *   `string stepName`: The name of the completed step.
*   **Returns:** `void`
*   **Throws:** `ArgumentNullException` if `sagaId` or `stepName` is null or empty.

### LogStepFailed
Records that a specific step within a saga has failed.
*   **Parameters:**
    *   `string sagaId`: The unique identifier for the saga.
    *   `string stepName`: The name of the failed step.
    *   `Exception ex`: The exception associated with the failure.
*   **Returns:** `void`
*   **Throws:** `ArgumentNullException` if `sagaId` or `stepName` is null or empty; `ArgumentNullException` if `ex` is null.

### LogCompensationStarted
Records that the compensation process for a failed step has begun.
*   **Parameters:**
    *   `string sagaId`: The unique identifier for the saga.
    *   `string stepName`: The name of the step for which compensation is starting.
*   **Returns:** `void`
*   **Throws:** `ArgumentNullException` if `sagaId` or `stepName` is null or empty.

### LogCompensationCompleted
Records that the compensation process for a failed step has completed successfully.
*   **Parameters:**
    *   `string sagaId`: The unique identifier for the saga.
    *   `string stepName`: The name of the compensated step.
*   **Returns:** `void`
*   **Throws:** `ArgumentNullException` if `sagaId` or `stepName` is null or empty.

### LogSagaCompleted
Records that the entire saga orchestration has completed successfully.
*   **Parameters:**
    *   `string sagaId`: The unique identifier for the saga.
*   **Returns:** `void`
*   **Throws:** `ArgumentNullException` if `sagaId` is null or empty.

### LogSagaFailed
Records that the entire saga orchestration has failed.
*   **Parameters:**
    *   `string sagaId`: The unique identifier for the saga.
    *   `Exception ex`: The exception that caused the saga failure.
*   **Returns:** `void`
*   **Throws:** `ArgumentNullException` if `sagaId` is null or empty; `ArgumentNullException` if `ex` is null.

### LogExecutionTimeline
Generates and logs the full execution timeline for a specific saga.
*   **Parameters:**
    *   `string sagaId`: The unique identifier for the saga.
*   **Returns:** `void`
*   **Throws:** `ArgumentNullException` if `sagaId` is null or empty.

## Usage

### Example 1: Basic Saga Logging
```csharp
public async Task ExecuteOrderSaga(string sagaId, ISagaLogger logger)
{
    logger.LogSagaCreated(sagaId);
    
    // Execute Step
    logger.LogStepStarted(sagaId, "ReserveInventory");
    await ReserveInventory();
    logger.LogStepCompleted(sagaId, "ReserveInventory");
    
    logger.LogSagaCompleted(sagaId);
}
```

### Example 2: Handling Failures and Compensations
```csharp
public async Task ExecutePaymentSaga(string sagaId, ISagaLogger logger)
{
    try 
    {
        logger.LogStepStarted(sagaId, "ProcessPayment");
        throw new Exception("Payment gateway unreachable.");
    }
    catch (Exception ex)
    {
        logger.LogStepFailed(sagaId, "ProcessPayment", ex);
        
        logger.LogCompensationStarted(sagaId, "ProcessPayment");
        await RollbackPayment();
        logger.LogCompensationCompleted(sagaId, "ProcessPayment");
        
        logger.LogSagaFailed(sagaId, ex);
    }
}
```

## Notes

*   **Thread Safety:** Implementations of `ISagaLogger` should be thread-safe, as saga orchestrations often involve asynchronous operations executed across different threads.
*   **Exceptions:** Methods will generally throw `ArgumentNullException` when mandatory parameters (e.g., `sagaId`, `stepName`, or `Exception` objects) are provided as null or empty values.
*   **Implementation:** The `SagaLogger` class is the standard implementation; however, custom implementations can be injected if different logging backends (e.g., database, external telemetry services) are required.
