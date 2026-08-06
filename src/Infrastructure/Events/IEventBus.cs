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