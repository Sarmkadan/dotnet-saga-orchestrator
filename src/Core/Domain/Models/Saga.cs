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
    /// <summary>Gets or sets the unique identifier of the saga.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>Gets or sets the correlation identifier for tracing across services.</summary>
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; }

    /// <summary>Gets or sets the current status of the saga.</summary>
    [JsonPropertyName("status")]
    public SagaStatus Status { get; set; }

    /// <summary>Gets or sets the definition of the saga.</summary>
    [JsonPropertyName("definition")]
    public SagaDefinition Definition { get; set; }

    /// <summary>Gets or sets the list of steps in the saga.</summary>
    [JsonPropertyName("steps")]
    public List<SagaStep> Steps { get; set; } = new();

    /// <summary>Gets or sets the time the saga was started.</summary>
    [JsonPropertyName("startedAt")]
    public DateTime StartedAt { get; set; }

    /// <summary>Gets or sets the time the saga was completed, if applicable.</summary>
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>Gets or sets the time the saga failed, if applicable.</summary>
    [JsonPropertyName("failedAt")]
    public DateTime? FailedAt { get; set; }

    /// <summary>Gets or sets the reason for failure, if applicable.</summary>
    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; set; }

    /// <summary>Gets or sets the time compensation started.</summary>
    [JsonPropertyName("compensationStartedAt")]
    public DateTime? CompensationStartedAt { get; set; }

    /// <summary>Gets or sets the number of retry attempts made.</summary>
    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    /// <summary>Gets or sets the maximum number of retries allowed.</summary>
    [JsonPropertyName("maxRetries")]
    public int MaxRetries { get; set; } = 3;

    /// <summary>Gets or sets the overall timeout for the saga in seconds.</summary>
    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; set; } = 300;

    /// <summary>Gets or sets metadata associated with the saga.</summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>Convenience accessor for the underlying saga definition's name.</summary>
    [JsonIgnore]
    public string Name => Definition.Name;

    /// <summary>Convenience accessor for the underlying saga definition's identifier.</summary>
    [JsonIgnore]
    public string DefinitionId => Definition.Id;

    /// <summary>Convenience accessor for when the saga was created (alias of <see cref="StartedAt"/>).</summary>
    [JsonIgnore]
    public DateTime CreatedAt => StartedAt;

    /// <summary>Convenience accessor for the compensation strategy configured on the saga definition.</summary>
    [JsonIgnore]
    public CompensationStrategy CompensationStrategy => Definition.CompensationStrategy;

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
    /// Initializes saga with definition and configuration.
    /// </summary>
    /// <param name="definition">The saga definition to use.</param>
    /// <param name="maxRetries">Maximum number of retries for the saga.</param>
    /// <param name="timeoutSeconds">Overall timeout for the saga in seconds.</param>
    public void Initialize(SagaDefinition definition, int maxRetries = 3, int timeoutSeconds = 300)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        MaxRetries = maxRetries;
        TimeoutSeconds = timeoutSeconds;
        Status = SagaStatus.Initialized;
    }

    /// <summary>
    /// Marks saga as started and begins step execution.
    /// </summary>
    public void Start()
    {
        if (!SagaStatus.Initialized.CanTransitionTo(SagaStatus.Running))
            throw new InvalidOperationException($"Cannot start saga in {Status} status");

        Status = SagaStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks saga as successfully completed.
    /// </summary>
    public void Complete()
    {
        if (!Status.CanTransitionTo(SagaStatus.Completed))
            throw new InvalidOperationException($"Cannot complete saga in {Status} status");

        Status = SagaStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks saga as failed and initiates compensation.
    /// </summary>
    /// <param name="reason">The reason for the failure.</param>
    public void Fail(string reason)
    {
        Status = SagaStatus.Failed;
        FailedAt = DateTime.UtcNow;
        FailureReason = reason;
    }

    /// <summary>
    /// Begins compensation process for rollback.
    /// </summary>
    public void BeginCompensation()
    {
        if (!Status.CanTransitionTo(SagaStatus.Compensating))
            throw new InvalidOperationException("Can only compensate failed sagas");

        Status = SagaStatus.Compensating;
        CompensationStartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Completes compensation process.
    /// </summary>
    public void CompleteCompensation()
    {
        Status = SagaStatus.Compensated;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if saga has exceeded timeout.
    /// </summary>
    /// <returns>True if the saga has timed out, false otherwise.</returns>
    public bool IsTimedOut()
    {
        var elapsed = DateTime.UtcNow - StartedAt;
        return elapsed.TotalSeconds > TimeoutSeconds && Status == SagaStatus.Running;
    }

    /// <summary>
    /// Checks if saga can be retried.
    /// </summary>
    /// <returns>True if the saga can be retried, false otherwise.</returns>
    public bool CanRetry()
    {
        return Status == SagaStatus.Failed && RetryCount < MaxRetries;
    }
}
