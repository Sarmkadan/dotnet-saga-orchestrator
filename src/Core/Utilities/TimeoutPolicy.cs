#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;

namespace SagaOrchestrator.Core.Utilities;

/// <summary>
/// Encapsulates timeout configuration for sagas and steps.
/// </summary>
public class TimeoutPolicy
{
    public int TimeoutSeconds { get; }
    public TimeSpan Timeout { get; }
    public bool IsRelaxed { get; }

    /// <summary>
    /// Creates a timeout policy
    /// </summary>
    public TimeoutPolicy(int timeoutSeconds)
    {
        if (timeoutSeconds <= 0)
            throw new ArgumentException("Timeout must be positive", nameof(timeoutSeconds));

        TimeoutSeconds = timeoutSeconds;
        Timeout = TimeSpan.FromSeconds(timeoutSeconds);
        IsRelaxed = timeoutSeconds >= 300;
    }

    /// <summary>
    /// Checks if a duration has exceeded timeout
    /// </summary>
    public bool HasExceeded(TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(nameof(elapsed));
        return elapsed >= Timeout;
    }

    /// <summary>
    /// Checks if a duration has exceeded timeout with buffer
    /// </summary>
    public bool HasExceeded(TimeSpan elapsed, TimeSpan buffer)
    {
        ArgumentNullException.ThrowIfNull(nameof(elapsed));
        ArgumentNullException.ThrowIfNull(nameof(buffer));
        return elapsed >= (Timeout - buffer);
    }

    /// <summary>
    /// Gets remaining time until timeout
    /// </summary>
    public TimeSpan GetRemainingTime(DateTime startTime)
    {
        ArgumentNullException.ThrowIfNull(nameof(startTime));
        var elapsed = DateTime.UtcNow - startTime;
        var remaining = Timeout - elapsed;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Checks if enough time remains
    /// </summary>
    public bool HasSufficientTime(DateTime startTime, TimeSpan requiredTime)
    {
        ArgumentNullException.ThrowIfNull(nameof(startTime));
        ArgumentNullException.ThrowIfNull(nameof(requiredTime));
        var remaining = GetRemainingTime(startTime);
        return remaining >= requiredTime;
    }

    /// <summary>
    /// Calculates percentage of timeout elapsed
    /// </summary>
    public double GetElapsedPercentage(DateTime startTime)
    {
        ArgumentNullException.ThrowIfNull(nameof(startTime));
        var elapsed = DateTime.UtcNow - startTime;
        var percentage = elapsed.TotalSeconds / Timeout.TotalSeconds * 100;
        return Math.Min(percentage, 100);
    }

    /// <summary>
    /// Creates a lenient timeout policy (5 minutes)
    /// </summary>
    public static TimeoutPolicy CreateLenient()
    {
        return new TimeoutPolicy(300);
    }

    /// <summary>
    /// Creates a standard timeout policy (1 minute)
    /// </summary>
    public static TimeoutPolicy CreateStandard()
    {
        return new TimeoutPolicy(60);
    }

    /// <summary>
    /// Creates a strict timeout policy (10 seconds)
    /// </summary>
    public static TimeoutPolicy CreateStrict()
    {
        return new TimeoutPolicy(10);
    }

    /// <summary>
    /// Creates a custom timeout policy
    /// </summary>
    public static TimeoutPolicy Create(int seconds)
    {
        return new TimeoutPolicy(seconds);
    }
}
