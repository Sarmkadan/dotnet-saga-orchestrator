#nullable enable

using FluentAssertions;
using SagaOrchestrator.Infrastructure.Resilience;
using Xunit;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Tests focused on the asynchronous open -> half-open -> closed recovery lifecycle of
/// <see cref="CircuitBreaker"/>, including how a probe success or failure moves the breaker
/// after the open window has elapsed. These use a one-second open window so the time-based
/// transition can be observed without a lengthy wait.
/// </summary>
public class CircuitBreakerRecoveryTests
{
    private const string Identifier = "payment-service";

    private static async Task TripOpenAsync(CircuitBreaker breaker, int failureThreshold)
    {
        for (var i = 0; i < failureThreshold; i++)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => breaker.ExecuteAsync<int>(
                    () => throw new InvalidOperationException("boom"), Identifier));
        }
    }

    /// <summary>
    /// After enough failures the breaker opens and rejects calls immediately; once the open window
    /// elapses it reports HalfOpen so a single trial request can be admitted.
    /// </summary>
    [Fact]
    public async Task Breaker_Opens_ThenBecomesHalfOpen_AfterTimeout()
    {
        var breaker = new CircuitBreaker(failureThreshold: 3, timeoutSeconds: 1);

        await TripOpenAsync(breaker, 3);
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.Open);

        // While open, calls are rejected without invoking the action.
        var invoked = false;
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync<int>(() =>
        {
            invoked = true;
            return Task.FromResult(1);
        }, Identifier));
        invoked.Should().BeFalse();

        // Wait past the open window; the breaker should now allow a probe.
        await Task.Delay(1200);
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.HalfOpen);
    }

    /// <summary>
    /// A successful probe in the half-open state closes the breaker and resumes normal traffic.
    /// </summary>
    [Fact]
    public async Task HalfOpenProbe_Success_ClosesBreaker()
    {
        var breaker = new CircuitBreaker(failureThreshold: 2, timeoutSeconds: 1);

        await TripOpenAsync(breaker, 2);
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.Open);

        await Task.Delay(1200);
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.HalfOpen);

        var result = await breaker.ExecuteAsync(() => Task.FromResult(42), Identifier);

        result.Should().Be(42);
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.Closed);
    }

    /// <summary>
    /// A failing probe in the half-open state sends the breaker straight back to open and
    /// restarts the wait, instead of prematurely resuming full traffic.
    /// </summary>
    [Fact]
    public async Task HalfOpenProbe_Failure_ReopensBreaker()
    {
        var breaker = new CircuitBreaker(failureThreshold: 2, timeoutSeconds: 1);

        await TripOpenAsync(breaker, 2);
        await Task.Delay(1200);
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.HalfOpen);

        // The probe fails.
        await Assert.ThrowsAsync<InvalidOperationException>(() => breaker.ExecuteAsync<int>(
            () => throw new InvalidOperationException("still down"), Identifier));

        // Immediately after the failed probe the breaker is open again.
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.Open);
    }

    /// <summary>
    /// Recovering one service must not affect the independent state of another; each identifier
    /// keeps its own breaker.
    /// </summary>
    [Fact]
    public async Task Recovery_IsIsolatedPerIdentifier()
    {
        var breaker = new CircuitBreaker(failureThreshold: 2, timeoutSeconds: 1);

        await TripOpenAsync(breaker, 2); // trips "payment-service"
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.Open);

        // A different service is still fully closed and usable.
        breaker.GetState("inventory-service").Should().Be(CircuitBreakerState.Closed);
        var ok = await breaker.ExecuteAsync(() => Task.FromResult(true), "inventory-service");
        ok.Should().BeTrue();
        breaker.GetState("inventory-service").Should().Be(CircuitBreakerState.Closed);

        // Recover the payment breaker; inventory is untouched.
        await Task.Delay(1200);
        await breaker.ExecuteAsync(() => Task.FromResult(1), Identifier);
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.Closed);
    }

    /// <summary>
    /// An explicit reset clears breaker state regardless of where it was in the recovery cycle.
    /// </summary>
    [Fact]
    public async Task Reset_ClearsOpenState_Immediately()
    {
        var breaker = new CircuitBreaker(failureThreshold: 2, timeoutSeconds: 60);

        await TripOpenAsync(breaker, 2);
        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.Open);

        breaker.Reset(Identifier);

        breaker.GetState(Identifier).Should().Be(CircuitBreakerState.Closed);
    }
}
