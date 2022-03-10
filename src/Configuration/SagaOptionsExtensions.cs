#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Provides extension methods for <see cref="SagaOptions"/> that offer
/// convenient ways to query and manipulate saga orchestrator configuration.
/// </summary>
public static class SagaOptionsExtensions
{
    /// <summary>
    /// Determines whether caching is enabled based on the cache policies configuration.
    /// </summary>
    /// <param name="options">The saga options to check.</param>
    /// <returns>
    /// <c>true</c> if caching is enabled; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    public static bool IsCachingEnabled(this SagaOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.CachePolicies.EnableCaching;
    }

    /// <summary>
    /// Gets the effective timeout for a saga step based on the configured policies.
    /// Takes into account the default timeout and applies maximum constraints.
    /// </summary>
    /// <param name="options">The saga options containing timeout policies.</param>
    /// <param name="requestedTimeoutSeconds">The requested timeout for the step.</param>
    /// <returns>
    /// The effective timeout in seconds, clamped between default and maximum values.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="requestedTimeoutSeconds"/> is less than or equal to 0.
    /// </exception>
    public static int GetEffectiveStepTimeout(this SagaOptions options, int requestedTimeoutSeconds)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(requestedTimeoutSeconds, 0);

        var timeoutPolicies = options.TimeoutPolicies;
        var effectiveTimeout = Math.Max(
            timeoutPolicies.DefaultStepTimeoutSeconds,
            requestedTimeoutSeconds
        );

        return Math.Min(effectiveTimeout, timeoutPolicies.MaxStepTimeoutSeconds);
    }

    /// <summary>
    /// Gets the effective timeout for a saga based on the configured policies.
    /// Takes into account the default timeout and applies maximum constraints.
    /// </summary>
    /// <param name="options">The saga options containing timeout policies.</param>
    /// <param name="requestedTimeoutSeconds">The requested timeout for the saga.</param>
    /// <returns>
    /// The effective timeout in seconds, clamped between default and maximum values.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="requestedTimeoutSeconds"/> is less than or equal to 0.
    /// </exception>
    public static int GetEffectiveSagaTimeout(this SagaOptions options, int requestedTimeoutSeconds)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(requestedTimeoutSeconds, 0);

        var timeoutPolicies = options.TimeoutPolicies;
        var effectiveTimeout = Math.Max(
            timeoutPolicies.DefaultSagaTimeoutSeconds,
            requestedTimeoutSeconds
        );

        return Math.Min(effectiveTimeout, timeoutPolicies.MaxSagaTimeoutSeconds);
    }

    /// <summary>
    /// Creates a new <see cref="SagaOptions"/> instance that inherits settings from the current instance
    /// but with specific overrides applied.
    /// </summary>
    /// <param name="options">The source saga options.</param>
    /// <param name="configure">An action that applies overrides to the new options.</param>
    /// <returns>A new <see cref="SagaOptions"/> instance with inherited settings and applied overrides.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> or <paramref name="configure"/> is <c>null</c>.
    /// </exception>
    public static SagaOptions WithOverrides(this SagaOptions options, Action<SagaOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(configure);

        var result = new SagaOptions
        {
            TimeoutPolicies = new TimeoutPolicies
            {
                DefaultStepTimeoutSeconds = options.TimeoutPolicies.DefaultStepTimeoutSeconds,
                DefaultSagaTimeoutSeconds = options.TimeoutPolicies.DefaultSagaTimeoutSeconds,
                MaxStepTimeoutSeconds = options.TimeoutPolicies.MaxStepTimeoutSeconds,
                MaxSagaTimeoutSeconds = options.TimeoutPolicies.MaxSagaTimeoutSeconds,
                CompensationTimeoutSeconds = options.TimeoutPolicies.CompensationTimeoutSeconds
            },
            RetryPolicies = new RetryPolicies
            {
                DefaultMaxRetries = options.RetryPolicies.DefaultMaxRetries,
                DefaultRetryDelayMs = options.RetryPolicies.DefaultRetryDelayMs,
                MaxRetries = options.RetryPolicies.MaxRetries,
                UseExponentialBackoff = options.RetryPolicies.UseExponentialBackoff,
                BackoffMultiplier = options.RetryPolicies.BackoffMultiplier,
                MaxBackoffDelayMs = options.RetryPolicies.MaxBackoffDelayMs
            },
            CachePolicies = new CachePolicies
            {
                EnableCaching = options.CachePolicies.EnableCaching,
                SagaCacheExpirationMinutes = options.CachePolicies.SagaCacheExpirationMinutes,
                DefinitionCacheExpirationMinutes = options.CachePolicies.DefinitionCacheExpirationMinutes,
                HealthCheckCacheExpirationSeconds = options.CachePolicies.HealthCheckCacheExpirationSeconds,
                MaxCacheSize = options.CachePolicies.MaxCacheSize
            },
            WorkerPolicies = new WorkerPolicies
            {
                EnableTimeoutWorker = options.WorkerPolicies.EnableTimeoutWorker,
                TimeoutWorkerIntervalSeconds = options.WorkerPolicies.TimeoutWorkerIntervalSeconds,
                EnableCompensationWorker = options.WorkerPolicies.EnableCompensationWorker,
                CompensationWorkerIntervalSeconds = options.WorkerPolicies.CompensationWorkerIntervalSeconds,
                EnableEventProcessingWorker = options.WorkerPolicies.EnableEventProcessingWorker,
                EventProcessingWorkerIntervalSeconds = options.WorkerPolicies.EventProcessingWorkerIntervalSeconds,
                MaxEventsToKeep = options.WorkerPolicies.MaxEventsToKeep
            },
            WebhookPolicies = new WebhookPolicies
            {
                EnableWebhooks = options.WebhookPolicies.EnableWebhooks,
                WebhookTimeoutSeconds = options.WebhookPolicies.WebhookTimeoutSeconds,
                MaxWebhookRetries = options.WebhookPolicies.MaxWebhookRetries,
                WebhookRetryDelayMs = options.WebhookPolicies.WebhookRetryDelayMs,
                MaxWebhookPayloadBytes = options.WebhookPolicies.MaxWebhookPayloadBytes
            }
        };

        configure(result);
        return result;
    }

    /// <summary>
    /// Gets the effective maximum number of retries for a step based on the configured policies.
    /// Takes into account the default maximum retries and applies the configured maximum limit.
    /// </summary>
    /// <param name="options">The saga options containing retry policies.</param>
    /// <param name="requestedMaxRetries">The requested maximum retries for the step.</param>
    /// <returns>
    /// The effective maximum number of retries, clamped between default and maximum values.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="requestedMaxRetries"/> is less than 0.
    /// </exception>
    public static int GetEffectiveMaxRetries(this SagaOptions options, int requestedMaxRetries)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegative(requestedMaxRetries);

        var retryPolicies = options.RetryPolicies;
        var effectiveRetries = Math.Max(retryPolicies.DefaultMaxRetries, requestedMaxRetries);

        return Math.Min(effectiveRetries, retryPolicies.MaxRetries);
    }
}