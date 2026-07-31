using System;
using Xunit;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests
{
    public class TimeoutPolicyExtensionsTests
    {
        [Fact]
        public void GetDescription_ReturnsStrict_ForTimeoutLessThanOrEqualTo10Seconds()
        {
            var policy = TimeoutPolicy.Create(5);
            var result = policy.GetDescription();
            Assert.Equal("Strict (≤10s)", result);
        }

        [Fact]
        public void GetDescription_ReturnsStandard_ForTimeoutLessThanOrEqualTo1Minute()
        {
            var policy = TimeoutPolicy.Create(30);
            var result = policy.GetDescription();
            Assert.Equal("Standard (≤1m)", result);
        }

        [Fact]
        public void GetDescription_ReturnsModerate_ForTimeoutLessThanOrEqualTo5Minutes()
        {
            var policy = TimeoutPolicy.Create(120);
            var result = policy.GetDescription();
            Assert.Equal("Moderate (≤5m)", result);
        }

        [Fact]
        public void GetDescription_ReturnsLenient_ForTimeoutGreaterThan5Minutes()
        {
            var policy = TimeoutPolicy.Create(600);
            var result = policy.GetDescription();
            Assert.StartsWith("Lenient (>5m", result);
        }

        [Fact]
        public void GetDescription_ThrowsArgumentNullException_ForNullPolicy()
        {
            TimeoutPolicy? policy = null;
            Assert.Throws<ArgumentNullException>(() => policy!.GetDescription());
        }

        [Fact]
        public void IsApproachingTimeout_ReturnsTrue_WhenThresholdExceeded()
        {
            var policy = TimeoutPolicy.Create(10);
            var startTime = DateTime.UtcNow.AddSeconds(-9); // 90% elapsed
            var result = policy.IsApproachingTimeout(startTime, 80.0);
            Assert.True(result);
        }

        [Fact]
        public void IsApproachingTimeout_ReturnsFalse_WhenThresholdNotMet()
        {
            var policy = TimeoutPolicy.Create(10);
            var startTime = DateTime.UtcNow.AddSeconds(-1); // 10% elapsed
            var result = policy.IsApproachingTimeout(startTime, 80.0);
            Assert.False(result);
        }

        [Fact]
        public void IsApproachingTimeout_ThrowsArgumentOutOfRangeException_ForInvalidThreshold()
        {
            var policy = TimeoutPolicy.Create(10);
            Assert.Throws<ArgumentOutOfRangeException>(() => policy.IsApproachingTimeout(DateTime.UtcNow, 150.0));
        }

        [Fact]
        public void GetWarningThresholds_ReturnsCorrectIntervals()
        {
            var policy = TimeoutPolicy.Create(10);
            var result = policy.GetWarningThresholds(3);
            Assert.Equal(3, result.Count);
            Assert.Equal(25.0, result[0]);
            Assert.Equal(50.0, result[1]);
            Assert.Equal(75.0, result[2]);
        }

        [Fact]
        public void GetWarningThresholds_ThrowsArgumentOutOfRangeException_ForNonPositiveCount()
        {
            var policy = TimeoutPolicy.Create(10);
            Assert.Throws<ArgumentOutOfRangeException>(() => policy.GetWarningThresholds(0));
        }

        [Fact]
        public void WithMultiplier_ReturnsNewPolicyWithAdjustedTimeout()
        {
            var policy = TimeoutPolicy.Create(10);
            var newPolicy = policy.WithMultiplier(2.0);
            Assert.Equal(20, newPolicy.TimeoutSeconds);
        }

        [Fact]
        public void WithMultiplier_ThrowsArgumentOutOfRangeException_ForNonPositiveMultiplier()
        {
            var policy = TimeoutPolicy.Create(10);
            Assert.Throws<ArgumentOutOfRangeException>(() => policy.WithMultiplier(-1.0));
        }
    }
}
