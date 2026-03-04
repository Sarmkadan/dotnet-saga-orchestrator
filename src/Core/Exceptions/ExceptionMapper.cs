#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;

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
            ArgumentNullException => (HttpStatusCode.BadRequest, "A required parameter is missing"),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid argument provided"),
            ArgumentOutOfRangeException => (HttpStatusCode.BadRequest, "Argument out of range"),
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

    public static string GetErrorCode(Exception ex) => ex switch
    {
        SagaNotFoundException => "SAGA_NOT_FOUND",
        SagaTimeoutException => "SAGA_TIMEOUT",
        InvalidSagaDefinitionException => "INVALID_DEFINITION",
        SagaStepExecutionException => "STEP_EXECUTION_FAILED",
        ArgumentNullException => "ARGUMENT_NULL",
        ArgumentException => "INVALID_ARGUMENT",
        ArgumentOutOfRangeException => "ARGUMENT_OUT_OF_RANGE",
        InvalidOperationException => "INVALID_OPERATION",
        _ => "INTERNAL_ERROR"
    };
}

public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ErrorResponse FromException(Exception ex, string requestId = "")
    {
        var (statusCode, message) = ExceptionMapper.MapException(ex);
        return new ErrorResponse
        {
            Code = ExceptionMapper.GetErrorCode(ex),
            Message = message,
            Details = ex.Message,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };
    }
}
