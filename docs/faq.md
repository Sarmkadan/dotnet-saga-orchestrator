# Frequently Asked Questions

Common questions and answers about the Saga Orchestrator.

## General Questions

### What is the Saga pattern?

The Saga pattern is a way to manage distributed transactions across multiple microservices. Instead of using traditional ACID transactions, sagas coordinate business logic across services and use compensating transactions to handle failures.

### When should I use the Saga Orchestrator?

Use the Saga Orchestrator when:
- You need to coordinate transactions across multiple microservices
- You require guaranteed consistency without distributed locks
- You want automatic retry and timeout handling
- You need audit trails and compensation capabilities

### Is this production-ready?

Yes. Phase 1 and Phase 2 are complete with:
- Full domain model and services
- Comprehensive infrastructure
- Exception handling and logging
- Metrics and monitoring
- 8,000+ lines of tested code

Phase 4 (database persistence) will add enterprise-scale reliability.

### What's the difference between Saga and traditional transactions?

| Aspect | Saga | Transaction |
|--------|------|-------------|
| Scope | Multiple services | Single database |
| Consistency | Eventual | Immediate |
| Rollback | Compensation | Rollback |
| Complexity | Higher | Lower |
| Scalability | Better | Limited |

## Installation & Setup

### Which .NET version is required?

.NET 10.0 or later. The project uses the latest C# language features.

### Can I use it with .NET 6, 7, or 8?

Not directly. However, the code is compatible and could be adapted. We recommend upgrading to .NET 10 for security and performance improvements.

### How do I add it to my project?

```csharp
// Method 1: Clone repository
git clone https://github.com/Sarmkadan/dotnet-saga-orchestrator.git
dotnet add reference ../dotnet-saga-orchestrator/dotnet-saga-orchestrator.csproj

// Method 2: NuGet (coming in Phase 4)
dotnet package add SagaOrchestrator
```

### What are the system requirements?

- 256 MB RAM minimum
- 50 MB disk space
- .NET 10 SDK (for development)
- .NET 10 runtime (for deployment)

## Usage Questions

### How many steps can a saga have?

No hard limit. In-memory implementation supports thousands. Database implementation (Phase 4) will support millions.

### What's the maximum timeout?

No technical limit. Set based on your requirements:
- Typical sagas: 5-10 minutes
- Long-running: 1-24 hours
- Very long: Configure as needed

### Can steps run in parallel?

The current implementation executes steps sequentially. Parallel execution with dependency resolution is planned for Phase 5.

### How do I pass data between steps?

Use the `Metadata` field on saga or steps:

```csharp
saga.Metadata = JsonConvert.SerializeObject(new { orderId = 123 });

var metadata = JsonConvert.DeserializeObject<dynamic>(saga.Metadata);
var orderId = metadata.orderId;
```

### Can I modify a saga after it starts?

No. Once a saga starts, it follows the definition. To change logic, create a new saga definition.

### How do I handle saga timeouts?

The system automatically:
1. Detects timeout via `SagaTimeoutWorker`
2. Marks saga as `TimedOut`
3. Initiates compensation

Or manually:

```csharp
if (saga.Status == SagaStatus.TimedOut)
{
    await orchestration.CompensateSagaAsync(saga.Id);
}
```

### How do I retry a failed step?

Failed sagas automatically retry based on retry policy. Manual retry:

```csharp
// Reset step and retry
var saga = await orchestration.GetSagaAsync(sagaId);
var failedStep = saga.Steps.First(s => s.Status == SagaStepStatus.Failed);

// Re-execute through compensation then restart
await orchestration.CompensateSagaAsync(saga.Id);
var newSaga = await orchestration.CreateSagaAsync(definition);
```

### What happens if a step timeout expires?

1. Step is marked as `TimedOut`
2. Saga is marked as `Failed`
3. Compensation is automatically triggered
4. Subsequent steps are skipped

### Can I abort a saga?

Yes:

```csharp
await orchestration.AbortSagaAsync(sagaId, "User requested cancellation");
```

This immediately stops execution and triggers compensation.

## Compensation Questions

### How do I define compensation?

When creating a step definition:

```csharp
var step = new SagaStepDefinition(
    "Reserve Inventory",
    "inventory-service",
    "http://inventory/reserve",        // Execution
    "http://inventory/release");       // Compensation
```

