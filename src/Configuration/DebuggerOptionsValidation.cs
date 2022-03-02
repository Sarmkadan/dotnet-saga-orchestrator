#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="DebuggerOptions"/> instances.
/// </summary>
public static class DebuggerOptionsValidation
{
    /// <summary>
    /// Validates the specified <see cref="DebuggerOptions"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <returns>An empty list if the instance is valid; otherwise, a list of validation error messages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DebuggerOptions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        if (value.MaxSnapshotsPerSaga < 1)
        {
            errors.Add($"{nameof(DebuggerOptions.MaxSnapshotsPerSaga)} must be at least 1, but was {value.MaxSnapshotsPerSaga}.");
        }
        else if (value.MaxSnapshotsPerSaga > 10_000)
        {
            errors.Add($"{nameof(DebuggerOptions.MaxSnapshotsPerSaga)} must be at most 10 000, but was {value.MaxSnapshotsPerSaga}.");
        }

        if (value.MaxBreakpointsPerSaga < 1)
        {
            errors.Add($"{nameof(DebuggerOptions.MaxBreakpointsPerSaga)} must be at least 1, but was {value.MaxBreakpointsPerSaga}.");
        }
        else if (value.MaxBreakpointsPerSaga > 100)
        {
            errors.Add($"{nameof(DebuggerOptions.MaxBreakpointsPerSaga)} must be at most 100, but was {value.MaxBreakpointsPerSaga}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="DebuggerOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The options instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this DebuggerOptions? value)
    {
        return value is not null && Validate(value).Count == 0;
    }

    /// <summary>
    /// Validates the specified <see cref="DebuggerOptions"/> instance and throws an <see cref="ArgumentException"/> if it is invalid.
    /// </summary>
    /// <param name="value">The options instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the instance is invalid. The exception message contains a list of all validation errors.
    /// </exception>
    public static void EnsureValid(this DebuggerOptions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"The {nameof(DebuggerOptions)} instance is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
    }
}