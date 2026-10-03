#nullable enable
using System;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>Thrown when a saga or step exceeds timeout.</summary>
public class SagaTimeoutException : SagaException
{
    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; init; }

    public SagaTimeoutException() : base() { }

    [JsonConstructor]
    public SagaTimeoutException(string message) : base(message) { }

    public SagaTimeoutException(string sagaId, int timeoutSeconds)
        : base($"Saga '{sagaId ?? throw new ArgumentNullException(nameof(sagaId))}' exceeded timeout of {(timeoutSeconds >= 0 ? timeoutSeconds : throw new ArgumentException("Timeout must be non-negative", nameof(timeoutSeconds)))} seconds.", sagaId, "SAGA_TIMEOUT")
    { TimeoutSeconds = timeoutSeconds; }

    public SagaTimeoutException(string sagaId, string stepName, int timeoutSeconds)
        : base($"Step '{stepName}' in saga '{sagaId ?? throw new ArgumentNullException(nameof(sagaId))}' exceeded timeout of {(timeoutSeconds >= 0 ? timeoutSeconds : throw new ArgumentException("Timeout must be non-negative", nameof(timeoutSeconds)))} seconds.", sagaId, "STEP_TIMEOUT")
    { TimeoutSeconds = timeoutSeconds; }
}
