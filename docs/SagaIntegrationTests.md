# SagaIntegrationTests

Integration test suite for the `dotnet-saga-orchestrator` library. The class contains a collection of asynchronous test methods that validate the end‑to‑end behavior of saga definition creation, saga instance lifecycle, concurrency handling, timeout and retry policies, and query operations. Each test is self‑contained and exercises the public API of the orchestrator to ensure correctness under typical and stress scenarios.

## API

### `public async Task EndToEnd_CreateDefinition_CreateSaga_ExecuteSteps_CompletesSuccessfully`
**Purpose**  
Verifies that a saga definition can be created, a saga instance instantiated from that definition, all steps executed, and the saga reaches a completed state without errors.  
**Parameters**  
None.  
**Return Value**  
A `Task` that completes when the test finishes. The task faults if any assertion fails or an unexpected exception occurs.  
**When it Throws**  
- `InvalidOperationException` if the orchestrator is not properly initialized.  
- `AssertionFailedException` (or the testing framework’s equivalent) if any step does not produce the expected outcome.  
- Any exception propagated from the underlying saga execution engine.

### `public async Task MoneyTransferScenario_DefinitionWithThreeSteps_ValidatesAndCreates`
**Purpose**  
Tests a realistic money‑transfer scenario consisting of three steps (debit, credit, notification) ensuring that the definition is validated correctly and a saga can be created from it.  
**Parameters**  
None.  
**Return Value**  
A `Task` completing when the validation and creation assertions have been evaluated.  
**When it Throws**  
- `InvalidOperationException` if the definition validation logic encounters an unexpected state.  
- `AssertionFailedException` if the created saga does not match the expected step count or initial status.  
- Exceptions from the definition builder or validator.

### `public async Task ConcurrentSagaCreation_MultipleThreads_AllSagasCreatedSuccessfully`
**Purpose**  
Confirms that multiple threads can create saga instances concurrently without conflicts, and that each created saga is correctly tracked by the orchestrator.  
**Parameters**  
None.  
**Return Value**  
A `Task` that finishes after all threads have completed and the assertions have been checked.  
**When it Throws**  
- `InvalidOperationException` if the orchestrator’s internal storage cannot handle concurrent writes.  
- `AssertionFailedException` if the number of sagas recorded differs from the expected count.  
- Any threading‑related exception (e.g., `SynchronizationLockException`) that bubbles up from the test harness.

### `public async Task ConcurrentSagaExecution_MultipleThreads_AllProcessWithoutErrors`
**Purpose**  
Ensures that multiple saga instances can be executed in parallel by different threads, and that each instance processes its steps without throwing unexpected errors.  
**Parameters**  
None.  
**Return Value**  
A `Task` completing when all parallel executions have finished and the success assertions have been evaluated.  
**When it Throws**  
- `InvalidOperationException` if the execution engine cannot handle concurrent step processing.  
- `AssertionFailedException` if any saga ends in a failed or compensated state when it should be completed.  
- Exceptions originating from step implementations (wrapped by the test harness).

### `public async Task SagaWithDifferentTimeouts_CreatesCorrectPolicies`
**Purpose**  
Validates that when a saga definition includes varying timeout values per step, the orchestrator creates the appropriate timeout policies for each step.  
**Parameters**  
None.  
**Return Value**  
A `Task` that finishes after the timeout policies have been inspected and asserted.  
**When it Throws**  
- `InvalidOperationException` if the policy creation API is misused.  
- `AssertionFailedException` if the actual timeout values do not match the expected ones.  
- Any exception thrown by the policy builder.

### `public async Task SagaWithDifferentRetryPolicies_CreatesCorrectConfigs`
**Purpose**  
Checks that differing retry policies specified in a saga definition are correctly translated into the orchestrator’s internal retry configuration objects.  
**Parameters**  
None.  
**Return Value**  
A `Task` completing after the retry configurations have been verified.  
**When it Throws**  
- `InvalidOperationException` if the retry configuration cannot be retrieved.  
- `AssertionFailedException` if the retrieved configuration does not match the supplied retry settings.  
- Exceptions from the retry policy factory.

### `public async Task RetrieveSaga_ByExistingId_ReturnsSaga`
**Purpose**  
Ensures that querying the orchestrator for a saga by a known identifier returns the correct saga instance with its current state and data.  
**Parameters**  
None.  
**Return Value**  
A `Task` that completes after the retrieval assertion has been performed.  
**When it Throws**  
- `InvalidOperationException` if the saga store is not accessible.  
- `AssertionFailedException` if the returned saga is null, has an incorrect ID, or its state does not match expectations.  
- Exceptions from the underlying storage layer.

