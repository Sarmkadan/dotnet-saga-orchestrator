# CacheKeyBuilderValidation

The `CacheKeyBuilderValidation` class serves as a centralized, static utility for enforcing naming conventions and structural integrity rules across various identifiers used within the Saga Orchestrator's caching layer. It provides a consistent set of validation mechanisms for critical entities such as Saga IDs, service names, user sessions, and cache keys themselves, offering three distinct interaction patterns per entity: boolean status checks, detailed error message retrieval, and exception-based enforcement to ensure data consistency before cache operations occur.

## API

### Validation Methods (Returning Error Lists)

These methods analyze the input against specific formatting rules and return a list of error messages. If the input is valid, an empty list is returned.

*   **`public static IReadOnlyList<string> ValidateSagaId(string sagaId)`**
    *   **Purpose**: Validates the format and content of a Saga identifier.
    *   **Parameters**: `sagaId` - The string identifier to validate.
    *   **Return Value**: An `IReadOnlyList<string>` containing error descriptions if invalid; otherwise, an empty list.
    *   **Throws**: None.

*   **`public static IReadOnlyList<string> ValidateDefinition(string definition)`**
    *   **Purpose**: Validates the saga definition string or key.
    *   **Parameters**: `definition` - The definition string to validate.
    *   **Return Value**: An `IReadOnlyList<string>` containing error descriptions if invalid; otherwise, an empty list.
    *   **Throws**: None.

*   **`public static IReadOnlyList<string> ValidateStatus(string status)`**
    *   **Purpose**: Validates the saga status string.
    *   **Parameters**: `status` - The status string to validate.
    *   **Return Value**: An `IReadOnlyList<string>` containing error descriptions if invalid; otherwise, an empty list.
    *   **Throws**: None.

*   **`public static IReadOnlyList<string> ValidateServiceName(string serviceName)`**
    *   **Purpose**: Validates the name of the service participating in the saga.
    *   **Parameters**: `serviceName` - The service name string to validate.
    *   **Return Value**: An `IReadOnlyList<string>` containing error descriptions if invalid; otherwise, an empty list.
    *   **Throws**: None.

*   **`public static IReadOnlyList<string> ValidateRateLimit(string rateLimit)`**
    *   **Purpose**: Validates the rate limit configuration string.
    *   **Parameters**: `rateLimit` - The rate limit string to validate.
    *   **Return Value**: An `IReadOnlyList<string>` containing error descriptions if invalid; otherwise, an empty list.
    *   **Throws**: None.

*   **`public static IReadOnlyList<string> ValidateUserId(string userId)`**
    *   **Purpose**: Validates the user identifier associated with the saga context.
    *   **Parameters**: `userId` - The user ID string to validate.
    *   **Return Value**: An `IReadOnlyList<string>` containing error descriptions if invalid; otherwise, an empty list.
    *   **Throws**: None.

*   **`public static IReadOnlyList<string> ValidateSessionId(string sessionId)`**
    *   **Purpose**: Validates the session identifier.
    *   **Parameters**: `sessionId` - The session ID string to validate.
    *   **Return Value**: An `IReadOnlyList<string>` containing error descriptions if invalid; otherwise, an empty list.
    *   **Throws**: None.

*   **`public static IReadOnlyList<string> ValidateWebhookId(string webhookId)`**
    *   **Purpose**: Validates the webhook identifier.
    *   **Parameters**: `webhookId` - The webhook ID string to validate.
    *   **Return Value**: An `IReadOnlyList<string>` containing error descriptions if invalid; otherwise, an empty list.
    *   **Throws**: None.

*   **`public static IReadOnlyList<string> ValidateCacheKey(string cacheKey)`**
    *   **Purpose**: Validates the final constructed cache key string.
    *   **Parameters**: `cacheKey` - The cache key string to validate.
    *   **Return Value**: An `IReadOnlyList<string>` containing error descriptions if invalid; otherwise, an empty list.
    *   **Throws**: None.

### Boolean Check Methods

These methods provide a quick boolean assessment of validity without generating error message objects.

*   **`public static bool IsValidSagaId(string sagaId)`**
    *   **Purpose**: Determines if the Saga ID is valid.
    *   **Parameters**: `sagaId` - The string to check.
    *   **Return Value**: `true` if valid; `false` otherwise.
    *   **Throws**: None.

*   **`public static bool IsValidDefinition(string definition)`**
    *   **Purpose**: Determines if the definition string is valid.
    *   **Parameters**: `definition` - The string to check.
    *   **Return Value**: `true` if valid; `false` otherwise.
    *   **Throws**: None.

*   **`public static bool IsValidStatus(string status)`**
    *   **Purpose**: Determines if the status string is valid.
    *   **Parameters**: `status` - The string to check.
    *   **Return Value**: `true` if valid; `false` otherwise.
    *   **Throws**: None.

*   **`public static bool IsValidServiceName(string serviceName)`**
    *   **Purpose**: Determines if the service name is valid.
    *   **Parameters**: `serviceName` - The string to check.
    *   **Return Value**: `true` if valid; `false` otherwise.
    *   **Throws**: None.

