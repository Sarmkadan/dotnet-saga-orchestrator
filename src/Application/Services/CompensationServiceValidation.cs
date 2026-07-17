#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Validation helpers for <see cref="CompensationService"/>.
/// </summary>
public static class CompensationServiceValidation
{
    private static readonly Lazy<FieldInfo> _compensationRepositoryField = new(() =>
        typeof(CompensationService).GetField("_compensationRepository", BindingFlags.NonPublic | BindingFlags.Instance)!);

    private static readonly Lazy<FieldInfo> _sagaRepositoryField = new(() =>
        typeof(CompensationService).GetField("_sagaRepository", BindingFlags.NonPublic | BindingFlags.Instance)!);

    private static readonly Lazy<FieldInfo> _stepRepositoryField = new(() =>
        typeof(CompensationService).GetField("_stepRepository", BindingFlags.NonPublic | BindingFlags.Instance)!);

    /// <summary>
    /// Validates the <see cref="CompensationService"/> instance and returns a collection of human‑readable problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of validation messages.
    /// If the instance is <c>null</c>, a single message describing the problem is returned.
    /// </returns>
    public static IReadOnlyList<string> Validate(this CompensationService? value)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("CompensationService instance is null.");
            return problems;
        }

        // Validate required dependencies using cached reflection
        if (_compensationRepositoryField.Value.GetValue(value) is null)
        {
            problems.Add("CompensationService._compensationRepository dependency is null.");
        }

        if (_sagaRepositoryField.Value.GetValue(value) is null)
        {
            problems.Add("CompensationService._sagaRepository dependency is null.");
        }

        if (_stepRepositoryField.Value.GetValue(value) is null)
        {
            problems.Add("CompensationService._stepRepository dependency is null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="CompensationService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this CompensationService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="CompensationService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are found. The exception message contains a semicolon‑separated list of problems.
    /// </exception>
    public static void EnsureValid(this CompensationService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", problems), nameof(value));
        }
    }
}
