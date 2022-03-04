using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Tests
{
    public static class CompensationServiceTestsValidation
    {
        /// <summary>
        /// Validates the CompensationServiceTests instance for common issues.
        /// </summary>
        /// <param name="value">The test instance to validate</param>
        /// <returns>List of validation errors; empty if valid</returns>
        public static IReadOnlyList<string> Validate(this CompensationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // CompensationServiceTests is a test class with no instance fields to validate
            // All validation is done through its test methods

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the CompensationServiceTests instance is valid.
        /// </summary>
        /// <param name="value">The test instance to check</param>
        /// <returns>True if valid; false otherwise</returns>
        public static bool IsValid(this CompensationServiceTests value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the CompensationServiceTests instance is valid, throwing if not.
        /// </summary>
        /// <param name="value">The test instance to validate</param>
        /// <exception cref="ArgumentException">Thrown if validation fails</exception>
        public static void EnsureValid(this CompensationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"CompensationServiceTests is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
            }
        }
    }
}