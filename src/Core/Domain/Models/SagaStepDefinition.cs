#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Defines the configuration and behavior of a step in a saga workflow.
/// </summary>
public class SagaStepDefinition
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; }

    [JsonPropertyName("serviceUrl")]
    public string ServiceUrl { get; set; }

    [JsonPropertyName("compensationUrl")]
    public string CompensationUrl { get; set; }

    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; set; } = 30;

    [JsonPropertyName("maxRetries")]
    public int MaxRetries { get; set; } = 3;

    [JsonPropertyName("retryDelayMilliseconds")]
    public int RetryDelayMilliseconds { get; set; } = 1000;

    [JsonPropertyName("isCompensable")]
    public bool IsCompensable { get; set; } = true;

    [JsonPropertyName("isAsync")]
    public bool IsAsync { get; set; } = false;

    [JsonPropertyName("httpMethod")]
    public string HttpMethod { get; set; } = "POST";

    /// <summary>
    /// Optional per-step retry policy. When set, overrides the global MaxRetries and
    /// RetryDelayMilliseconds values and enables exponential backoff with optional jitter.
    /// </summary>
    [JsonIgnore]
    public RetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// Arbitrary key/value metadata attached to the step (e.g. circuit breaker
    /// thresholds, async flags) for use by builders and orchestration logic.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();

    // Constructor
    public SagaStepDefinition()
    {
        Id = Guid.NewGuid().ToString();
        Name = "Undefined Step";
        Description = "No description provided";
        ServiceName = "unknown";
        ServiceUrl = string.Empty;
        CompensationUrl = string.Empty;
    }

    /// <summary>
    /// Creates a step definition with required parameters
    /// </summary>
    public SagaStepDefinition(string name, string serviceName, string serviceUrl, string compensationUrl)
    {
        Id = Guid.NewGuid().ToString();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ServiceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        ServiceUrl = serviceUrl ?? throw new ArgumentNullException(nameof(serviceUrl));
        CompensationUrl = compensationUrl ?? throw new ArgumentNullException(nameof(compensationUrl));
        Description = $"Step: {name}";
    }

    /// <summary>
    /// Validates the step definition
    /// </summary>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (string.IsNullOrWhiteSpace(ServiceName))
            return false;

        if (string.IsNullOrWhiteSpace(ServiceUrl))
            return false;

        if (IsCompensable && string.IsNullOrWhiteSpace(CompensationUrl))
            return false;

        if (TimeoutSeconds <= 0)
            return false;

        if (MaxRetries < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Configures timeout for the step
    /// </summary>
    public void SetTimeout(int seconds)
    {
        if (seconds <= 0)
            throw new ArgumentException("Timeout must be positive", nameof(seconds));

        TimeoutSeconds = seconds;
    }

    /// <summary>
    /// Configures retry policy using a RetryPolicy instance.
    /// This overrides MaxRetries and RetryDelayMilliseconds.
    /// </summary>
    public void SetRetryPolicy(RetryPolicy policy)
    {
        RetryPolicy = policy ?? throw new ArgumentNullException(nameof(policy));
        MaxRetries = policy.MaxRetries;
        RetryDelayMilliseconds = policy.InitialDelayMs;
    }

    /// <summary>
    /// Configures retry policy
    /// </summary>
    public void SetRetryPolicy(int maxRetries, int delayMilliseconds)
    {
        if (maxRetries < 0)
            throw new ArgumentException("Max retries cannot be negative", nameof(maxRetries));

        if (delayMilliseconds < 0)
            throw new ArgumentException("Delay cannot be negative", nameof(delayMilliseconds));

        MaxRetries = maxRetries;
        RetryDelayMilliseconds = delayMilliseconds;
    }

    /// <summary>
    /// Marks step as compensable or non-compensable
    /// </summary>
    public void SetCompensable(bool compensable, string? compensationUrl = null)
    {
        IsCompensable = compensable;

        if (compensable && string.IsNullOrWhiteSpace(compensationUrl))
            throw new ArgumentException("Compensation URL required for compensable steps", nameof(compensationUrl));

        if (compensable)
            CompensationUrl = compensationUrl!;
    }

    /// <summary>
    /// Sets whether step is asynchronous
    /// </summary>
    public void SetAsync(bool async)
    {
        IsAsync = async;
    }

    /// <summary>
    /// Clones the step definition
    /// </summary>
    public SagaStepDefinition Clone()
    {
        return new SagaStepDefinition(Name, ServiceName, ServiceUrl, CompensationUrl)
        {
            Id = Id,
            Description = Description,
            Order = Order,
            TimeoutSeconds = TimeoutSeconds,
            MaxRetries = MaxRetries,
            RetryDelayMilliseconds = RetryDelayMilliseconds,
            IsCompensable = IsCompensable,
            IsAsync = IsAsync,
            HttpMethod = HttpMethod,
            RetryPolicy = RetryPolicy,
            Metadata = new Dictionary<string, string>(Metadata)
        };
    }
}
