#nullable enable
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
/// Represents a distributed saga that coordinates a business transaction across multiple microservices.
/// Sagas ensure eventual consistency using compensating transactions.
/// </summary>
public class Saga
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; }

    [JsonPropertyName("status")]
    public SagaStatus Status { get; set; }

    [JsonPropertyName("definition")]
    public SagaDefinition Definition { get; set; }

    [JsonPropertyName("steps")]
    public List<SagaStep> Steps { get; set; } = new();

    [JsonPropertyName("startedAt")]
    public DateTime StartedAt { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("failedAt")]
    public DateTime? FailedAt { get; set; }

    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; set; }

    [JsonPropertyName("compensationStartedAt")]
    public DateTime? CompensationStartedAt { get; set; }

    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    [JsonPropertyName("maxRetries")]
    public int MaxRetries { get; set; } = 3;

    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; set; } = 300;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Constructor
    public Saga()
    {
        Id = Guid.NewGuid().ToString();
        CorrelationId = Guid.NewGuid().ToString();
        Status = SagaStatus.Pending;
        StartedAt = DateTime.UtcNow;
        Definition = new SagaDefinition();
    }

    /// <summary>
    /// Initializes saga with definition and configuration
    /// </summary>
    public void Initialize(SagaDefinition definition, int maxRetries = 3, int timeoutSeconds = 300)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        MaxRetries = maxRetries;
        TimeoutSeconds = timeoutSeconds;
        Status = SagaStatus.Initialized;
    }

    /// <summary>
    /// Marks saga as started and begins step execution
    /// </summary>
    public void Start()
    {
        if (Status != SagaStatus.Initialized)
            throw new InvalidOperationException($"Cannot start saga in {Status} status");

        Status = SagaStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks saga as successfully completed
    /// </summary>
    public void Complete()
    {
        Status = SagaStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks saga as failed and initiates compensation
    /// </summary>
    public void Fail(string reason)
    {
        Status = SagaStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;
    }

    /// <summary>
    /// Begins compensation process for rollback
    /// </summary>
    public void BeginCompensation()
    {
        if (Status != SagaStatus.Failed)
            throw new InvalidOperationException("Can only compensate failed sagas");

        Status = SagaStatus.Compensating;
        CompensationStartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Completes compensation process
    /// </summary>
    public void CompleteCompensation()
    {
        Status = SagaStatus.Compensated;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if saga has exceeded timeout
    /// </summary>
    public bool IsTimedOut()
    {
        var elapsed = DateTime.UtcNow - StartedAt;
        return elapsed.TotalSeconds > TimeoutSeconds && Status == SagaStatus.Running;
    }

    /// <summary>
    /// Checks if saga can be retried
    /// </summary>
    public bool CanRetry()
    {
        return Status == SagaStatus.Failed && RetryCount < MaxRetries;
    }
}
