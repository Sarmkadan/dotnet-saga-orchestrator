#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Represents a single step execution within a saga.
/// Each step can succeed, fail, or be compensated.
/// </summary>
public class SagaStep
{
    /// <summary>Gets or sets the unique identifier of the step instance.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>Gets or sets the identifier of the saga this step belongs to.</summary>
    [JsonPropertyName("sagaId")]
    public string SagaId { get; set; }

    /// <summary>Gets or sets the human-readable name of the step.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>Gets or sets the execution order of the step within the saga (1-based).</summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>Gets or sets the current status of the step.</summary>
    [JsonPropertyName("status")]
    public SagaStepStatus Status { get; set; }

    /// <summary>Gets or sets the URL of the forward action endpoint for the step.</summary>
    [JsonPropertyName("serviceUrl")]
    public string ServiceUrl { get; set; }

    /// <summary>Gets or sets the URL of the compensating action endpoint for the step.</summary>
    [JsonPropertyName("compensationUrl")]
    public string CompensationUrl { get; set; }

    /// <summary>Gets or sets the request payload sent to the forward action.</summary>
    [JsonPropertyName("payload")]
    public Dictionary<string, object> Payload { get; set; } = new();

    /// <summary>Gets or sets the response returned by the forward action.</summary>
    [JsonPropertyName("response")]
    public Dictionary<string, object> Response { get; set; } = new();

    /// <summary>Gets or sets the time the step started executing, if applicable.</summary>
    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; set; }

    /// <summary>Gets or sets the time the step completed, if applicable.</summary>
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>Gets or sets the time the step was compensated, if applicable.</summary>
    [JsonPropertyName("compensatedAt")]
    public DateTime? CompensatedAt { get; set; }

    /// <summary>Gets or sets the error message recorded when the step failed.</summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }


    /// <summary>Gets or sets the number of execution attempts made for this step (including initial attempt).</summary>
    [JsonPropertyName("attemptCount")]
    public int AttemptCount { get; set; }
    /// <summary>Gets or sets the number of retry attempts made for this step.</summary>
    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    /// <summary>Gets or sets the maximum number of retries allowed for this step.</summary>
    [JsonPropertyName("maxRetries")]
    public int MaxRetries { get; set; } = 3;

    /// <summary>Gets or sets the per-step execution timeout in seconds.</summary>
    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Per-step retry policy. When set, overrides MaxRetries for delay calculation
    /// and enables exponential backoff with optional jitter.
    /// </summary>
    [JsonIgnore]
    public RetryPolicy? RetryPolicy { get; set; }

    // Constructor
    public SagaStep()
    {
        Id = Guid.NewGuid().ToString();
        Status = SagaStepStatus.Pending;
        AttemptCount = 0;
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