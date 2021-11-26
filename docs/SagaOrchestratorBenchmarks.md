# SagaOrchestratorBenchmarks

SagaOrchestratorBenchmarks is a benchmark harness for evaluating the performance and behavior of saga orchestration workflows in the dotnet-saga-orchestrator project. It provides a structured way to define, configure, and execute sagas while measuring their execution characteristics under various conditions.

## API

### `Setup`
Initializes the benchmark environment and prepares necessary resources for saga execution. This method must be called before any other operations to ensure proper state.

**Parameters:** None  
**Returns:** `void`  
**Throws:**  
- `InvalidOperationException` if called multiple times without resetting the orchestrator.  
- `ArgumentNullException` if required dependencies are not configured.

---

### `CreateDefinition`
Creates a new saga definition that outlines the workflow structure. The definition acts as a blueprint for subsequent saga instances.

**Parameters:** None  
**Returns:** `void`  
**Throws:**  
- `InvalidOperationException` if a definition already exists and has not been finalized.  
- `ObjectDisposedException` if the orchestrator has been disposed.

---

### `AddStep`
Adds a step to the current saga definition. Steps represent individual units of work within the saga workflow.

**Parameters:** None  
**Returns:** `void`  
**Throws:**  
- `InvalidOperationException` if no definition is currently being created.  
- `ArgumentNullException` if the step configuration is invalid.  
- `ObjectDisposedException` if the orchestrator has been disposed.

---

### `CreateSaga`
Instantiates a saga instance based on the previously defined workflow. The created saga is ready for execution but not yet started.

**Parameters:** None  
**Returns:** `void`  
**Throws:**  
- `InvalidOperationException` if no definition has been created or finalized.  
- `ObjectDisposedException` if the orchestrator has been disposed.

---

### `StartSaga`
Begins execution of the created saga instance. This method triggers the first step in the workflow.

**Parameters:** None  
**Returns:** `void`  
**Throws:**  
- `InvalidOperationException` if no saga instance exists or if the saga is already running.  
- `ObjectDisposedException` if the orchestrator has been disposed.

---

### `ExecuteNextStep`
Advances the saga execution to the next step in the workflow. This method is typically called after the current step completes successfully.

**Parameters:** None  
**Returns:** `void`  
**Throws:**  
- `InvalidOperationException` if the saga is not running or has reached its final step.  
- `ObjectDisposedException` if the orchestrator has been disposed.

---

### `Main`
Entry point for the benchmark application. Configures and runs the benchmark scenarios defined in the orchestrator.

**Parameters:** None  
**Returns:** `void`  
**Throws:**  
- `Exception` for unhandled errors during benchmark execution.

## Usage

### Example 1: Basic Saga Workflow Setup and Execution
```csharp
var orchestrator = new SagaOrchestratorBenchmarks();
orchestrator.Setup();
orchestrator.CreateDefinition();
orchestrator.AddStep(); // Add step 1
orchestrator.AddStep(); // Add step 2
orchestrator.CreateSaga();
orchestrator.StartSaga();
orchestrator.ExecuteNextStep(); // Proceed to step 2
```

### Example 2: Multiple Saga Instances with Shared Definition
```csharp
var orchestrator = new SagaOrchestratorBenchmarks();
orchestrator.Setup();
orchestrator.CreateDefinition();
orchestrator.AddStep(); // Define common steps
orchestrator.AddStep();

// Create and start first saga instance
orchestrator.CreateSaga();
orchestrator.StartSaga();
orchestrator.ExecuteNextStep();

// Create and start second saga instance using the same definition
orchestrator.CreateSaga();
orchestrator.StartSaga();
orchestrator.ExecuteNextStep();
```

## Notes

- **Edge Cases:**  
  - Calling `AddStep` after `CreateSaga` will throw an `InvalidOperationException`.  
  - Invoking `ExecuteNextStep` on a completed saga or before `StartSaga` results in undefined behavior.  
  - Reusing the same orchestrator instance without reinitializing via `Setup` may lead to stale state conflicts.  

- **Thread Safety:**  
  - This class is not thread-safe. Concurrent calls to methods like `AddStep`, `CreateSaga`, or `ExecuteNextStep` may corrupt internal state or produce inconsistent results.  
  - External synchronization is required when using the orchestrator in multi-threaded contexts.  

- **Error Handling:**  
  - Custom exceptions are thrown for invalid state transitions, ensuring clear diagnostic information during benchmark failures.  
  - Disposal of the orchestrator invalidates all subsequent operations until reinitialization.
