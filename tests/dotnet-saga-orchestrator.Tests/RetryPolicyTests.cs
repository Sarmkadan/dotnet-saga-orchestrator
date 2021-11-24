#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Utilities;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

public class RetryPolicyTests
{
    [Fact]
    public void Constructor_NegativeMaxRetries_ThrowsArgumentException()
    {
        var act = () => new RetryPolicy(maxRetries: -1);

        act.Should().Throw<ArgumentException>().WithMessage("*negative*");
    }

    [Fact]
    public void Constructor_NegativeInitialDelay_ThrowsArgumentException()
    {
        var act = () => new RetryPolicy(maxRetries: 3, initialDelayMs: -100);

        act.Should().Throw<ArgumentException>().WithMessage("*negative*");
    }

    [Fact]
    public void Constructor_BackoffMultiplierBelowOne_ThrowsArgumentException()
    {
        var act = () => new RetryPolicy(maxRetries: 3, initialDelayMs: 1000, backoffMultiplier: 0.5);

        act.Should().Throw<ArgumentException>().WithMessage("*>= 1.0*");
    }

    [Fact]
    public void Constructor_MaxDelayLessThanInitialDelay_ThrowsArgumentException()
    {
        var act = () => new RetryPolicy(maxRetries: 3, initialDelayMs: 2000, backoffMultiplier: 2.0, maxDelayMs: 500);

        act.Should().Throw<ArgumentException>().WithMessage("*>= initial delay*");
    }

    [Fact]
    public void CalculateDelay_FirstAttempt_ReturnsInitialDelay()
    {
        var policy = new RetryPolicy(maxRetries: 3, initialDelayMs: 1000, backoffMultiplier: 2.0, maxDelayMs: 60000);

        policy.CalculateDelay(1).Should().Be(1000);
    }

    [Fact]
    public void CalculateDelay_SecondAttempt_AppliesExponentialBackoff()
    {
        var policy = new RetryPolicy(maxRetries: 3, initialDelayMs: 1000, backoffMultiplier: 2.0, maxDelayMs: 60000);

        policy.CalculateDelay(2).Should().Be(2000);
    }

    [Fact]
    public void CalculateDelay_ThirdAttempt_SquaresTheMultiplier()
    {
        var policy = new RetryPolicy(maxRetries: 3, initialDelayMs: 1000, backoffMultiplier: 2.0, maxDelayMs: 60000);

        policy.CalculateDelay(3).Should().Be(4000);
    }

    [Fact]
    public void CalculateDelay_LargeAttemptNumber_CapsAtMaxDelay()
    {
        var policy = new RetryPolicy(maxRetries: 5, initialDelayMs: 1000, backoffMultiplier: 10.0, maxDelayMs: 5000);

        policy.CalculateDelay(3).Should().Be(5000);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CalculateDelay_AttemptBelowOne_ThrowsArgumentException(int attempt)
    {
        var policy = new RetryPolicy(maxRetries: 3);

        var act = () => policy.CalculateDelay(attempt);

        act.Should().Throw<ArgumentException>().WithMessage("*>= 1*");
    }

    [Fact]
    public void CalculateDelay_AttemptExceedsMaxRetries_ThrowsInvalidOperationException()
    {
        var policy = new RetryPolicy(maxRetries: 2);

        var act = () => policy.CalculateDelay(3);

        act.Should().Throw<InvalidOperationException>().WithMessage("*maximum retry*");
    }

    [Fact]
    public void CreateLinear_AllAttemptsReturnSameFixedDelay()
    {
        var policy = RetryPolicy.CreateLinear(maxRetries: 3, delayMs: 500);

        policy.CalculateDelay(1).Should().Be(500);
        policy.CalculateDelay(2).Should().Be(500);
        policy.CalculateDelay(3).Should().Be(500);
    }

    [Fact]
    public void CreateNoRetry_SetsZeroMaxRetriesAndDelay()
    {
        var policy = RetryPolicy.CreateNoRetry();

        policy.MaxRetries.Should().Be(0);
        policy.InitialDelayMs.Should().Be(0);
        policy.CanRetry(0).Should().BeFalse();
    }

    [Fact]
    public void CreateExponential_UsesDoubleBackoffMultiplier()
    {
        var policy = RetryPolicy.CreateExponential(maxRetries: 3, initialDelayMs: 500);

        policy.BackoffMultiplier.Should().Be(2.0);
        policy.MaxRetries.Should().Be(3);
        policy.CalculateDelay(1).Should().Be(500);
        policy.CalculateDelay(2).Should().Be(1000);
    }

    [Fact]
    public void CanRetry_WhenBelowMaxRetries_ReturnsTrue()
    {
        var policy = new RetryPolicy(maxRetries: 3);

        policy.CanRetry(2).Should().BeTrue();
    }

    [Fact]
    public void CanRetry_WhenAtMaxRetries_ReturnsFalse()
    {
        var policy = new RetryPolicy(maxRetries: 3);

        policy.CanRetry(3).Should().BeFalse();
    }
}

public class SagaIdGeneratorAdditionalTests
{
    [Fact]
    public void GenerateSagaId_ReturnsIdWithSagaPrefix()
    {
        var id = SagaIdGenerator.GenerateSagaId();

        id.Should().StartWith("saga_");
        id.Length.Should().BeGreaterThan(5);
    }

    [Fact]
    public void GenerateSagaId_EachCallReturnsUniqueId()
    {
        var id1 = SagaIdGenerator.GenerateSagaId();
        var id2 = SagaIdGenerator.GenerateSagaId();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void GenerateCorrelationId_ReturnsIdWithCorrPrefix()
    {
        var id = SagaIdGenerator.GenerateCorrelationId();

        id.Should().StartWith("corr_");
        id.Length.Should().BeGreaterThan(5);
    }

    [Fact]
    public void GenerateStepId_ReturnsIdWithStepPrefix()
    {
        var id = SagaIdGenerator.GenerateStepId();

        id.Should().StartWith("step_");
    }

    [Fact]
    public void GenerateRequestId_ReturnsIdWithReqPrefix()
    {
        var id = SagaIdGenerator.GenerateRequestId();

        id.Should().StartWith("req_");
    }

    [Fact]
    public void GenerateRequestId_EachCallReturnsUniqueId()
    {
        var id1 = SagaIdGenerator.GenerateRequestId();
        var id2 = SagaIdGenerator.GenerateRequestId();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void IsValidSagaId_WithGeneratedId_ReturnsTrue()
    {
        var id = SagaIdGenerator.GenerateSagaId();

        SagaIdGenerator.IsValidSagaId(id).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-id")]
    [InlineData("corr_abc")]
    [InlineData("saga_")] // prefix only, no body
    public void IsValidSagaId_WithInvalidFormats_ReturnsFalse(string id)
    {
        SagaIdGenerator.IsValidSagaId(id).Should().BeFalse();
    }

    [Fact]
    public void IsValidCorrelationId_WithGeneratedId_ReturnsTrue()
    {
        var id = SagaIdGenerator.GenerateCorrelationId();

        SagaIdGenerator.IsValidCorrelationId(id).Should().BeTrue();
    }

    [Fact]
    public void IsValidCorrelationId_WithGuidString_ReturnsTrue()
    {
        var guid = Guid.NewGuid().ToString();

        SagaIdGenerator.IsValidCorrelationId(guid).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-correlation-id")]
    public void IsValidCorrelationId_WithInvalidId_ReturnsFalse(string id)
    {
        SagaIdGenerator.IsValidCorrelationId(id).Should().BeFalse();
    }
}
