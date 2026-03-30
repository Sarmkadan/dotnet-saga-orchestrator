# Migration Guide: v1.x to v2.0

This document covers breaking changes and migration steps for upgrading from v1.x to v2.0 of the Distributed Saga Orchestrator.

## Breaking Changes

### 1. Default Port Changed from 80 to 8080

The container now listens on port **8080** instead of port 80. This follows the .NET 8+ convention of running containers as non-root users, where binding to ports below 1024 requires elevated privileges.

**Before (v1.x):**

```yaml
ports:
- "5000:80"
```

**After (v2.0):**

```yaml
ports:
- "8080:8080"
```

If you have a reverse proxy or load balancer pointing to the old port, update accordingly.

### 2. Docker Base Image Changed to `aspnet`

The runtime base image switched from `mcr.microsoft.com/dotnet/runtime:10.0` to `mcr.microsoft.com/dotnet/aspnet:10.0` to support the upcoming REST API endpoints and health check middleware.

### 3. Docker Compose Schema Version Removed

The `version: '3.8'` key has been removed from `docker-compose.yml` per the Compose Specification. Docker Compose v2+ ignores this field and emits a warning. No action needed unless you are running Compose v1 (EOL).

### 4. `ASPNETCORE_URLS` Environment Variable

The environment variable `ASPNETCORE_URLS` now defaults to `http://+:8080`. If you override this variable, ensure the port matches what you expose in your Dockerfile or Compose file.

## Migration Steps

### Step 1 - Update Docker Port Mappings

In your `docker-compose.yml` or `docker run` commands, change port mappings from `80` to `8080`:

```bash
# Old
docker run -p 5000:80 saga-orchestrator

# New
docker run -p 8080:8080 saga-orchestrator
```

### Step 2 - Update Health Check URLs

If you have external health checks configured (Kubernetes liveness/readiness probes, load balancer checks), update the port:

```yaml
# Kubernetes example
livenessProbe:
  httpGet:
    path: /health
    port: 8080
```

### Step 3 - Update NuGet Package Reference

```bash
dotnet add package Zaiets.dotnet.saga.orchestrator --version 2.0.0
```

### Step 4 - Rebuild Docker Images

```bash
docker compose build --no-cache
docker compose up -d
```

### Step 5 - Verify

```bash
curl http://localhost:8080/health
```

## New Features in v2.0

### Distributed Saga Debugger with Time-Travel Inspection

v2.0 introduces a powerful distributed saga debugger that enables time-travel debugging of saga execution flows. This feature allows you to:


- **Capture Debug Snapshots**: Automatic snapshots of saga state at key lifecycle events
- **Timeline Inspection**: Visual timeline showing saga execution history
- **State Rollback**: Reconstruct saga state at any point in time
- **Compensation Analysis**: Detailed view of compensation flows
- **Performance Profiling**: Identify bottlenecks and timeouts

#### Key Components

- `SagaDebuggerService` - Main debugger service
- `SagaDebugTimeline` - Timeline builder and analyzer
- `SagaDebugSnapshot` - State snapshots for rollback
- `ISagaDebugger` - Debugger interface

#### Usage Example

```csharp
var debugger = provider.GetRequiredService<ISagaDebugger>();

// Get timeline for a saga
var timeline = await debugger.GetTimelineAsync(sagaId);

// Inspect steps at specific time
var stepStates = timeline.GetStepStatesAt(DateTime.UtcNow.AddMinutes(-5));

// Rollback to specific state
var rolledBackSaga = await debugger.RollbackToAsync(sagaId, "2025-05-18T10:00:00Z");

// Analyze compensation flow
var compensationAnalysis = timeline.AnalyzeCompensationFlow();
```

### Enhanced Saga Visualization

Improved visualization of saga execution flows with:

- **Graph-based rendering** of saga definitions and execution paths
- **Color-coded status indicators** for easy identification
- **Interactive CLI commands** for exploring execution history
- **Export to DOT format** for integration with Graphviz

#### New CLI Commands

```bash
# Visualize saga definition
dotnet run -- visualize --definition <definition-id>

# Show execution timeline
dotnet run -- timeline --saga <saga-id>

# Export to Graphviz format
dotnet run -- export --saga <saga-id> --format dot --output saga.dot
```

### Improved Compensation Strategies

New compensation strategy options and enhancements:

- **Parallel compensation** for independent steps
- **Selective compensation** from failure point
- **Manual intervention** mode with pause/resume
- **Compensation timeout** configuration

#### Strategy Examples

