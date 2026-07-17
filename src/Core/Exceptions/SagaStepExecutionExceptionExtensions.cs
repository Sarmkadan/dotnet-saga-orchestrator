#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="SagaStepExecutionException"/> to enhance error handling and diagnostics
/// for saga step execution failures with structured error information and retry analysis.
/// </summary>
public static class SagaStepExecutionExceptionExtensions
{
    /// <summary>
    /// Creates a formatted error message suitable for logging or user display that includes
    /// the step name, step order, saga ID, and the original exception message.
    /// </summary>
    /// <param name="exception">The saga step execution exception.</param>
    /// <returns>A formatted error message.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static string ToErrorMessage(this SagaStepExecutionException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return $"Saga step execution failed: Step '{exception.StepName}' (order {exception.StepOrder}) in saga '{exception.SagaId}'. {exception.Message}";
    }

    /// <summary>
    /// Determines whether this exception represents a retryable failure based on the exception type
    /// or message content. Useful for deciding if a saga step should be retried.
    /// </summary>
    /// <param name="exception">The saga step execution exception.</param>
    /// <returns>True if the failure is retryable; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static bool IsRetryable(this SagaStepExecutionException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        // Check for common retryable exceptions in the inner exception chain
        Exception? current = exception.InnerException;
        while (current != null)
        {
            Type type = current.GetType();
            string typeName = type.FullName ?? type.Name;

            // Common retryable exceptions - use pattern matching for better readability
            if (typeName.Contains("TimeoutException", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("TransientException", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("RetryableException", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("SqlException", StringComparison.OrdinalIgnoreCase) ||
                typeName.Contains("HttpRequestException", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            current = current.InnerException;
        }

        // Check message for retry-related keywords
        string message = exception.Message ?? string.Empty;
        return message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("retry", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("transient", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates a dictionary containing structured data about the failed step that can be used
    /// for logging, monitoring, or error tracking systems.
    /// </summary>
    /// <param name="exception">The saga step execution exception.</param>
    /// <returns>A dictionary with structured error data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static IReadOnlyDictionary<string, object> ToErrorContext(this SagaStepExecutionException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var context = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["stepName"] = exception.StepName ?? string.Empty,
            ["stepOrder"] = exception.StepOrder,
            ["sagaId"] = exception.SagaId ?? string.Empty,
            ["errorType"] = "SagaStepExecutionFailed",
            ["errorCode"] = "STEP_EXECUTION_FAILED",
            ["message"] = exception.Message ?? string.Empty,
            ["timestamp"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)
        };

        if (exception.InnerException != null)
        {
            context["innerExceptionType"] = exception.InnerException.GetType().FullName ?? exception.InnerException.GetType().Name;
            context["innerExceptionMessage"] = exception.InnerException.Message ?? string.Empty;
        }

        return context;
    }

    /// <summary>
    /// Creates a simplified representation of the exception suitable for telemetry systems
    /// that track saga execution metrics.
    /// </summary>
    /// <param name="exception">The saga step execution exception.</param>
    /// <returns>A tuple containing step name, step order, and error code.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static (string StepName, int StepOrder, string ErrorCode) ToTelemetryKey(this SagaStepExecutionException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return (exception.StepName ?? string.Empty, exception.StepOrder, "STEP_EXECUTION_FAILED");
    }
}