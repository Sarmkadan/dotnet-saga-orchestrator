#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Infrastructure.Logging;

/// <summary>
/// Pairs sanitized <see cref="ErrorResponse"/> creation with server-side logging of the full exception.
/// </summary>
public static class ErrorResponseLoggingExtensions
{
    /// <summary>
    /// Creates a sanitized <see cref="ErrorResponse"/> for an exception. For server-side failures
    /// (HTTP 5xx) the full exception, including its message and stack trace, is logged under the same
    /// correlation identifier that is returned to the client.
    /// </summary>
    /// <param name="logger">The logger that receives the full exception detail.</param>
    /// <param name="exception">The exception to convert and, when it is a server-side failure, log.</param>
    /// <param name="requestId">
    /// Correlation identifier, typically the request trace identifier. A new identifier is generated
    /// when <paramref name="requestId"/> is null or whitespace.
    /// </param>
    /// <returns>A client-safe <see cref="ErrorResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> or <paramref name="exception"/> is null.</exception>
    public static ErrorResponse ToErrorResponseAndLog(this ILogger logger, Exception exception, string? requestId = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(exception);

        var response = ErrorResponse.FromException(exception, requestId);
        var (statusCode, _) = ExceptionMapper.MapException(exception);

        if (statusCode >= HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Request {RequestId} failed with {ErrorCode}", response.RequestId, response.Code);
        }

        return response;
    }
}
