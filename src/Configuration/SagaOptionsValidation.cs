#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Provides validation helpers for <see cref="SagaOptions"/> configuration.
/// </summary>
public static class SagaOptionsValidation
{
    /// <summary>
    /// Validates the saga configuration options and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The saga options to validate.</param>
    /// <returns>An enumerable of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate TimeoutPolicies
        errors.AddRange(value.TimeoutPolicies.Validate());

        // Validate RetryPolicies
        errors.AddRange(value.RetryPolicies.Validate());

        // Validate CachePolicies
        errors.AddRange(value.CachePolicies.Validate());

        // Validate WorkerPolicies
        errors.AddRange(value.WorkerPolicies.Validate());

        // Validate WebhookPolicies
        errors.AddRange(value.WebhookPolicies.Validate());

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates the saga configuration options and returns whether they are valid.
    /// </summary>
    /// <param name="value">The saga options to validate.</param>
    /// <returns>True if valid; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SagaOptions value) => value.Validate().Count == 0;

    /// <summary>
    /// Validates the saga configuration options and throws an exception with all validation problems.
    /// </summary>
    /// <param name="value">The saga options to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails with one or more problems.</exception>
    public static void EnsureValid(this SagaOptions value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SagaOptions validation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates timeout policy configuration.
    /// </summary>
    /// <param name="policies">The timeout policies to validate.</param>
    /// <returns>An enumerable of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policies"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this TimeoutPolicies policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        var errors = new List<string>();

        if (policies.DefaultStepTimeoutSeconds <= 0)
        {
            errors.Add(
                $"TimeoutPolicies.DefaultStepTimeoutSeconds must be greater than 0, but was {policies.DefaultStepTimeoutSeconds}.");
        }

        if (policies.DefaultSagaTimeoutSeconds <= 0)
        {
            errors.Add(
                $"TimeoutPolicies.DefaultSagaTimeoutSeconds must be greater than 0, but was {policies.DefaultSagaTimeoutSeconds}.");
        }

        if (policies.MaxStepTimeoutSeconds <= 0)
        {
            errors.Add(
                $"TimeoutPolicies.MaxStepTimeoutSeconds must be greater than 0, but was {policies.MaxStepTimeoutSeconds}.");
        }

        if (policies.MaxSagaTimeoutSeconds <= 0)
        {
            errors.Add(
                $"TimeoutPolicies.MaxSagaTimeoutSeconds must be greater than 0, but was {policies.MaxSagaTimeoutSeconds}.");
        }

        if (policies.CompensationTimeoutSeconds <= 0)
        {
            errors.Add(
                $"TimeoutPolicies.CompensationTimeoutSeconds must be greater than 0, but was {policies.CompensationTimeoutSeconds}.");
        }

        if (policies.DefaultStepTimeoutSeconds > policies.MaxStepTimeoutSeconds)
        {
            errors.Add(
                $"TimeoutPolicies.DefaultStepTimeoutSeconds ({policies.DefaultStepTimeoutSeconds}) cannot exceed MaxStepTimeoutSeconds ({policies.MaxStepTimeoutSeconds}).");
        }

        if (policies.DefaultSagaTimeoutSeconds > policies.MaxSagaTimeoutSeconds)
        {
            errors.Add(
                $"TimeoutPolicies.DefaultSagaTimeoutSeconds ({policies.DefaultSagaTimeoutSeconds}) cannot exceed MaxSagaTimeoutSeconds ({policies.MaxSagaTimeoutSeconds}).");
        }

        if (policies.CompensationTimeoutSeconds > policies.MaxStepTimeoutSeconds)
        {
            errors.Add(
                $"TimeoutPolicies.CompensationTimeoutSeconds ({policies.CompensationTimeoutSeconds}) cannot exceed MaxStepTimeoutSeconds ({policies.MaxStepTimeoutSeconds}).");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates retry policy configuration.
    /// </summary>
    /// <param name="policies">The retry policies to validate.</param>
    /// <returns>An enumerable of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policies"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RetryPolicies policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        var errors = new List<string>();

        if (policies.DefaultMaxRetries < 0)
        {
            errors.Add(
                $"RetryPolicies.DefaultMaxRetries must be non-negative, but was {policies.DefaultMaxRetries}.");
        }

        if (policies.DefaultRetryDelayMs < 0)
        {
            errors.Add(
                $"RetryPolicies.DefaultRetryDelayMs must be non-negative, but was {policies.DefaultRetryDelayMs}.");
        }

        if (policies.MaxRetries < 0)
        {
            errors.Add(
                $"RetryPolicies.MaxRetries must be non-negative, but was {policies.MaxRetries}.");
        }

        if (policies.MaxBackoffDelayMs < 0)
        {
            errors.Add(
                $"RetryPolicies.MaxBackoffDelayMs must be non-negative, but was {policies.MaxBackoffDelayMs}.");
        }

        if (policies.DefaultMaxRetries > policies.MaxRetries)
        {
            errors.Add(
                $"RetryPolicies.DefaultMaxRetries ({policies.DefaultMaxRetries}) cannot exceed MaxRetries ({policies.MaxRetries}).");
        }

        if (policies.BackoffMultiplier < 1.0)
        {
            errors.Add(
                $"RetryPolicies.BackoffMultiplier must be >= 1.0, but was {policies.BackoffMultiplier.ToString(CultureInfo.InvariantCulture)}.");
        }

        if (policies.UseExponentialBackoff && policies.BackoffMultiplier <= 1.0)
        {
            errors.Add(
                $"RetryPolicies.BackoffMultiplier must be > 1.0 when UseExponentialBackoff is true, but was {policies.BackoffMultiplier.ToString(CultureInfo.InvariantCulture)}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates cache policy configuration.
    /// </summary>
    /// <param name="policies">The cache policies to validate.</param>
    /// <returns>An enumerable of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policies"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this CachePolicies policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        var errors = new List<string>();

        if (policies.SagaCacheExpirationMinutes <= 0)
        {
            errors.Add(
                $"CachePolicies.SagaCacheExpirationMinutes must be greater than 0, but was {policies.SagaCacheExpirationMinutes}.");
        }

        if (policies.DefinitionCacheExpirationMinutes <= 0)
        {
            errors.Add(
                $"CachePolicies.DefinitionCacheExpirationMinutes must be greater than 0, but was {policies.DefinitionCacheExpirationMinutes}.");
        }

        if (policies.HealthCheckCacheExpirationSeconds <= 0)
        {
            errors.Add(
                $"CachePolicies.HealthCheckCacheExpirationSeconds must be greater than 0, but was {policies.HealthCheckCacheExpirationSeconds}.");
        }

        if (policies.MaxCacheSize < 0)
        {
            errors.Add(
                $"CachePolicies.MaxCacheSize must be non-negative, but was {policies.MaxCacheSize}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates worker policy configuration.
    /// </summary>
    /// <param name="policies">The worker policies to validate.</param>
    /// <returns>An enumerable of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policies"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WorkerPolicies policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        var errors = new List<string>();

        if (policies.TimeoutWorkerIntervalSeconds <= 0)
        {
            errors.Add(
                $"WorkerPolicies.TimeoutWorkerIntervalSeconds must be greater than 0, but was {policies.TimeoutWorkerIntervalSeconds}.");
        }

        if (policies.CompensationWorkerIntervalSeconds <= 0)
        {
            errors.Add(
                $"WorkerPolicies.CompensationWorkerIntervalSeconds must be greater than 0, but was {policies.CompensationWorkerIntervalSeconds}.");
        }

        if (policies.EventProcessingWorkerIntervalSeconds <= 0)
        {
            errors.Add(
                $"WorkerPolicies.EventProcessingWorkerIntervalSeconds must be greater than 0, but was {policies.EventProcessingWorkerIntervalSeconds}.");
        }

        if (policies.MaxEventsToKeep < 0)
        {
            errors.Add(
                $"WorkerPolicies.MaxEventsToKeep must be non-negative, but was {policies.MaxEventsToKeep}.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Validates webhook policy configuration.
    /// </summary>
    /// <param name="policies">The webhook policies to validate.</param>
    /// <returns>An enumerable of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policies"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this WebhookPolicies policies)
    {
        ArgumentNullException.ThrowIfNull(policies);

        var errors = new List<string>();

        if (policies.WebhookTimeoutSeconds <= 0)
        {
            errors.Add(
                $"WebhookPolicies.WebhookTimeoutSeconds must be greater than 0, but was {policies.WebhookTimeoutSeconds}.");
        }

        if (policies.MaxWebhookRetries < 0)
        {
            errors.Add(
                $"WebhookPolicies.MaxWebhookRetries must be non-negative, but was {policies.MaxWebhookRetries}.");
        }

        if (policies.WebhookRetryDelayMs < 0)
        {
            errors.Add(
                $"WebhookPolicies.WebhookRetryDelayMs must be non-negative, but was {policies.WebhookRetryDelayMs}.");
        }

        if (policies.MaxWebhookPayloadBytes <= 0)
        {
            errors.Add(
                $"WebhookPolicies.MaxWebhookPayloadBytes must be greater than 0, but was {policies.MaxWebhookPayloadBytes}.");
        }

        return errors.AsReadOnly();
    }
}