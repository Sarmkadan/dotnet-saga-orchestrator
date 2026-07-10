# IServiceRegistry

The `IServiceRegistry` defines the contract for a central mechanism responsible for managing service registrations, tracking health status, and maintaining configuration metadata within the `dotnet-saga-orchestrator` framework. It facilitates dynamic service discovery and health monitoring, ensuring robust communication and fault tolerance during distributed saga execution.

## API

### Constructors and Initialization
*   **`ServiceRegistry()`**: Initializes a new instance of the `ServiceRegistry` class.

### Service Management Methods
*   **`Task RegisterServiceAsync(...)`**: Registers a new service within the registry.
*   **`Task<ServiceDescriptor?> GetServiceAsync(string name)`**: Retrieves the `ServiceDescriptor` for the service with the specified name. Returns `null` if the service is not found.
*   **`Task<List<ServiceDescriptor>> GetAllServicesAsync()`**: Retrieves a list of all currently registered services.
*   **`Task UnregisterServiceAsync(string name)`**: Removes the service registration for the specified name.

### Health Monitoring Methods
*   **`Task<bool> IsServiceHealthyAsync(string name)`**: Performs a health check for the specified service. Returns `true` if the service is healthy, otherwise `false`.
*   **`Task UpdateServiceHealthAsync(string name, bool isHealthy)`**: Updates the health status of the specified service in the registry.

### Properties
*   **`string Name`**: The name of the service.
*   **`string Url`**: The base URL of the service endpoint.
*   **`string? ApiKey`**: The optional API key used for authenticating requests to the service.
*   **`int Timeout`**: The configured timeout in milliseconds for requests sent to this service.
*   **`int MaxRetries`**: The maximum number of retry attempts for failed requests.
*   **`bool IsHealthy`**: Indicates whether the service is currently considered healthy.
*   **`DateTime RegisteredAt`**: The timestamp indicating when the service was registered.
*   **`DateTime LastHealthCheckTime`**: The timestamp of the most recent health check attempt.
*   **`Dictionary<string, string> Metadata`**: A collection of additional key-value pairs associated with the service configuration.

### Supporting Type: ServiceDescriptor
*   **`ServiceDescriptor()`**: Initializes a new default instance of `ServiceDescriptor`.
*   **`ServiceDescriptor(...)`**: Initializes a new instance of `ServiceDescriptor` with specified parameters.
*   **`override string ToString()`**: Returns a string representation of the service descriptor.

## Usage

### Example 1: Registering and Retrieving a Service
```csharp
var registry = new ServiceRegistry();

// Register a new service
await registry.RegisterServiceAsync(new ServiceDescriptor {
    Name = "OrderService",
    Url = "https://api.orders.example.com",
    Timeout = 5000,
    MaxRetries = 3
});

// Retrieve and verify the service
var service = await registry.GetServiceAsync("OrderService");
if (service != null && service.IsHealthy)
{
    Console.WriteLine($"Service {service.Name} is available at {service.Url}");
}
```

### Example 2: Monitoring Service Health
```csharp
var registry = new ServiceRegistry();

// Update health status based on a custom check
bool isResponsive = await CheckRemoteServiceHealth("OrderService");
await registry.UpdateServiceHealthAsync("OrderService", isResponsive);

// Check if a service is healthy before dispatching a saga step
if (await registry.IsServiceHealthyAsync("OrderService"))
{
    // Proceed with saga execution...
}
```

## Notes

*   **Thread Safety**: The implementation of `IServiceRegistry` is designed to be thread-safe for concurrent registration, retrieval, and health status updates.
*   **Asynchronous Operations**: All methods dealing with state (registration, unregistration, health updates) are asynchronous and should be awaited to ensure consistency.
*   **Error Handling**: Methods such as `GetServiceAsync` and `IsServiceHealthyAsync` may throw exceptions if the underlying data storage or network connectivity is unavailable.
*   **Metadata**: The `Metadata` dictionary allows for flexible, environment-specific configuration but should be managed with care to avoid excessive memory usage.
