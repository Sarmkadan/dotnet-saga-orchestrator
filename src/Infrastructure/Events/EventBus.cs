#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Infrastructure.Events;

/// <summary>
/// In-memory event bus for pub/sub pattern implementation.
/// Allows saga events to be published and subscribed across the application.
/// </summary>
public interface IEventBus
{
    void Subscribe<T>(Func<T, Task> handler) where T : DomainEvent;
    void Unsubscribe<T>(Func<T, Task> handler) where T : DomainEvent;
    Task PublishAsync<T>(T @event) where T : DomainEvent;
    IReadOnlyList<DomainEvent> GetEventHistory();
    void ClearHistory();
}

public class EventBus : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers;
    private readonly List<DomainEvent> _eventHistory;
    private readonly object _lock = new();

    public EventBus()
    {
        _handlers = new();
        _eventHistory = new();
    }

    public void Subscribe<T>(Func<T, Task> handler) where T : DomainEvent
    {
        lock (_lock)
        {
            var eventType = typeof(T);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new();
            }
            _handlers[eventType].Add(handler);
        }
    }

    public void Unsubscribe<T>(Func<T, Task> handler) where T : DomainEvent
    {
        lock (_lock)
        {
            var eventType = typeof(T);
            if (_handlers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
            }
        }
    }

    public async Task PublishAsync<T>(T @event) where T : DomainEvent
    {
        // Record event in history
        lock (_lock)
        {
            _eventHistory.Add(@event);
        }

        // Get handlers for this event type
        List<Delegate>? handlers;
        lock (_lock)
        {
            _handlers.TryGetValue(typeof(T), out handlers);
        }

        if (handlers == null || handlers.Count == 0)
            return;

        // Execute all handlers, ensuring one faulty handler does not stop others
        var tasks = new List<Task>();
        foreach (var handler in handlers.Cast<Func<T, Task>>())
        {
            try
            {
                var task = handler(@event);
                // Guard against a handler that throws synchronously instead of returning a faulted Task
                if (task == null)
                {
                    tasks.Add(Task.CompletedTask);
                }
                else
                {
                    tasks.Add(task);
                }
            }
            catch
            {
                // Swallow synchronous exceptions so other handlers can continue
                // Optionally log the exception here
                tasks.Add(Task.CompletedTask);
            }
        }

        // Await all tasks, ignoring any exceptions they may produce
        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
            // Exceptions are intentionally ignored to keep the bus resilient
        }
    }

    public IReadOnlyList<DomainEvent> GetEventHistory()
    {
        lock (_lock)
        {
            return _eventHistory.AsReadOnly();
        }
    }

    public void ClearHistory()
    {
        lock (_lock)
        {
            _eventHistory.Clear();
        }
    }
}

public abstract class DomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty;

    protected DomainEvent()
    {
        EventType = GetType().Name;
    }
}

public class SagaCreatedEvent : DomainEvent
{
    public string SagaId { get; set; } = string.Empty;
    public string SagaName { get; set; } = string.Empty;
    public string DefinitionId { get; set; } = string.Empty;
    public int StepCount { get; set; }
}

public class SagaStepStartedEvent : DomainEvent
{
    public string SagaId { get; set; } = string.Empty;
    public string StepId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public int Order { get; set; }
}

public class SagaStepCompletedEvent : DomainEvent
{
    public string SagaId { get; set; } = string.Empty;
    public string StepId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public long DurationMs { get; set; }
}

public class SagaStepFailedEvent : DomainEvent
{
    public string SagaId { get; set; } = string.Empty;
    public string StepId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
}

public class SagaCompletedEvent : DomainEvent
{
    public string SagaId { get; set; } = string.Empty;
    public string SagaName { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public int CompletedSteps { get; set; }
    public int TotalSteps { get; set; }
}

public class SagaFailedEvent : DomainEvent
{
    public string SagaId { get; set; } = string.Empty;
    public string SagaName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string FailedStepName { get; set; } = string.Empty;
}

public class CompensationStartedEvent : DomainEvent
{
    public string SagaId { get; set; } = string.Empty;
    public string CompensationStrategy { get; set; } = string.Empty;
    public int StepsToCompensate { get; set; }
}

public class CompensationCompletedEvent : DomainEvent
{
    public string SagaId { get; set; } = string.Empty;
    public int CompensatedSteps { get; set; }
    public long DurationMs { get; set; }
}
