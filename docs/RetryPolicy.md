# RetryPolicy
The `RetryPolicy` type is designed to manage retry logic in applications, allowing developers to define policies for handling failures and exceptions. It provides a flexible way to specify the number of retries, delay between retries, and other parameters to suit various use cases and requirements.

## API
### Properties
* `MaxRetries`: The maximum number of times a failed operation should be retried.
* `InitialDelayMs`: The initial delay in milliseconds before the first retry.
* `BackoffMultiplier`: A multiplier used to calculate the delay between retries.
* `MaxDelayMs`: The maximum delay in milliseconds between retries.
* `UseJitter`: A flag indicating whether to introduce randomness (jitter) into the delay between retries.

### Constructors
* `RetryPolicy`: Creates a new instance of the `RetryPolicy` class.

### Methods
* `CalculateDelay`: Calculates the delay before the next retry based on the current retry attempt.
* `CanRetry`: Determines whether a retry is allowed based on the current state of the policy.

### Static Factory Methods
* `CreateLinear`: Creates a `RetryPolicy` instance with a linear retry delay.
* `CreateExponential`: Creates a `RetryPolicy` instance with an exponential retry delay.
* `CreateExponentialWithJitter`: Creates a `RetryPolicy` instance with an exponential retry delay and jitter.
* `CreateNoRetry`: Creates a `RetryPolicy` instance that does not allow any retries.

## Usage
The following examples demonstrate how to use the `RetryPolicy` class in a C# application:
```csharp
// Example 1: Using a linear retry policy
var policy = RetryPolicy.CreateLinear(3, 1000); // 3 retries with 1-second delay
for (int i = 0; i < 5; i++)
{
    try
    {
        // Simulate a failing operation
        throw new Exception("Operation failed");
    }
    catch (Exception ex)
    {
        if (policy.CanRetry())
        {
            Console.WriteLine($"Retry {i + 1} after {policy.CalculateDelay()}ms");
            // Wait for the calculated delay before retrying
            Thread.Sleep(policy.CalculateDelay());
        }
        else
        {
            Console.WriteLine("No more retries allowed");
            break;
        }
    }
}

// Example 2: Using an exponential retry policy with jitter
var exponentialPolicy = RetryPolicy.CreateExponentialWithJitter(5, 500, 2); // 5 retries with initial 500ms delay and exponential backoff
for (int i = 0; i < 10; i++)
{
    try
    {
        // Simulate a failing operation
        throw new Exception("Operation failed");
    }
    catch (Exception ex)
    {
        if (exponentialPolicy.CanRetry())
        {
            Console.WriteLine($"Retry {i + 1} after {exponentialPolicy.CalculateDelay()}ms");
            // Wait for the calculated delay before retrying
            Thread.Sleep(exponentialPolicy.CalculateDelay());
        }
        else
        {
            Console.WriteLine("No more retries allowed");
            break;
        }
    }
}
```

## Notes
When using the `RetryPolicy` class, consider the following edge cases and thread-safety remarks:
* The `MaxRetries` property should be set to a reasonable value to avoid infinite loops.
* The `InitialDelayMs` and `MaxDelayMs` properties should be set to values that balance between retrying quickly and avoiding overwhelming the system with retries.
* The `UseJitter` property can help prevent the "thundering herd" problem, where multiple retries occur simultaneously.
* The `RetryPolicy` class is thread-safe, but the `CalculateDelay` method may return different values for concurrent retry attempts due to the use of jitter.
* When using the static factory methods, consider the specific requirements of your application and choose the most suitable retry policy.