### What's the difference between compensation strategies?

- **ReverseOrder (LIFO)**: Undo in reverse order (default, most common)
- **ForwardOrder (FIFO)**: Undo in execution order
- **FromFailurePoint**: Undo only failed step and subsequent
- **Parallel**: Undo all concurrently
- **Manual**: Require human approval

### What if compensation fails?

The system:
1. Marks compensation as `PartiallyFailed`
2. Retries based on policy
3. Eventually requires manual intervention

### Can compensation itself fail?

Yes. If a compensation step fails:
- It's retried automatically
- After max retries, it requires manual intervention
- Use `ManualIntervention` strategy for complex rollbacks

### How do I implement idempotent compensation?

Ensure compensation is idempotent:

```csharp
// Compensation endpoint
[HttpPost("/release")]
public async Task Release([FromBody] ReleaseRequest request)
{
    var reservation = await _db.Reservations
        .FirstOrDefaultAsync(r => r.Id == request.Id);
    
    if (reservation?.Status == "released")
        return Ok(); // Already compensated
    
    await _db.SaveChangesAsync();
    return Ok();
}
```

## Performance Questions

### How fast can it process sagas?

Depends on:
- Step execution time (network latency)
- Retry policies
- System resources
- Database performance (Phase 4)

Typical: 100-1000 sagas/minute on t3.medium instance.

### Does caching improve performance?

Yes, significantly. Enable it:

```csharp
services.AddCaching(ttlMinutes: 10);
```

Can improve retrieval speed by 10-100x.

### How much memory does it use?

- Idle: ~50 MB
- Per saga: ~10 KB
- Per step: ~5 KB
- 1000 sagas: ~150 MB

Tune cache settings if memory is limited.

### What's the throughput capacity?

Single instance (t3.medium):
- 50-100 concurrent sagas
- 1000+ sagas total
- With database (Phase 4): 10,000+ sagas

### How do I scale horizontally?

1. Use database instead of in-memory (Phase 4)
2. Use distributed cache (Redis)
3. Set up load balancer
4. Deploy multiple instances

## Reliability Questions

### What happens on application restart?

In-memory implementation:
- All sagas are lost
- Not suitable for production

Phase 4 (database):
- Sagas persist
- Can resume from last known state
- Automatic recovery

### How do I ensure saga execution isn't lost?

Use database persistence (Phase 4):

```csharp
services.AddSagaOrchestrator()
    .UseDatabase("Server=...;Database=SagaOrchestrator");
```

### What's the recovery mechanism?

The system:
1. Identifies failed sagas on startup
2. Checks if compensation was initiated
3. Resumes from last successful step
4. Marks sagas that timed out as `TimedOut`

### Can I run multiple instances?

Yes, with proper setup:
- Shared database (Phase 4)
- Distributed cache (Redis)
- Stateless services
- Load balancer coordination

### How do I prevent duplicate saga execution?

- Use correlation IDs
- Implement idempotency at service level
- Check `exists before create` in microservices

## Monitoring Questions

### How do I monitor saga execution?

Three ways:

```csharp
// 1. Direct polling
var saga = await orchestration.GetSagaAsync(sagaId);
logger.LogInformation($"Status: {saga.Status}");

// 2. Event subscription
eventBus.Subscribe<SagaCompletedEvent>(async @event =>
{
    logger.LogInformation($"Saga {event.SagaId} completed");
});

// 3. Metrics
var metrics = metricsService.GetMetrics();
logger.LogInformation($"Success rate: {metrics.SuccessRate:P2}");
```

### What metrics are available?

- Total sagas created
- Success/failure counts
- Success rate
- Average duration
- Percentile latencies (P50, P95, P99)
- Per-step metrics

### How do I set up health checks?

```bash
curl http://localhost:5000/health

# Response
{
  "status": "Healthy",
  "activeSagas": 42,
  "uptime": "5h 30m"
}
```

### Can I export metrics?

Yes, in Phase 4:

```csharp
var metrics = metricsService.GetMetrics();
await metricsService.ExportPrometheusAsync();
```

### How do I set up alerting?

Check metrics regularly:

```csharp
var metrics = metricsService.GetMetrics();

if (metrics.FailureRate > 0.05)  // > 5%
{
    await alertingService.SendAlertAsync(
        "High saga failure rate",
        severity: AlertSeverity.Critical);
}
```

