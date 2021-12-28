# ServiceConfiguration

The `ServiceConfiguration` class serves as the central extension point for registering the Saga Orchestrator components into a .NET dependency injection container. It provides a set of static methods designed to modularly configure saga orchestration logic, persistence repositories, and supporting services within an `IServiceCollection`, enabling flexible composition of the saga infrastructure based on application requirements.

## API

### AddSagaOrchestrator

Registers the core saga orchestrator services required to manage state transitions and coordinate saga execution.

*   **Purpose**: Initializes the primary orchestrator engine and its dependencies within the service collection.
*   **Parameters**:
    *   `services`: The `IServiceCollection` to which the services are added.
    *   `options`: (Implicit in overload) An optional configuration action to customize orchestrator behavior.
*   **Return Value**: Returns the modified `IServiceCollection` to allow for method chaining.
*   **Throws**: Throws `ArgumentNullException` if the `services` argument is null. May throw `InvalidOperationException` if the orchestrator has already been registered in the collection.

### AddSagaOrchestrator

An overloaded variant of the core registration method, typically accepting specific generic type constraints or detailed configuration delegates.

*   **Purpose**: Provides an alternative signature for registering the orchestrator, often used when specifying concrete saga types or custom middleware pipelines.
*   **Parameters**:
    *   `services`: The `IServiceCollection` to which the services are added.
    *   `configuration`: A delegate or specific type parameter defining the scope or behavior of the orchestrator.
*   **Return Value**: Returns the modified `IServiceCollection` to allow for method chaining.
*   **Throws**: Throws `ArgumentNullException` if the `services` argument is null. Throws `ArgumentException` if the provided configuration is invalid or conflicting.

### AddSagaRepositories

Registers the persistence layer implementations required for storing saga state and event logs.

*   **Purpose**: Adds the necessary repository interfaces and concrete implementations to ensure saga state durability across restarts and failures.
*   **Parameters**:
    *   `services`: The `IServiceCollection` to which the services are added.
*   **Return Value**: Returns the modified `IServiceCollection` to allow for method chaining.
*   **Throws**: Throws `ArgumentNullException` if the `services` argument is null.

### AddSagaServices

Registers auxiliary services and helpers utilized by the saga infrastructure, such as correlation ID generators, time providers, or notification dispatchers.

*   **Purpose**: Complements the core orchestrator and repositories by injecting supporting utilities required for full saga lifecycle management.
*   **Parameters**:
    *   `services`: The `IServiceCollection` to which the services are added.
*   **Return Value**: Returns the modified `IServiceCollection` to allow for method chaining.
*   **Throws**: Throws `ArgumentNullException` if the `services` argument is null.

## Usage

### Basic Registration
The following example demonstrates the standard setup where all components are registered sequentially during application startup.

```csharp
using Microsoft.Extensions.DependencyInjection;
using DotNetSagaOrchestrator.Configuration;

public class Program
{
    public static void Main(string[] args)
    {
        var services = new ServiceCollection();

        // Register core components
        services.AddSagaOrchestrator()
                .AddSagaRepositories()
                .AddSagaServices();

        var serviceProvider = services.BuildServiceProvider();
        
        // Resolve and use the orchestrator
        var orchestrator = serviceProvider.GetRequiredService<ISagaOrchestrator>();
    }
}
```

### Modular Configuration
In scenarios where only specific layers are required or when custom configuration options are passed to the orchestrator, methods can be called independently.

```csharp
using Microsoft.Extensions.DependencyInjection;
using DotNetSagaOrchestrator.Configuration;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register only the persistence layer for a read-only consumer
        services.AddSagaRepositories();

        // Register the orchestrator with specific timeout configurations
        services.AddSagaOrchestrator(options => 
        {
            options.DefaultTimeout = TimeSpan.FromSeconds(30);
        });

        // Register supporting utilities separately
        services.AddSagaServices();
    }
}
```

## Notes

*   **Registration Order**: While the methods return the `IServiceCollection` to support fluent chaining, the order of registration generally does not impact runtime resolution due to the nature of dependency injection containers. However, it is recommended to register repositories before the orchestrator to ensure dependencies are resolvable during container validation.
*   **Idempotency**: These methods are not idempotent. Calling `AddSagaOrchestrator` multiple times on the same `IServiceCollection` instance without distinct generic parameters or scopes may result in an `InvalidOperationException` or unintended service replacement depending on the underlying container implementation.
*   **Thread Safety**: The static methods within `ServiceConfiguration` are thread-safe for read operations. However, the `IServiceCollection` passed as an argument is not thread-safe for modification. All configuration calls must occur on a single thread during the application composition root setup, prior to building the `ServiceProvider`.
*   **Null Arguments**: All methods strictly enforce non-null arguments for the `services` parameter. Passing null will immediately terminate execution with an `ArgumentNullException`, failing fast to prevent silent configuration errors.
