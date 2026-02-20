// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Represents a domain event emitted during saga execution.
/// Used for audit trails, monitoring, and event-driven patterns.
/// </summary>
public class SagaEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("sagaId")]
    public string SagaId { get; set; }

    [JsonPropertyName("eventType")]
    public string EventType { get; set; }

    [JsonPropertyName("eventName")]
    public string EventName { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("severity")]
    public EventSeverity Severity { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, object> Data { get; set; } = new();

    [JsonPropertyName("stepId")]
    public string? StepId { get; set; }

    [JsonPropertyName("stepName")]
    public string? StepName { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    // Constructor
    public SagaEvent()
    {
        Id = Guid.NewGuid().ToString();
        Timestamp = DateTime.UtcNow;
        EventType = "SagaEvent";
        EventName = "Generic Event";
        Source = "SagaOrchestrator";
        Severity = EventSeverity.Information;
        Description = string.Empty;
    }

    /// <summary>
    /// Creates a saga lifecycle event
    /// </summary>
    public static SagaEvent CreateLifecycleEvent(string sagaId, string eventName, string description)
    {
        return new SagaEvent
        {
            SagaId = sagaId,
            EventType = "Lifecycle",
            EventName = eventName,
            Description = description,
            Severity = EventSeverity.Information
        };
    }

    /// <summary>
    /// Creates a step execution event
    /// </summary>
    public static SagaEvent CreateStepEvent(string sagaId, string stepId, string stepName, string eventName, string description)
    {
        return new SagaEvent
        {
            SagaId = sagaId,
            StepId = stepId,
            StepName = stepName,
            EventType = "StepExecution",
            EventName = eventName,
            Description = description,
            Severity = EventSeverity.Information
        };
    }

    /// <summary>
    /// Creates an error event
    /// </summary>
    public static SagaEvent CreateErrorEvent(string sagaId, string stepName, string errorMessage)
    {
        return new SagaEvent
        {
            SagaId = sagaId,
            StepName = stepName,
            EventType = "Error",
            EventName = "ExecutionError",
            Description = errorMessage,
            Severity = EventSeverity.Error
        };
    }

    /// <summary>
    /// Adds data to the event
    /// </summary>
    public void AddData(string key, object value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key cannot be null or empty", nameof(key));

        Data[key] = value;
    }
}

/// <summary>
/// Event severity levels
/// </summary>
public enum EventSeverity
{
    Information = 0,
    Warning = 1,
    Error = 2,
    Critical = 3
}
