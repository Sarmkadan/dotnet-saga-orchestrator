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
    private const StringComparison OrdinalComparison = StringComparison.Ordinal;

    /// <summary>
    /// Filters events by severity level.
    /// </summary>
    /// <param name="events">Collection of events to filter. Cannot be null.</param>
    /// <param name="severity">Minimum severity level to include.</param>
    /// <returns>Filtered events with severity greater than or equal to the specified level.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="events"/> is null.</exception>
    public static IEnumerable<SagaEvent> FilterBySeverity(this IEnumerable<SagaEvent> events, EventSeverity severity)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return events.Where(e => e.Severity >= severity);
    }

    /// <summary>
    /// Finds the first error or critical event in a sequence.
    /// </summary>
    /// <param name="events">Collection of events to search. Cannot be null.</param>
    /// <returns>The first error or critical event, or null if none found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="events"/> is null.</exception>
    public static SagaEvent? FindFirstError(this IEnumerable<SagaEvent> events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return events.FirstOrDefault(e => e.Severity >= EventSeverity.Error);
    }

    /// <summary>
    /// Checks if any event in the collection has the specified event type.
    /// </summary>
    /// <param name="events">Collection of events to check. Cannot be null.</param>
    /// <param name="eventType">Event type to search for. Cannot be null or whitespace.</param>
    /// <returns>True if any event matches the type; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="events"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="eventType"/> is null or whitespace.</exception>
    public static bool HasEventType(this IEnumerable<SagaEvent> events, string eventType)
    {
        return events is not null && !string.IsNullOrWhiteSpace(eventType)
            && events.Any(e => string.Equals(e.EventType, eventType, OrdinalComparison));
    }

    /// <summary>
    /// Gets all events for a specific saga.
    /// </summary>
    /// <param name="events">Collection of events to filter. Cannot be null.</param>
    /// <param name="sagaId">Saga ID to match. Cannot be null or whitespace.</param>
    /// <returns>Events belonging to the specified saga.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="events"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> is null or whitespace.</exception>
    public static IEnumerable<SagaEvent> ForSaga(this IEnumerable<SagaEvent> events, string sagaId)
    {
        return events is not null && !string.IsNullOrWhiteSpace(sagaId)
            ? events.Where(e => string.Equals(e.SagaId, sagaId, OrdinalComparison))
            : throw new ArgumentException("Saga ID cannot be null or empty", nameof(sagaId));
    }
}
