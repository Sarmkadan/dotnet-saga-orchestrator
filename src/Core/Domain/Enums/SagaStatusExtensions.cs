using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Core.Domain.Enums;

public static class SagaStatusExtensions
    {
        /// <summary>
        /// Determines whether the specified saga status is terminal (no further transitions possible).
        /// </summary>
        /// <param name="status">The saga status to check.</param>
        /// <returns>True if the status is terminal; otherwise, false.</returns>
        public static bool IsTerminal(this SagaStatus status)
        {
            return status switch
            {
                SagaStatus.Completed => true,
                SagaStatus.Failed => true,
                SagaStatus.Compensated => true,
                SagaStatus.Aborted => true,
                SagaStatus.TimedOut => true,
                _ => false
            };
        }

        /// <summary>
        /// Determines whether a transition from the current status to the target status is allowed.
        /// </summary>
        /// <param name="fromStatus">The current saga status.</param>
        /// <param name="toStatus">The target saga status.</param>
        /// <returns>True if the transition is allowed; otherwise, false.</returns>
        public static bool CanTransitionTo(this SagaStatus fromStatus, SagaStatus toStatus)
        {
            // Terminal states cannot transition to any other state
            if (fromStatus.IsTerminal())
                return false;

            // Define allowed transitions based on saga lifecycle
            return (fromStatus, toStatus) switch
            {
                // From Pending
                (SagaStatus.Pending, SagaStatus.Initialized) => true,

                // From Initialized
                (SagaStatus.Initialized, SagaStatus.Running) => true,

                // From Running
                (SagaStatus.Running, SagaStatus.Completed) => true,
                (SagaStatus.Running, SagaStatus.Failed) => true,
                (SagaStatus.Running, SagaStatus.Compensating) => true,
                (SagaStatus.Running, SagaStatus.Aborted) => true,
                (SagaStatus.Running, SagaStatus.TimedOut) => true,

                // From Compensating
                (SagaStatus.Compensating, SagaStatus.Compensated) => true,
                (SagaStatus.Compensating, SagaStatus.Failed) => true,

                // All other transitions are not allowed
                _ => false
            };
        }

        /// <summary>
        /// Converts the saga status to a user-friendly display string.
        /// </summary>
        /// <param name="status">The saga status to convert.</param>
        /// <returns>A string representing the status for display purposes.</returns>
        public static string ToDisplayString(this SagaStatus status)
        {
            return status switch
            {
                SagaStatus.Pending => "Pending",
                SagaStatus.Initialized => "Initialized",
                SagaStatus.Running => "Running",
                SagaStatus.Completed => "Completed",
                SagaStatus.Failed => "Failed",
                SagaStatus.Compensating => "Compensating",
                SagaStatus.Compensated => "Compensated",
                SagaStatus.Aborted => "Aborted",
                SagaStatus.TimedOut => "Timed Out",
                _ => status.ToString()
            };
        }
    }