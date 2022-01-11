#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DateTime"/> and <see cref="TimeSpan"/> operations.
/// Includes utilities for time calculations, formatting, and validation.
/// </summary>
public static class DateTimeExtensions
{
    private const int MaxRelativeTimeSeconds = 60;
    private const int MaxRelativeTimeMinutes = 60;
    private const int MaxRelativeTimeHours = 24;
    private const int MaxRelativeTimeDays = 30;
    /// <summary>
    /// Determines whether the specified deadline has expired.
    /// </summary>
    /// <param name="deadline">The deadline to check.</param>
    /// <param name="now">The current time to use for comparison. If null, uses <see cref="DateTime.UtcNow"/>.</param>
    /// <returns><see langword="true"/> if the deadline has expired; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="deadline"/> is in the future but within 1 second of <paramref name="now"/>.</exception>
    public static bool IsExpired(this DateTime deadline, DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        return deadline <= current;
    }

    /// <summary>
    /// Calculates the time remaining until the specified deadline.
    /// </summary>
    /// <param name="deadline">The deadline to calculate remaining time for.</param>
    /// <param name="now">The current time to use for calculation. If null, uses <see cref="DateTime.UtcNow"/>.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the remaining time until the deadline, or <see cref="TimeSpan.Zero"/> if the deadline has already passed.</returns>
    public static TimeSpan TimeUntil(this DateTime deadline, DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        var remaining = deadline - current;
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    /// <summary>
    /// Calculates the elapsed time since the specified start time.
    /// </summary>
    /// <param name="startTime">The start time to calculate elapsed time from.</param>
    /// <param name="now">The current time to use for calculation. If null, uses <see cref="DateTime.UtcNow"/>.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the elapsed time since the start time.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="startTime"/> is in the future.</exception>
    public static TimeSpan ElapsedSince(this DateTime startTime, DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        return current - startTime;
    }

    /// <summary>
    /// Rounds down the date and time to the nearest second.
    /// </summary>
    /// <param name="dt">The date and time to round down.</param>
    /// <returns>A <see cref="DateTime"/> rounded down to the nearest second with UTC kind.</returns>
    public static DateTime RoundDownToSecond(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);
    }

