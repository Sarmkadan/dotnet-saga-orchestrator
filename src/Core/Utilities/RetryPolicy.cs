#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Utilities;

/// <summary>
/// Encapsulates retry policy configuration and delay calculation.
/// </summary>
public class RetryPolicy
{
    private static readonly Random _jitterRandom = new();

    public int MaxRetries { get; }
    public int InitialDelayMs { get; }
    public double BackoffMultiplier { get; }
    public int MaxDelayMs { get; }

    /// <summary>
    /// When true, adds random jitter (±25%) to each calculated delay to
    /// spread retries across multiple instances and reduce thundering herd.
    /// </summary>
    public bool UseJitter { get; }

    /// <summary>
    /// Creates a retry policy with exponential backoff
    /// </summary>
    public RetryPolicy(int maxRetries = 3, int initialDelayMs = 1000, double backoffMultiplier = 2.0, int maxDelayMs = 60000, bool useJitter = false)
    {
        if (maxRetries < 0)
            throw new ArgumentException("Max retries cannot be negative", nameof(maxRetries));

        if (initialDelayMs < 0)
            throw new ArgumentException("Initial delay cannot be negative", nameof(initialDelayMs));

        if (backoffMultiplier < 1.0)
            throw new ArgumentException("Backoff multiplier must be >= 1.0", nameof(backoffMultiplier));

        if (maxDelayMs < initialDelayMs)
            throw new ArgumentException("Max delay must be >= initial delay", nameof(maxDelayMs));

        MaxRetries = maxRetries;
        InitialDelayMs = initialDelayMs;
        BackoffMultiplier = backoffMultiplier;
        MaxDelayMs = maxDelayMs;
        UseJitter = useJitter;
    }

    /// <summary>
    /// Calculates delay for given retry attempt, optionally applying jitter.
    /// </summary>
    public int CalculateDelay(int attemptNumber)
    {
        if (attemptNumber < 1)
            throw new ArgumentException("Attempt number must be >= 1", nameof(attemptNumber));

        if (attemptNumber > MaxRetries)
            throw new InvalidOperationException("Exceeded maximum retry attempts");

        var delay = (int)(InitialDelayMs * Math.Pow(BackoffMultiplier, attemptNumber - 1));
        delay = Math.Min(delay, MaxDelayMs);

        if (UseJitter)
        {
            // Apply ±25% random jitter
            var jitterFactor = 0.75 + _jitterRandom.NextDouble() * 0.5;
            delay = (int)(delay * jitterFactor);
        }

        return delay;
    }

    /// <summary>
    /// Checks if retry is allowed
    /// </summary>
    public bool CanRetry(int currentAttempt)
    {
        return currentAttempt < MaxRetries;
    }

    /// <summary>
    /// Creates a linear retry policy (fixed delays)
    /// </summary>
    public static RetryPolicy CreateLinear(int maxRetries = 3, int delayMs = 1000)
    {
        return new RetryPolicy(maxRetries, delayMs, 1.0, delayMs);
    }

    /// <summary>
    /// Creates an exponential retry policy
    /// </summary>
    public static RetryPolicy CreateExponential(int maxRetries = 3, int initialDelayMs = 1000)
    {
        return new RetryPolicy(maxRetries, initialDelayMs, 2.0, 60000);
    }

    /// <summary>
    /// Creates an exponential retry policy with jitter
    /// </summary>
    public static RetryPolicy CreateExponentialWithJitter(int maxRetries = 3, int initialDelayMs = 1000)
    {
        return new RetryPolicy(maxRetries, initialDelayMs, 2.0, 60000, useJitter: true);
    }

    /// <summary>
    /// Creates a no-retry policy
    /// </summary>
    public static RetryPolicy CreateNoRetry()
    {
        return new RetryPolicy(0, 0, 1.0, 0);
    }
}
