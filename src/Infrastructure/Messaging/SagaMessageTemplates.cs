#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Infrastructure.Messaging;

/// <summary>
/// Message templates for saga event notifications and communications.
/// Provides formatted messages for events, errors, and status updates.
/// </summary>
public static class SagaMessageTemplates
{
    public static class SagaCreated
    {
        public static string Format(string sagaId, string sagaName, int stepCount)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaId);
            ArgumentException.ThrowIfNullOrEmpty(sagaName);
            return $"Saga '{sagaName}' (ID: {sagaId}) created with {stepCount}";
        }

        public static string Detailed(string sagaId, string sagaName, string definitionId, int stepCount)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaId);
            ArgumentException.ThrowIfNullOrEmpty(sagaName);
            ArgumentException.ThrowIfNullOrEmpty(definitionId);
            return $"New saga instance created\n" +
                   $"  ID: {sagaId}\n" +
                   $"  Name: {sagaName}\n" +
                   $"  Definition: {definitionId}\n" +
                   $"  Steps: {stepCount}";
        }
    }

    public static class StepStarted
    {
        public static string Format(string stepName, int stepOrder)
        {
            ArgumentException.ThrowIfNullOrEmpty(stepName);
            return $"Executing step {stepOrder}: {stepName}";
        }

        public static string Detailed(string sagaId, string stepName, int stepOrder, int totalSteps)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaId);
            ArgumentException.ThrowIfNullOrEmpty(stepName);
            return $"Step execution started\n" +
                   $"  Saga: {sagaId}\n" +
                   $"  Step: {stepName}\n" +
                   $"  Progress: {stepOrder}/{totalSteps}";
        }
    }

    public static class StepCompleted
    {
        public static string Format(string stepName, long durationMs)
        {
            ArgumentException.ThrowIfNullOrEmpty(stepName);
            return $"Step '{stepName}' completed in {durationMs}ms";
        }

        public static string Detailed(string stepName, long durationMs, string result)
        {
            ArgumentException.ThrowIfNullOrEmpty(stepName);
            ArgumentException.ThrowIfNullOrEmpty(result);
            return $"Step execution completed\n" +
                   $"  Step: {stepName}\n" +
                   $"  Duration: {durationMs}ms\n" +
                   $"  Result: {result}";
        }
    }

    public static class StepFailed
    {
        public static string Format(string stepName, string error)
        {
            ArgumentException.ThrowIfNullOrEmpty(stepName);
            ArgumentException.ThrowIfNullOrEmpty(error);
            return $"Step '{stepName}' failed: {error}";
        }

        public static string WithRetry(string stepName, string error, int retryCount, int maxRetries)
        {
            ArgumentException.ThrowIfNullOrEmpty(stepName);
            ArgumentException.ThrowIfNullOrEmpty(error);
            return $"Step '{stepName}' failed (attempt {retryCount}/{maxRetries}): {error}";
        }

        public static string Detailed(string sagaId, string stepName, string error, int attemptNumber)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaId);
            ArgumentException.ThrowIfNullOrEmpty(stepName);
            ArgumentException.ThrowIfNullOrEmpty(error);
            return $"Step execution failed\n" +
                   $"  Saga: {sagaId}\n" +
                   $"  Step: {stepName}\n" +
                   $"  Attempt: {attemptNumber}\n" +
                   $"  Error: {error}";
        }
    }

    public static class SagaCompleted
    {
        public static string Format(string sagaName, long durationMs, int completedSteps, int totalSteps)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaName);
            return $"Saga '{sagaName}' completed successfully in {durationMs}ms ({completedSteps}/{totalSteps} steps)";
        }

        public static string Detailed(string sagaId, string sagaName, long durationMs, int completedSteps, int totalSteps)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaId);
            ArgumentException.ThrowIfNullOrEmpty(sagaName);
            return $"Saga execution completed\n" +
                   $"  ID: {sagaId}\n" +
                   $"  Name: {sagaName}\n" +
                   $"  Duration: {durationMs}ms\n" +
                   $"  Steps: {completedSteps}/{totalSteps}\n" +
                   $"  Status: SUCCESS";
        }
    }

    public static class SagaFailed
    {
        public static string Format(string sagaName, string error)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaName);
            ArgumentException.ThrowIfNullOrEmpty(error);
            return $"Saga '{sagaName}' failed: {error}";
        }

        public static string Detailed(string sagaId, string sagaName, string failedStepName, string error)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaId);
            ArgumentException.ThrowIfNullOrEmpty(sagaName);
            ArgumentException.ThrowIfNullOrEmpty(failedStepName);
            ArgumentException.ThrowIfNullOrEmpty(error);
            return $"Saga execution failed\n" +
                   $"  ID: {sagaId}\n" +
                   $"  Name: {sagaName}\n" +
                   $"  Failed Step: {failedStepName}\n" +
                   $"  Error: {error}\n" +
                   $"  Status: FAILED";
        }
    }

    public static class CompensationStarted
    {
        public static string Format(string strategy, int stepsToCompensate)
        {
            ArgumentException.ThrowIfNullOrEmpty(strategy);
            return $"Compensation started ({strategy} strategy) for {stepsToCompensate} steps";
        }

        public static string Detailed(string sagaId, string strategy, int stepsToCompensate)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaId);
            ArgumentException.ThrowIfNullOrEmpty(strategy);
            return $"Compensation initiated\n" +
                   $"  Saga: {sagaId}\n" +
                   $"  Strategy: {strategy}\n" +
                   $"  Steps to compensate: {stepsToCompensate}";
        }
    }

    public static class CompensationCompleted
    {
        public static string Format(int compensatedSteps, long durationMs)
        {
            return $"Compensation completed for {compensatedSteps} steps in {durationMs}ms";
        }

        public static string Detailed(string sagaId, int compensatedSteps, long durationMs)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaId);
            return $"Compensation completed\n" +
                   $"  Saga: {sagaId}\n" +
                   $"  Steps compensated: {compensatedSteps}\n" +
                   $"  Duration: {durationMs}ms";
        }
    }

    public static class SagaTimeout
    {
        public static string Format(string sagaName, int timeoutSeconds)
        {
            ArgumentException.ThrowIfNullOrEmpty(sagaName);
            return $"Saga '{sagaName}' exceeded timeout limit of {timeoutSeconds} seconds";
        }

        public static string StepTimeout(string stepName, int timeoutSeconds)
        {
            ArgumentException.ThrowIfNullOrEmpty(stepName);
            return $"Step '{stepName}' exceeded timeout limit of {timeoutSeconds} seconds";
        }
    }

    public static class DefinitionInvalid
    {
        public static string Format(string reason)
        {
            ArgumentException.ThrowIfNullOrEmpty(reason);
            return $"Saga definition is invalid: {reason}";
        }

        public static string MissingSteps() =>
            "Saga definition must contain at least one step";

        public static string InvalidStep(string stepName, string reason)
        {
            ArgumentException.ThrowIfNullOrEmpty(stepName);
            ArgumentException.ThrowIfNullOrEmpty(reason);
            return $"Step '{stepName}' is invalid: {reason}";
        }
    }

    public static string ServiceHealth(string serviceName, bool isHealthy)
    {
        ArgumentException.ThrowIfNullOrEmpty(serviceName);
        return $"Service '{serviceName}' is {(isHealthy ? "healthy" : "unhealthy")}";
    }

    public static string WebhookDelivery(string url, string eventType, bool success)
    {
        ArgumentException.ThrowIfNullOrEmpty(url);
        ArgumentException.ThrowIfNullOrEmpty(eventType);
        return $"Webhook delivery {(success ? "succeeded" : "failed")} for {eventType} to {url}";
    }
}