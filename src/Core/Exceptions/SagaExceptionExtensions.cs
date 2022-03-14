using System;

namespace SagaOrchestrator.Core.Exceptions
{
    /// <summary>
    /// Provides extension methods for <see cref="SagaException"/> to facilitate common exception handling scenarios.
    /// </summary>
    public static class SagaExceptionExtensions
    {
        /// <summary>
        /// Determines whether the exception represents a saga not found scenario.
        /// </summary>
        /// <param name="ex">The exception to check. Cannot be null.</param>
        /// <returns>True if the error code is "SAGA_NOT_FOUND"; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
        /// <remarks>This method checks the error code of the exception to determine if it represents a saga not found scenario.</remarks>
        public static bool IsSagaNotFound(this SagaException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return ex.ErrorCode == "SAGA_NOT_FOUND";
        }

        /// <summary>
        /// Determines whether the exception represents a saga timeout scenario.
        /// </summary>
        /// <param name="ex">The exception to check. Cannot be null.</param>
        /// <returns>True if the error code is "SAGA_TIMEOUT"; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
        /// <remarks>This method checks the error code of the exception to determine if it represents a saga timeout scenario.</remarks>
        public static bool IsSagaTimeout(this SagaException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return ex.ErrorCode == "SAGA_TIMEOUT";
        }

        /// <summary>
        /// Creates a detailed message string containing all relevant saga exception information.
        /// </summary>
        /// <param name="ex">The exception containing the saga information. Cannot be null.</param>
        /// <returns>A formatted string with saga ID, error code, and message.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex"/> is null.</exception>
        /// <remarks>This method formats a string with the saga ID, error code, and message from the exception.</remarks>
        public static string GetDetailedMessage(this SagaException ex)
        {
            ArgumentNullException.ThrowIfNull(ex);
            return $"Saga Id: {ex.SagaId}, Error Code: {ex.ErrorCode}, Message: {ex.Message}";
        }
    }
}
