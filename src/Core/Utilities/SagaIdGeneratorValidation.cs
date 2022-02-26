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
    /// <returns>An empty list if valid, otherwise a list of human-readable validation problems</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate all generated IDs are non-null and properly formatted
        try
        {
            var sagaId = SagaIdGenerator.GenerateSagaId();
            if (string.IsNullOrWhiteSpace(sagaId))
                problems.Add("GenerateSagaId() returned null or whitespace");
            else if (!SagaIdGenerator.IsValidSagaId(sagaId))
                problems.Add("GenerateSagaId() returned invalid format: " + sagaId);
        }
        catch (Exception ex)
        {
            problems.Add("GenerateSagaId() threw exception: " + ex.Message);
        }

        try
        {
            var correlationId = SagaIdGenerator.GenerateCorrelationId();
            if (string.IsNullOrWhiteSpace(correlationId))
                problems.Add("GenerateCorrelationId() returned null or whitespace");
            else if (!SagaIdGenerator.IsValidCorrelationId(correlationId))
                problems.Add("GenerateCorrelationId() returned invalid format: " + correlationId);
        }
        catch (Exception ex)
        {
            problems.Add("GenerateCorrelationId() threw exception: " + ex.Message);
        }

        try
        {
            var stepId = SagaIdGenerator.GenerateStepId();
            if (string.IsNullOrWhiteSpace(stepId))
                problems.Add("GenerateStepId() returned null or whitespace");
        }
        catch (Exception ex)
        {
            problems.Add("GenerateStepId() threw exception: " + ex.Message);
        }

        try
        {
            var traceId = SagaIdGenerator.GenerateTraceId();
            if (string.IsNullOrWhiteSpace(traceId))
                problems.Add("GenerateTraceId() returned null or whitespace");
        }
        catch (Exception ex)
        {
            problems.Add("GenerateTraceId() threw exception: " + ex.Message);
        }

        try
        {
            var requestId = SagaIdGenerator.GenerateRequestId();
            if (string.IsNullOrWhiteSpace(requestId))
                problems.Add("GenerateRequestId() returned null or whitespace");
        }
        catch (Exception ex)
        {
            problems.Add("GenerateRequestId() threw exception: " + ex.Message);
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the SagaIdGenerator static class is valid.
    /// </summary>
    /// <returns>True if valid, otherwise false</returns>
    public static bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the SagaIdGenerator static class is valid, throwing an exception if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if SagaIdGenerator is invalid, containing validation details</exception>
    public static void EnsureValid()
    {
        var problems = Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                "SagaIdGenerator is invalid. Problems:\n" +
                string.Join("\n", problems));
        }
    }
}