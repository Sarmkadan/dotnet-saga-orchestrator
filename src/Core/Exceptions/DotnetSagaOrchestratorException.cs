#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Base exception for errors occurring within the saga orchestrator that are not covered by more specific exception types.
/// </summary>
public class DotnetSagaOrchestratorException : Exception
{
    public DotnetSagaOrchestratorException()
    {
    }

    public DotnetSagaOrchestratorException(string message)
        : base(message)
    {
    }

    public DotnetSagaOrchestratorException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
