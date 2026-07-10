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

        // Validate RoundDownToSecond - should not be default(DateTime)
        var roundDownToSecond = value.RoundDownToSecond();
        if (roundDownToSecond == default)
        {
            errors.Add("RoundDownToSecond result is default DateTime (uninitialized).");
        }

        // Validate RoundDownToMinute - should not be default(DateTime)
        var roundDownToMinute = value.RoundDownToMinute();
        if (roundDownToMinute == default)
        {
            errors.Add("RoundDownToMinute result is default DateTime (uninitialized).");
        }

        // Validate RoundDownToHour - should not be default(DateTime)
        var roundDownToHour = value.RoundDownToHour();
        if (roundDownToHour == default)
        {
            errors.Add("RoundDownToHour result is default DateTime (uninitialized).");
        }

        // Validate StartOfDay - should not be default(DateTime)
        var startOfDay = value.StartOfDay();
        if (startOfDay == default)
        {
            errors.Add("StartOfDay result is default DateTime (uninitialized).");
        }

        // Validate EndOfDay - should not be default(DateTime) and should be after StartOfDay
        var endOfDay = value.EndOfDay();
        if (endOfDay == default)
        {
            errors.Add("EndOfDay result is default DateTime (uninitialized).");
        }
        else if (endOfDay <= startOfDay)
        {
            errors.Add("EndOfDay result is not after StartOfDay.");
        }

        // Validate StartOfMonth - should not be default(DateTime)
        var startOfMonth = value.StartOfMonth();
        if (startOfMonth == default)
        {
            errors.Add("StartOfMonth result is default DateTime (uninitialized).");
        }

        // Validate EndOfMonth - should not be default(DateTime) and should be after StartOfMonth
        var endOfMonth = value.EndOfMonth();
        if (endOfMonth == default)
        {
            errors.Add("EndOfMonth result is default DateTime (uninitialized).");
        }
        else if (endOfMonth <= startOfMonth)
        {
            errors.Add("EndOfMonth result is not after StartOfMonth.");
        }

        // Validate StartOfYear - should not be default(DateTime)
        var startOfYear = value.StartOfYear();
        if (startOfYear == default)
        {
            errors.Add("StartOfYear result is default DateTime (uninitialized).");
        }

        // Validate EndOfYear - should not be default(DateTime) and should be after StartOfYear
        var endOfYear = value.EndOfYear();
        if (endOfYear == default)
        {
            errors.Add("EndOfYear result is default DateTime (uninitialized).");
        }
        else if (endOfYear <= startOfYear)
        {
            errors.Add("EndOfYear result is not after StartOfYear.");
        }

        // Validate ToUnixTimestamp - should be non-negative for dates after epoch
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
            errors.Add("ToUnixTimestamp threw an exception (invalid date conversion).");
        }

        // Validate FromUnixTimestamp - should not throw and produce valid DateTime
        try
        {
            var fromUnix = DateTimeExtensions.FromUnixTimestamp(0);
            if (fromUnix == default)
            {
                errors.Add("FromUnixTimestamp(0) result is default DateTime (uninitialized).");
            }
        }
        catch
        {
            errors.Add("FromUnixTimestamp threw an exception (invalid timestamp).");
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
    /// <exception cref="ArgumentException">Thrown if the value is not valid, containing all validation errors.</exception>
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