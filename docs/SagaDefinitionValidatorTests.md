# SagaDefinitionValidatorTests
The `SagaDefinitionValidatorTests` class is designed to test the validation logic of saga definitions in the `dotnet-saga-orchestrator` project. It provides a comprehensive set of test cases to ensure that the validation process correctly identifies and reports errors in saga definitions, including invalid names, step configurations, service URLs, compensation URLs, timeouts, and retry settings.

## API
The `SagaDefinitionValidatorTests` class contains the following public members:
* `ValidateAsync_WithValidDefinition_DoesNotThrow`: Verifies that the validation process does not throw an exception when given a valid saga definition.
* `ValidateAsync_WithInvalidDefinition_Throws`: Verifies that the validation process throws an exception when given an invalid saga definition.
* `ValidateAndGetErrorsAsync_NullName_ReturnsError`: Tests that a null name in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_NameTooLong_ReturnsError`: Tests that a name that is too long in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_NoSteps_ReturnsError`: Tests that a saga definition with no steps returns an error.
* `ValidateAndGetErrorsAsync_TooManySteps_ReturnsError`: Tests that a saga definition with too many steps returns an error.
* `ValidateAndGetErrorsAsync_InvalidStepName_ReturnsError`: Tests that an invalid step name in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_InvalidServiceUrl_ReturnsError`: Tests that an invalid service URL in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_InvalidCompensationUrl_ReturnsError`: Tests that an invalid compensation URL in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_TimeoutZero_ReturnsError`: Tests that a timeout of zero in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_TimeoutTooLarge_ReturnsError`: Tests that a timeout that is too large in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_NegativeRetries_ReturnsError`: Tests that a negative number of retries in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_TooManyRetries_ReturnsError`: Tests that too many retries in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_DuplicateStepOrder_ReturnsError`: Tests that duplicate step orders in the saga definition return an error.
* `ValidateAndGetErrorsAsync_OrderDoesNotStartAtOne_ReturnsError`: Tests that a step order that does not start at one in the saga definition returns an error.
* `ValidateAndGetErrorsAsync_MultipleErrors_ReturnsAll`: Tests that multiple errors in the saga definition are all returned.
* `ValidateAsync_ThrowsWithAllErrors_InExceptionMessage`: Verifies that the validation process throws an exception with all errors in the exception message.
* `ValidateCreateSagaAsync_WithValidRequest_DoesNotThrow`: Verifies that the validation process for creating a saga does not throw an exception when given a valid request.
* `ValidateCreateSagaAsync_MissingDefinitionId_Throws`: Tests that a missing definition ID in the create saga request throws an exception.
* `ValidateCreateSagaAsync_DefinitionIdTooLong_Throws`: Tests that a definition ID that is too long in the create saga request throws an exception.

## Usage
Here are two examples of using the `SagaDefinitionValidatorTests` class:
```csharp
// Example 1: Validating a saga definition
var validator = new SagaDefinitionValidator();
var definition = new SagaDefinition { Name = "My Saga", Steps = new[] { new Step { Name = "Step 1", ServiceUrl = "https://example.com" } } };
await validator.ValidateAsync(definition); // Does not throw

// Example 2: Validating a create saga request
var request = new CreateSagaRequest { DefinitionId = "my-definition" };
await validator.ValidateCreateSagaAsync(request); // Does not throw
```

## Notes
The `SagaDefinitionValidatorTests` class is designed to be thread-safe, as it does not maintain any internal state between test runs. However, it is still important to ensure that the test environment is properly configured and that any dependencies are correctly injected. Additionally, the validation logic is case-sensitive, so care should be taken when constructing saga definitions and create saga requests to ensure that the correct casing is used. The validation process also checks for duplicate step orders, so it is essential to ensure that step orders are unique within a saga definition.
