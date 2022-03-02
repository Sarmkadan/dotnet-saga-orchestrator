using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SagaOrchestrator.Data.Repositories
{
    /// <summary>
    /// Provides validation helpers for <see cref="InMemoryCompensationTransactionRepository"/>.
    /// </summary>
    public static class InMemoryCompensationTransactionRepositoryValidation
    {
        /// <summary>
        /// Validates the specified <see cref="InMemoryCompensationTransactionRepository"/> instance.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this InMemoryCompensationTransactionRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Repository-level validations would go here if there were any state to validate
            // For now, we validate the public contract through the methods

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="InMemoryCompensationTransactionRepository"/> instance is valid.
        /// </summary>
        /// <param name="value">The repository instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this InMemoryCompensationTransactionRepository value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="InMemoryCompensationTransactionRepository"/> instance is valid.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this InMemoryCompensationTransactionRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"Repository is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}