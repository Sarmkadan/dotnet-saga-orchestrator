// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Domain.Enums;

/// <summary>
/// Represents the status of an individual step within a saga.
/// </summary>
public enum SagaStepStatus
{
    /// <summary>
    /// Step waiting to be executed
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Step is currently executing
    /// </summary>
    Executing = 1,

    /// <summary>
    /// Step completed successfully
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Step execution failed
    /// </summary>
    Failed = 3,

    /// <summary>
    /// Step waiting for retry after failure
    /// </summary>
    WaitingForRetry = 4,

    /// <summary>
    /// Step has been compensated
    /// </summary>
    Compensated = 5,

    /// <summary>
    /// Step execution timed out
    /// </summary>
    TimedOut = 6,

    /// <summary>
    /// Step execution was skipped
    /// </summary>
    Skipped = 7
}
