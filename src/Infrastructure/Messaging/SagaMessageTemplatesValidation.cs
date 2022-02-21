#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Infrastructure.Messaging;

/// <summary>
/// Provides validation helpers for <see cref="SagaMessageTemplates"/> message templates.
/// Validates that message template parameters are within expected ranges and formats.
/// </summary>
public static class SagaMessageTemplatesValidation
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
    public static IReadOnlyList<string> ValidateSagaCreated(
        string sagaId,
        string sagaName,
        string definitionId,
        int stepCount)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            errors.Add("Saga ID cannot be null or whitespace.");
        }
        else if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (string.IsNullOrWhiteSpace(sagaName))
        {
            errors.Add("Saga name cannot be null or whitespace.");
        }
        else if (sagaName.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga name length cannot exceed {MaxSagaNameLength} characters. Current: {sagaName.Length}.");
        }

        if (string.IsNullOrWhiteSpace(definitionId))
        {
            errors.Add("Definition ID cannot be null or whitespace.");
        }
        else if (definitionId.Length > MaxDefinitionIdLength)
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
    public static IReadOnlyList<string> ValidateStepStarted(
        string sagaId,
        string stepName,
        int stepOrder,
        int totalSteps)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            errors.Add("Saga ID cannot be null or whitespace.");
        }
        else if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (string.IsNullOrWhiteSpace(stepName))
        {
            errors.Add("Step name cannot be null or whitespace.");
        }
        else if (stepName.Length > MaxStepNameLength)
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
        else if (stepOrder > totalSteps)
        {
            errors.Add("Step order cannot exceed total steps.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for StepCompleted.Format and StepCompleted.Detailed methods.
    /// </summary>
    public static IReadOnlyList<string> ValidateStepCompleted(
        string stepName,
        long durationMs)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(stepName))
        {
            errors.Add("Step name cannot be null or whitespace.");
        }
        else if (stepName.Length > MaxStepNameLength)
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
    public static IReadOnlyList<string> ValidateStepFailed(
        string sagaId,
        string stepName,
        string error,
        int attemptNumber,
        int maxRetries = 0)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            errors.Add("Saga ID cannot be null or whitespace.");
        }
        else if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (string.IsNullOrWhiteSpace(stepName))
        {
            errors.Add("Step name cannot be null or whitespace.");
        }
        else if (stepName.Length > MaxStepNameLength)
        {
            errors.Add($"Step name length cannot exceed {MaxStepNameLength} characters. Current: {stepName.Length}.");
        }

        if (string.IsNullOrWhiteSpace(error))
        {
            errors.Add("Error message cannot be null or whitespace.");
        }
        else if (error.Length > MaxErrorMessageLength)
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
    public static IReadOnlyList<string> ValidateSagaCompleted(
        string sagaId,
        string sagaName,
        long durationMs,
        int completedSteps,
        int totalSteps)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            errors.Add("Saga ID cannot be null or whitespace.");
        }
        else if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (string.IsNullOrWhiteSpace(sagaName))
        {
            errors.Add("Saga name cannot be null or whitespace.");
        }
        else if (sagaName.Length > MaxSagaNameLength)
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
    public static IReadOnlyList<string> ValidateSagaFailed(
        string sagaId,
        string sagaName,
        string failedStepName,
        string error)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            errors.Add("Saga ID cannot be null or whitespace.");
        }
        else if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (string.IsNullOrWhiteSpace(sagaName))
        {
            errors.Add("Saga name cannot be null or whitespace.");
        }
        else if (sagaName.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga name length cannot exceed {MaxSagaNameLength} characters. Current: {sagaName.Length}.");
        }

        if (string.IsNullOrWhiteSpace(failedStepName))
        {
            errors.Add("Failed step name cannot be null or whitespace.");
        }
        else if (failedStepName.Length > MaxStepNameLength)
        {
            errors.Add($"Failed step name length cannot exceed {MaxStepNameLength} characters. Current: {failedStepName.Length}.");
        }

        if (string.IsNullOrWhiteSpace(error))
        {
            errors.Add("Error message cannot be null or whitespace.");
        }
        else if (error.Length > MaxErrorMessageLength)
        {
            errors.Add($"Error message length cannot exceed {MaxErrorMessageLength} characters. Current: {error.Length}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for CompensationStarted.Format and CompensationStarted.Detailed methods.
    /// </summary>
    public static IReadOnlyList<string> ValidateCompensationStarted(
        string sagaId,
        string strategy,
        int stepsToCompensate)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            errors.Add("Saga ID cannot be null or whitespace.");
        }
        else if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

        if (string.IsNullOrWhiteSpace(strategy))
        {
            errors.Add("Compensation strategy cannot be null or whitespace.");
        }
        else if (strategy.Length > MaxStrategyLength)
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
    public static IReadOnlyList<string> ValidateCompensationCompleted(
        string sagaId,
        int compensatedSteps,
        long durationMs)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            errors.Add("Saga ID cannot be null or whitespace.");
        }
        else if (sagaId.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga ID length cannot exceed {MaxSagaNameLength} characters. Current: {sagaId.Length}.");
        }

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
    public static IReadOnlyList<string> ValidateSagaTimeout(
        string sagaName,
        string stepName,
        int timeoutSeconds)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaName))
        {
            errors.Add("Saga name cannot be null or whitespace.");
        }
        else if (sagaName.Length > MaxSagaNameLength)
        {
            errors.Add($"Saga name length cannot exceed {MaxSagaNameLength} characters. Current: {sagaName.Length}.");
        }

        if (string.IsNullOrWhiteSpace(stepName))
        {
            errors.Add("Step name cannot be null or whitespace.");
        }
        else if (stepName.Length > MaxStepNameLength)
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
    public static IReadOnlyList<string> ValidateDefinitionInvalid(
        string reason,
        string stepName = null)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(reason))
        {
            errors.Add("Reason cannot be null or whitespace.");
        }
        else if (reason.Length > MaxReasonLength)
        {
            errors.Add($"Reason length cannot exceed {MaxReasonLength} characters. Current: {reason.Length}.");
        }

        if (stepName != null && stepName.Length > MaxStepNameLength)
        {
            errors.Add($"Step name length cannot exceed {MaxStepNameLength} characters. Current: {stepName.Length}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for ServiceHealth method.
    /// </summary>
    public static IReadOnlyList<string> ValidateServiceHealth(
        string serviceName,
        bool isHealthy)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            errors.Add("Service name cannot be null or whitespace.");
        }
        else if (serviceName.Length > MaxServiceNameLength)
        {
            errors.Add($"Service name length cannot exceed {MaxServiceNameLength} characters. Current: {serviceName.Length}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates parameters for WebhookDelivery method.
    /// </summary>
    public static IReadOnlyList<string> ValidateWebhookDelivery(
        string url,
        string eventType,
        bool success)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(url))
        {
            errors.Add("URL cannot be null or whitespace.");
        }
        else if (url.Length > MaxUrlLength)
        {
            errors.Add($"URL length cannot exceed {MaxUrlLength} characters. Current: {url.Length}.");
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            errors.Add("Event type cannot be null or whitespace.");
        }
        else if (eventType.Length > MaxEventTypeLength)
        {
            errors.Add($"Event type length cannot exceed {MaxEventTypeLength} characters. Current: {eventType.Length}.");
        }

        return errors.AsReadOnly();
    }
}
