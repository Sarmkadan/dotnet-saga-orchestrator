#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Core.Utilities;

/// <summary>
/// Provides validation helpers for SagaIdGenerator operations.
/// </summary>
public static class SagaIdGeneratorValidation
{
    /// <summary>
    /// Validates the SagaIdGenerator static class and returns a list of validation problems.
    /// </summary>
    /// <returns>An empty list if valid, otherwise a list of human-readable validation problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if any generated ID is null.</exception>
    public static IReadOnlyList<string> Validate()
    {
        ArgumentNullException.ThrowIfNull(typeof(SagaIdGenerator));

        var problems = new List<string>();

        // Validate all generated IDs are non-null and properly formatted
        ValidateId(
            "GenerateSagaId",
            SagaIdGenerator.GenerateSagaId,
            SagaIdGenerator.IsValidSagaId,
            problems);

        ValidateId(
            "GenerateCorrelationId",
            SagaIdGenerator.GenerateCorrelationId,
            SagaIdGenerator.IsValidCorrelationId,
            problems);

        ValidateId(
            "GenerateStepId",
            SagaIdGenerator.GenerateStepId,
            id => !string.IsNullOrWhiteSpace(id),
            problems);

        ValidateId(
            "GenerateTraceId",
            SagaIdGenerator.GenerateTraceId,
            id => !string.IsNullOrWhiteSpace(id),
            problems);

        ValidateId(
            "GenerateRequestId",
            SagaIdGenerator.GenerateRequestId,
            id => !string.IsNullOrWhiteSpace(id),
            problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the SagaIdGenerator static class is valid.
    /// </summary>
    /// <returns>True if valid, otherwise false.</returns>
    public static bool IsValid() => Validate().Count == 0;

    /// <summary>
    /// Ensures the SagaIdGenerator static class is valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if SagaIdGenerator is invalid, containing validation details.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SagaIdGenerator is invalid. Problems:{Environment.NewLine}" +
                string.Join(Environment.NewLine, problems));
        }
    }

    private static void ValidateId(
        string methodName,
        Func<string> idGenerator,
        Func<string, bool> validator,
        ICollection<string> problems)
    {
        ArgumentNullException.ThrowIfNull(methodName);
        ArgumentNullException.ThrowIfNull(idGenerator);
        ArgumentNullException.ThrowIfNull(validator);
        ArgumentNullException.ThrowIfNull(problems);

        try
        {
            var id = idGenerator();
            if (id is null)
            {
                problems.Add($"{methodName}() returned null");
                return;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                problems.Add($"{methodName}() returned whitespace or empty string");
                return;
            }

            if (!validator(id))
            {
                problems.Add($"{methodName}() returned invalid format: {id}");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"{methodName}() threw exception: {ex.Message}");
        }
    }
}