#nullable enable

using SagaOrchestrator.Core.Utilities;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

public class TimeoutPolicyTests
{
    [Fact]
    public void Constructor_ValidSeconds_InitializesCorrectly()
    {
        var policy = new TimeoutPolicy(60);

        policy.TimeoutSeconds.Should().Be(60);
        policy.Timeout.Should().Be(TimeSpan.FromSeconds(60));
        policy.IsRelaxed.Should().BeFalse();
    }

    [Fact]
    public void Constructor_LargeTimeout_MarksAsRelaxed()
    {
        var policy = new TimeoutPolicy(300);

        policy.IsRelaxed.Should().BeTrue();
    }

    [Fact]
    public void Constructor_SmallTimeout_NotRelaxed()
    {
        var policy = new TimeoutPolicy(299);

        policy.IsRelaxed.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ZeroOrNegative_ThrowsArgumentException()
    {
        var act = () => new TimeoutPolicy(0);
        act.Should().Throw<ArgumentException>().WithMessage("*positive*");

        act = () => new TimeoutPolicy(-50);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void HasExceeded_WhenElapsedEqualsTimeout_ReturnsTrue()
    {
        var policy = new TimeoutPolicy(60);
        var elapsed = TimeSpan.FromSeconds(60);

        policy.HasExceeded(elapsed).Should().BeTrue();
    }

    [Fact]
    public void HasExceeded_WhenElapsedLessThanTimeout_ReturnsFalse()
    {
        var policy = new TimeoutPolicy(60);
        var elapsed = TimeSpan.FromSeconds(59);

        policy.HasExceeded(elapsed).Should().BeFalse();
    }

    [Fact]
    public void HasExceeded_WhenElapsedExceedsTimeout_ReturnsTrue()
    {
        var policy = new TimeoutPolicy(60);
        var elapsed = TimeSpan.FromSeconds(120);

        policy.HasExceeded(elapsed).Should().BeTrue();
    }

    [Fact]
    public void HasExceeded_WithBuffer_AdjustsThreshold()
    {
        var policy = new TimeoutPolicy(60);
        var buffer = TimeSpan.FromSeconds(10);
        var elapsed = TimeSpan.FromSeconds(52);

        policy.HasExceeded(elapsed, buffer).Should().BeTrue();
    }

    [Fact]
    public void HasExceeded_WithBuffer_StillBelowThreshold()
    {
        var policy = new TimeoutPolicy(60);
        var buffer = TimeSpan.FromSeconds(10);
        var elapsed = TimeSpan.FromSeconds(49);

        policy.HasExceeded(elapsed, buffer).Should().BeFalse();
    }

    [Fact]
    public void GetRemainingTime_ReturnsCorrectTimeLeft()
    {
        var policy = new TimeoutPolicy(10);
        var startTime = DateTime.UtcNow.AddSeconds(-5);

        var remaining = policy.GetRemainingTime(startTime);

        remaining.Should().BeLessThanOrEqualTo(TimeSpan.FromSeconds(5));
        remaining.Should().BeGreaterThan(TimeSpan.FromSeconds(4));
    }

    [Fact]
    public void GetRemainingTime_AfterTimeout_ReturnsZero()
    {
        var policy = new TimeoutPolicy(5);
        var startTime = DateTime.UtcNow.AddSeconds(-10);

        var remaining = policy.GetRemainingTime(startTime);

        remaining.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void HasSufficientTime_WithEnoughTime_ReturnsTrue()
    {
        var policy = new TimeoutPolicy(100);
        var startTime = DateTime.UtcNow.AddSeconds(-50);
        var requiredTime = TimeSpan.FromSeconds(40);

        policy.HasSufficientTime(startTime, requiredTime).Should().BeTrue();
    }

    [Fact]
    public void HasSufficientTime_WithInsufficientTime_ReturnsFalse()
    {
        var policy = new TimeoutPolicy(100);
        var startTime = DateTime.UtcNow.AddSeconds(-50);
        var requiredTime = TimeSpan.FromSeconds(60);

        policy.HasSufficientTime(startTime, requiredTime).Should().BeFalse();
    }

    [Fact]
    public void GetElapsedPercentage_AtStart_ReturnsNearZero()
    {
        var policy = new TimeoutPolicy(60);
        var startTime = DateTime.UtcNow;

        var percentage = policy.GetElapsedPercentage(startTime);

        percentage.Should().BeLessThan(1);
    }

    [Fact]
    public void GetElapsedPercentage_MidTimeout_ReturnsApproxFifty()
    {
        var policy = new TimeoutPolicy(10);
        var startTime = DateTime.UtcNow.AddSeconds(-5);

        var percentage = policy.GetElapsedPercentage(startTime);

        percentage.Should().BeGreaterThan(45);
        percentage.Should().BeLessThan(55);
    }

    [Fact]
    public void GetElapsedPercentage_AfterTimeout_ReturnsCappedAtHundred()
    {
        var policy = new TimeoutPolicy(5);
        var startTime = DateTime.UtcNow.AddSeconds(-10);

        var percentage = policy.GetElapsedPercentage(startTime);

        percentage.Should().Be(100);
    }

    [Fact]
    public void CreateLenient_CreatesThreeHundredSecondPolicy()
    {
        var policy = TimeoutPolicy.CreateLenient();

        policy.TimeoutSeconds.Should().Be(300);
        policy.IsRelaxed.Should().BeTrue();
    }

    [Fact]
    public void CreateStandard_CreatesOneMinutePolicy()
    {
        var policy = TimeoutPolicy.CreateStandard();

        policy.TimeoutSeconds.Should().Be(60);
        policy.IsRelaxed.Should().BeFalse();
    }

    [Fact]
    public void CreateStrict_CreatesTenSecondPolicy()
    {
        var policy = TimeoutPolicy.CreateStrict();

        policy.TimeoutSeconds.Should().Be(10);
        policy.IsRelaxed.Should().BeFalse();
    }

    [Fact]
    public void Create_CustomSeconds_CreatesCorrectPolicy()
    {
        var policy = TimeoutPolicy.Create(45);

        policy.TimeoutSeconds.Should().Be(45);
        policy.Timeout.Should().Be(TimeSpan.FromSeconds(45));
    }
}
