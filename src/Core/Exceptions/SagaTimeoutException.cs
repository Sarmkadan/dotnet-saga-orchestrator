#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Thrown when a saga or saga step execution exceeds the configured timeout.
/// </summary>
public class SagaTimeoutException : SagaException
{
    public int TimeoutSeconds { get; }

    public SagaTimeoutException(string sagaId, int timeoutSeconds)
        : base($"Saga '{sagaId}' exceeded timeout of {timeoutSeconds} seconds.", sagaId, "SAGA_TIMEOUT")
    {
        TimeoutSeconds = timeoutSeconds;
    }

    public SagaTimeoutException(string sagaId, string stepName, int timeoutSeconds)
        : base($"Step '{stepName}' in saga '{sagaId}' exceeded timeout of {timeoutSeconds} seconds.",
               sagaId, "STEP_TIMEOUT")
    {
        TimeoutSeconds = timeoutSeconds;
    }
}
