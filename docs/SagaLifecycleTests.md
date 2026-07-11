# SagaLifecycleTests

Test suite verifying the lifecycle state transitions, validation rules, and step management behaviors of the saga orchestrator. Covers initialization, execution, failure handling, compensation, retry logic, and saga definition construction.

## API

### Initialize_WithValidDefinition_SetsStatusToInitialized
Verifies that calling `Initialize` with a valid `SagaDefinition` transitions the saga instance to the `Initialized` status.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if status is not set to `Initialized`.

### Initialize_WithNullDefinition_ThrowsArgumentNullException
Verifies that calling `Initialize` with a `null` definition throws `ArgumentNullException`.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if exception is not thrown or has incorrect type.

### Start_WhenInitialized_TransitionsToRunning
Verifies that calling `Start` on an initialized saga transitions status to `Running`.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if status is not `Running` after start.

### Start_WhenNotInitialized_ThrowsInvalidOperationException
Verifies that calling `Start` on a saga not in `Initialized` state throws `InvalidOperationException`.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if exception is not thrown or has incorrect type.

### Fail_SetsFailedStatusAndCapturesReason
Verifies that calling `Fail` sets status to `Failed` and records the provided failure reason.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if status or reason mismatch.

### BeginCompensation_WhenFailed_TransitionsToCompensating
Verifies that calling `BeginCompensation` on a failed saga transitions status to `Compensating`.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if status is not `Compensating`.

### BeginCompensation_WhenNotFailed_ThrowsInvalidOperationException
Verifies that calling `BeginCompensation` on a saga not in `Failed` state throws `InvalidOperationException`.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if exception is not thrown or has incorrect type.

### CanRetry_WhenBelowMaxRetries_ReturnsTrue
Verifies that `CanRetry` returns `true` when current retry count is less than configured maximum.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if return value is not `true`.

### CanRetry_WhenAtMaxRetries_ReturnsFalse
Verifies that `CanRetry` returns `false` when current retry count equals configured maximum.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if return value is not `false`.

### CanRetry_WhenStatusIsNotFailed_ReturnsFalse
Verifies that `CanRetry` returns `false` when saga status is not `Failed`, regardless of retry count.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if return value is not `false`.

### CompleteCompensation_SetsCompensatedStatus
Verifies that calling `CompleteCompensation` transitions status to `Compensated`.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if status is not `Compensated`.

### SagaDefinition_AddStep_AssignsSequentialOrder
Verifies that adding steps via `AddStep` assigns sequential order values starting from 1.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if order values are not sequential.

### SagaDefinition_AddStep_WithNull_ThrowsArgumentNullException
Verifies that calling `AddStep` with a `null` step definition throws `ArgumentNullException`.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if exception is not thrown or has incorrect type.

### SagaDefinition_GetStepByName_ReturnsMatchingStep
Verifies that `GetStepByName` returns the step matching the provided name.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if returned step does not match.

### SagaDefinition_GetStepByName_WhenNotFound_ReturnsNull
Verifies that `GetStepByName` returns `null` when no step matches the provided name.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if return value is not `null`.

### SagaDefinition_GetStepByOrder_ReturnsMatchingStep
Verifies that `GetStepByOrder` returns the step matching the provided order index.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if returned step does not match.

### SagaStepDefinition_Clone_ReturnsNewInstanceWithIdenticalValues
Verifies that `Clone` produces a new instance with all property values copied.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if cloned instance differs or is same reference.

### SagaStepDefinition_SetTimeout_NegativeValue_ThrowsArgumentException
Verifies that setting a negative timeout via `SetTimeout` throws `ArgumentException`.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if exception is not thrown or has incorrect type.

### SagaStepDefinition_SetRetryPolicy_UpdatesMaxRetriesAndDelay
Verifies that `SetRetryPolicy` correctly updates both `MaxRetries` and `RetryDelay` properties.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if properties do not match provided values.

### SagaStepDefinition_Validate_WhenCompensableWithoutUrl_ReturnsFalse
Verifies that `Validate` returns `false` when a step is marked compensable but lacks a compensation URL.

**Parameters**: None (test method)  
**Returns**: `void`  
**Throws**: `AssertFailedException` if return value is not `false`.

## Usage

```csharp
// Running the full test suite via dotnet CLI
dotnet test --filter "FullyQualifiedName~SagaLifecycleTests"
```

```csharp
// Example: Verifying a custom saga definition passes validation before execution
var definition = new SagaDefinition("OrderProcessing")
    .AddStep(new SagaStepDefinition("ReserveInventory", "POST", "/api/inventory/reserve")
        .SetTimeout(TimeSpan.FromSeconds(30))
        .SetRetryPolicy(3, TimeSpan.FromSeconds(5)))
    .AddStep(new SagaStepDefinition("ChargePayment", "POST", "/api/payments/charge")
        .SetCompensationUrl("/api/payments/refund")
        .SetTimeout(TimeSpan.FromSeconds(15))
        .SetRetryPolicy(2, TimeSpan.FromSeconds(2)));

var validationResult = definition.Validate();
Assert.True(validationResult.IsValid, "Saga definition must be valid before initialization");
```

## Notes

- Tests assume single-threaded execution; saga state transitions are not thread-safe and require external synchronization if accessed concurrently.
- `CanRetry` depends on both status (`Failed`) and retry count; callers must ensure status is checked before invoking retry logic.
- `Validate` on `SagaStepDefinition` only checks compensable steps for compensation URL presence; other validation (e.g., HTTP method format) is not covered.
- Step order assignment is 1-based and immutable after addition; reordering requires rebuilding the definition.
- Negative timeout values are rejected at configuration time, not at execution time.
- Compensation flow requires explicit `BeginCompensation` call after `Fail`; automatic compensation is not implemented.
