#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Core.Builders;

/// <summary>
/// Extension methods for <see cref="SagaStepBuilder"/> that provide additional fluent configuration options.
/// </summary>
public static class SagaStepBuilderExtensions
{
    /// <summary>
    /// Sets a description for the saga step.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="description">The step description.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static SagaStepBuilder WithDescription(this SagaStepBuilder builder, string description)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        builder.WithMetadata("description", description);
        return builder;
    }

    /// <summary>
    /// Sets the HTTP method for the step (GET, POST, PUT, DELETE, etc.).
    /// Defaults to POST if not specified.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="httpMethod">The HTTP method to use.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    /// <exception cref="ArgumentException">Thrown when httpMethod is null or empty or contains invalid characters.</exception>
    public static SagaStepBuilder WithHttpMethod(this SagaStepBuilder builder, string httpMethod)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentException.ThrowIfNullOrEmpty(httpMethod, nameof(httpMethod));

        if (!IsValidHttpMethod(httpMethod))
            throw new ArgumentException("Invalid HTTP method. Valid methods are: GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS", nameof(httpMethod));

        builder.WithMetadata("httpMethod", httpMethod.ToUpperInvariant());
        return builder;
    }

    /// <summary>
    /// Configures the step as compensable or non-compensable.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="isCompensable">Whether the step should be compensable.</param>
    /// <param name="compensationUrl">Optional compensation URL. If null and isCompensable is true, uses the step's action URL with "/compensate" suffix.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static SagaStepBuilder WithCompensable(this SagaStepBuilder builder, bool isCompensable, string? compensationUrl = null)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        if (isCompensable && string.IsNullOrWhiteSpace(compensationUrl))
        {
            // Auto-generate compensation URL from action URL
            var actionUrl = builder.Build().ServiceUrl;
            if (!string.IsNullOrWhiteSpace(actionUrl))
            {
                compensationUrl = EnsureTrailingSlash(actionUrl) + "compensate";
            }
        }

        builder.WithCompensation(compensationUrl);
        builder.WithMetadata("isCompensable", isCompensable.ToString());
        return builder;
    }

    /// <summary>
    /// Applies retry policy from an existing SagaStepDefinition to this builder.
    /// Useful for copying retry configuration from one step to another.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="sourceDefinition">The source SagaStepDefinition to copy retry policy from.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder or sourceDefinition is null.</exception>
    public static SagaStepBuilder WithRetryPolicyFromDefinition(this SagaStepBuilder builder, SagaStepDefinition sourceDefinition)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));
        ArgumentNullException.ThrowIfNull(sourceDefinition, nameof(sourceDefinition));

        if (sourceDefinition.RetryPolicy != null)
        {
            builder.WithRetryPolicy(sourceDefinition.RetryPolicy);
        }
        else if (sourceDefinition.MaxRetries > 0)
        {
            builder.WithRetryPolicy(sourceDefinition.MaxRetries, sourceDefinition.RetryDelayMilliseconds);
        }

        return builder;
    }

    /// <summary>
    /// Sets the step to use exponential backoff retry policy.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="initialDelayMs">Initial delay in milliseconds before first retry.</param>
    /// <param name="useJitter">Whether to add random jitter to delays to prevent thundering herd.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static SagaStepBuilder WithExponentialRetryPolicy(
        this SagaStepBuilder builder,
        int maxRetries = 3,
        int initialDelayMs = 1000,
        bool useJitter = false)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var policy = new RetryPolicy(
            maxRetries: maxRetries,
            initialDelayMs: initialDelayMs,
            backoffMultiplier: 2.0,
            maxDelayMs: 60000,
            useJitter: useJitter
        );

        builder.WithRetryPolicy(policy);
        return builder;
    }

    /// <summary>
    /// Sets the step to use linear retry policy (fixed delays between retries).
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="delayMs">Fixed delay in milliseconds between retries.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static SagaStepBuilder WithLinearRetryPolicy(
        this SagaStepBuilder builder,
        int maxRetries = 3,
        int delayMs = 1000)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var policy = RetryPolicy.CreateLinear(maxRetries, delayMs);
        builder.WithRetryPolicy(policy);
        return builder;
    }

    /// <summary>
    /// Sets the step to use no retry policy (fail immediately on error).
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static SagaStepBuilder WithNoRetryPolicy(this SagaStepBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var policy = RetryPolicy.CreateNoRetry();
        builder.WithRetryPolicy(policy);
        return builder;
    }

    /// <summary>
    /// Adds multiple metadata entries from a dictionary.
    /// </summary>
    /// <param name="builder">The builder instance.</param>
    /// <param name="metadata">Dictionary of metadata key-value pairs.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static SagaStepBuilder WithMetadata(this SagaStepBuilder builder, Dictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                builder.WithMetadata(kvp.Key, kvp.Value);
            }
        }

        return builder;
    }

    /// <summary>
    /// Validates that the string is a valid HTTP method.
    /// </summary>
    private static bool IsValidHttpMethod(string method)
    {
        return method.Equals("GET", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PATCH", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("HEAD", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Ensures a string ends with a trailing slash.
    /// </summary>
    private static string EnsureTrailingSlash(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        return url.EndsWith('/') ? url : url + '/';
    }
}