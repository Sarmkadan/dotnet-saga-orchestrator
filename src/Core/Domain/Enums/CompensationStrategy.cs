// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Domain.Enums;

/// <summary>
/// Defines the strategy for compensating failed saga steps.
/// </summary>
public enum CompensationStrategy
{
    /// <summary>
    /// Compensate in reverse order of completion (LIFO - Last In, First Out)
    /// </summary>
    ReverseOrder = 0,

    /// <summary>
    /// Compensate in forward order of execution (FIFO - First In, First Out)
    /// </summary>
    ForwardOrder = 1,

    /// <summary>
    /// Compensate only the failed step and subsequent steps
    /// </summary>
    FromFailurePoint = 2,

    /// <summary>
    /// Compensate all steps in parallel
    /// </summary>
    Parallel = 3,

    /// <summary>
    /// Manual compensation with external intervention
    /// </summary>
    Manual = 4
}
