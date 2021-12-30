# InfrastructureConfiguration

Static class that provides extension methods to register infrastructure services in the dependency injection container for the dotnet-saga-orchestrator project. These methods encapsulate the configuration of caching, event handling, integration, formatting, and background processing components required by the application.

## API

### `AddInfrastructureServices`

Registers core infrastructure services including event handling, integration, caching, formatting, and background workers. This method serves as a convenience to apply all infrastructure configurations at once.

- **Parameters**
  - `IServiceCollection services`: The service collection to configure.
  - `IConfiguration configuration`: Application configuration used to bind infrastructure settings.
- **Returns**
  - `IServiceCollection`: The configured service collection for method chaining.
- **Throws**
  - `ArgumentNullException`: If `services` or `configuration` is `null`.

### `AddCaching`

Registers distributed caching services using the configured cache provider (e.g., Redis). Configures cache policies and health checks.

- **Parameters**
  - `IServiceCollection services`: The service collection to configure.
  - `IConfiguration configuration`: Application configuration used to bind cache settings.
- **Returns**
  - `IServiceCollection`: The configured service collection for method chaining.
- **Throws**
  - `ArgumentNullException`: If `services` or `configuration` is `null`.

### `AddEventHandling`

Registers event bus and message handling infrastructure, including consumers, handlers, and event processors. Configures serialization and retry policies.

- **Parameters**
  - `IServiceCollection services`: The service collection to configure.
  - `IConfiguration configuration`: Application configuration used to bind event bus settings.
- **Returns**
  - `IServiceCollection`: The configured service collection for method chaining.
- **Throws**
  - `ArgumentNullException`: If `services` or `configuration` is `null`.

### `AddIntegration`

Registers integration services such as HTTP clients, external API connectors, and message transformers. Configures resilience and retry policies.

- **Parameters**
  - `IServiceCollection services`: The service collection to configure.
  - `IConfiguration configuration`: Application configuration used to bind integration settings.
- **Returns**
  - `IServiceCollection`: The configured service collection for method chaining.
- **Throws**
  - `ArgumentNullException`: If `services` or `configuration` is `null`.

### `AddFormatting`

Registers services for content formatting, serialization, and model binding. Includes JSON, XML, and custom formatter registrations.

- **Parameters**
  - `IServiceCollection services`: The service collection to configure.
- **Returns**
  - `IServiceCollection`: The configured service collection for method chaining.
- **Throws**
  - `ArgumentNullException`: If `services` is `null`.

### `AddBackgroundWorkers`

Registers hosted services and background task workers for long-running or periodic operations. Configures worker pools and lifecycle management.

- **Parameters**
  - `IServiceCollection services`: The service collection to configure.
  - `IConfiguration configuration`: Application configuration used to bind worker settings.
- **Returns**
  - `IServiceCollection`: The configured service collection for method chaining.
- **Throws**
  - `ArgumentNullException`: If `services` or `configuration` is `null`.

## Usage
