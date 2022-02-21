using System;
using System.Collections.Generic;
using System.Globalization;
using SagaOrchestrator.Benchmarks;

namespace SagaOrchestrator.Benchmarks
{
    /// <summary>
    /// Provides validation helpers for <see cref="SagaOrchestratorBenchmarks"/> instances.
    /// </summary>
    public static class SagaOrchestratorBenchmarksValidation
    {
        /// <summary>
        /// Validates the specified <see cref="SagaOrchestratorBenchmarks"/> instance.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <returns>A list of validation errors; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        public static IReadOnlyList<string> Validate(this SagaOrchestratorBenchmarks value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate ServiceProvider
            if (value._serviceProvider is null)
            {
                errors.Add("ServiceProvider is null");
            }

            // Validate DefinitionService
            if (value._definitionService is null)
            {
                errors.Add("DefinitionService is null");
            }

            // Validate OrchestrationService
            if (value._orchestrationService is null)
            {
                errors.Add("OrchestrationService is null");
            }

            // Validate SagaStepCount
            if (value.SagaStepCount < 1)
            {
                errors.Add($"SagaStepCount must be at least 1, but was {value.SagaStepCount}");
            }

            // Validate IterationCount
            if (value.IterationCount < 1)
            {
                errors.Add($"IterationCount must be at least 1, but was {value.IterationCount}");
            }

            return errors.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="SagaOrchestratorBenchmarks"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to check.</param>
        /// <returns>True if valid; otherwise, false.</returns>
        public static bool IsValid(this SagaOrchestratorBenchmarks value)
        {
            return value.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="SagaOrchestratorBenchmarks"/> instance is valid.
        /// </summary>
        /// <param name="value">The instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid, containing a list of validation errors.</exception>
        public static void EnsureValid(this SagaOrchestratorBenchmarks value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = value.Validate();

            if (errors.Count > 0)
            {
                throw new ArgumentException(
                    $"SagaOrchestratorBenchmarks instance is invalid. Errors: {string.Join("; ", errors)}");
            }
        }
    }
}
