#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="ErrorResponse"/> instances.
/// Ensures that error responses contain valid data before being returned to clients.
/// </summary>
public static class ExceptionMapperValidation
{
    /// <summary>
    /// Validates that an <see cref="ErrorResponse"/> contains valid data.
    /// </summary>
    /// <param name="value">The error response to validate.</param>
    /// <returns>A list of validation problems (empty if valid).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ErrorResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Code))
        {
            problems.Add("ErrorResponse.Code must not be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("ErrorResponse.Message must not be null or whitespace.");
        }

        if (value.Timestamp == default)
        {
            problems.Add("ErrorResponse.Timestamp must be a valid UTC date.");
        }

        if (string.IsNullOrWhiteSpace(value.RequestId))
        {
            problems.Add("ErrorResponse.RequestId must not be null or whitespace.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an <see cref="ErrorResponse"/> contains valid data.
    /// </summary>
    /// <param name="value">The error response to check.</param>
    /// <returns>True if the error response is valid; otherwise, false.</returns>
    public static bool IsValid(this ErrorResponse value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an <see cref="ErrorResponse"/> contains valid data.
    /// </summary>
    /// <param name="value">The error response to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the error response contains validation problems.</exception>
    public static void EnsureValid(this ErrorResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "ErrorResponse validation failed. " +
                string.Join(" ", problems),
                nameof(value));
        }
    }
}
