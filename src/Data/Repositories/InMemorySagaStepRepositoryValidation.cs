#nullable enable

using System;
using System.Collections.Generic;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Provides validation helpers for <see cref="InMemorySagaStepRepository"/> instances and <see cref="SagaStep"/> entities.
/// </summary>
public static class InMemorySagaStepRepositoryValidation
{
    /// <summary>
    /// Validates the specified <see cref="InMemorySagaStepRepository"/> instance.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this InMemorySagaStepRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Repository-level validations would go here if there were any state to validate
        // For now, we validate the public contract through the methods

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a saga step entity.
    /// </summary>
    /// <param name="step">The saga step to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="step"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        var problems = new List<string>();

        // Validate required properties
        if (string.IsNullOrWhiteSpace(step.Id))
        {
            problems.Add("SagaStep.Id must be a non-empty string.");
        }

        if (string.IsNullOrWhiteSpace(step.SagaId))
        {
            problems.Add("SagaStep.SagaId must be a non-empty string.");
        }

        if (string.IsNullOrWhiteSpace(step.Name))
        {
            problems.Add("SagaStep.Name must be a non-empty string.");
        }

        if (step.Order < 1)
        {
            problems.Add("SagaStep.Order must be a positive integer (1-based).");
        }

        if (step.Status is not (SagaStepStatus.Pending or SagaStepStatus.Executing or SagaStepStatus.Completed or SagaStepStatus.Failed or SagaStepStatus.WaitingForRetry or SagaStepStatus.Compensated or SagaStepStatus.TimedOut or SagaStepStatus.Skipped))
        {
            problems.Add($"SagaStep.Status has invalid value: {step.Status}.");
        }

        if (string.IsNullOrWhiteSpace(step.ServiceUrl))
        {
            problems.Add("SagaStep.ServiceUrl must be a non-empty string.");
        }

        if (step.MaxRetries < 0)
        {
            problems.Add("SagaStep.MaxRetries must be a non-negative integer.");
        }

        if (step.TimeoutSeconds <= 0)
        {
            problems.Add("SagaStep.TimeoutSeconds must be a positive integer.");
        }

        if (step.Status == SagaStepStatus.Completed && step.CompletedAt == null)
        {
            problems.Add("SagaStep.CompletedAt must be set when Status is Completed.");
        }

        if (step.Status == SagaStepStatus.Completed && step.StartedAt != null && step.CompletedAt < step.StartedAt)
        {
            problems.Add("SagaStep.CompletedAt cannot be earlier than StartedAt.");
        }

        if (step.Status == SagaStepStatus.Failed && string.IsNullOrWhiteSpace(step.ErrorMessage))
        {
            problems.Add("SagaStep.ErrorMessage must be set when Status is Failed.");
        }

        if (step.Status == SagaStepStatus.Failed && step.StartedAt != null && step.CompletedAt < step.StartedAt)
        {
            problems.Add("SagaStep.CompletedAt cannot be earlier than StartedAt.");
        }

        if (step.Status == SagaStepStatus.Executing && step.StartedAt == null)
        {
            problems.Add("SagaStep.StartedAt must be set when Status is Executing.");
        }

        if (step.Status == SagaStepStatus.Compensated && step.CompletedAt == null)
        {
            problems.Add("SagaStep.CompletedAt must be set when Status is Compensated.");
        }

        if (step.Status == SagaStepStatus.Compensated && step.CompletedAt != null && step.CompensatedAt != null && step.CompensatedAt < step.CompletedAt)
        {
            problems.Add("SagaStep.CompensatedAt cannot be earlier than CompletedAt.");
        }

        // Validate payloads if present
        if (step.Payload != null)
        {
            if (step.Payload.Count == 0)
            {
                problems.Add("SagaStep.Payload should not be empty if initialized.");
            }
        }

        if (step.Response != null)
        {
            if (step.Response.Count == 0)
            {
                problems.Add("SagaStep.Response should not be empty if initialized.");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="InMemorySagaStepRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this InMemorySagaStepRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Determines whether the specified saga step is valid.
    /// </summary>
    /// <param name="step">The saga step to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="step"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this SagaStep step)
    {
        return step?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="InMemorySagaStepRepository"/> instance is valid.
    /// </summary>
    /// <param name="value">The repository instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this InMemorySagaStepRepository value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Repository is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }

    /// <summary>
    /// Ensures that the specified saga step is valid.
    /// </summary>
    /// <param name="step">The saga step to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="step"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="step"/> is not valid, containing a list of problems.</exception>
    public static void EnsureValid(this SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);

        var problems = step.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"SagaStep is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
        }
    }
}