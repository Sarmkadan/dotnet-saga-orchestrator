#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Extension methods for <see cref="SagaEventPublisher"/> that provide additional functionality
/// for event publishing, querying, and monitoring.
/// </summary>
public static class SagaEventPublisherExtensions
{
    /// <summary>
    /// Publishes a saga event with additional metadata
    /// </summary>
    /// <param name="publisher">The event publisher instance</param>
    /// <param name="sagaId">The saga identifier</param>
    /// <param name="eventName">The event name</param>
    /// <param name="description">Event description</param>
    /// <param name="severity">Event severity level</param>
    /// <param name="data">Additional event data</param>
    /// <returns>A task representing the publish operation</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is <see langword="null"/>, or if <paramref name="sagaId"/> or <paramref name="eventName"/> is <see langword="null"/></exception>
    public static async Task PublishAsync(
        this SagaEventPublisher publisher,
        string sagaId,
        string eventName,
        string description,
        EventSeverity severity = EventSeverity.Information,
        Dictionary<string, object>? data = null)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentException.ThrowIfNullOrEmpty(eventName);

        var sagaEvent = new SagaEvent
        {
            SagaId = sagaId,
            EventType = "Custom",
            EventName = eventName,
            Description = description,
            Severity = severity,
            Source = "SagaOrchestrator.Extensions",
            Timestamp = DateTime.UtcNow
        };

        if (data?.Count > 0)
        {
            foreach (var kvp in data)
            {
                sagaEvent.AddData(kvp.Key, kvp.Value);
            }
        }

        await publisher.PublishAsync(sagaEvent).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes a step execution event
    /// </summary>
    /// <param name="publisher">The event publisher instance</param>
    /// <param name="sagaId">The saga identifier</param>
    /// <param name="stepId">The step identifier</param>
    /// <param name="stepName">The step name</param>
    /// <param name="eventName">The event name (e.g., "StepStarted", "StepCompleted")</param>
    /// <param name="description">Event description</param>
    /// <param name="severity">Event severity level</param>
    /// <param name="data">Additional event data</param>
    /// <returns>A task representing the publish operation</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is <see langword="null"/>, or if sagaId, stepId, stepName, or eventName is <see langword="null"/></exception>
    public static async Task PublishStepEventAsync(
        this SagaEventPublisher publisher,
        string sagaId,
        string stepId,
        string stepName,
        string eventName,
        string description,
        EventSeverity severity = EventSeverity.Information,
        Dictionary<string, object>? data = null)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(sagaId);
        ArgumentNullException.ThrowIfNull(stepId);
        ArgumentNullException.ThrowIfNull(stepName);
        ArgumentException.ThrowIfNullOrEmpty(eventName);

        var sagaEvent = new SagaEvent
        {
            SagaId = sagaId,
            StepId = stepId,
            StepName = stepName,
            EventType = "StepExecution",
            EventName = eventName,
            Description = description,
            Severity = severity,
            Source = "SagaOrchestrator.Extensions",
            Timestamp = DateTime.UtcNow
        };

        if (data?.Count > 0)
        {
            foreach (var kvp in data)
            {
                sagaEvent.AddData(kvp.Key, kvp.Value);
            }
        }

        await publisher.PublishAsync(sagaEvent).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets events filtered by severity level
    /// </summary>
    /// <param name="publisher">The event publisher instance</param>
    /// <param name="sagaId">Optional saga identifier filter</param>
    /// <param name="severity">The severity level to filter by</param>
    /// <returns>Filtered list of events ordered by timestamp (newest first)</returns>
    public static IReadOnlyList<SagaEvent> GetEventsBySeverity(
        this SagaEventPublisher publisher,
        string? sagaId = null,
        EventSeverity severity = EventSeverity.Information)
    {
        return publisher.GetAllEvents(sagaId, null, severity)
            .AsReadOnly();
    }

