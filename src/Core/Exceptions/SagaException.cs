// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Base exception for all saga orchestration errors.
/// </summary>
public class SagaException : Exception
{
    public string? SagaId { get; }
    public string? ErrorCode { get; }

    public SagaException(string message) : base(message)
    {
    }

    public SagaException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public SagaException(string message, string sagaId) : base(message)
    {
        SagaId = sagaId;
    }

    public SagaException(string message, string sagaId, string errorCode) : base(message)
    {
        SagaId = sagaId;
        ErrorCode = errorCode;
    }

    public SagaException(string message, string sagaId, string errorCode, Exception? innerException)
        : base(message, innerException)
    {
        SagaId = sagaId;
        ErrorCode = errorCode;
    }
}
