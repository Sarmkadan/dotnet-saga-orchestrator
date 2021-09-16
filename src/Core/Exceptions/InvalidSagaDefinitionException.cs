// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Thrown when a saga definition is invalid or malformed.
/// </summary>
public class InvalidSagaDefinitionException : SagaException
{
    public InvalidSagaDefinitionException(string message)
        : base(message)
    {
    }

    public InvalidSagaDefinitionException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    public InvalidSagaDefinitionException(string definitionId, string message)
        : base($"Saga definition '{definitionId}' is invalid: {message}", definitionId, "INVALID_SAGA_DEFINITION")
    {
    }
}