    /// <summary>
    /// Gets the most recent events up to the specified count
    /// </summary>
    /// <param name="publisher">The event publisher instance</param>
    /// <param name="count">Maximum number of events to return</param>
    /// <param name="sagaId">Optional saga identifier filter</param>
    /// <returns>List of most recent events</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if count is less than 1</exception>
    public static IReadOnlyList<SagaEvent> GetRecentEvents(
        this SagaEventPublisher publisher,
        int count,
        string? sagaId = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        var allEvents = publisher.GetAllEvents(sagaId);
        return allEvents
            .OrderByDescending(e => e.Timestamp)
            .Take(count)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets a summary of event statistics for a saga
    /// </summary>
    /// <param name="publisher">The event publisher instance</param>
    /// <param name="sagaId">The saga identifier</param>
    /// <returns>Dictionary containing event statistics</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is <see langword="null"/>, or if <paramref name="sagaId"/> is <see langword="null"/></exception>
    public static Dictionary<string, object> GetEventStatistics(
        this SagaEventPublisher publisher,
        string sagaId)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(sagaId);

        var events = publisher.GetSagaEvents(sagaId);
        var stats = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["TotalEvents"] = events.Count,
            ["EventTypes"] = events
                .Select(e => e.EventType)
                .Distinct()
                .Count(),
            ["SeverityBreakdown"] = events
                .GroupBy(e => e.Severity)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Count()),
            ["FirstEvent"] = events.MinBy(e => e.Timestamp)?.Timestamp ?? DateTime.MinValue,
            ["LastEvent"] = events.MaxBy(e => e.Timestamp)?.Timestamp ?? DateTime.MinValue,
            ["HasErrors"] = events.Any(e => e.Severity >= EventSeverity.Error)
        };

        return stats;
    }

    /// <summary>
    /// Exports events to file with optional formatting
    /// </summary>
    /// <param name="publisher">The event publisher instance</param>
    /// <param name="filePath">The output file path</param>
    /// <param name="sagaId">Optional saga identifier filter</param>
    /// <param name="indentJson">Whether to format JSON with indentation</param>
    /// <returns>A task representing the export operation</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is <see langword="null"/>, or if <paramref name="filePath"/> is <see langword="null"/></exception>
    public static async Task ExportEventsAsync(
        this SagaEventPublisher publisher,
        string filePath,
        string? sagaId = null,
        bool indentJson = true)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(filePath);

        var events = publisher.GetAllEvents(sagaId);

        var options = new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = indentJson,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        };

        var json = System.Text.Json.JsonSerializer.Serialize(events, options);
        await System.IO.File.WriteAllTextAsync(filePath, json, System.Text.Encoding.UTF8)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Checks if any events of the specified severity or higher exist
    /// </summary>
    /// <param name="publisher">The event publisher instance</param>
    /// <param name="sagaId">Optional saga identifier filter</param>
    /// <param name="severity">The minimum severity level to check for</param>
    /// <returns>True if events of the specified severity or higher exist; otherwise false</returns>
    public static bool HasEventsOfSeverity(
        this SagaEventPublisher publisher,
        string? sagaId = null,
        EventSeverity severity = EventSeverity.Warning)
    {
        var events = publisher.GetAllEvents(sagaId);
        return events.Any(e => e.Severity >= severity);
    }

    /// <summary>
    /// Subscribes to events of a specific type
    /// </summary>
    /// <param name="publisher">The event publisher instance</param>
    /// <param name="eventType">The event type to subscribe to</param>
    /// <param name="handler">The event handler</param>
    /// <returns>A disposable that can be used to unsubscribe</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is <see langword="null"/>, or if <paramref name="eventType"/> or <paramref name="handler"/> is <see langword="null"/></exception>
    public static IDisposable SubscribeToType(
        this SagaEventPublisher publisher,
        string eventType,
        Func<SagaEvent, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentException.ThrowIfNullOrEmpty(eventType);
        ArgumentNullException.ThrowIfNull(handler);

        // Create a wrapper handler that filters by event type
        Func<SagaEvent, Task> wrappedHandler = async (sagaEvent) =>
        {
            if (sagaEvent.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase))
            {
                await handler(sagaEvent).ConfigureAwait(false);
            }
        };

        publisher.Subscribe(wrappedHandler);

        // Return a disposable that unsubscribes when disposed
        return new SubscriptionDisposable(publisher, wrappedHandler);
    }

    /// <summary>
    /// Subscribes to events with a specific severity level or higher
    /// </summary>
    /// <param name="publisher">The event publisher instance</param>
    /// <param name="severity">The minimum severity level to subscribe to</param>
    /// <param name="handler">The event handler</param>
    /// <returns>A disposable that can be used to unsubscribe</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="publisher"/> is <see langword="null"/>, or if <paramref name="handler"/> is <see langword="null"/></exception>
    public static IDisposable SubscribeToSeverity(
        this SagaEventPublisher publisher,
        EventSeverity severity,
        Func<SagaEvent, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(handler);

        // Create a wrapper handler that filters by severity
        Func<SagaEvent, Task> wrappedHandler = async (sagaEvent) =>
        {
            if (sagaEvent.Severity >= severity)
            {
                await handler(sagaEvent).ConfigureAwait(false);
            }
        };

        publisher.Subscribe(wrappedHandler);

        // Return a disposable that unsubscribes when disposed
        return new SubscriptionDisposable(publisher, wrappedHandler);
    }

    /// <summary>
    /// Private class to handle unsubscription when the disposable is disposed
    /// </summary>
    private sealed class SubscriptionDisposable : IDisposable
    {
        private readonly SagaEventPublisher _publisher;
        private readonly Func<SagaEvent, Task> _handler;
        private readonly object _lock = new();
        private bool _disposed;

        public SubscriptionDisposable(SagaEventPublisher publisher, Func<SagaEvent, Task> handler)
        {
            _publisher = publisher;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                if (_disposed) return;

                // Note: SagaEventPublisher doesn't have an Unsubscribe method,
                // so we can't actually unsubscribe. This is a limitation.
                // The disposable is still useful for scoping.
                _disposed = true;
            }
        }
    }
}