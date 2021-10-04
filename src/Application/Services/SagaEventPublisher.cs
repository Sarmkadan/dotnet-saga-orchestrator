#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Service for publishing and managing saga domain events.
/// Maintains event audit trail for monitoring and compliance.
/// </summary>
public class SagaEventPublisher
{
    private readonly List<SagaEvent> _events = new();
    private readonly object _lockObject = new();
    private readonly List<Func<SagaEvent, Task>> _subscribers = new();

    /// <summary>
    /// Subscribes to all saga events
    /// </summary>
    public void Subscribe(Func<SagaEvent, Task> handler)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        lock (_lockObject)
        {
            _subscribers.Add(handler);
        }
    }

    /// <summary>
    /// Publishes a saga event to all subscribers
    /// </summary>
    public async Task PublishAsync(SagaEvent sagaEvent)
    {
        if (sagaEvent == null)
            throw new ArgumentNullException(nameof(sagaEvent));

        lock (_lockObject)
        {
            _events.Add(sagaEvent);
        }

        // Notify all subscribers
        var subscribers = GetSubscribers();
        var tasks = subscribers.Select(handler => handler(sagaEvent));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes multiple events
    /// </summary>
    public async Task PublishAsync(params SagaEvent[] events)
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));

        var tasks = events.Select(PublishAsync);
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets all events for a saga
    /// </summary>
    public List<SagaEvent> GetSagaEvents(string sagaId)
    {
        if (string.IsNullOrEmpty(sagaId))
            return new List<SagaEvent>();

        lock (_lockObject)
        {
            return _events.Where(e => e.SagaId == sagaId).OrderBy(e => e.Timestamp).ToList();
        }
    }

    /// <summary>
    /// Gets events filtered by type
    /// </summary>
    public List<SagaEvent> GetEventsByType(string sagaId, string eventType)
    {
        if (string.IsNullOrEmpty(sagaId))
            return new List<SagaEvent>();

        lock (_lockObject)
        {
            return _events
                .Where(e => e.SagaId == sagaId && e.EventType == eventType)
                .OrderBy(e => e.Timestamp)
                .ToList();
        }
    }

    /// <summary>
    /// Gets all events with optional filtering
    /// </summary>
    public List<SagaEvent> GetAllEvents(string? sagaId = null, string? eventType = null, EventSeverity? severity = null)
    {
        lock (_lockObject)
        {
            var results = _events.AsEnumerable();

            if (!string.IsNullOrEmpty(sagaId))
                results = results.Where(e => e.SagaId == sagaId);

            if (!string.IsNullOrEmpty(eventType))
                results = results.Where(e => e.EventType == eventType);

            if (severity.HasValue)
                results = results.Where(e => e.Severity == severity.Value);

            return results.OrderByDescending(e => e.Timestamp).ToList();
        }
    }

    /// <summary>
    /// Gets the count of events
    /// </summary>
    public int GetEventCount(string? sagaId = null)
    {
        lock (_lockObject)
        {
            if (string.IsNullOrEmpty(sagaId))
                return _events.Count;

            return _events.Count(e => e.SagaId == sagaId);
        }
    }

    /// <summary>
    /// Clears all events (for testing)
    /// </summary>
    public void ClearEvents()
    {
        lock (_lockObject)
        {
            _events.Clear();
        }
    }

    /// <summary>
    /// Exports events to file (stub)
    /// </summary>
    public async Task ExportEventsAsync(string filePath, string? sagaId = null)
    {
        var events = GetAllEvents(sagaId);

        var json = System.Text.Json.JsonSerializer.Serialize(events, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        await System.IO.File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
    }

    // Private helper methods

    private List<Func<SagaEvent, Task>> GetSubscribers()
    {
        lock (_lockObject)
        {
            return new List<Func<SagaEvent, Task>>(_subscribers);
        }
    }
}
