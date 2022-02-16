#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Represents configuration and metadata for saga event extensions.
/// Contains settings for event filtering, processing, and enrichment.
/// </summary>
public sealed record SagaEventExtensions
{
    /// <summary>
    /// Gets the minimum severity level for events to be included in processing.
    /// </summary>
    [JsonPropertyName("minSeverity")]
    public EventSeverity MinSeverity { get; init; } = EventSeverity.Information;

    /// <summary>
    /// Gets a value indicating whether error events should be automatically retried.
    /// </summary>
    [JsonPropertyName("retryErrors")]
    public bool RetryErrors { get; init; } = false;

    /// <summary>
    /// Gets the maximum number of retry attempts for failed events.
    /// </summary>
    [JsonPropertyName("maxRetryCount")]
    public int MaxRetryCount { get; init; } = 3;

    /// <summary>
    /// Gets a value indicating whether events should be enriched with additional metadata.
    /// </summary>
    [JsonPropertyName("enableEnrichment")]
    public bool EnableEnrichment { get; init; } = true;

    /// <summary>
    /// Gets custom tags to apply to events matching this configuration.
    /// </summary>
    [JsonPropertyName("tags")]
    public HashSet<string> Tags { get; init; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets event type filters to include specific event types only.
    /// </summary>
    [JsonPropertyName("includeEventTypes")]
    public HashSet<string> IncludeEventTypes { get; init; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets event type filters to exclude specific event types.
    /// </summary>
    [JsonPropertyName("excludeEventTypes")]
    public HashSet<string> ExcludeEventTypes { get; init; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets a value indicating whether to log event processing details.
    /// </summary>
    [JsonPropertyName("enableLogging")]
    public bool EnableLogging { get; init; } = true;

    /// <summary>
    /// Gets the maximum event age in hours before events are automatically archived.
    /// </summary>
    [JsonPropertyName("maxEventAgeHours")]
    public int MaxEventAgeHours { get; init; } = 24;

    /// <summary>
    /// Initializes a new instance of the <see cref="SagaEventExtensions"/> record.
    /// </summary>
    public SagaEventExtensions()
    {
    }

    /// <summary>
    /// Creates a default configuration for saga event extensions.
    /// </summary>
    /// <returns>A new SagaEventExtensions instance with default values</returns>
    public static SagaEventExtensions CreateDefault()
    {
        return new SagaEventExtensions
        {
            MinSeverity = EventSeverity.Information,
            RetryErrors = false,
            MaxRetryCount = 3,
            EnableEnrichment = true,
            Tags = new HashSet<string>(StringComparer.Ordinal),
            IncludeEventTypes = new HashSet<string>(StringComparer.Ordinal),
            ExcludeEventTypes = new HashSet<string>(StringComparer.Ordinal),
            EnableLogging = true,
            MaxEventAgeHours = 24
        };
    }
}