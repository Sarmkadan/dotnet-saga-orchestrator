#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SagaOrchestrator.Application.DTOs;

/// <summary>
/// Provides validation helpers for <see cref="SagaResponse"/> instances.
/// </summary>
public static class SagaResponseValidation
{
    /// <summary>
    /// Validates a <see cref="SagaResponse"/> instance and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The saga response to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this SagaResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate string properties
        ValidateString(value.Id, nameof(value.Id), problems);
        ValidateString(value.CorrelationId, nameof(value.CorrelationId), problems);
        ValidateString(value.Status, nameof(value.Status), problems);
        ValidateString(value.DefinitionId, nameof(value.DefinitionId), problems);
        ValidateString(value.DefinitionName, nameof(value.DefinitionName), problems);
        ValidateString(value.FailureReason, nameof(value.FailureReason), problems);

        // Validate numeric properties
        ValidatePositiveNumber(value.StepCount, nameof(value.StepCount), problems);
        ValidateNonNegativeNumber(value.CompletedSteps, nameof(value.CompletedSteps), problems);
        ValidateNonNegativeNumber(value.FailedSteps, nameof(value.FailedSteps), problems);
        ValidateNonNegativeNumber(value.RetryCount, nameof(value.RetryCount), problems);

        // Validate date properties
        ValidatePastDate(value.StartedAt, nameof(value.StartedAt), problems);
        ValidateFutureDate(value.CompletedAt, nameof(value.CompletedAt), problems);

        // Validate step count consistency
        if (value.StepCount < 0)
        {
            problems.Add($"{nameof(value.StepCount)} ({value.StepCount}) must be non-negative.");
        }

        if (value.CompletedSteps < 0)
        {
            problems.Add($"{nameof(value.CompletedSteps)} ({value.CompletedSteps}) must be non-negative.");
        }

        if (value.FailedSteps < 0)
        {
            problems.Add($"{nameof(value.FailedSteps)} ({value.FailedSteps}) must be non-negative.");
        }

        if (value.CompletedSteps > value.StepCount)
        {
            problems.Add($"{nameof(value.CompletedSteps)} ({value.CompletedSteps}) cannot exceed {nameof(value.StepCount)} ({value.StepCount}).");
        }

        if (value.FailedSteps > value.StepCount)
        {
            problems.Add($"{nameof(value.FailedSteps)} ({value.FailedSteps}) cannot exceed {nameof(value.StepCount)} ({value.StepCount}).");
        }

        // Validate steps collection
        if (value.Steps is null)
        {
            problems.Add($"{nameof(value.Steps)} cannot be null.");
        }
        else if (value.Steps.Count != value.StepCount)
        {
            problems.Add($"{nameof(value.Steps)}.Count ({value.Steps.Count}) must equal {nameof(value.StepCount)} ({value.StepCount}).");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="SagaResponse"/> instance is valid.
    /// </summary>
    /// <param name="value">The saga response to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this SagaResponse value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SagaResponse"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The saga response to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is invalid, containing a list of validation problems.</exception>
    public static void EnsureValid(this SagaResponse value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"SagaResponse is invalid. Problems: {string.Join(", ", problems)}. {FormatValues(value)}",
            nameof(value));
    }

    private static void ValidateString(string? value, string propertyName, List<string> problems)
    {
        if (value is null)
        {
            problems.Add($"{propertyName} cannot be null.");
        }
        else if (string.IsNullOrWhiteSpace(value))
        {
            problems.Add($"{propertyName} cannot be empty or whitespace.");
        }
    }

    private static void ValidatePositiveNumber(int value, string propertyName, List<string> problems)
    {
        if (value <= 0)
        {
            problems.Add($"{propertyName} ({value}) must be positive.");
        }
    }

    private static void ValidateNonNegativeNumber(int value, string propertyName, List<string> problems)
    {
        if (value < 0)
        {
            problems.Add($"{propertyName} ({value}) must be non-negative.");
        }
    }

    private static void ValidatePastDate(DateTime date, string propertyName, List<string> problems)
    {
        if (date == default)
        {
            problems.Add($"{propertyName} cannot be default (DateTime.MinValue).");
        }
        else if (date > DateTime.UtcNow.AddMinutes(1))
        {
            problems.Add($"{propertyName} ({date:O}) must be in the past.");
        }
    }

    private static void ValidateFutureDate(DateTime? date, string propertyName, List<string> problems)
    {
        if (date.HasValue)
        {
            if (date.Value == default)
            {
                problems.Add($"{propertyName} cannot be default (DateTime.MinValue).");
            }
            else if (date.Value.Kind != DateTimeKind.Utc)
            {
                problems.Add($"{propertyName} must be in UTC.");
            }
            else if (date.Value < DateTime.UtcNow.AddMinutes(-1))
            {
                problems.Add($"{propertyName} ({date.Value:O}) must be in the future or null.");
            }
        }
    }

    private static string FormatValues(SagaResponse value)
    {
        return $"Values: Id='{value.Id}', CorrelationId='{value.CorrelationId}', Status='{value.Status}', DefinitionId='{value.DefinitionId}', DefinitionName='{value.DefinitionName}', StartedAt={value.StartedAt:O}, CompletedAt={value.CompletedAt?.ToString("O") ?? "null"}, FailureReason={(value.FailureReason == null ? "null" : $"'" + value.FailureReason + "'")}, StepCount={value.StepCount}, CompletedSteps={value.CompletedSteps}, FailedSteps={value.FailedSteps}, RetryCount={value.RetryCount}, Steps.Count={value.Steps?.Count ?? 0}";
    }
}
