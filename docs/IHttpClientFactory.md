# IHttpClientFactory

The `IHttpClientFactory` interface within the `dotnet-saga-orchestrator` project defines the contract for creating and managing HTTP clients tailored for saga orchestration workflows. It provides a centralized mechanism to configure base addresses, authentication tokens, default headers, and timeout policies, ensuring consistent communication patterns across distributed saga steps while leveraging policy-based message handling for resilience.

## API

### `public HttpClientFactory`
Represents the concrete implementation or factory accessor associated with the interface. This member exposes the underlying factory instance responsible for instantiating `HttpClient` objects with the configured saga-specific policies and settings.

### `public HttpClient CreateClient`
A method or property accessor that generates a new `HttpClient` instance.
*   **Purpose**: Instantiates an `HttpClient` pre-configured with the `BaseUrl`, `TimeoutSeconds`, `AuthToken`, `DefaultHeaders`, and the `PolicyHttpMessageHandler`.
*   **Parameters**: None (as defined in the provided signature).
*   **Return Value**: Returns a configured `HttpClient` ready for use.
*   **Throws**: May throw configuration exceptions if the internal policy handler or base URL is invalid.

### `public async Task<T> SendAsync<T>`
Executes an HTTP request and deserializes the response.
*   **Purpose**: Sends an HTTP request using the configured client and policies, automatically handling the deserialization of the response content into the specified type `T`.
*   **Parameters**: Implicitly requires an `HttpRequestMessage` or equivalent context (signature details inferred from usage patterns), though specific parameters are not listed in the provided summary.
*   **Return Value**: Returns a `Task<T>` representing the asynchronous operation, where `T` is the deserialized response object.
*   **Throws**: Throws `HttpRequestException` on network failures, `TimeoutException` if the operation exceeds `TimeoutSeconds`, or deserialization errors if the response content cannot be mapped to `T`.

### `public string BaseUrl`
*   **Purpose**: Gets or sets the base address Uniform Resource Identifier (URI) used for all requests created by this factory.
*   **Parameters**: N/A (Property).
*   **Return Value**: Returns the current base URL string.
*   **Throws**: May throw `ArgumentException` if set to an invalid URI format.

### `public int TimeoutSeconds`
*   **Purpose**: Gets or sets the time-out value in seconds for requests initiated through this factory.
*   **Parameters**: N/A (Property).
*   **Return Value**: Returns the timeout duration as an integer.
*   **Throws**: May throw `ArgumentOutOfRangeException` if set to a negative value.

### `public string? AuthToken`
*   **Purpose**: Gets or sets the optional authentication token (e.g., Bearer token) to be included in the `Authorization` header of outgoing requests.
*   **Parameters**: N/A (Property).
*   **Return Value**: Returns the token string or `null` if no authentication is configured.
*   **Throws**: None.

### `public Dictionary<string, string> DefaultHeaders`
*   **Purpose**: Gets or sets a collection of key-value pairs representing HTTP headers to be added to every request by default.
*   **Parameters**: N/A (Property).
*   **Return Value**: Returns the dictionary of default headers.
*   **Throws**: May throw `ArgumentNullException` if assigned a null dictionary, depending on implementation strictness.

### `public PolicyHttpMessageHandler`
*   **Purpose**: Exposes the underlying `PolicyHttpMessageHandler` used to inject resilience policies (such as retries, circuit breakers, or bulkhead isolation) into the HTTP pipeline.
*   **Parameters**: N/A (Property/Field).
*   **Return Value**: Returns the configured handler instance.
*   **Throws**: None.

## Usage

### Example 1: Configuring and Creating a Client
This example demonstrates initializing the factory with saga-specific configuration and generating a client for direct use.

```csharp
var factory = new HttpClientFactory
{
    BaseUrl = "https://api.saga-orchestrator.local/v1",
    TimeoutSeconds = 30,
    AuthToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    DefaultHeaders = new Dictionary<string, string>
    {
        { "X-Saga-Id", "saga-12345" },
        { "Content-Type", "application/json" }
    }
};

// Create a client with all policies and headers applied
using var client = factory.CreateClient();

// Use the standard HttpClient methods if custom control is needed
var response = await client.GetAsync("/status");
```

### Example 2: Sending a Typed Request
This example utilizes the `SendAsync<T>` method to execute a request and automatically deserialize the result into a specific domain object.

```csharp
var factory = new HttpClientFactory
{
    BaseUrl = "https://api.saga-orchestrator.local/v1",
    TimeoutSeconds = 15,
    AuthToken = null, // Public endpoint
    DefaultHeaders = new Dictionary<string, string>()
};

// Execute request and deserialize directly to SagaStateDto
try 
{
    var sagaState = await factory.SendAsync<SagaStateDto>(new HttpRequestMessage 
    { 
        Method = HttpMethod.Get, 
        RequestUri = new Uri("/saga/12345", UriKind.Relative) 
    });

    Console.WriteLine($"Current Status: {sagaState.Status}");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Saga communication failed: {ex.Message}");
}
```

## Notes

*   **Thread Safety**: While `HttpClient` is designed to be reused, the `CreateClient` method suggests a per-operation or scoped instantiation pattern driven by the factory. The `DefaultHeaders` dictionary is not inherently thread-safe for concurrent modifications; if the factory instance is shared across threads, the dictionary should be treated as immutable after initialization or accessed via locking mechanisms.
*   **Timeout Behavior**: The `TimeoutSeconds` property likely configures a `CancellationToken` or a `HttpClient.Timeout` value. Requests exceeding this duration will terminate abruptly, potentially leaving saga steps in a transient failure state requiring retry logic via the `PolicyHttpMessageHandler`.
*   **Authentication Precedence**: If `AuthToken` is set, it typically overrides any `Authorization` header manually added to `DefaultHeaders`. Care should be taken to avoid duplicate or conflicting authorization headers.
*   **Handler Disposal**: The `PolicyHttpMessageHandler` manages the underlying HTTP pipeline. When using `CreateClient`, ensure the lifecycle of the resulting `HttpClient` aligns with the handler's expectations to prevent socket exhaustion or premature disposal of resilience policies.