*   **`public static bool IsValidRateLimit(string rateLimit)`**
    *   **Purpose**: Determines if the rate limit string is valid.
    *   **Parameters**: `rateLimit` - The string to check.
    *   **Return Value**: `true` if valid; `false` otherwise.
    *   **Throws**: None.

*   **`public static bool IsValidUserId(string userId)`**
    *   **Purpose**: Determines if the user ID is valid.
    *   **Parameters**: `userId` - The string to check.
    *   **Return Value**: `true` if valid; `false` otherwise.
    *   **Throws**: None.

*   **`public static bool IsValidSessionId(string sessionId)`**
    *   **Purpose**: Determines if the session ID is valid.
    *   **Parameters**: `sessionId` - The string to check.
    *   **Return Value**: `true` if valid; `false` otherwise.
    *   **Throws**: None.

*   **`public static bool IsValidWebhookId(string webhookId)`**
    *   **Purpose**: Determines if the webhook ID is valid.
    *   **Parameters**: `webhookId` - The string to check.
    *   **Return Value**: `true` if valid; `false` otherwise.
    *   **Throws**: None.

*   **`public static bool IsValidCacheKey(string cacheKey)`**
    *   **Purpose**: Determines if the cache key is valid.
    *   **Parameters**: `cacheKey` - The string to check.
    *   **Return Value**: `true` if valid; `false` otherwise.
    *   **Throws**: None.

### Enforcement Methods

These methods perform validation and immediately throw an exception if the input is invalid, ensuring execution halts on bad data.

*   **`public static void EnsureValidSagaId(string sagaId)`**
    *   **Purpose**: Validates the Saga ID and throws an exception if invalid.
    *   **Parameters**: `sagaId` - The string to validate.
    *   **Return Value**: None (returns `void` on success).
    *   **Throws**: Throws an exception (typically `ArgumentException` or a custom validation exception) if the ID is invalid.

*   **`public static void EnsureValidDefinition(string definition)`**
    *   **Purpose**: Validates the definition string and throws an exception if invalid.
    *   **Parameters**: `definition` - The string to validate.
    *   **Return Value**: None (returns `void` on success).
    *   **Throws**: Throws an exception if the definition is invalid.

## Usage

### Example 1: Pre-flight Validation with Error Reporting
This pattern is useful when accepting user input or external configuration where returning specific error messages to the caller is preferred over crashing the process.

```csharp
using SagaOrchestrator.Validation;

public void ConfigureSaga(string incomingSagaId, string serviceName)
{
    // Validate Saga ID
    var sagaErrors = CacheKeyBuilderValidation.ValidateSagaId(incomingSagaId);
    if (sagaErrors.Count > 0)
    {
        Console.WriteLine("Invalid Saga ID:");
        foreach (var error in sagaErrors)
        {
            Console.WriteLine($"- {error}");
        }
        return;
    }

    // Validate Service Name
    var serviceErrors = CacheKeyBuilderValidation.ValidateServiceName(serviceName);
    if (serviceErrors.Count > 0)
    {
        // Handle service name errors
        throw new ConfigurationException(string.Join("; ", serviceErrors));
    }

    // Proceed with valid data
    InitializeSaga(incomingSagaId, serviceName);
}
```

### Example 2: Guard Clause Enforcement
This pattern is ideal for internal methods where invalid data represents a critical logic failure that should halt execution immediately.

```csharp
using SagaOrchestrator.Validation;

public void ProcessWebhook(string webhookId, string definition)
{
    // Throws immediately if webhookId or definition does not meet criteria
    // No need to manually check booleans or parse lists
    CacheKeyBuilderValidation.EnsureValidSagaId(webhookId); 
    CacheKeyBuilderValidation.EnsureValidDefinition(definition);

    // Execution continues only if both are valid
    var cacheKey = BuildCacheKey(webhookId, definition);
    
    if (!CacheKeyBuilderValidation.IsValidCacheKey(cacheKey))
    {
        // Fallback logic for constructed keys that might fail composite rules
        throw new InvalidOperationException("Constructed cache key failed validation.");
    }

    CacheService.Set(cacheKey, GetData());
}
```

## Notes

*   **Null and Empty Handling**: Given the static nature of these validators, passing `null` or empty strings to any `Validate` or `Ensure` method will likely result in validation failures. The `Validate` methods will return a list containing the specific error, while `Ensure` methods will throw.
*   **Thread Safety**: As the class exposes only `static` methods and relies on immutable return types (`IReadOnlyList`, `bool`, `void`) without maintaining internal mutable state, all members are inherently thread-safe and can be called concurrently from multiple threads without synchronization.
*   **Consistency**: The boolean methods (e.g., `IsValidSagaId`) are logically consistent with the list-based methods (e.g., `ValidateSagaId`). If `IsValidSagaId` returns `false`, `ValidateSagaId` is guaranteed to return a non-empty list.
*   **Exception Types**: While the specific exception type thrown by `EnsureValidSagaId` and `EnsureValidDefinition` is not exposed in the signature, callers should anticipate standard argument validation exceptions (such as `ArgumentException` or `ArgumentNullException`) when these methods fail.
