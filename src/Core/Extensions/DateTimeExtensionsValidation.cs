#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Validation extension methods for DateTime.
/// Provides validation helpers to ensure DateTime values are valid for DateTimeExtensions operations.
/// </summary>
public static class DateTimeExtensionsValidation
{
    /// <summary>
    /// Validates a DateTime value for use with DateTimeExtensions methods.
    /// Checks that the DateTime is not default and produces valid results for common operations.
    /// </summary>
    /// <param name="value">The DateTime value to validate.</param>
    /// <returns>An empty list if valid, otherwise a list of validation error messages.</returns>
    /// <exception cref="ArgumentException">Thrown if value is default DateTime.</exception>
    public static IReadOnlyList<string> Validate(this DateTime value)
    {
        var errors = new List<string>();

        // Check if value is default (uninitialized)
        if (value == default)
        {
            errors.Add("DateTime value is default (uninitialized).");
            return errors.AsReadOnly();
        }

        // Validate RoundDownToSecond - should produce valid DateTime
        try
        {
            var roundDownToSecond = value.RoundDownToSecond();
            if (roundDownToSecond.Kind != DateTimeKind.Utc)
            {
                errors.Add("RoundDownToSecond result has incorrect DateTimeKind (expected UTC).");
            }
        }
        catch
        {
            errors.Add("RoundDownToSecond threw an exception.");
        }

        // Validate RoundDownToMinute - should produce valid DateTime
        try
        {
            var roundDownToMinute = value.RoundDownToMinute();
            if (roundDownToMinute.Kind != DateTimeKind.Utc)
            {
                errors.Add("RoundDownToMinute result has incorrect DateTimeKind (expected UTC).");
            }
        }
        catch
        {
            errors.Add("RoundDownToMinute threw an exception.");
        }

        // Validate RoundDownToHour - should produce valid DateTime
        try
        {
            var roundDownToHour = value.RoundDownToHour();
            if (roundDownToHour.Kind != DateTimeKind.Utc)
            {
                errors.Add("RoundDownToHour result has incorrect DateTimeKind (expected UTC).");
            }
        }
        catch
        {
            errors.Add("RoundDownToHour threw an exception.");
        }

        // Validate StartOfDay - should produce valid DateTime
        try
        {
            var startOfDay = value.StartOfDay();
            if (startOfDay.Kind != DateTimeKind.Utc)
            {
                errors.Add("StartOfDay result has incorrect DateTimeKind (expected UTC).");
            }
        }
        catch
        {
            errors.Add("StartOfDay threw an exception.");
        }

        // Validate EndOfDay - should produce valid DateTime and be after StartOfDay
        try
        {
            var startOfDay = value.StartOfDay();
            var endOfDay = value.EndOfDay();

            if (endOfDay.Kind != DateTimeKind.Utc)
            {
                errors.Add("EndOfDay result has incorrect DateTimeKind (expected UTC).");
            }
            else if (endOfDay <= startOfDay)
            {
                errors.Add("EndOfDay result is not after StartOfDay.");
            }
        }
        catch
        {
            errors.Add("EndOfDay threw an exception.");
        }

        // Validate StartOfMonth - should produce valid DateTime
        try
        {
            var startOfMonth = value.StartOfMonth();
            if (startOfMonth.Kind != DateTimeKind.Utc)
            {
                errors.Add("StartOfMonth result has incorrect DateTimeKind (expected UTC).");
            }
        }
        catch
        {
            errors.Add("StartOfMonth threw an exception.");
        }

        // Validate EndOfMonth - should produce valid DateTime and be after StartOfMonth
        try
        {
            var startOfMonth = value.StartOfMonth();
            var endOfMonth = value.EndOfMonth();

            if (endOfMonth.Kind != DateTimeKind.Utc)
            {
                errors.Add("EndOfMonth result has incorrect DateTimeKind (expected UTC).");
            }
            else if (endOfMonth <= startOfMonth)
            {
                errors.Add("EndOfMonth result is not after StartOfMonth.");
            }
        }
        catch
        {
            errors.Add("EndOfMonth threw an exception.");
        }

        // Validate StartOfYear - should produce valid DateTime
        try
        {
            var startOfYear = value.StartOfYear();
            if (startOfYear.Kind != DateTimeKind.Utc)
            {
                errors.Add("StartOfYear result has incorrect DateTimeKind (expected UTC).");
            }
        }
        catch
        {
            errors.Add("StartOfYear threw an exception.");
        }

        // Validate EndOfYear - should produce valid DateTime and be after StartOfYear
        try
        {
            var startOfYear = value.StartOfYear();
            var endOfYear = value.EndOfYear();

            if (endOfYear.Kind != DateTimeKind.Utc)
            {
                errors.Add("EndOfYear result has incorrect DateTimeKind (expected UTC).");
            }
            else if (endOfYear <= startOfYear)
            {
                errors.Add("EndOfYear result is not after StartOfYear.");
            }
        }
        catch
        {
            errors.Add("EndOfYear threw an exception.");
        }

        // Validate ToUnixTimestamp - should not throw and produce non-negative result for dates after epoch
        try
        {
            var unixTimestamp = value.ToUnixTimestamp();
            if (unixTimestamp < 0)
            {
                errors.Add("ToUnixTimestamp result is negative (date is before Unix epoch).");
            }
        }
        catch
        {
            errors.Add("ToUnixTimestamp threw an exception.");
        }

        // Validate FromUnixTimestamp - should not throw and produce valid DateTime
        try
        {
            var fromUnix = DateTimeExtensions.FromUnixTimestamp(value.ToUnixTimestamp());
            if (fromUnix.Kind != DateTimeKind.Utc)
            {
                errors.Add("FromUnixTimestamp result has incorrect DateTimeKind (expected UTC).");
            }
        }
        catch
        {
            errors.Add("FromUnixTimestamp threw an exception.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a DateTime value is valid for use with DateTimeExtensions methods.
    /// </summary>
    /// <param name="value">The DateTime value to check.</param>
    /// <returns>True if valid, otherwise false.</returns>
    public static bool IsValid(this DateTime value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that a DateTime value is valid for use with DateTimeExtensions methods,
    /// throwing an ArgumentException if not.
    /// </summary>
    /// <param name="value">The DateTime value to validate.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this DateTime value)
    {
        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", errors)}");
        }
    }
}