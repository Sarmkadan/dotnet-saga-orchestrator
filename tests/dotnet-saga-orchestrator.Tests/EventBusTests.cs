#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SagaOrchestrator.Infrastructure.Events;
using Xunit;

namespace DotnetSagaOrchestrator.Tests;

/// <summary>
/// Tests for the in‑memory <see cref="EventBus"/> implementation.
/// </summary>
public class EventBusTests
{
    private sealed class TestEvent : DomainEvent
    {
        public string? Payload { get; set; }
    }

    [Fact]
    public async Task Subscriber_Receives_Published_Event()
    {
        var bus = new EventBus();
        var received = false;

        bus.Subscribe<TestEvent>(_ =>
        {
            received = true;
            return Task.CompletedTask;
        });

        await bus.PublishAsync(new TestEvent { Payload = "hello" });

        Assert.True(received);
    }

    [Fact]
    public async Task Multiple_Subscribers_All_Receive_Event()
    {
        var bus = new EventBus();
        var callCount = 0;

        bus.Subscribe<TestEvent>(_ =>
        {
            Interlocked.Increment(ref callCount);
            return Task.CompletedTask;
        });

        bus.Subscribe<TestEvent>(_ =>
        {
            Interlocked.Increment(ref callCount);
            return Task.CompletedTask;
        });

        await bus.PublishAsync(new TestEvent());

        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task Unsubscribe_Stops_Delivery_To_Handler()
    {
        var bus = new EventBus();
        var called = false;
        Func<TestEvent, Task> handler = _ =>
        {
            called = true;
            return Task.CompletedTask;
        };

        bus.Subscribe(handler);
        bus.Unsubscribe(handler);

        await bus.PublishAsync(new TestEvent());

        Assert.False(called);
    }

    [Fact]
    public async Task Publishing_With_No_Subscribers_Does_Not_Throw()
    {
        var bus = new EventBus();

        // Should complete without throwing an exception
        await bus.PublishAsync(new TestEvent());
    }

    [Fact]
    public async Task Throwing_Subscriber_Does_Not_Prevent_Others_From_Receiving()
    {
        var bus = new EventBus();
        var successfulCalls = 0;

        // This subscriber throws an exception
        bus.Subscribe<TestEvent>(_ => throw new Exception("boom"));

        // This subscriber should still be invoked
        bus.Subscribe<TestEvent>(_ =>
        {
            Interlocked.Increment(ref successfulCalls);
            return Task.CompletedTask;
        });

        // Publish and swallow any exception from the bus
        try
        {
            await bus.PublishAsync(new TestEvent());
        }
        catch
        {
            // Ignored – we only care that the non‑throwing subscriber ran
        }

        Assert.Equal(1, successfulCalls);
    }
}