```csharp
// Parallel compensation for independent steps
var saga = await orchestration.CreateSagaAsync(
    definition,
    compensationStrategy: CompensationStrategy.Parallel
);

// Selective compensation from failure point
var saga = await orchestration.CreateSagaAsync(
    definition,
    compensationStrategy: CompensationStrategy.FromFailurePoint
);

// Manual intervention mode
await orchestration.PauseSagaAsync(saga.Id);
// Manual compensation steps...
await orchestration.ResumeSagaAsync(saga.Id);
```

### Enhanced Metrics and Monitoring

New metrics endpoints and dashboard integration:

- **Prometheus metrics exporter**
- **Grafana dashboard templates**
- **Real-time metrics** via CLI
- **Custom metric hooks** for integration

#### Metrics Endpoints

```bash
# Get all metrics
curl http://localhost:8080/metrics

# Get specific metrics
curl http://localhost:8080/metrics/sagas
curl http://localhost:8080/metrics/steps
curl http://localhost:8080/metrics/compensations
```

### Advanced Configuration Options

New configuration options for fine-tuning:

- **Step-level timeouts** with millisecond precision
- **Custom retry policies** per step
- **Circuit breaker configuration** per service
- **Rate limiting** per service endpoint
- **Cache TTL** configuration

#### Configuration Example

```csharp
services.AddSagaOrchestrator()
    .WithDefaultStepTimeout(60) // 60 seconds
    .WithStepTimeout("payment-service", 90) // Service-specific timeout
    .WithRetryPolicy("inventory-service", new RetryPolicy
    {
        MaxRetries = 5,
        InitialDelayMs = 100,
        BackoffMultiplier = 2.0,
        MaxDelayMs = 30000
    })
    .WithCircuitBreaker("payment-service", 
        failureThreshold: 5,
        timeoutSeconds: 30)
    .WithRateLimit("shipping-service", requestsPerSecond: 20)
    .Build();
```

### New Background Workers

Enhanced background workers with better configuration:

- **Compensation worker** with configurable interval
- **Timeout worker** with adaptive polling
- **Event processing worker** with batch processing
- **Health check worker** for periodic monitoring

#### Worker Configuration

```csharp
services.AddSagaOrchestrator()
    .WithCompensationWorker(
        enabled: true,
        checkIntervalSeconds: 10,
        batchSize: 50
    )
    .WithTimeoutWorker(
        enabled: true,
        checkIntervalSeconds: 15,
        adaptive: true
    )
    .WithEventProcessingWorker(
        enabled: true,
        batchSize: 100,
        maxWaitSeconds: 5
    )
    .Build();
```

## Configuration Changes

### Environment Variables

| Variable | v1.x Default | v2.0 Default | Description |
|----------|---------------|--------------|-------------|
| `SAGA_PORT` | `80` | `8080` | Container port |
| `ASPNETCORE_URLS` | `http://+:80` | `http://+:8080` | ASP.NET URLs |
| `SAGA_TIMEOUT_WORKER_INTERVAL` | `10` | `15` | Timeout worker interval |
| `SAGA_COMPENSATION_WORKER_INTERVAL` | `5` | `10` | Compensation worker interval |

### API Changes

#### New APIs in v2.0

```csharp
// Saga debugger
public interface ISagaDebugger
{
    Task<SagaDebugTimeline> GetTimelineAsync(string sagaId);
    Task<Saga> RollbackToAsync(string sagaId, string timestamp);
    Task<CompensationAnalysis> AnalyzeCompensationFlowAsync(string sagaId);
}

// Saga visualization
public interface ISagaVisualizationService
{
    Task<string> RenderDefinitionAsync(string definitionId);
    Task<string> RenderExecutionAsync(string sagaId);
    Task<string> ExportToDotAsync(string sagaId);
}

// Enhanced metrics
public interface IMetricsService
{
    Task<Dictionary<string, Metric>> GetAllMetricsAsync();
    Task<Metric> GetSagaMetricsAsync();
    Task<Metric> GetStepMetricsAsync();
    Task<Metric> GetCompensationMetricsAsync();
}

// Configuration builders
public class SagaConfigurationBuilder
{
    public SagaConfigurationBuilder WithStepTimeout(string serviceName, int timeoutSeconds);
    public SagaConfigurationBuilder WithRetryPolicy(string serviceName, RetryPolicy policy);
    public SagaConfigurationBuilder WithCircuitBreaker(string serviceName, int failureThreshold, int timeoutSeconds);
    public SagaConfigurationBuilder WithRateLimit(string serviceName, int requestsPerSecond);
}
```

#### Modified APIs in v2.0

```csharp
// SagaOrchestrationService now accepts compensationStrategy parameter
public Task<Saga> CreateSagaAsync(
    SagaDefinition definition,
    int maxRetries = 3,
    int timeoutSeconds = 300,
    CompensationStrategy compensationStrategy = CompensationStrategy.ReverseOrder
);

// New compensation strategy parameter
public Task<Saga> CompensateSagaAsync(
    string sagaId,
    CompensationStrategy strategy = CompensationStrategy.ReverseOrder,
    int timeoutSeconds = 300
);
```

