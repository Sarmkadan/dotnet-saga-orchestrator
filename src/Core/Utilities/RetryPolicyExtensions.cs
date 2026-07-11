using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SagaOrchestrator.Core.Utilities
{
    /// <summary>
    /// Extension methods for <see cref="RetryPolicy"/> to enhance retry behavior configuration and inspection.
    /// </summary>
    public static class RetryPolicyExtensions
    {
        /// <summary>
        /// Generates a sequence of retry delays for each attempt up to the maximum number of retries.
        /// </summary>
        /// <param name="policy">The retry policy to calculate delays for.</param>
        /// <returns>An <see cref="IReadOnlyList{Int32}"/> containing the calculated delay in milliseconds for each retry attempt.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is null.</exception>
        public static IReadOnlyList<int> GetRetryDelays(this RetryPolicy policy)
        {
            ArgumentNullException.ThrowIfNull(policy);

            var delays = new List<int>(policy.MaxRetries);
            for (int attempt = 1; attempt <= policy.MaxRetries; attempt++)
            {
                delays.Add(policy.CalculateDelay(attempt));
            }
            return new ReadOnlyCollection<int>(delays);
        }

        /// <summary>
        /// Generates a human-readable description of the retry policy.
        /// </summary>
        /// <param name="policy">The retry policy to describe.</param>
        /// <returns>A string describing the policy's configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is null.</exception>
        public static string ToDescription(this RetryPolicy policy)
        {
            ArgumentNullException.ThrowIfNull(policy);

            return $"RetryPolicy: MaxRetries={policy.MaxRetries}, InitialDelay={policy.InitialDelayMs}ms, " +
                   $"BackoffMultiplier={policy.BackoffMultiplier:F2}, MaxDelay={policy.MaxDelayMs}ms, " +
                   $"UseJitter={policy.UseJitter}";
        }
    }
}
