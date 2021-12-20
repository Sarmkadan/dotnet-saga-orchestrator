# TimeoutPolicy
The `TimeoutPolicy` type is designed to manage and track timeouts in a saga orchestrator, providing a flexible way to handle time-sensitive operations. It allows for the creation of policies with varying levels of strictness, enabling developers to balance between leniency and strict adherence to time limits.

## API
* `public int TimeoutSeconds`: Gets the timeout in seconds.
* `public TimeSpan Timeout`: Gets the timeout as a `TimeSpan`.
* `public bool IsRelaxed`: Indicates whether the policy is relaxed.
* `public TimeoutPolicy()`: Initializes a new instance of the `TimeoutPolicy` class.
* `public bool HasExceeded`: Gets a value indicating whether the timeout has been exceeded.
* `public TimeSpan GetRemainingTime`: Gets the remaining time before the timeout is exceeded.
* `public bool HasSufficientTime`: Gets a value indicating whether there is sufficient time before the timeout is exceeded.
* `public double GetElapsedPercentage`: Gets the percentage of elapsed time.
* `public static TimeoutPolicy CreateLenient()`: Creates a lenient timeout policy.
* `public static TimeoutPolicy CreateStandard()`: Creates a standard timeout policy.
* `public static TimeoutPolicy CreateStrict()`: Creates a strict timeout policy.
* `public static TimeoutPolicy Create()`: Creates a new instance of the `TimeoutPolicy` class.

## Usage
```csharp
// Example 1: Creating a standard timeout policy
var policy = TimeoutPolicy.CreateStandard();
if (policy.HasSufficientTime)
{
    // Perform time-sensitive operation
}
else
{
    // Handle timeout exceeded scenario
}

// Example 2: Using a lenient timeout policy
var lenientPolicy = TimeoutPolicy.CreateLenient();
var remainingTime = lenientPolicy.GetRemainingTime;
if (remainingTime > TimeSpan.FromSeconds(30))
{
    // Perform operation with sufficient time
}
else
{
    // Handle low time remaining scenario
}
```

## Notes
When using the `TimeoutPolicy` class, consider the following edge cases:
* If `TimeoutSeconds` is set to 0, `HasExceeded` will immediately return `true`.
* If `IsRelaxed` is `true`, the policy will be more lenient when checking for timeout exceedance.
* The `GetRemainingTime` and `GetElapsedPercentage` methods will return accurate values only if the policy has not exceeded its timeout.
* The `TimeoutPolicy` class is not thread-safe by default; if used in a multithreaded environment, proper synchronization mechanisms should be employed to ensure data integrity.
* The static `Create` methods provide a convenient way to create policies with predefined settings; however, they may not be suitable for all scenarios, and the instance constructor can be used for more fine-grained control.
