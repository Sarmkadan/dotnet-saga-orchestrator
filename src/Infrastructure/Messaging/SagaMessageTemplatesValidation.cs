#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SagaOrchestrator.Infrastructure.Messaging;

/// <summary>
/// Provides validation helpers for <see cref="SagaMessageTemplates"/> message templates.
/// Validates that message template parameters are within expected ranges and formats.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SagaMessageTemplatesValidation
{
    private const int MaxSagaNameLength = 200;
    private const int MaxStepNameLength = 100;
    private const int MaxErrorMessageLength = 1000;
    private const int MaxReasonLength = 500;
    private const int MaxServiceNameLength = 100;
    private const int MaxUrlLength = 500;
    private const int MaxStrategyLength = 50;
    private const int MaxEventTypeLength = 100;
    private const int MaxDefinitionIdLength = 200;

    /// <summary>
    /// Validates parameters for SagaCreated.Format and SagaCreated.Detailed methods.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="sagaName">The name of the saga.</param>
    /// <param name="definitionId">The definition identifier.</param>
    /// <param name="stepCount">The number of steps in the saga.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sagaId"/>, <paramref name="sagaName"/>, or <paramref name="definitionId"/> is null.</exception>
    public IReadOnlyList<string> ValidateSagaCreated(
        [DisallowNull] string sagaId,
        [DisallowNull] string sagaName,
        [DisallowNull] string definitionId,
        int stepCount)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentNullException.ThrowIfNull(sagaName);
        ArgumentNullException.ThrowIfNull(definitionId);

        var errors = new List<string>();

        if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (sagaName.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga name length cannot exceed {MaxSagaNameLength} characters. Current: {sagaName.Length}.");
        }

        if (definitionId.Length > MaxDefinitionIdLength)
        {
            errors.Add($"Definition ID length cannot exceed {MaxDefinitionIdLength} characters. Current: {definitionId.Length}.");
        }

        if (stepCount < 0)
        {
            errors.Add("Step count cannot be negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for StepStarted.Format and StepStarted.Detailed methods.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="stepName">The name of the step being started.</param>
    /// <param name="stepOrder">The zero-based order of the step.</param>
    /// <param name="totalSteps">The total number of steps in the saga.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sagaId"/> or <paramref name="stepName"/> is null.</exception>
    public IReadOnlyList<string> ValidateStepStarted(
        [DisallowNull] string sagaId,
        [DisallowNull] string stepName,
        int stepOrder,
        int totalSteps)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentNullException.ThrowIfNull(stepName);

        var errors = new List<string>();

        if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (stepName.Length > MaxStepNameLength)
        {
            errors.Add($"Step name length cannot exceed {MaxStepNameLength} characters. Current: {stepName.Length}.");
        }

        if (stepOrder < 0)
        {
            errors.Add("Step order cannot be negative.");
        }

        if (totalSteps <= 0)
        {
            errors.Add("Total steps must be positive.");
        }
        else if (stepOrder >= totalSteps)
        {
            errors.Add("Step order must be less than total steps.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for StepCompleted.Format and StepCompleted.Detailed methods.
    /// </summary>
    /// <param name="stepName">The name of the completed step.</param>
    /// <param name="durationMs">The duration of the step execution in milliseconds.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="stepName"/> is null.</exception>
    public IReadOnlyList<string> ValidateStepCompleted(
        [DisallowNull] string stepName,
        long durationMs)
    {
        ArgumentNullException.ThrowIfNull(stepName);

        var errors = new List<string>();

        if (stepName.Length > MaxStepNameLength)
        {
            errors.Add($"Step name length cannot exceed {MaxStepNameLength} characters. Current: {stepName.Length}.");
        }

        if (durationMs < 0)
        {
            errors.Add("Duration cannot be negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for StepFailed.Format, StepFailed.WithRetry, and StepFailed.Detailed methods.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="stepName">The name of the failed step.</param>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="attemptNumber">The attempt number that failed.</param>
    /// <param name="maxRetries">The maximum number of retry attempts allowed.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sagaId"/>, <paramref name="stepName"/>, or <paramref name="error"/> is null.</exception>
    public IReadOnlyList<string> ValidateStepFailed(
        [DisallowNull] string sagaId,
        [DisallowNull] string stepName,
        [DisallowNull] string error,
        int attemptNumber,
        int maxRetries = 0)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentNullException.ThrowIfNull(stepName);
        ArgumentNullException.ThrowIfNull(error);

        var errors = new List<string>();

        if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (stepName.Length > MaxStepNameLength)
        {
            errors.Add($"Step name length cannot exceed {MaxStepNameLength} characters. Current: {stepName.Length}.");
        }

        if (error.Length > MaxErrorMessageLength)
        {
            errors.Add($"Error message length cannot exceed {MaxErrorMessageLength} characters. Current: {error.Length}.");
        }

        if (attemptNumber < 0)
        {
            errors.Add("Attempt number cannot be negative.");
        }
        else if (maxRetries > 0 && attemptNumber > maxRetries)
        {
            errors.Add($"Attempt number cannot exceed max retries. Attempt: {attemptNumber}, Max: {maxRetries}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for SagaCompleted.Format and SagaCompleted.Detailed methods.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="sagaName">The name of the completed saga.</param>
    /// <param name="durationMs">The total duration of the saga execution in milliseconds.</param>
    /// <param name="completedSteps">The number of steps that completed successfully.</param>
    /// <param name="totalSteps">The total number of steps in the saga.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sagaId"/> or <paramref name="sagaName"/> is null.</exception>
    public IReadOnlyList<string> ValidateSagaCompleted(
        [DisallowNull] string sagaId,
        [DisallowNull] string sagaName,
        long durationMs,
        int completedSteps,
        int totalSteps)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentNullException.ThrowIfNull(sagaName);

        var errors = new List<string>();

        if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (sagaName.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga name length cannot exceed {MaxSagaNameLength} characters. Current: {sagaName.Length}.");
        }

        if (durationMs < 0)
        {
            errors.Add("Duration cannot be negative.");
        }

        if (completedSteps < 0)
        {
            errors.Add("Completed steps cannot be negative.");
        }

        if (totalSteps <= 0)
        {
            errors.Add("Total steps must be positive.");
        }
        else if (completedSteps > totalSteps)
        {
            errors.Add("Completed steps cannot exceed total steps.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for SagaFailed.Format and SagaFailed.Detailed methods.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="sagaName">The name of the saga.</param>
    /// <param name="failedStepName">The name of the step that failed.</param>
    /// <param name="error">The error message describing the failure.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sagaId"/>, <paramref name="sagaName"/>, <paramref name="failedStepName"/>, or <paramref name="error"/> is null.</exception>
    public IReadOnlyList<string> ValidateSagaFailed(
        [DisallowNull] string sagaId,
        [DisallowNull] string sagaName,
        [DisallowNull] string failedStepName,
        [DisallowNull] string error)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentNullException.ThrowIfNull(sagaName);
        ArgumentNullException.ThrowIfNull(failedStepName);
        ArgumentNullException.ThrowIfNull(error);

        var errors = new List<string>();

        if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (sagaName.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga name length cannot exceed {MaxSagaNameLength} characters. Current: {sagaName.Length}.");
        }

        if (failedStepName.Length > MaxStepNameLength)
        {
            errors.Add($"Failed step name length cannot exceed {MaxStepNameLength} characters. Current: {failedStepName.Length}.");
        }

        if (error.Length > MaxErrorMessageLength)
        {
            errors.Add($"Error message length cannot exceed {MaxErrorMessageLength} characters. Current: {error.Length}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for CompensationStarted.Format and CompensationStarted.Detailed methods.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="strategy">The compensation strategy being used.</param>
    /// <param name="stepsToCompensate">The number of steps to compensate.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sagaId"/> or <paramref name="strategy"/> is null.</exception>
    public IReadOnlyList<string> ValidateCompensationStarted(
        [DisallowNull] string sagaId,
        [DisallowNull] string strategy,
        int stepsToCompensate)
    {
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentNullException.ThrowIfNull(strategy);

        var errors = new List<string>();

        if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (strategy.Length > MaxStrategyLength)
        {
            errors.Add($"Compensation strategy length cannot exceed {MaxStrategyLength} characters. Current: {strategy.Length}.");
        }

        if (stepsToCompensate < 0)
        {
            errors.Add("Steps to compensate cannot be negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for CompensationCompleted.Format and CompensationCompleted.Detailed methods.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="compensatedSteps">The number of steps successfully compensated.</param>
    /// <param name="durationMs">The duration of the compensation process in milliseconds.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sagaId"/> is null.</exception>
    public IReadOnlyList<string> ValidateCompensationCompleted(
        [DisallowNull] string sagaId,
        int compensatedSteps,
        long durationMs)
    {
        ArgumentNullException.ThrowIfNull(sagaId);

        var errors = new List<string>();

        if (compensatedSteps < 0)
        {
            errors.Add("Compensated steps cannot be negative.");
        }

        if (durationMs < 0)
        {
            errors.Add("Duration cannot be negative.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for SagaTimeout.Format and SagaTimeout.StepTimeout methods.
    /// </summary>
    /// <param name="sagaName">The name of the saga.</param>
    /// <param name="stepName">The name of the timed-out step.</param>
    /// <param name="timeoutSeconds">The timeout duration in seconds.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="sagaName"/> or <paramref name="stepName"/> is null.</exception>
    public IReadOnlyList<string> ValidateSagaTimeout(
        [DisallowNull] string sagaName,
        [DisallowNull] string stepName,
        int timeoutSeconds)
    {
        ArgumentNullException.ThrowIfNull(sagaName);
        ArgumentNullException.ThrowIfNull(stepName);

        var errors = new List<string>();

        if (sagaName.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga name length cannot exceed {MaxSagaNameLength} characters. Current: {sagaName.Length}.");
        }

        if (stepName.Length > MaxStepNameLength)
        {
            errors.Add($"Step name length cannot exceed {MaxStepNameLength} characters. Current: {stepName.Length}.");
        }

        if (timeoutSeconds <= 0)
        {
            errors.Add("Timeout must be positive.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for DefinitionInvalid.Format, DefinitionInvalid.InvalidStep, and related methods.
    /// </summary>
    /// <param name="reason">The reason why the definition is invalid.</param>
    /// <param name="stepName">Optional step name associated with the invalid definition.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="reason"/> is null.</exception>
    public IReadOnlyList<string> ValidateDefinitionInvalid(
        [DisallowNull] string reason,
        string? stepName = null)
    {
        ArgumentNullException.ThrowIfNull(reason);

        var errors = new List<string>();

        if (reason.Length > MaxReasonLength)
        {
            errors.Add($"Reason length cannot exceed {MaxReasonLength} characters. Current: {reason.Length}.");
        }

        if (stepName is not null && stepName.Length > MaxStepNameLength)
        {
            errors.Add($"Step name length cannot exceed {MaxStepNameLength} characters. Current: {stepName.Length}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for ServiceHealth method.
    /// </summary>
    /// <param name="serviceName">The name of the service being checked.</param>
    /// <param name="isHealthy">Whether the service is healthy.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="serviceName"/> is null.</exception>
    public IReadOnlyList<string> ValidateServiceHealth(
        [DisallowNull] string serviceName,
        bool isHealthy)
    {
        ArgumentNullException.ThrowIfNull(serviceName);

        var errors = new List<string>();

        if (serviceName.Length > MaxServiceNameLength)
        {
            errors.Add($"Service name length cannot exceed {MaxServiceNameLength} characters. Current: {serviceName.Length}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for WebhookDelivery method.
    /// </summary>
    /// <param name="url">The webhook URL.</param>
    /// <param name="eventType">The type of event being delivered.</param>
    /// <param name="success">Whether the delivery was successful.</param>
    /// <returns>A list of validation errors; empty if validation passes.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="url"/> or <paramref name="eventType"/> is null.</exception>
    public IReadOnlyList<string> ValidateWebhookDelivery(
        [DisallowNull] string url,
        [DisallowNull] string eventType,
        bool success)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(eventType);

        var errors = new List<string>();

        if (url.Length > MaxUrlLength)
        {
            errors.Add($"URL length cannot exceed {MaxUrlLength} characters. Current: {url.Length}.");
        }

        if (eventType.Length > MaxEventTypeLength)
        {
            errors.Add($"Event type length cannot exceed {MaxEventTypeLength} characters. Current: {eventType.Length}.");
        }

        return errors.AsReadOnly();
    }
}