## Code Examples: Old vs New API

### Example 1: Creating a Saga

**v1.x:**
```csharp
var saga = await orchestration.CreateSagaAsync(definition);
await orchestration.StartSagaAsync(saga.Id);
```

**v2.0:**
```csharp
// With default compensation strategy (ReverseOrder)
var saga = await orchestration.CreateSagaAsync(definition);

// Or with explicit strategy
var saga = await orchestration.CreateSagaAsync(
    definition,
    compensationStrategy: CompensationStrategy.Parallel
);

await orchestration.StartSagaAsync(saga.Id);
```

### Example 2: Compensating a Saga

**v1.x:**
```csharp
await orchestration.CompensateSagaAsync(saga.Id);
```

**v2.0:**
```csharp
// Default strategy (ReverseOrder)
await orchestration.CompensateSagaAsync(saga.Id);

// Explicit strategy
await orchestration.CompensateSagaAsync(
    saga.Id,
    strategy: CompensationStrategy.FromFailurePoint
);

// With timeout
await orchestration.CompensateSagaAsync(
    saga.Id,
    strategy: CompensationStrategy.Parallel,
    timeoutSeconds: 600
);
```

### Example 3: Advanced Configuration

**v1.x:**
```csharp
services.AddSagaOrchestrator()
    .WithDefaultStepTimeout(30)
    .WithDefaultMaxRetries(3)
    .Build();
```

**v2.0:**
```csharp
services.AddSagaOrchestrator()
    .WithDefaultStepTimeout(60) // Increased from 30
    .WithDefaultMaxRetries(5) // Increased from 3
    .WithStepTimeout("payment-service", 90) // Service-specific
    .WithRetryPolicy("inventory-service", new RetryPolicy
    {
        MaxRetries = 5,
        InitialDelayMs = 100,
        BackoffMultiplier = 2.0
    })
    .WithCircuitBreaker("payment-service", 5, 30)
    .WithRateLimit("shipping-service", 20)
    .Build();
```

## Migration Checklist

- [ ] Update Docker port mappings from `80` to `8080`
- [ ] Update health check endpoints to use port `8080`
- [ ] Update any hardcoded URLs from `:80` to `:8080`
- [ ] Update Kubernetes/Helm charts with new port
- [ ] Update load balancer/ingress rules
- [ ] Update monitoring dashboards and alerts
- [ ] Update CI/CD pipelines with new port
- [ ] Test health checks and readiness probes
- [ ] Verify saga execution flows
- [ ] Test compensation strategies
- [ ] Validate metrics collection
- [ ] Update documentation with new endpoints

## Troubleshooting

### Port Binding Errors

**Error:** `Failed to bind to address http://[::]:80`

**Solution:** Ensure you're not running as root, or update to use port 8080:
```bash
docker run -p 8080:8080 saga-orchestrator
```

### Health Check Failures

**Error:** Health check returns 503

**Solution:** Wait for container to fully start (v2.0 has longer startup time):
```bash
sleep 15 && curl http://localhost:8080/health
```

### API Compatibility Issues

**Error:** `Method not found: CreateSagaAsync`

**Solution:** Update to v2.0 API signatures. Check breaking changes in method signatures.

## Rollback Plan

If issues are encountered, rollback to v1.x:

```bash
# Update package
cd your-project
dotnet add package Zaiets.dotnet.saga.orchestrator --version 1.0.0

# Update docker-compose.yml
sed -i 's/:8080/:80/g' docker-compose.yml
sed -i 's/8080:8080/5000:80/g' docker-compose.yml

# Rebuild and redeploy
docker compose build
docker compose up -d
```

## Support

For migration assistance, please:
- Check the [GitHub Discussions](https://github.com/sarmkadan/dotnet-saga-orchestrator/discussions)
- Review [GitHub Issues](https://github.com/sarmkadan/dotnet-saga-orchestrator/issues)
- Consult the [v2.0 documentation](https://github.com/sarmkadan/dotnet-saga-orchestrator/tree/main/docs)

## Version Compatibility Matrix


| v1.x | v2.0 | Compatible |
|-------|-------|------------|
| 1.0.0 | 2.0.0 | ✅ Yes |
| 1.1.0 | 2.0.0 | ✅ Yes |
| 1.2.0 | 2.0.0 | ✅ Yes |
| 1.3.0 | 2.0.0 | ✅ Yes |

All v1.x versions are compatible with v2.0 with the migration steps above.
