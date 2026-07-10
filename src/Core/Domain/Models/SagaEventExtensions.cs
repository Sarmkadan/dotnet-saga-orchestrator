#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Extension methods for SagaEvent providing additional functionality
/// for event filtering, searching, and utility operations.
/// </summary>
public static class SagaEventExtensions
{
    /// <summary>
    /// Filters events by severity level
    /// </summary>
    /// <param name="events">Collection of events to filter</param>
    /// <param name="severity">Minimum severity level to include</param>
    /// <returns>Filtered events with severity >= specified level</returns>
    public static IEnumerable<SagaEvent> FilterBySeverity(this IEnumerable<SagaEvent> events, EventSeverity severity)
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));

        return events.Where(e => e.Severity >= severity);
    }

    /// <summary>
    /// Finds the first error or critical event in a sequence
    /// </summary>
    /// <param name="events">Collection of events to search</param>
    /// <returns>First error/critical event or null if none found</returns>
    public static SagaEvent? FindFirstError(this IEnumerable<SagaEvent> events)
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));

        return events.FirstOrDefault(e => e.Severity >= EventSeverity.Error);
    }

    /// <summary>
    /// Checks if any event in the collection has the specified event type
    /// </summary>
    /// <param name="events">Collection of events to check</param>
    /// <param name="eventType">Event type to search for</param>
    /// <returns>True if any event matches the type, false otherwise</returns>
    public static bool HasEventType(this IEnumerable<SagaEvent> events, string eventType)
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));
        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));

        return events.Any(e => string.Equals(e.EventType, eventType, StringComparison.Ordinal));
    }

    /// <summary>
    /// Gets all events for a specific saga
    /// </summary>
    /// <param name="events">Collection of events to filter</param>
    /// <param name="sagaId">Saga ID to match</param>
    /// <returns>Events belonging to the specified saga</returns>
    public static IEnumerable<SagaEvent> ForSaga(this IEnumerable<SagaEvent> events, string sagaId)
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));

        return events.Where(e => string.Equals(e.SagaId, sagaId, StringComparison.Ordinal));
    }
}