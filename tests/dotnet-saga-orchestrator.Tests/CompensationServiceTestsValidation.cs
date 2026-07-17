using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Tests
{
    /// <summary>
    /// Provides validation extension methods for <see cref="CompensationServiceTests"/> instances.
    /// </summary>
    public static class CompensationServiceTestsValidation
    {
        /// <summary>
        /// Validates the <see cref="CompensationServiceTests"/> instance for common issues.
        /// Since <see cref="CompensationServiceTests"/> is a test class with no instance fields to validate,
        /// this method always returns an empty list of errors.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <returns>List of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this CompensationServiceTests value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // CompensationServiceTests is a test class with no instance fields to validate
            // All validation is done through its test methods

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the <see cref="CompensationServiceTests"/> instance is valid.
        /// </summary>
        /// <param name="value">The test instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this CompensationServiceTests value) =>
            Validate(value).Count == 0;

        /// <summary>
        /// Ensures the <see cref="CompensationServiceTests"/> instance is valid, throwing if not.
        /// </summary>
        /// <param name="value">The test instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if validation fails.</exception>
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