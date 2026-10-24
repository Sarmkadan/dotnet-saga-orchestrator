#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Maps domain exceptions to HTTP status codes and error responses.
/// Provides consistent error handling across the application.
/// </summary>
public static class ExceptionMapper
{
    public static (HttpStatusCode statusCode, string message) MapException(Exception ex)
    {
        return ex switch
        {
            SagaNotFoundException => (HttpStatusCode.NotFound, "The requested saga was not found"),
            SagaTimeoutException => (HttpStatusCode.RequestTimeout, "Saga execution timed out"),
            InvalidSagaDefinitionException => (HttpStatusCode.BadRequest, "The saga definition is invalid"),
            SagaStepExecutionException => (HttpStatusCode.InternalServerError, "A saga step failed during execution"),
            ConfigurationException => (HttpStatusCode.BadRequest, "Configuration error"),
            ValidationException => (HttpStatusCode.BadRequest, "Validation error"),
            DotnetSagaOrchestratorException => (HttpStatusCode.InternalServerError, "An internal orchestrator error occurred"),
            SagaException => (HttpStatusCode.InternalServerError, "An unexpected error occurred"),
            ArgumentNullException => (HttpStatusCode.BadRequest, "A required parameter is missing"),
            ArgumentOutOfRangeException => (HttpStatusCode.BadRequest, "Argument out of range"),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid argument provided"),
            InvalidOperationException => (HttpStatusCode.BadRequest, "Invalid operation"),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };
    }

    public static bool IsSagaException(Exception ex) =>
        ex is SagaException;

    public static bool IsValidationError(Exception ex) =>
        ex is ArgumentException || ex is ArgumentNullException || ex is ArgumentOutOfRangeException;

    public static bool IsNotFound(Exception ex) =>
        ex is SagaNotFoundException;

    public static bool IsTimeout(Exception ex) =>
        ex is SagaTimeoutException;

    /// <summary>
    /// Determines whether the message of an exception may be returned to clients in
    /// <see cref="ErrorResponse.Details"/>. Only types whose messages are written for callers are allowlisted;
    /// every other exception (including unmapped ones) is reported without its message.
    /// </summary>
    /// <param name="ex">The exception to test.</param>
    /// <returns>True if the exception message is client-safe; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
    internal static bool ExposesMessage(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        return ex is ArgumentException or ValidationException or InvalidSagaDefinitionException or SagaNotFoundException;
    }

    public static string GetErrorCode(Exception ex) => ex switch
    {
        SagaNotFoundException => "SAGA_NOT_FOUND",
        SagaTimeoutException => "SAGA_TIMEOUT",
        InvalidSagaDefinitionException => "INVALID_DEFINITION",
        SagaStepExecutionException => "STEP_EXECUTION_FAILED",
        ConfigurationException => "CONFIG_ERROR",
        ValidationException => "VALIDATION_ERROR",
        DotnetSagaOrchestratorException => "ORCHESTRATOR_ERROR",
        SagaException => "ORCHESTRATOR_ERROR",
        ArgumentNullException => "ARGUMENT_NULL",
        ArgumentOutOfRangeException => "ARGUMENT_OUT_OF_RANGE",
        ArgumentException => "INVALID_ARGUMENT",
        InvalidOperationException => "INVALID_OPERATION",
        _ => "INTERNAL_ERROR"
    };
}

/// <summary>
/// Client-facing error payload. Built only from sanitized data: the internal exception message
/// is copied into <see cref="Details"/> only for exception types that are designed to carry
/// client-safe messages, and stack traces, inner exceptions and other internals are never included.
/// </summary>
public class ErrorResponse
{
    /// <summary>Stable, machine-readable error code.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Generic, client-safe description of the failure.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional client-safe detail, or <c>null</c> when the exception type does not expose its message.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Correlation identifier for support lookup. Matches the identifier under which the full exception
    /// is logged server-side.
    /// </summary>
    public string RequestId { get; set; } = string.Empty;

    /// <summary>UTC instant at which the error response was created.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Creates a sanitized error response from an exception.
    /// </summary>
    /// <param name="ex">The exception to convert.</param>
    /// <param name="requestId">
    /// Correlation identifier, typically the request trace identifier. A new identifier is generated
    /// when <paramref name="requestId"/> is null or whitespace.
    /// </param>
    /// <returns>An <see cref="ErrorResponse"/> that contains no internal exception message, stack trace or inner details.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="ex"/> is null.</exception>
    public static ErrorResponse FromException(Exception ex, string? requestId = null)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var (_, message) = ExceptionMapper.MapException(ex);
        return new ErrorResponse
        {
            Code = ExceptionMapper.GetErrorCode(ex),
            Message = message,
            Details = ExceptionMapper.ExposesMessage(ex) ? ex.Message : null,
            RequestId = string.IsNullOrWhiteSpace(requestId) ? SagaIdGenerator.GenerateRequestId() : requestId,
            Timestamp = DateTime.UtcNow
        };
    }
}
