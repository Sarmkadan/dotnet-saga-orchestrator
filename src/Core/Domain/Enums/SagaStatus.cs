// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Domain.Enums;

/// <summary>
/// Represents the overall status of a saga throughout its lifecycle.
/// </summary>
public enum SagaStatus
{
    /// <summary>
    /// Saga created but not yet initialized
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Saga initialized with definition
    /// </summary>
    Initialized = 1,

    /// <summary>
    /// Saga is actively executing steps
    /// </summary>
    Running = 2,

    /// <summary>
    /// All saga steps completed successfully
    /// </summary>
    Completed = 3,

    /// <summary>
    /// One or more steps failed
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Compensation in progress to rollback failed saga
    /// </summary>
    Compensating = 5,

    /// <summary>
    /// Compensation completed, saga rolled back
    /// </summary>
    Compensated = 6,

    /// <summary>
    /// Saga aborted manually
    /// </summary>
    Aborted = 7,

    /// <summary>
    /// Saga timed out
    /// </summary>
    TimedOut = 8
}
