// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Thrown when a saga with the specified ID is not found.
/// </summary>
public class SagaNotFoundException : SagaException
{
    public SagaNotFoundException(string sagaId)
        : base($"Saga '{sagaId}' not found.", sagaId, "SAGA_NOT_FOUND")
    {
    }

    public SagaNotFoundException(string message, string sagaId)
        : base(message, sagaId, "SAGA_NOT_FOUND")
    {
    }
}
