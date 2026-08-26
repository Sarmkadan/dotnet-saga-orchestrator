#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Extensions;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Configuration options for saga orchestrator behavior and policies.
/// Can be loaded from appsettings.json or configured programmatically.
/// </summary>
public class SagaOptions
{
    public const string SectionName = "SagaOrchestrator";

    public TimeoutPolicies TimeoutPolicies { get; set; } = new();
    public RetryPolicies RetryPolicies { get; set; } = new();
    public CachePolicies CachePolicies { get; set; } = new();
    public WorkerPolicies WorkerPolicies { get; set; } = new();
    public WebhookPolicies WebhookPolicies { get; set; } = new();

    public override string ToString() =>
        $"SagaOptions {{ TimeoutPolicies = {TimeoutPolicies}, RetryPolicies = {RetryPolicies}, CachePolicies = {CachePolicies}, WorkerPolicies = {WorkerPolicies}, WebhookPolicies = {WebhookPolicies}, DefaultStepTimeoutSeconds = {TimeoutPolicies.DefaultStepTimeoutSeconds} }}";
}

public class TimeoutPolicies
{
    public int DefaultStepTimeoutSeconds { get; set; } = 30;
    public int DefaultSagaTimeoutSeconds { get; set; } = 300;
    public int MaxStepTimeoutSeconds { get; set; } = 3600;
    public int MaxSagaTimeoutSeconds { get; set; } = 86400;
    public int CompensationTimeoutSeconds { get; set; } = 120;
    public int StaleSagaTimeoutSeconds { get; set; } = 3600;

    /// <summary>
    /// Gets the lenient timeout value (5 minutes)
    /// </summary>
    public int LenientTimeoutSeconds => 300;

    /// <summary>
    /// Gets the standard timeout value (1 minute)
    /// </summary>
    public int StandardTimeoutSeconds => 60;

    /// <summary>
    /// Gets the strict timeout value (10 seconds)
    /// </summary>
    public int StrictTimeoutSeconds => 10;
}

public class RetryPolicies
{
    public int DefaultMaxRetries { get; set; } = 3;
    public int DefaultRetryDelayMs { get; set; } = 1000;
    public int MaxRetries { get; set; } = 10;
    public bool UseExponentialBackoff { get; set; } = true;
    public double BackoffMultiplier { get; set; } = 2.0;
    public int MaxBackoffDelayMs { get; set; } = 30000;
}

public class CachePolicies
{
    public bool EnableCaching { get; set; } = true;
    public int SagaCacheExpirationMinutes { get; set; } = 15;
    public int DefinitionCacheExpirationMinutes { get; set; } = 60;
    public int HealthCheckCacheExpirationSeconds { get; set; } = 30;
    public int MaxCacheSize { get; set; } = 10000;
}

public class WorkerPolicies
{
    public bool EnableTimeoutWorker { get; set; } = true;
    public int TimeoutWorkerIntervalSeconds { get; set; } = 30;

    public bool EnableCompensationWorker { get; set; } = true;
    public int CompensationWorkerIntervalSeconds { get; set; } = 15;

    public bool EnableEventProcessingWorker { get; set; } = true;
    public int EventProcessingWorkerIntervalSeconds { get; set; } = 10;

    public int MaxEventsToKeep { get; set; } = 10000;
}

public class WebhookPolicies
{
    public bool EnableWebhooks { get; set; } = true;
    public int WebhookTimeoutSeconds { get; set; } = 10;
    public int MaxWebhookRetries { get; set; } = 3;
    public int WebhookRetryDelayMs { get; set; } = 1000;
    public int MaxWebhookPayloadBytes { get; set; } = 1024000; // 1MB
}

/// <summary>
/// Builder for fluent configuration of saga options.
/// </summary>
public class SagaOptionsBuilder
{
    private readonly SagaOptions _options = new();

    public SagaOptionsBuilder WithDefaultStepTimeout(int seconds)
    {
        _options.TimeoutPolicies.DefaultStepTimeoutSeconds = seconds.GreaterThan(0, nameof(seconds));
        return this;
    }

    public SagaOptionsBuilder WithDefaultSagaTimeout(int seconds)
    {
        _options.TimeoutPolicies.DefaultSagaTimeoutSeconds = seconds.GreaterThan(0, nameof(seconds));
        return this;
    }

public SagaOptionsBuilder WithStaleSagaTimeout(int seconds)
{
    _options.TimeoutPolicies.StaleSagaTimeoutSeconds = seconds.GreaterThan(0, nameof(seconds));
    return this;
}

    public SagaOptionsBuilder WithDefaultMaxRetries(int retries)
    {
        _options.RetryPolicies.DefaultMaxRetries = retries.GreaterThanOrEqual(0, nameof(retries));
        return this;
    }

    public SagaOptionsBuilder WithCachingEnabled(bool enabled)
    {
        _options.CachePolicies.EnableCaching = enabled;
        return this;
    }

    public SagaOptionsBuilder WithSagaCacheExpiration(int minutes)
    {
        _options.CachePolicies.SagaCacheExpirationMinutes = minutes.GreaterThan(0, nameof(minutes));
        return this;
    }

    public SagaOptionsBuilder WithWebhooksEnabled(bool enabled)
    {
        _options.WebhookPolicies.EnableWebhooks = enabled;
        return this;
    }

    public SagaOptionsBuilder WithTimeoutWorker(bool enabled, int intervalSeconds = 30)
    {
        _options.WorkerPolicies.EnableTimeoutWorker = enabled;
        if (intervalSeconds > 0)
            _options.WorkerPolicies.TimeoutWorkerIntervalSeconds = intervalSeconds;
        return this;
    }

    public SagaOptionsBuilder WithCompensationWorker(bool enabled, int intervalSeconds = 15)
    {
        _options.WorkerPolicies.EnableCompensationWorker = enabled;
        if (intervalSeconds > 0)
            _options.WorkerPolicies.CompensationWorkerIntervalSeconds = intervalSeconds;
        return this;
    }

    public SagaOptionsBuilder WithExponentialBackoff(bool enabled, double multiplier = 2.0)
    {
        _options.RetryPolicies.UseExponentialBackoff = enabled;
        if (multiplier > 1.0)
            _options.RetryPolicies.BackoffMultiplier = multiplier;
        return this;
    }

    public SagaOptions Build()
    {
        ValidateOptions();
        return _options;
    }

    private void ValidateOptions()
    {
        if (_options.TimeoutPolicies.DefaultStepTimeoutSeconds > _options.TimeoutPolicies.MaxStepTimeoutSeconds)
            throw new InvalidOperationException("Default step timeout cannot exceed max timeout");

        if (_options.TimeoutPolicies.DefaultSagaTimeoutSeconds > _options.TimeoutPolicies.MaxSagaTimeoutSeconds)
            throw new InvalidOperationException("Default saga timeout cannot exceed max timeout");

        if (_options.TimeoutPolicies.StaleSagaTimeoutSeconds > _options.TimeoutPolicies.MaxSagaTimeoutSeconds)
            throw new InvalidOperationException("Stale saga timeout cannot exceed max timeout");

        if (_options.RetryPolicies.DefaultMaxRetries > _options.RetryPolicies.MaxRetries)
            throw new InvalidOperationException("Default max retries cannot exceed maximum allowed retries");

        if (_options.RetryPolicies.BackoffMultiplier < 1.0)
            throw new InvalidOperationException("Backoff multiplier must be >= 1.0");
    }
}
