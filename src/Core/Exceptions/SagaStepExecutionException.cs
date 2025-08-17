// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Thrown when a saga step fails during execution.
/// </summary>
public class SagaStepExecutionException : SagaException
{
    public string? StepName { get; }
    public int StepOrder { get; }

    public SagaStepExecutionException(string sagaId, string stepName, int stepOrder, string message)
        : base($"Step '{stepName}' (order {stepOrder}) failed in saga '{sagaId}': {message}",
               sagaId, "STEP_EXECUTION_FAILED")
    {
        StepName = stepName;
        StepOrder = stepOrder;
    }

    public SagaStepExecutionException(string sagaId, string stepName, int stepOrder, string message,
                                     Exception? innerException)
        : base($"Step '{stepName}' (order {stepOrder}) failed in saga '{sagaId}': {message}",
               sagaId, "STEP_EXECUTION_FAILED", innerException)
    {
        StepName = stepName;
        StepOrder = stepOrder;
    }
}
