#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Represents validation errors that are not covered by domain‑specific validation exceptions.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException()
    {
    }

    public ValidationException(string message)
        : base(message)
    {
    }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
