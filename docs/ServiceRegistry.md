# ServiceRegistry

`ServiceRegistry` is the in-memory implementation of `IServiceRegistry`. It stores external service descriptors by name, supports concurrent access, tracks each service's current health state, and logs registration, health-update, and unregistration operations. Registrations live only for the lifetime of the registry instance and are not persisted or discovered over the network.

## Public API

### `ServiceRegistry(ILogger<ServiceRegistry> logger)`

Creates an empty registry using the supplied logger.

- **Parameters**: `logger` — logger used for registry operation messages.
- **Throws**: `ArgumentNullException` when `logger` is `null`.

### `Task RegisterServiceAsync(ServiceDescriptor service)`

Adds a service under `service.Name`, or replaces the descriptor already registered with that name.

- **Parameters**: `service` — the descriptor to store.
- **Throws**: `ArgumentNullException` when `service` or its name is `null`; `ArgumentException` when its name is empty.

### `Task<ServiceDescriptor?> GetServiceAsync(string name)`

Returns the descriptor registered under `name`, or `null` when no matching service exists.

### `Task<List<ServiceDescriptor>> GetAllServicesAsync()`

Returns a snapshot list of the descriptors currently in the registry. The order is unspecified.

### `Task<bool> IsServiceHealthyAsync(string name)`

Returns the registered descriptor's `IsHealthy` value. An unknown service is considered unhealthy and returns `false`.

### `Task UpdateServiceHealthAsync(string name, bool isHealthy)`

Atomically replaces an existing descriptor with a copy whose `IsHealthy` value is set to `isHealthy` and whose `LastHealthCheckTime` is set to the current UTC time. The remaining properties and metadata entries are copied. If the service is not registered, the method makes no change.

### `Task UnregisterServiceAsync(string name)`

Removes the service registered under `name`. If the service is unknown, the method makes no change.

## Usage

The default infrastructure configuration registers `ServiceRegistry` as the singleton implementation of `IServiceRegistry` when integration infrastructure is enabled:

```csharp
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Infrastructure.Integration;

var services = new ServiceCollection();
services.AddLogging();
InfrastructureConfiguration.Default.ConfigureServices(services);

using var provider = services.BuildServiceProvider();
var registry = provider.GetRequiredService<IServiceRegistry>();

await registry.RegisterServiceAsync(new ServiceDescriptor(
    "OrderService",
    "https://orders.example.com"));

await registry.UpdateServiceHealthAsync("OrderService", isHealthy: false);

var service = await registry.GetServiceAsync("OrderService");
Console.WriteLine($"{service?.Name}: healthy = {service?.IsHealthy}");
```

## Notes

- A `ConcurrentDictionary<string, ServiceDescriptor>` provides thread-safe registry operations. Health updates use compare-and-swap replacement so that concurrent updates do not overwrite a newer descriptor.
- `RegisterServiceAsync` stores the supplied descriptor instance directly. Callers can therefore mutate that instance after registration; use `UpdateServiceHealthAsync` when changing health state through the registry.
- Service names are dictionary keys and use the default case-sensitive string comparison.
- The methods expose asynchronous signatures but perform only in-memory work.
