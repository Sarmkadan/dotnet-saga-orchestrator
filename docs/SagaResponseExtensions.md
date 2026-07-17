# SagaResponseExtensions
The `SagaResponseExtensions` class provides a set of extension methods for working with saga responses, allowing developers to easily inspect and analyze the status and progress of a saga. These methods enable the extraction of various metrics and details from a saga response, such as completion status, duration, and step-level information.

## API
* `IsCompletedSuccessfully`: Returns a boolean indicating whether the saga has completed successfully.
	+ Parameters: None
	+ Return Value: `bool`
	+ Throws: None
* `IsInProgress`: Returns a boolean indicating whether the saga is currently in progress.
	+ Parameters: None
	+ Return Value: `bool`
	+ Throws: None
* `IsFailed`: Returns a boolean indicating whether the saga has failed.
	+ Parameters: None
	+ Return Value: `bool`
	+ Throws: None
* `GetDurationMilliseconds`: Returns the total duration of the saga in milliseconds, or `null` if the duration is not available.
	+ Parameters: None
	+ Return Value: `long?`
	+ Throws: None
* `GetCompletionPercentage`: Returns the percentage of completion for the saga.
	+ Parameters: None
	+ Return Value: `int`
	+ Throws: None
* `GetFailedSteps`: Returns a list of steps that have failed during the saga execution.
	+ Parameters: None
	+ Return Value: `IReadOnlyList<SagaStepResponse>`
	+ Throws: None
* `GetCompletedSteps`: Returns a list of steps that have completed successfully during the saga execution.
	+ Parameters: None
	+ Return Value: `IReadOnlyList<SagaStepResponse>`
	+ Throws: None
* `GetInProgressSteps`: Returns a list of steps that are currently in progress during the saga execution.
	+ Parameters: None
	+ Return Value: `IReadOnlyList<SagaStepResponse>`
	+ Throws: None
* `GetPendingSteps`: Returns a list of steps that are pending execution during the saga execution.
	+ Parameters: None
	+ Return Value: `IReadOnlyList<SagaStepResponse>`
	+ Throws: None
* `GetAverageStepDurationMilliseconds`: Returns the average duration of each step in the saga in milliseconds, or `null` if the average duration is not available.
	+ Parameters: None
	+ Return Value: `double?`
	+ Throws: None
* `GetRetryCountString`: Returns a string representation of the retry count for the saga.
	+ Parameters: None
	+ Return Value: `string`
	+ Throws: None
* `GetFailureReasonOrDefault`: Returns the failure reason for the saga, or a default value if no failure reason is available.
	+ Parameters: None
	+ Return Value: `string?`
	+ Throws: None

## Usage
The following examples demonstrate how to use the `SagaResponseExtensions` class to analyze a saga response:
```csharp
// Example 1: Checking saga completion status
var sagaResponse = GetSagaResponse();
if (sagaResponse.IsCompletedSuccessfully())
{
    Console.WriteLine("Saga completed successfully.");
}
else if (sagaResponse.IsFailed())
{
    Console.WriteLine("Saga failed.");
}

// Example 2: Extracting saga metrics
var sagaResponse = GetSagaResponse();
Console.WriteLine($"Saga duration: {sagaResponse.GetDurationMilliseconds()}ms");
Console.WriteLine($"Saga completion percentage: {sagaResponse.GetCompletionPercentage()}%");
Console.WriteLine($"Failed steps: {sagaResponse.GetFailedSteps().Count}");
```

## Notes
When using the `SagaResponseExtensions` class, note the following edge cases and thread-safety considerations:
* The `GetDurationMilliseconds` and `GetAverageStepDurationMilliseconds` methods may return `null` if the duration information is not available.
* The `GetFailedSteps`, `GetCompletedSteps`, `GetInProgressSteps`, and `GetPendingSteps` methods return read-only lists, which are thread-safe for iteration but may not reflect changes made to the underlying saga response.
* The `GetRetryCountString` and `GetFailureReasonOrDefault` methods return string representations of the retry count and failure reason, respectively, which may be `null` or empty if no such information is available.
* The `SagaResponseExtensions` class is designed to be thread-safe, but the underlying saga response object may not be. Therefore, it is recommended to synchronize access to the saga response object when using the `SagaResponseExtensions` class in a multi-threaded environment.
