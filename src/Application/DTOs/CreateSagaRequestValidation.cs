#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SagaOrchestrator.Application.DTOs;

/// <summary>
/// Provides validation helpers for <see cref="CreateSagaRequest"/>.
/// </summary>
public static class CreateSagaRequestValidation
{
    /// <summary>
    /// Validates the <see cref="CreateSagaRequest"/> instance and returns a collection of human‑readable problems.
    /// </summary>
    /// <param name="value">The request to validate.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> containing validation error messages. Empty when the request is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static IReadOnlyList<string> Validate(this CreateSagaRequest value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Either DefinitionId or DefinitionName must be supplied.
        if (string.IsNullOrWhiteSpace(value.DefinitionId) && string.IsNullOrWhiteSpace(value.DefinitionName))
        {
            errors.Add("Either DefinitionId or DefinitionName must be provided.");
        }

        // MaxRetries, if supplied, must be non‑negative.
        if (value.MaxRetries.HasValue && value.MaxRetries.Value < 0)
        {
            errors.Add(string.Format(CultureInfo.InvariantCulture,
                "MaxRetries must be greater than or equal to 0, but was {0}.", value.MaxRetries.Value));
        }

        // TimeoutSeconds, if supplied, must be greater than zero.
        if (value.TimeoutSeconds.HasValue && value.TimeoutSeconds.Value <= 0)
        {
            errors.Add(string.Format(CultureInfo.InvariantCulture,
                "TimeoutSeconds must be greater than 0, but was {0}.", value.TimeoutSeconds.Value));
        }

        return errors;
    }

    /// <summary>
    /// Determines whether the <see cref="CreateSagaRequest"/> instance is valid.
    /// </summary>
    /// <param name="value">The request to evaluate.</param>
    /// <returns><c>true</c> if the request contains no validation problems; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this CreateSagaRequest value) => value.Validate().Count == 0;

    /// <summary>
    /// Ensures that the <see cref="CreateSagaRequest"/> instance is valid.
    /// </summary>
    /// <param name="value">The request to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when the request is invalid. The exception message contains a semicolon‑separated list of validation problems.</exception>
    public static void EnsureValid(this CreateSagaRequest value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", errors));
        }
    }
}