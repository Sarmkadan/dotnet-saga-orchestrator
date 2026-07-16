// ... existing content ...

## SagaIdGenerator

The `SagaIdGenerator` class provides a set of utility methods for generating and validating unique identifiers used in saga workflows. These identifiers are essential for tracking sagas, steps, correlations, and requests.

### Usage Example

```csharp
using SagaOrchestrator.Core.Utilities;

var sagaId = SagaIdGenerator.GenerateSagaId();
var correlationId = SagaIdGenerator.GenerateCorrelationId();
var stepId = SagaIdGenerator.GenerateStepId();
var traceId = SagaIdGenerator.GenerateTraceId();
var requestId = SagaIdGenerator.GenerateRequestId();

Console.WriteLine($"Saga ID: {sagaId}"); // e.g. saga_xxxxxxxxxxxx
Console.WriteLine($"Correlation ID: {correlationId}"); // e.g. corr_xxxxxxxxxxxx or xxxxxxxxxxxx
Console.WriteLine($"Step ID: {stepId}"); // e.g. step_xxxxxxxxxxxx
Console.WriteLine($"Trace ID: {traceId}"); // e.g. trace_xxxxxxxxxxxx
Console.WriteLine($"Request ID: {requestId}"); // e.g. req_xxxxxxxx_xxxx

bool isValidSaga = SagaIdGenerator.IsValidSagaId(sagaId);
bool isValidCorrelation = SagaIdGenerator.IsValidCorrelationId(correlationId);

Console.WriteLine($"Is valid saga ID: {isValidSaga}"); // True
Console.WriteLine($"Is valid correlation ID: {isValidCorrelation}"); // True
```
