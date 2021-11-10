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
    public int MaxRetries { get; }
    public int InitialDelayMs { get; }
    public double BackoffMultiplier { get; }
    public int MaxDelayMs { get; }

    /// <summary>
    /// Creates a retry policy with exponential backoff
    /// </summary>
    public RetryPolicy(int maxRetries = 3, int initialDelayMs = 1000, double backoffMultiplier = 2.0, int maxDelayMs = 60000)
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
    }

    /// <summary>
    /// Calculates delay for given retry attempt
    /// </summary>
    public int CalculateDelay(int attemptNumber)
    {
        if (attemptNumber < 1)
            throw new ArgumentException("Attempt number must be >= 1", nameof(attemptNumber));

        if (attemptNumber > MaxRetries)
            throw new InvalidOperationException("Exceeded maximum retry attempts");

        var delay = (int)(InitialDelayMs * Math.Pow(BackoffMultiplier, attemptNumber - 1));
        return Math.Min(delay, MaxDelayMs);
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
    /// Creates a no-retry policy
    /// </summary>
    public static RetryPolicy CreateNoRetry()
    {
        return new RetryPolicy(0, 0, 1.0, 0);
    }
}
