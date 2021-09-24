// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Extension methods for DateTime and TimeSpan operations.
/// Provides utilities for time calculations, formatting, and validation.
/// </summary>
public static class DateTimeExtensions
{
    // Check if datetime is expired
    public static bool IsExpired(this DateTime deadline, DateTime? now = null)
    {
        return DateTime.UtcNow > deadline;
    }

    // Get time remaining until deadline
    public static TimeSpan TimeUntil(this DateTime deadline, DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        var remaining = deadline - current;
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    // Calculate elapsed time
    public static TimeSpan ElapsedSince(this DateTime startTime, DateTime? now = null)
    {
        var current = now ?? DateTime.UtcNow;
        return current - startTime;
    }

    // Round down to nearest second/minute/hour
    public static DateTime RoundDownToSecond(this DateTime dt) =>
        new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, DateTimeKind.Utc);

    public static DateTime RoundDownToMinute(this DateTime dt) =>
        new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, DateTimeKind.Utc);

    public static DateTime RoundDownToHour(this DateTime dt) =>
        new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, DateTimeKind.Utc);

    // Check if datetime is within range
    public static bool IsWithinRange(this DateTime dt, DateTime start, DateTime end)
    {
        return dt >= start && dt <= end;
    }

    // Get human-readable relative time (e.g., "2 minutes ago")
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

    // ISO 8601 formatting
    public static string ToIso8601String(this DateTime dt) =>
        dt.ToString("o"); // ISO 8601 format

    // Convert to Unix timestamp
    public static long ToUnixTimestamp(this DateTime dt) =>
        (long)(dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

    // Convert from Unix timestamp
    public static DateTime FromUnixTimestamp(long timestamp) =>
        new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);

    // Add business days (excluding weekends)
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

    // Get start of day
    public static DateTime StartOfDay(this DateTime dt) =>
        dt.Date;

    // Get end of day
    public static DateTime EndOfDay(this DateTime dt) =>
        dt.Date.AddDays(1).AddTicks(-1);

    // Get start of month
    public static DateTime StartOfMonth(this DateTime dt) =>
        new DateTime(dt.Year, dt.Month, 1);

    // Get end of month
    public static DateTime EndOfMonth(this DateTime dt) =>
        dt.StartOfMonth().AddMonths(1).AddTicks(-1);

    // Get start of year
    public static DateTime StartOfYear(this DateTime dt) =>
        new DateTime(dt.Year, 1, 1);

    // Get end of year
    public static DateTime EndOfYear(this DateTime dt) =>
        new DateTime(dt.Year, 12, 31, 23, 59, 59, 999);

    // Format duration as human-readable
    public static string FormatDuration(this TimeSpan ts)
    {
        if (ts.TotalSeconds < 1) return "< 1 second";
        if (ts.TotalMinutes < 1) return $"{(int)ts.TotalSeconds} seconds";
        if (ts.TotalHours < 1) return $"{(int)ts.TotalMinutes} minutes";
        if (ts.TotalDays < 1) return $"{(int)ts.TotalHours} hours";
        return $"{(int)ts.TotalDays} days";
    }

    // Measure execution time
    public static TimeSpan Measure(Action action)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        action();
        watch.Stop();
        return watch.Elapsed;
    }

    // Measure async execution time
    public static async Task<TimeSpan> MeasureAsync(Func<Task> action)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        await action();
        watch.Stop();
        return watch.Elapsed;
    }
}
