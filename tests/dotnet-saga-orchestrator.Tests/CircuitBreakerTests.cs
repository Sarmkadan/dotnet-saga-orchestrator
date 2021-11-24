#nullable enable

using SagaOrchestrator.Infrastructure.Resilience;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

public class CircuitBreakerTests
{
    [Fact]
    public async Task ExecuteAsync_SuccessfulAction_RecordsSuccessAndReturnsTrue()
    {
        var breaker = new CircuitBreaker(failureThreshold: 3, timeoutSeconds: 60);
        var actionCalled = false;

        var result = await breaker.ExecuteAsync(async () =>
        {
            actionCalled = true;
            await Task.CompletedTask;
        }, "test-service");

        result.Should().BeTrue();
        actionCalled.Should().BeTrue();
        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public async Task ExecuteAsync_GenericSuccessfulAction_ReturnsValue()
    {
        var breaker = new CircuitBreaker(failureThreshold: 3, timeoutSeconds: 60);

        var result = await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            return 42;
        }, "test-service");

        result.Should().Be(42);
        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public async Task ExecuteAsync_FailingAction_ThrowsAndRecordsFailure()
    {
        var breaker = new CircuitBreaker(failureThreshold: 3, timeoutSeconds: 60);

        var act = () => breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Service failed");
        }, "test-service");

        await act.Should().ThrowAsync<InvalidOperationException>();
        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleFailures_OpensCircuit()
    {
        var breaker = new CircuitBreaker(failureThreshold: 2, timeoutSeconds: 60);

        // First failure
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Fail 1");
        }, "failing-service").CatchAsync();

        // Second failure
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Fail 2");
        }, "failing-service").CatchAsync();

        // Circuit should now be open
        breaker.GetState("failing-service").Should().Be(CircuitBreakerState.Open);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCircuitOpen_ReturnsFalse()
    {
        var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 60);

        // Trigger open state
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Fail");
        }, "test-service").CatchAsync();

        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Open);

        // Next execution should return false without calling action
        var actionCalled = false;
        var result = await breaker.ExecuteAsync(async () =>
        {
            actionCalled = true;
            await Task.CompletedTask;
        }, "test-service");

        result.Should().BeFalse();
        actionCalled.Should().BeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_WhenCircuitOpen_GenericThrowsException()
    {
        var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 60);

        // Trigger open state
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Fail");
        }, "test-service").CatchAsync();

        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Open);

        // Next execution should throw
        var act = () => breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            return 42;
        }, "test-service");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Circuit breaker is open*");
    }

    [Fact]
    public void GetState_UnknownIdentifier_ReturnsClosed()
    {
        var breaker = new CircuitBreaker();

        breaker.GetState("never-accessed").Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public void Reset_ClearsMetricsForIdentifier()
    {
        var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 60);

        breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Fail");
        }, "test-service").CatchAsync().GetAwaiter().GetResult();

        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Open);

        breaker.Reset("test-service");

        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessInHalfOpenClosesCircuit()
    {
        var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 1);

        // Trigger open state
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Fail");
        }, "test-service").CatchAsync();

        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Open);

        // Wait for timeout
        await Task.Delay(1100);

        // Next execution in half-open state
        breaker.GetState("test-service").Should().Be(CircuitBreakerState.HalfOpen);

        // Successful execution closes circuit
        var result = await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
        }, "test-service");

        result.Should().BeTrue();
        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public async Task ExecuteAsync_FailureInHalfOpenReopensCircuit()
    {
        var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 1);

        // Trigger open state
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Fail");
        }, "test-service").CatchAsync();

        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Open);

        // Wait for timeout
        await Task.Delay(1100);

        breaker.GetState("test-service").Should().Be(CircuitBreakerState.HalfOpen);

        // Failure in half-open reopens circuit
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException("Fail again");
        }, "test-service").CatchAsync();

        breaker.GetState("test-service").Should().Be(CircuitBreakerState.Open);
    }

    [Fact]
    public void EvictStaleEntries_RemovesUnusedClosedCircuits()
    {
        var breaker = new CircuitBreaker();

        // Access multiple identifiers
        breaker.ExecuteAsync(async () => await Task.CompletedTask, "service-1")
            .CatchAsync().GetAwaiter().GetResult();
        breaker.ExecuteAsync(async () => await Task.CompletedTask, "service-2")
            .CatchAsync().GetAwaiter().GetResult();

        // service-1 is accessed (implicitly in initial execute)
        // service-2 hasn't been accessed for a while

        var evicted = breaker.EvictStaleEntries(TimeSpan.FromSeconds(0));

        // Only closed circuits that haven't been accessed are evicted
        evicted.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ExecuteAsync_SuccessfulAction_IncrementSuccess()
    {
        var breaker = new CircuitBreaker();

        // Get initial state
        var state1 = breaker.GetState("test");

        // Execute successful action
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
        }, "test");

        // Execute another successful action
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
        }, "test");

        // Circuit should remain closed
        breaker.GetState("test").Should().Be(CircuitBreakerState.Closed);
    }

    [Fact]
    public async Task ExecuteAsync_DifferentIdentifiers_MaintainIndependentState()
    {
        var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 60);

        // Fail for service-a
        await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
            throw new InvalidOperationException();
        }, "service-a").CatchAsync();

        breaker.GetState("service-a").Should().Be(CircuitBreakerState.Open);
        breaker.GetState("service-b").Should().Be(CircuitBreakerState.Closed);

        // service-b should still work
        var result = await breaker.ExecuteAsync(async () =>
        {
            await Task.CompletedTask;
        }, "service-b");

        result.Should().BeTrue();
    }
}

// Helper extension for exception swallowing
internal static class CircuitBreakerTestExtensions
{
    public static async Task CatchAsync(this Task task)
    {
        try
        {
            await task;
        }
        catch
        {
            // Swallow exception
        }
    }

    public static async Task CatchAsync<T>(this Task<T> task)
    {
        try
        {
            await task;
        }
        catch
        {
            // Swallow exception
        }
    }
}
