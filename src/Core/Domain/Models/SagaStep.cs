// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Represents a single step execution within a saga.
/// Each step can succeed, fail, or be compensated.
/// </summary>
public class SagaStep
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("sagaId")]
    public string SagaId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("status")]
    public SagaStepStatus Status { get; set; }

    [JsonPropertyName("serviceUrl")]
    public string ServiceUrl { get; set; }

    [JsonPropertyName("compensationUrl")]
    public string CompensationUrl { get; set; }

    [JsonPropertyName("payload")]
    public Dictionary<string, object> Payload { get; set; } = new();

    [JsonPropertyName("response")]
    public Dictionary<string, object> Response { get; set; } = new();

    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("compensatedAt")]
    public DateTime? CompensatedAt { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    [JsonPropertyName("maxRetries")]
    public int MaxRetries { get; set; } = 3;

    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; set; } = 30;

    // Constructor
    public SagaStep()
    {
        Id = Guid.NewGuid().ToString();
        Status = SagaStepStatus.Pending;
    }

    /// <summary>
    /// Initializes step with required parameters
    /// </summary>
    public void Initialize(string name, int order, string serviceUrl, string compensationUrl)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Order = order;
        ServiceUrl = serviceUrl ?? throw new ArgumentNullException(nameof(serviceUrl));
        CompensationUrl = compensationUrl ?? throw new ArgumentNullException(nameof(compensationUrl));
    }

    /// <summary>
    /// Marks step as started
    /// </summary>
    public void Start()
    {
        if (Status != SagaStepStatus.Pending && Status != SagaStepStatus.WaitingForRetry)
            throw new InvalidOperationException($"Cannot start step in {Status} status");

        Status = SagaStepStatus.Executing;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks step as successfully completed
    /// </summary>
    public void Complete(Dictionary<string, object>? response = null)
    {
        Status = SagaStepStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        if (response != null)
            Response = new Dictionary<string, object>(response);
    }

    /// <summary>
    /// Marks step as failed with error details
    /// </summary>
    public void Fail(string errorMessage, Dictionary<string, object>? response = null)
    {
        Status = SagaStepStatus.Failed;
        ErrorMessage = errorMessage;

        if (response != null)
            Response = new Dictionary<string, object>(response);
    }

    /// <summary>
    /// Marks step as ready for retry
    /// </summary>
    public void PrepareForRetry()
    {
        if (!CanRetry())
            throw new InvalidOperationException("Step has exceeded maximum retries");

        RetryCount++;
        Status = SagaStepStatus.WaitingForRetry;
    }

    /// <summary>
    /// Marks step as compensated
    /// </summary>
    public void Compensate()
    {
        if (Status != SagaStepStatus.Completed)
            throw new InvalidOperationException("Can only compensate completed steps");

        Status = SagaStepStatus.Compensated;
        CompensatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if step can be retried
    /// </summary>
    public bool CanRetry()
    {
        return RetryCount < MaxRetries && Status == SagaStepStatus.Failed;
    }

    /// <summary>
    /// Checks if step has exceeded timeout
    /// </summary>
    public bool IsTimedOut()
    {
        if (StartedAt == null)
            return false;

        var elapsed = DateTime.UtcNow - StartedAt.Value;
        return elapsed.TotalSeconds > TimeoutSeconds && Status == SagaStepStatus.Executing;
    }

    /// <summary>
    /// Sets payload data for the step
    /// </summary>
    public void SetPayload(Dictionary<string, object> data)
    {
        Payload = data ?? throw new ArgumentNullException(nameof(data));
    }
}