## Troubleshooting Questions

### Why are sagas stuck in "Running"?

1. Check if service is responding
2. Verify network connectivity
3. Check step timeout:

```csharp
var saga = await orchestration.GetSagaAsync(sagaId);
var runningStep = saga.Steps.First(s => s.Status == SagaStepStatus.Running);
logger.LogInformation($"Running since: {runningStep.StartedAt}");
```

### Why don't steps execute?

Check saga status:

```csharp
var saga = await orchestration.GetSagaAsync(sagaId);

if (saga.Status != SagaStatus.Running)
{
    logger.LogError($"Saga not running: {saga.Status}");
    await orchestration.StartSagaAsync(saga.Id);
}
```

### Why doesn't compensation trigger?

Check failure conditions:

```csharp
var saga = await orchestration.GetSagaAsync(sagaId);

if (saga.Status != SagaStatus.Failed)
{
    logger.LogInformation($"Cannot compensate: saga is {saga.Status}");
}

// Only Failed sagas can be compensated
if (saga.Status == SagaStatus.Failed)
{
    await orchestration.CompensateSagaAsync(saga.Id);
}
```

### Why are retries not working?

Verify retry policy:

```csharp
var step = sagaDefinition.Steps.First();
logger.LogInformation($"Max retries: {step.RetryPolicy.MaxRetries}");
logger.LogInformation($"Delay: {step.RetryPolicy.InitialDelayMs}ms");
```

### How do I debug failed steps?

Enable detailed logging:

```csharp
services.AddLogging(config =>
{
    config.SetMinimumLevel(LogLevel.Debug);
    config.AddConsole();
});
```

Check step details:

```csharp
var saga = await orchestration.GetSagaAsync(sagaId);
var failedStep = saga.Steps.First(s => s.Status == SagaStepStatus.Failed);
logger.LogInformation($"Error: {failedStep.ErrorMessage}");
logger.LogInformation($"Retries: {failedStep.RetryCount}");
```

## Integration Questions

### How do I integrate with my microservices?

Each step calls your service endpoints:

```csharp
var step = new SagaStepDefinition(
    "Reserve Inventory",
    "inventory-service",
    "http://inventory-service:5001/api/reserve",  // Your endpoint
    "http://inventory-service:5001/api/release");

step.SetTimeout(30);
await definitionService.AddStepAsync(defId, step);
```

Your service must:
1. Implement both execution and compensation endpoints
2. Be idempotent
3. Return success/failure
4. Handle timeouts gracefully

### Can I use HTTPS?

Yes, all HTTP calls support HTTPS:

```csharp
var step = new SagaStepDefinition(
    "Process Payment",
    "payment-service",
    "https://api.payment.com/charge",  // HTTPS
    "https://api.payment.com/refund");
```

### How do I add authentication headers?

Configure the HTTP factory:

```csharp
var httpClient = httpClientFactory.CreateClient("payment-service");
httpClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);
```

### Can I use message queues instead of HTTP?

Not in Phase 1-2. Phase 5 will include RabbitMQ/Kafka support.

## Contributing Questions

### How can I contribute?

1. Fork the repository
2. Create a feature branch
3. Make changes following code style
4. Add tests
5. Create pull request

### What's the code style?

- C# conventions
- PascalCase for public members
- camelCase for private
- 4 spaces indentation
- XML documentation

### How do I run tests?

```bash
dotnet test
```

### Is there a contribution guide?

See `CONTRIBUTING.md` in repository root.

## License Questions

### What license is this?

MIT License - Copyright (c) 2026 Vladyslav Zaiets

### Can I use it commercially?

Yes, fully. MIT license allows commercial use.

### What are my obligations?

- Include license notice
- Include copyright notice
- Provide source code to modifications (if required by local law)

### Can I modify it?

Yes, fully. MIT allows modifications.

## Contact & Support

### How do I report bugs?

Create an issue on GitHub:
https://github.com/Sarmkadan/dotnet-saga-orchestrator/issues

### Where can I ask questions?

- GitHub Discussions
- Email: rutova2@gmail.com
- Website: https://sarmkadan.com

### Is there professional support?

Not yet, but consulting is available through sarmkadan.com

### How do I stay updated?

- Watch the GitHub repository
- Follow updates on website
- Subscribe to releases
