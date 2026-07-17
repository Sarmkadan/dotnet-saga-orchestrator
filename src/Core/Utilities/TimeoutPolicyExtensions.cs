#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Core.Utilities;

/// <summary>
/// Provides extension methods for <see cref="TimeoutPolicy"/> to enhance timeout management scenarios.
/// </summary>
public static class TimeoutPolicyExtensions
{
    /// <summary>
    /// Creates a human-readable description of the timeout policy based on its duration.
    /// </summary>
    /// <param name="policy">The timeout policy instance.</param>
    /// <returns>A description categorizing the timeout duration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is <see langword="null"/>.</exception>
    public static string GetDescription(this TimeoutPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return policy.Timeout.TotalSeconds switch
        {
            <= 10 => "Strict (≤10s)",
            <= 60 => "Standard (≤1m)",
            <= 300 => "Moderate (≤5m)",
            _ => $"Lenient (>5m, IsRelaxed={policy.IsRelaxed})"
        };
    }

    /// <summary>
    /// Determines if the timeout is approaching based on elapsed percentage.
    /// </summary>
    /// <param name="policy">The timeout policy instance.</param>
    /// <param name="startTime">The start time of the operation.</param>
    /// <param name="thresholdPercentage">The percentage threshold to consider as approaching (e.g., 80 for 80%).</param>
    /// <returns>True if the timeout is approaching; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="thresholdPercentage"/> is not between 0 and 100.</exception>
    public static bool IsApproachingTimeout(this TimeoutPolicy policy, DateTime startTime, double thresholdPercentage)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (thresholdPercentage is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(thresholdPercentage), "Threshold must be between 0 and 100");
        }

        var elapsedPercentage = policy.GetElapsedPercentage(startTime);
        return elapsedPercentage >= thresholdPercentage;
    }

    /// <summary>
    /// Gets a sequence of warning thresholds based on the timeout policy.
    /// </summary>
    /// <param name="policy">The timeout policy instance.</param>
    /// <param name="warningCount">The number of warnings to generate (must be positive).</param>
    /// <returns>A read-only list of warning thresholds as percentages.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="warningCount"/> is not positive.</exception>
    public static IReadOnlyList<double> GetWarningThresholds(this TimeoutPolicy policy, int warningCount)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (warningCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(warningCount), "Warning count must be positive");
        }

        var thresholds = new List<double>(warningCount);
        var increment = 100.0 / (warningCount + 1);

        for (var i = 1; i <= warningCount; i++)
        {
            thresholds.Add(i * increment);
        }

        return thresholds.AsReadOnly();
    }

    /// <summary>
    /// Creates a new timeout policy with adjusted timeout based on a multiplier.
    /// </summary>
    /// <param name="policy">The timeout policy instance.</param>
    /// <param name="multiplier">The multiplier to apply to the timeout (must be positive).</param>
    /// <returns>A new timeout policy with adjusted timeout.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="multiplier"/> is not positive.</exception>
    public static TimeoutPolicy WithMultiplier(this TimeoutPolicy policy, double multiplier)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (multiplier <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(multiplier), "Multiplier must be positive");
        }

        var newTimeoutSeconds = (int)(policy.TimeoutSeconds * multiplier);
        return TimeoutPolicy.Create(newTimeoutSeconds);
    }
}