    /// <summary>
    /// Rounds down the date and time to the nearest minute.
    /// </summary>
    /// <param name="dt">The date and time to round down.</param>
    /// <returns>A <see cref="DateTime"/> rounded down to the nearest minute with UTC kind.</returns>
    public static DateTime RoundDownToMinute(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Rounds down the date and time to the nearest hour.
    /// </summary>
    /// <param name="dt">The date and time to round down.</param>
    /// <returns>A <see cref="DateTime"/> rounded down to the nearest hour with UTC kind.</returns>
    public static DateTime RoundDownToHour(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    /// Determines whether the specified date and time is within the specified range (inclusive).
    /// </summary>
    /// <param name="dt">The date and time to check.</param>
    /// <param name="start">The start of the range.</param>
    /// <param name="end">The end of the range.</param>
    /// <returns><see langword="true"/> if <paramref name="dt"/> is within the range; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> is greater than <paramref name="end"/>.</exception>
    public static bool IsWithinRange(this DateTime dt, DateTime start, DateTime end)
    {
        if (start > end)
        {
            throw new ArgumentOutOfRangeException(nameof(end), "Start date must be less than or equal to end date.");
        }

        return dt >= start && dt <= end;
    }

    /// <summary>
    /// Converts the date and time to a human-readable relative time string (e.g., "2 minutes ago").
    /// </summary>
    /// <param name="dt">The date and time to convert.</param>
    /// <returns>A string representing the relative time.</returns>
    public static string ToRelativeTime(this DateTime dt)
    {
        var timeSpan = DateTime.UtcNow - dt;

        return timeSpan.TotalSeconds < 60
            ? $"{(int)timeSpan.TotalSeconds} seconds ago"
            : timeSpan.TotalMinutes < 60
            ? $"{(int)timeSpan.TotalMinutes} minutes ago"
            : timeSpan.TotalHours < 24
            ? $"{(int)timeSpan.TotalHours} hours ago"
            : timeSpan.TotalDays < 30
            ? $"{(int)timeSpan.TotalDays} days ago"
            : $"{(int)(timeSpan.TotalDays / 30)} months ago";
    }

    /// <summary>
    /// Converts the date and time to an ISO 8601 formatted string.
    /// </summary>
    /// <param name="dt">The date and time to format.</param>
    /// <returns>An ISO 8601 formatted string.</returns>
    public static string ToIso8601String(this DateTime dt)
    {
        return dt.ToString("o"); // ISO 8601 format
    }

    /// <summary>
    /// Converts the date and time to a Unix timestamp (seconds since 1970-01-01 00:00:00 UTC).
    /// </summary>
    /// <param name="dt">The date and time to convert.</param>
    /// <returns>The Unix timestamp in seconds.</returns>
    public static long ToUnixTimestamp(this DateTime dt)
    {
        return (long)(dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }

    /// <summary>
    /// Converts a Unix timestamp to a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="timestamp">The Unix timestamp in seconds.</param>
    /// <returns>A <see cref="DateTime"/> representing the timestamp.</returns>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
    }

    /// <summary>
    /// Adds the specified number of business days (excluding weekends) to the date.
    /// </summary>
    /// <param name="dt">The date to add business days to.</param>
    /// <param name="days">The number of business days to add. Can be positive or negative.</param>
    /// <returns>A <see cref="DateTime"/> representing the resulting date after adding business days.</returns>
    public static DateTime AddBusinessDays(this DateTime dt, int days)
    {
        var direction = days > 0 ? 1 : -1;
        var count = 0;
        while (count < Math.Abs(days))
        {
            dt = dt.AddDays(direction);
            if (dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday)
                count++;
        }
        return dt;
    }

    /// <summary>
    /// Gets the start of the day (midnight) for the specified date.
    /// </summary>
    /// <param name="dt">The date to get the start of day for.</param>
    /// <returns>A <see cref="DateTime"/> representing midnight at the start of the day.</returns>
    public static DateTime StartOfDay(this DateTime dt)
    {
        return dt.Date;
    }

    /// <summary>
    /// Gets the end of the day (one tick before midnight of the next day) for the specified date.
    /// </summary>
    /// <param name="dt">The date to get the end of day for.</param>
    /// <returns>A <see cref="DateTime"/> representing the last tick of the day.</returns>
    public static DateTime EndOfDay(this DateTime dt)
    {
        return dt.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the month (first day at midnight) for the specified date.
    /// </summary>
    /// <param name="dt">The date to get the start of month for.</param>
    /// <returns>A <see cref="DateTime"/> representing the first day of the month at midnight.</returns>
    public static DateTime StartOfMonth(this DateTime dt)
    {
        return new DateTime(dt.Year, dt.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month (last day at 23:59:59.999) for the specified date.
    /// </summary>
    /// <param name="dt">The date to get the end of month for.</param>
    /// <returns>A <see cref="DateTime"/> representing the last moment of the month.</returns>
    public static DateTime EndOfMonth(this DateTime dt)
    {
        return dt.StartOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the year (January 1st at midnight) for the specified date.
    /// </summary>
    /// <param name="dt">The date to get the start of year for.</param>
    /// <returns>A <see cref="DateTime"/> representing the first day of the year at midnight.</returns>
    public static DateTime StartOfYear(this DateTime dt)
    {
        return new DateTime(dt.Year, 1, 1);
    }

    /// <summary>
    /// Gets the end of the year (December 31st at 23:59:59.999) for the specified date.
    /// </summary>
    /// <param name="dt">The date to get the end of year for.</param>
    /// <returns>A <see cref="DateTime"/> representing the last moment of the year.</returns>
    public static DateTime EndOfYear(this DateTime dt)
    {
        return new DateTime(dt.Year, 12, 31, 23, 59, 59, 999);
    }

    /// <summary>
    /// Formats a <see cref="TimeSpan"/> as a human-readable duration string.
    /// </summary>
    /// <param name="ts">The time span to format.</param>
    /// <returns>A human-readable duration string.</returns>
    public static string FormatDuration(this TimeSpan ts)
    {
        if (ts.TotalSeconds < 1) return "< 1 second";
        if (ts.TotalMinutes < 1) return $"{(int)ts.TotalSeconds} seconds";
        if (ts.TotalHours < 1) return $"{(int)ts.TotalMinutes} minutes";
        if (ts.TotalDays < 1) return $"{(int)ts.TotalHours} hours";
        return $"{(int)ts.TotalDays} days";
    }

    /// <summary>
    /// Measures the execution time of the specified action.
    /// </summary>
    /// <param name="action">The action to measure.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the elapsed time.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    public static TimeSpan Measure(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        action();
        watch.Stop();
        return watch.Elapsed;
    }

    /// <summary>
    /// Measures the execution time of the specified async action.
    /// </summary>
    /// <param name="action">The async action to measure.</param>
    /// <returns>A <see cref="TimeSpan"/> representing the elapsed time.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    public static async Task<TimeSpan> MeasureAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        await action();
        watch.Stop();
        return watch.Elapsed;
    }
}
