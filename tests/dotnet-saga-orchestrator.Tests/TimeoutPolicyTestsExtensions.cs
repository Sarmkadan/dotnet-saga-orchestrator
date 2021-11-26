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
        /// Returns a lenient timeout policy (300 seconds) using the production factory method.
        /// </summary>
        public static TimeoutPolicy GetLenientPolicy(this TimeoutPolicyTests _)
            => TimeoutPolicy.CreateLenient();

        /// <summary>
        /// Asserts that <see cref="TimeoutPolicy.HasExceeded(TimeSpan)"/> returns the expected result.
        /// </summary>
        public static void AssertHasExceeded(this TimeoutPolicyTests _, TimeoutPolicy policy, TimeSpan elapsed, bool expected)
        {
            bool actual = policy.HasExceeded(elapsed);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Asserts that the remaining time reported by the policy is within an acceptable tolerance.
        /// </summary>
        public static void AssertRemainingTime(
            this TimeoutPolicyTests _,
            TimeoutPolicy policy,
            DateTime start,
            TimeSpan expectedRemaining,
            TimeSpan? tolerance = null)
        {
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
        public static void AssertElapsedPercentage(
            this TimeoutPolicyTests _,
            TimeoutPolicy policy,
            DateTime start,
            double expectedPercentage,
            double tolerance = 0.01)
        {
            double actual = policy.GetElapsedPercentage(start);
            Assert.InRange(actual, expectedPercentage - tolerance, expectedPercentage + tolerance);
        }
    }
}