### `public async Task SagaLifecycle_Create_Start_Fail_BeginCompensation_Workflow`
**Purpose**  
Tests the full lifecycle of a saga that is created, started, forced to fail at a designated step, and then verifies that compensation begins correctly.  
**Parameters**  
None.  
**Return Value**  
A `Task` finishing after the compensation workflow assertions have been validated.  
**When it Throws**  
- `InvalidOperationException` if the saga cannot be transitioned to the failed state.  
- `AssertionFailedException` if compensation is not triggered or the saga ends in an unexpected state.  
- Any exception thrown by the step that is intended to fail.

### `public async Task GetSagasByStatus_ReturnsOnlyMatchingStatus`
**Purpose**  
Confirms that the query method for retrieving sagas by status filters correctly and returns only those sagas whose status matches the supplied value.  
**Parameters**  
None.  
**Return Value**  
A `Task` completing after the filtering assertions have been checked.  
**When it Throws**  
- `InvalidOperationException` if the status‑based query mechanism is not functional.  
- `AssertionFailedException` if the returned collection contains sagas with non‑matching status or omits matching ones.  
- Exceptions from the query provider.

### `public async Task SagaWithManySteps_Handles100Steps`
**Purpose**  
Verifies that the orchestrator can manage a saga definition containing a large number of steps (100) without performance degradation or loss of state.  
**Parameters**  
None.  
**Return Value**  
A `Task` that finishes after all steps have been executed and the final state assertions have been made.  
**When it Throws**  
- `InvalidOperationException` if the step limit is exceeded internally.  
- `AssertionFailedException` if any step is skipped, executed out of order, or the saga does not complete.  
- Exceptions arising from step execution (e.g., timeouts) that are not handled as expected.

### `public async Task CreateMultipleDefinitions_TracksThemIndependently`
**Purpose**  
Ensures that multiple saga definitions can be registered with the orchestrator and that each is tracked separately, preventing cross‑definition interference.  
**Parameters**  
None.  
**Return Value**  
A `Task` completing after the independence assertions have been validated.  
**When it Throws**  
- `InvalidOperationException` if the definition registry fails to isolate entries.  
- `AssertionFailedException` if a definition’s steps or metadata are incorrectly merged with another definition’s.  
- Exceptions from the definition storage mechanism.

## Usage

The class is intended to be used with a test runner such as xUnit, NUnit, or MSTest. Each method is an asynchronous test case and can be invoked directly or via the test framework’s attributes.

### Example 1 – Direct invocation (useful for debugging)

```csharp
using System.Threading.Tasks;
using DotnetSagaOrchestrator.Tests; // namespace containing SagaIntegrationTests

public class Demo
{
    public async Task RunSampleTest()
    {
        var tests = new SagaIntegrationTests();
        await tests.EndToEnd_CreateDefinition_CreateSaga_ExecuteSteps_CompletesSuccessfully();
        // If the method completes without throwing, the test passed.
    }
}
```

### Example 2 – Using xUnit attributes

```csharp
using System.Threading.Tasks;
using Xunit;
using DotnetSagaOrchestrator.Tests;

public class SagaIntegrationTestsWrapper
{
    private readonly SagaIntegrationTests _sut = new SagaIntegrationTests();

    [Fact]
    public async Task MoneyTransferScenario_DefinitionWithThreeSteps_ValidatesAndCreates()
    {
        await _sut.MoneyTransferScenario_DefinitionWithThreeSteps_ValidatesAndCreates();
    }

    [Fact]
    public async Task ConcurrentSagaCreation_MultipleThreads_AllSagasCreatedSuccessfully()
    {
        await _sut.ConcurrentSagaCreation_MultipleThreads_AllSagasCreatedSuccessfully();
    }
}
```

## Notes

- **State isolation** – Each test method assumes a clean state of the orchestrator (e.g., in‑memory store or a freshly initialized database). Calling multiple methods on the same instance without resetting the underlying store may lead to false positives or negatives due to leftover data.
- **Thread‑safety** – `SagaIntegrationTests` itself does not contain any mutable shared state; however, the methods that test concurrency (`ConcurrentSagaCreation_*` and `ConcurrentSagaExecution_*`) deliberately invoke the orchestrator from multiple threads. The orchestrator must be thread‑safe for those tests to pass. If the orchestrator is not thread‑safe, those specific tests will fail, indicating a defect in the production code.
- **Exception expectations** – The documented “when it throws” sections reflect the most common failure modes observed in the test implementations. They are not exhaustive; any unexpected exception will cause the test to fail, which is the intended behavior for a unit‑test suite.
- **Performance considerations** – The `SagaWithManySteps_Handles100Steps` test executes a large number of steps sequentially within a single saga. While it validates correctness, it may take longer to run than the other tests; test runners should be configured with an appropriate timeout if necessary.
- **No public constructors or properties** – The class only exposes the asynchronous test methods listed above; there are no additional public members to document. Any other members are considered implementation details and are outside the scope of this documentation.
