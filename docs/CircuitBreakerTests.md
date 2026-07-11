# CircuitBreakerTests

The `CircuitBreakerTests` class contains unit tests for validating the behavior of a circuit breaker implementation in the `dotnet-saga-orchestrator` project. These tests ensure that the circuit breaker correctly transitions between states (closed, open, half-open), records metrics (successes, failures), and handles execution of actions based on its current state. The tests cover both generic and non-generic execution paths, state management, and edge cases such as stale entries and independent state tracking for different identifiers.

## API

### `ExecuteAsync_SuccessfulAction_RecordsSuccessAndReturnsTrue`
**Purpose**: Verifies that a successful action execution records a success metric and returns `true`.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if the expected behavior is not observed.

---

### `ExecuteAsync_GenericSuccessfulAction_ReturnsValue`
**Purpose**: Validates that a successful generic action execution returns the expected value.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if the returned value does not match expectations.

---

### `ExecuteAsync_FailingAction_ThrowsAndRecordsFailure`
**Purpose**: Ensures that a failing action throws an exception and records a failure metric.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if the exception is not thrown or the failure is not recorded.

---

### `ExecuteAsync_MultipleFailures_OpensCircuit`
**Purpose**: Confirms that repeated failures transition the circuit breaker to the open state.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if the circuit does not open after the configured failure threshold.

---

### `ExecuteAsync_WhenCircuitOpen_ReturnsFalse`
**Purpose**: Tests that execution attempts return `false` when the circuit is open.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if the method does not return `false` in the open state.

---

### `ExecuteAsync_WhenCircuitOpen_GenericThrowsException`
**Purpose**: Verifies that generic execution throws an exception when the circuit is open.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if the expected exception is not thrown.

---

### `GetState_UnknownIdentifier_ReturnsClosed`
**Purpose**: Ensures that querying the state of an unknown identifier returns `Closed` (default state).
**Parameters**: None (test method).
**Return Value**: `void`.
**Throws**: Assertion failures if the state is not `Closed`.

---

### `Reset_ClearsMetricsForIdentifier`
**Purpose**: Validates that resetting the circuit breaker clears its metrics (success/failure counts).
**Parameters**: None (test method).
**Return Value**: `void`.
**Throws**: Assertion failures if metrics are not cleared.

---

### `ExecuteAsync_SuccessInHalfOpenClosesCircuit`
**Purpose**: Tests that a successful execution in the half-open state transitions the circuit back to closed.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if the circuit does not close.

---

### `ExecuteAsync_FailureInHalfOpenReopensCircuit`
**Purpose**: Ensures that a failed execution in the half-open state reopens the circuit.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if the circuit does not reopen.

---

### `EvictStaleEntries_RemovesUnusedClosedCircuits`
**Purpose**: Confirms that stale (unused) closed circuits are evicted from the cache.
**Parameters**: None (test method).
**Return Value**: `void`.
**Throws**: Assertion failures if stale entries are not removed.

---

### `ExecuteAsync_SuccessfulAction_IncrementSuccess`
**Purpose**: Validates that a successful action increments the success counter.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if the success count is not incremented.

---

### `ExecuteAsync_DifferentIdentifiers_MaintainIndependentState`
**Purpose**: Ensures that circuit breakers with different identifiers maintain independent states and metrics.
**Parameters**: None (test method).
**Return Value**: `Task` (async test completion).
**Throws**: Assertion failures if states or metrics are shared incorrectly.

---

### `CatchAsync`
**Purpose**: Static helper method to execute an action and catch exceptions, returning `false` on failure.
**Parameters**: None (test utility).
**Return Value**: `Task<bool>` – `true` if the action succeeds, `false` if an exception is caught.
**Throws**: None (swallows exceptions).

---

### `CatchAsync<T>`
**Purpose**: Static generic helper method to execute an action and catch exceptions, returning a default value on failure.
**Parameters**: None (test utility).
**Return Value**: `Task<T>` – the result of the action if successful, or the default value of `T` if an exception is caught.
**Throws**: None (swallows exceptions).

## Usage

### Example 1: Testing Circuit Breaker State Transitions
