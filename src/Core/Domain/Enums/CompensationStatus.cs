#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Domain.Enums;

/// <summary>
/// Represents the status of a compensation transaction.
/// </summary>
public enum CompensationStatus
{
    /// <summary>
    /// Compensation transaction created but not started
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Compensation is currently in progress
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Compensation completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Compensation failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Compensation timed out
    /// </summary>
    TimedOut = 4,

    /// <summary>
    /// Compensation was skipped
    /// </summary>
    Skipped = 5
}
