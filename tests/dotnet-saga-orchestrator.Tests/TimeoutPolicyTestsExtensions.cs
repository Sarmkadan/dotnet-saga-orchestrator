using System;
using Xunit;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests
{
    /// <summary>
    /// Extension methods that make writing <see cref="TimeoutPolicyTests"/> clearer and more expressive.
    /// </summary>
    public static class TimeoutPolicyTestsExtensions
    {
        /// <summary>
        /// Returns a lenient timeout policy (300 seconds) using the production factory method.
        /// </summary>
        /// <param name="_">The test instance (discarded).</param>
        /// <returns>A lenient timeout policy with 300 seconds timeout.</returns>
        public static TimeoutPolicy GetLenientPolicy(this TimeoutPolicyTests _)
            => TimeoutPolicy.CreateLenient();

        /// <summary>
        /// Asserts that <see cref="TimeoutPolicy.HasExceeded(TimeSpan)"/> returns the expected result.
        /// </summary>
        /// <param name="_">The test instance (discarded).</param>
        /// <param name="policy">The timeout policy to test.</param>
        /// <param name="elapsed">The elapsed time to check against the policy.</param>
        /// <param name="expected">The expected result of the HasExceeded check.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is <see langword="null"/>.</exception>
        public static void AssertHasExceeded(this TimeoutPolicyTests _, TimeoutPolicy policy, TimeSpan elapsed, bool expected)
        {
            ArgumentNullException.ThrowIfNull(policy);

            bool actual = policy.HasExceeded(elapsed);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Asserts that the remaining time reported by the policy is within an acceptable tolerance.
        /// </summary>
        /// <param name="_">The test instance (discarded).</param>
        /// <param name="policy">The timeout policy to test.</param>
        /// <param name="start">The start time from which to calculate remaining time.</param>
        /// <param name="expectedRemaining">The expected remaining time.</param>
        /// <param name="tolerance">Optional tolerance for the comparison. Defaults to 1 second.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is <see langword="null"/>.</exception>
        public static void AssertRemainingTime(
            this TimeoutPolicyTests _,
            TimeoutPolicy policy,
            DateTime start,
            TimeSpan expectedRemaining,
            TimeSpan? tolerance = null)
        {
            ArgumentNullException.ThrowIfNull(policy);

            TimeSpan actual = policy.GetRemainingTime(start);
            TimeSpan tol = tolerance ?? TimeSpan.FromSeconds(1);
            Assert.InRange(
                actual.TotalSeconds,
                expectedRemaining.TotalSeconds - tol.TotalSeconds,
                expectedRemaining.TotalSeconds + tol.TotalSeconds);
        }

        /// <summary>
        /// Asserts that the elapsed percentage reported by the policy is close to the expected value.
        /// </summary>
        /// <param name="_">The test instance (discarded).</param>
        /// <param name="policy">The timeout policy to test.</param>
        /// <param name="start">The start time from which to calculate elapsed percentage.</param>
        /// <param name="expectedPercentage">The expected elapsed percentage (0-100).</param>
        /// <param name="tolerance">Optional tolerance for the comparison. Defaults to 0.01 (1%).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="policy"/> is <see langword="null"/>.</exception>
        public static void AssertElapsedPercentage(
            this TimeoutPolicyTests _,
            TimeoutPolicy policy,
            DateTime start,
            double expectedPercentage,
            double tolerance = 0.01)
        {
            ArgumentNullException.ThrowIfNull(policy);

            double actual = policy.GetElapsedPercentage(start);
            Assert.InRange(actual, expectedPercentage - tolerance, expectedPercentage + tolerance);
        }
    }
}
