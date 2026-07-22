#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Tests for RetryPolicy behavior including success scenarios, retry exhaustion, delay growth, and cancellation.
/// </summary>
public class RetryPolicyTestsBehavior
{
    /// <summary>
    /// Tests that a successful operation on first try doesn't require any retry logic.
    /// </summary>
    [Fact]
    public void CalculateDelay_SuccessFirstTry_NoRetryLogicApplied()
    {
        // Arrange
        var policy = new RetryPolicy(maxRetries: 3, initialDelayMs: 1000);

        // Act - first attempt should return initial delay
        var delay = policy.CalculateDelay(1);

        // Assert - delay should be initial delay, no backoff applied
        delay.Should().Be(1000);
    }

    /// <summary>
    /// Tests that when retries are exhausted, CalculateDelay throws InvalidOperationException.
    /// </summary>
    [Fact]
    public void CalculateDelay_RetriesExhausted_ThrowsInvalidOperationException()
    {
        // Arrange
        var policy = new RetryPolicy(maxRetries: 2, initialDelayMs: 1000);

        // Act & Assert - attempting beyond max retries should throw
        var act = () => policy.CalculateDelay(3);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*maximum retry*");
    }

    /// <summary>
    /// Tests that delay grows exponentially with each retry attempt.
    /// </summary>
    [Fact]
    public void CalculateDelay_DelayGrowsExponentially()
    {
        // Arrange
        var policy = new RetryPolicy(maxRetries: 5, initialDelayMs: 100, backoffMultiplier: 2.0);

        // Act & Assert - verify exponential growth pattern
        policy.CalculateDelay(1).Should().Be(100);   // 100 * 2^0 = 100
        policy.CalculateDelay(2).Should().Be(200);   // 100 * 2^1 = 200
        policy.CalculateDelay(3).Should().Be(400);   // 100 * 2^2 = 400
        policy.CalculateDelay(4).Should().Be(800);   // 100 * 2^3 = 800
        policy.CalculateDelay(5).Should().Be(1600);  // 100 * 2^4 = 1600
    }

    /// <summary>
    /// Tests that delay is capped at MaxDelayMs even with exponential growth.
    /// </summary>
    [Fact]
    public void CalculateDelay_DelayCappedAtMaxDelay()
    {
        // Arrange - policy with small max delay
        var policy = new RetryPolicy(
            maxRetries: 10,
            initialDelayMs: 1000,
            backoffMultiplier: 2.0,
            maxDelayMs: 5000
        );

        // Act & Assert - delays should cap at maxDelayMs
        policy.CalculateDelay(1).Should().Be(1000);
        policy.CalculateDelay(2).Should().Be(2000);
        policy.CalculateDelay(3).Should().Be(4000);
        policy.CalculateDelay(4).Should().Be(5000); // Capped here
        policy.CalculateDelay(5).Should().Be(5000); // Still capped
        policy.CalculateDelay(6).Should().Be(5000); // Still capped
    }

    /// <summary>
    /// Tests that UseJitter adds random variation to delays.
    /// </summary>
    [Fact]
    public void CalculateDelay_WithJitter_AppliesRandomVariation()
    {
        // Arrange
        var policy = new RetryPolicy(
            maxRetries: 3,
            initialDelayMs: 1000,
            backoffMultiplier: 2.0,
            maxDelayMs: 60000,
            useJitter: true
        );

        // Act - call multiple times to verify jitter is applied
        var delay1 = policy.CalculateDelay(1);
        var delay2 = policy.CalculateDelay(1);
        var delay3 = policy.CalculateDelay(1);

        // Assert - delays should vary due to jitter (±25%)
        // Jitter range: 750ms to 1250ms for initial delay
        delay1.Should().BeInRange(750, 1250);
        delay2.Should().BeInRange(750, 1250);
        delay3.Should().BeInRange(750, 1250);

        // All three should not be identical (high probability)
        (delay1 == delay2 && delay2 == delay3).Should().BeFalse();
    }

    /// <summary>
    /// Tests that CanRetry returns true for attempts within max retries.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CanRetry_WithinMaxRetries_ReturnsTrue(int currentAttempt)
    {
        // Arrange
        var policy = new RetryPolicy(maxRetries: 3);

        // Act & Assert
        policy.CanRetry(currentAttempt).Should().BeTrue();
    }

    /// <summary>
    /// Tests that CanRetry returns false when current attempt equals max retries.
    /// </summary>
    [Fact]
    public void CanRetry_AtMaxRetries_ReturnsFalse()
    {
        // Arrange
        var policy = new RetryPolicy(maxRetries: 3);

        // Act & Assert
        policy.CanRetry(3).Should().BeFalse();
    }

    /// <summary>
    /// Tests that CanRetry returns false for attempts beyond max retries.
    /// </summary>
    [Fact]
    public void CanRetry_BeyondMaxRetries_ReturnsFalse()
    {
        // Arrange
        var policy = new RetryPolicy(maxRetries: 3);

        // Act & Assert
        policy.CanRetry(4).Should().BeFalse();
        policy.CanRetry(5).Should().BeFalse();
    }

    /// <summary>
    /// Tests that CreateExponentialWithJitter factory method creates policy with jitter enabled.
    /// </summary>
    [Fact]
    public void CreateExponentialWithJitter_JitterEnabled()
    {
        // Arrange & Act
        var policy = RetryPolicy.CreateExponentialWithJitter(maxRetries: 3, initialDelayMs: 1000);

        // Assert
        policy.UseJitter.Should().BeTrue();
        policy.MaxRetries.Should().Be(3);
        policy.InitialDelayMs.Should().Be(1000);
        policy.BackoffMultiplier.Should().Be(2.0);
        policy.MaxDelayMs.Should().Be(60000);
    }

    /// <summary>
    /// Tests that delays grow linearly for linear retry policy.
    /// </summary>
    [Fact]
    public void CreateLinear_DelaysGrowLinearly()
    {
        // Arrange
        var policy = RetryPolicy.CreateLinear(maxRetries: 5, delayMs: 250);

        // Act & Assert - all delays should be the same for linear policy
        policy.CalculateDelay(1).Should().Be(250);
        policy.CalculateDelay(2).Should().Be(250);
        policy.CalculateDelay(3).Should().Be(250);
        policy.CalculateDelay(4).Should().Be(250);
        policy.CalculateDelay(5).Should().Be(250);
    }

    /// <summary>
    /// Tests that default RetryPolicy has expected default values.
    /// </summary>
    [Fact]
    public void DefaultConstructor_UsesExpectedDefaultValues()
    {
        // Arrange & Act
        var policy = new RetryPolicy();

        // Assert
        policy.MaxRetries.Should().Be(3);
        policy.InitialDelayMs.Should().Be(1000);
        policy.BackoffMultiplier.Should().Be(2.0);
        policy.MaxDelayMs.Should().Be(60000);
        policy.UseJitter.Should().BeFalse();
    }

    /// <summary>
    /// Tests that custom RetryPolicy with all parameters set correctly.
    /// </summary>
    [Fact]
    public void CustomConstructor_SetsAllPropertiesCorrectly()
    {
        // Arrange & Act
        var policy = new RetryPolicy(
            maxRetries: 5,
            initialDelayMs: 500,
            backoffMultiplier: 3.0,
            maxDelayMs: 10000,
            useJitter: true
        );

        // Assert
        policy.MaxRetries.Should().Be(5);
        policy.InitialDelayMs.Should().Be(500);
        policy.BackoffMultiplier.Should().Be(3.0);
        policy.MaxDelayMs.Should().Be(10000);
        policy.UseJitter.Should().BeTrue();
    }
}
