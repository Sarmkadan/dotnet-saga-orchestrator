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
/// Represents a compensating transaction that undoes a completed saga step.
/// Enables rollback and eventual consistency in distributed transactions.
/// </summary>
public class CompensationTransaction
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("sagaId")]
    public string SagaId { get; set; }

    [JsonPropertyName("stepId")]
    public string StepId { get; set; }

    [JsonPropertyName("stepName")]
    public string StepName { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("status")]
    public CompensationStatus Status { get; set; }

    [JsonPropertyName("compensationUrl")]
    public string CompensationUrl { get; set; }

    [JsonPropertyName("requestPayload")]
    public Dictionary<string, object> RequestPayload { get; set; } = new();

    [JsonPropertyName("responsePayload")]
    public Dictionary<string, object> ResponsePayload { get; set; } = new();

    [JsonPropertyName("initiatedAt")]
    public DateTime InitiatedAt { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("failedAt")]
    public DateTime? FailedAt { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    [JsonPropertyName("maxRetries")]
    public int MaxRetries { get; set; } = 3;

    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; set; } = 30;

    // Constructor
    public CompensationTransaction()
    {
        Id = Guid.NewGuid().ToString();
        Status = CompensationStatus.Pending;
        InitiatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Initializes compensation for a saga step
    /// </summary>
    public void Initialize(string sagaId, string stepId, string stepName, int order, string compensationUrl)
    {
        SagaId = sagaId ?? throw new ArgumentNullException(nameof(sagaId));
        StepId = stepId ?? throw new ArgumentNullException(nameof(stepId));
        StepName = stepName ?? throw new ArgumentNullException(nameof(stepName));
        Order = order;
        CompensationUrl = compensationUrl ?? throw new ArgumentNullException(nameof(compensationUrl));
    }

    /// <summary>
    /// Marks compensation as started
    /// </summary>
    public void Start()
    {
        if (Status != CompensationStatus.Pending)
            throw new InvalidOperationException($"Cannot start compensation in {Status} status");

        Status = CompensationStatus.InProgress;
    }

    /// <summary>
    /// Marks compensation as successfully completed
    /// </summary>
    public void Complete(Dictionary<string, object>? response = null)
    {
        Status = CompensationStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        if (response != null)
            ResponsePayload = new Dictionary<string, object>(response);
    }

    /// <summary>
    /// Marks compensation as failed
    /// </summary>
    public void Fail(string errorMessage, Dictionary<string, object>? response = null)
    {
        Status = CompensationStatus.Failed;
        FailedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;

        if (response != null)
            ResponsePayload = new Dictionary<string, object>(response);
    }

    /// <summary>
    /// Prepares compensation for retry
    /// </summary>
    public void PrepareForRetry()
    {
        if (!CanRetry())
            throw new InvalidOperationException("Compensation has exceeded maximum retries");

        RetryCount++;
        Status = CompensationStatus.Pending;
    }

    /// <summary>
    /// Checks if compensation can be retried
    /// </summary>
    public bool CanRetry()
    {
        return RetryCount < MaxRetries && Status == CompensationStatus.Failed;
    }

    /// <summary>
    /// Checks if compensation has exceeded timeout
    /// </summary>
    public bool IsTimedOut()
    {
        var elapsed = DateTime.UtcNow - InitiatedAt;
        return elapsed.TotalSeconds > TimeoutSeconds && Status == CompensationStatus.InProgress;
    }

    /// <summary>
    /// Sets the request payload for compensation call
    /// </summary>
    public void SetRequestPayload(Dictionary<string, object> payload)
    {
        RequestPayload = payload ?? throw new ArgumentNullException(nameof(payload));
    }
}
