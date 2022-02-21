#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SagaOrchestrator.Infrastructure.Telemetry;

/// <summary>
/// Validation helpers for SagaActivitySource method parameters.
/// </summary>
public static class SagaActivitySourceValidation
{
    /// <summary>
    /// Validates a saga start operation.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="definitionId">The saga definition identifier.</param>
    /// <param name="correlationId">Optional correlation identifier.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
    public static IReadOnlyList<string> Validate(string sagaId, string definitionId, string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);
        ArgumentException.ThrowIfNullOrEmpty(definitionId);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            problems.Add("Saga identifier cannot be whitespace or empty.");
        }

        if (string.IsNullOrWhiteSpace(definitionId))
        {
            problems.Add("Saga definition identifier cannot be whitespace or empty.");
        }

        if (!string.IsNullOrWhiteSpace(correlationId) && string.IsNullOrWhiteSpace(correlationId))
        {
            problems.Add("Correlation identifier cannot be whitespace or empty.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a saga completion operation.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="finalStatus">The final saga status.</param>
    /// <param name="totalSteps">Total number of steps in the saga.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
    public static IReadOnlyList<string> Validate(string sagaId, string finalStatus, int totalSteps)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);
        ArgumentException.ThrowIfNullOrEmpty(finalStatus);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            problems.Add("Saga identifier cannot be whitespace or empty.");
        }

        if (string.IsNullOrWhiteSpace(finalStatus))
        {
            problems.Add("Final status cannot be whitespace or empty.");
        }

        if (totalSteps < 0)
        {
            problems.Add("Total steps cannot be negative.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a step start operation.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="stepName">The step name.</param>
    /// <param name="order">The step order/index.</param>
    /// <param name="attempt">The attempt number (1-based).</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
    public static IReadOnlyList<string> Validate(string sagaId, string stepId, string stepName, int order, int attempt = 1)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);
        ArgumentException.ThrowIfNullOrEmpty(stepId);
        ArgumentException.ThrowIfNullOrEmpty(stepName);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            problems.Add("Saga identifier cannot be whitespace or empty.");
        }

        if (string.IsNullOrWhiteSpace(stepId))
        {
            problems.Add("Step identifier cannot be whitespace or empty.");
        }

        if (string.IsNullOrWhiteSpace(stepName))
        {
            problems.Add("Step name cannot be whitespace or empty.");
        }

        if (order < 0)
        {
            problems.Add("Step order cannot be negative.");
        }

        if (attempt < 1)
        {
            problems.Add("Attempt number must be at least 1.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a compensation start operation.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="compensationId">The compensation transaction identifier.</param>
    /// <param name="stepName">The name of the step being compensated.</param>
    /// <param name="stepOrder">The order of the step being compensated.</param>
    /// <returns>List of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if required parameters are null.</exception>
    public static IReadOnlyList<string> Validate(string sagaId, string compensationId, string stepName, int stepOrder)
    {
        ArgumentException.ThrowIfNullOrEmpty(sagaId);
        ArgumentException.ThrowIfNullOrEmpty(compensationId);
        ArgumentException.ThrowIfNullOrEmpty(stepName);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            problems.Add("Saga identifier cannot be whitespace or empty.");
        }

        if (string.IsNullOrWhiteSpace(compensationId))
        {
            problems.Add("Compensation identifier cannot be whitespace or empty.");
        }

        if (string.IsNullOrWhiteSpace(stepName))
        {
            problems.Add("Step name cannot be whitespace or empty.");
        }

        if (stepOrder < 0)
        {
            problems.Add("Step order cannot be negative.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if the saga start parameters are valid.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="definitionId">The saga definition identifier.</param>
    /// <param name="correlationId">Optional correlation identifier.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(string sagaId, string definitionId, string? correlationId = null)
    {
        return Validate(sagaId, definitionId, correlationId).Count == 0;
    }

    /// <summary>
    /// Checks if the saga completion parameters are valid.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="finalStatus">The final saga status.</param>
    /// <param name="totalSteps">Total number of steps in the saga.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(string sagaId, string finalStatus, int totalSteps)
    {
        return Validate(sagaId, finalStatus, totalSteps).Count == 0;
    }

    /// <summary>
    /// Checks if the step start parameters are valid.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="stepName">The step name.</param>
    /// <param name="order">The step order/index.</param>
    /// <param name="attempt">The attempt number (1-based).</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(string sagaId, string stepId, string stepName, int order, int attempt = 1)
    {
        return Validate(sagaId, stepId, stepName, order, attempt).Count == 0;
    }

    /// <summary>
    /// Checks if the compensation start parameters are valid.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="compensationId">The compensation transaction identifier.</param>
    /// <param name="stepName">The name of the step being compensated.</param>
    /// <param name="stepOrder">The order of the step being compensated.</param>
    /// <returns>True if valid; otherwise false.</returns>
    public static bool IsValid(string sagaId, string compensationId, string stepName, int stepOrder)
    {
        return Validate(sagaId, compensationId, stepName, stepOrder).Count == 0;
    }

    /// <summary>
    /// Ensures that saga start parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="definitionId">The saga definition identifier.</param>
    /// <param name="correlationId">Optional correlation identifier.</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValid(string sagaId, string definitionId, string? correlationId = null)
    {
        var problems = Validate(sagaId, definitionId, correlationId);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that saga completion parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="finalStatus">The final saga status.</param>
    /// <param name="totalSteps">Total number of steps in the saga.</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValid(string sagaId, string finalStatus, int totalSteps)
    {
        var problems = Validate(sagaId, finalStatus, totalSteps);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that step start parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="stepName">The step name.</param>
    /// <param name="order">The step order/index.</param>
    /// <param name="attempt">The attempt number (1-based).</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValid(string sagaId, string stepId, string stepName, int order, int attempt = 1)
    {
        var problems = Validate(sagaId, stepId, stepName, order, attempt);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that compensation start parameters are valid, throwing an exception if not.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="compensationId">The compensation transaction identifier.</param>
    /// <param name="stepName">The name of the step being compensated.</param>
    /// <param name="stepOrder">The order of the step being compensated.</param>
    /// <exception cref="ArgumentException">Thrown if parameters are invalid.</exception>
    public static void EnsureValid(string sagaId, string compensationId, string stepName, int stepOrder)
    {
        var problems = Validate(sagaId, compensationId, stepName, stepOrder);
        if (problems.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", problems));
        }
    }
}