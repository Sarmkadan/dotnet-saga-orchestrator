#nullable enable
// =============================================================================
// Author: [Your Name]
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SagaOrchestrator.Application.DTOs;

/// <summary>
/// Validation helpers for <see cref="SagaCommandResult"/> and <see cref="SagaCommandResult{T}"/>.
/// </summary>
public static class SagaCommandResultValidation
{
    /// <summary>
    /// Validates a <see cref="SagaCommandResult"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaCommandResult? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.Message))
        {
            problems.Add("Message is required.");
        }

        if (value.Timestamp == DateTime.MinValue || value.Timestamp == DateTime.MaxValue)
        {
            problems.Add("Timestamp must be a valid date.");
        }

        if (string.IsNullOrEmpty(value.RequestId))
        {
            problems.Add("RequestId is required.");
        }

        if (value.Success && value.Errors.Count > 0)
        {
            problems.Add("Success cannot be true when there are errors.");
        }

        return problems;
    }

    /// <summary>
    /// Validates a <see cref="SagaCommandResult{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(this SagaCommandResult<T>? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrEmpty(value.Message))
        {
            problems.Add("Message is required.");
        }

        if (value.Timestamp == DateTime.MinValue || value.Timestamp == DateTime.MaxValue)
        {
            problems.Add("Timestamp must be a valid date.");
        }

        if (string.IsNullOrEmpty(value.RequestId))
        {
            problems.Add("RequestId is required.");
        }

        if (value.Success && value.Errors.Count > 0)
        {
            problems.Add("Success cannot be true when there are errors.");
        }

        return problems;
    }

    /// <summary>
    /// Checks if a <see cref="SagaCommandResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>true if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SagaCommandResult value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Checks if a <see cref="SagaCommandResult{T}"/> instance is valid.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="value">The instance to check.</param>
    /// <returns>true if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid<T>(this SagaCommandResult<T> value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures a <see cref="SagaCommandResult"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to ensure.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this SagaCommandResult value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid SagaCommandResult: {string.Join(", ", problems)}", nameof(value));
        }
    }

    /// <summary>
    /// Ensures a <see cref="SagaCommandResult{T}"/> instance is valid.
    /// </summary>
    /// <typeparam name="T">The type of data.</typeparam>
    /// <param name="value">The instance to ensure.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid<T>(this SagaCommandResult<T> value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid SagaCommandResult: {string.Join(", ", problems)}", nameof(value));
        }
    }
}
