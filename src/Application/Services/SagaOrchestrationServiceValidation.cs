#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Validation helpers for <see cref="SagaOrchestrationService"/>.
/// </summary>
public static class SagaOrchestrationServiceValidation
{
    private static readonly Lazy<FieldInfo> _sagaRepositoryField = new(() =>
        typeof(SagaOrchestrationService).GetField("_sagaRepository", BindingFlags.NonPublic | BindingFlags.Instance)!);

    private static readonly Lazy<FieldInfo> _stepRepositoryField = new(() =>
        typeof(SagaOrchestrationService).GetField("_stepRepository", BindingFlags.NonPublic | BindingFlags.Instance)!);

    private static readonly Lazy<FieldInfo> _compensationServiceField = new(() =>
        typeof(SagaOrchestrationService).GetField("_compensationService", BindingFlags.NonPublic | BindingFlags.Instance)!);

    /// <summary>
    /// Validates the <see cref="SagaOrchestrationService"/> instance and returns a collection of human‑readable problems.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <returns>
    /// An <see cref="IReadOnlyList{T}"/> of validation messages.
    /// If the instance is <c>null</c>, a single message describing the problem is returned.
    /// </returns>
    public static IReadOnlyList<string> Validate(this SagaOrchestrationService? value)
    {
        var problems = new List<string>();

        if (value is null)
        {
            problems.Add("SagaOrchestrationService instance is null.");
            return problems;
        }

        // Validate required dependencies using cached reflection
        if (_sagaRepositoryField.Value.GetValue(value) is null)
        {
            problems.Add("SagaOrchestrationService._sagaRepository dependency is null.");
        }

        if (_stepRepositoryField.Value.GetValue(value) is null)
        {
            problems.Add("SagaOrchestrationService._stepRepository dependency is null.");
        }

        if (_compensationServiceField.Value.GetValue(value) is null)
        {
            problems.Add("SagaOrchestrationService._compensationService dependency is null.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="SagaOrchestrationService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to check.</param>
    /// <returns><c>true</c> if the instance has no validation problems; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static bool IsValid(this SagaOrchestrationService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="SagaOrchestrationService"/> instance is valid.
    /// </summary>
    /// <param name="value">The service instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are found. The exception message contains a semicolon‑separated list of problems.
    /// </exception>
    public static void EnsureValid(this SagaOrchestrationService value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join("; ", problems), nameof(value));
        }
    }
}