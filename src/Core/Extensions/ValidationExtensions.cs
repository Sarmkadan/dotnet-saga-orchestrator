#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Validation extension methods for common validation patterns.
/// Provides fluent validation API for parameter checking.
/// </summary>
public static class ValidationExtensions
{
    public static T NotNull<T>(this T? value, string paramName) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    public static string NotNullOrEmpty(this string? value, string paramName)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        return value;
    }

    public static string NotNullOrWhiteSpace(this string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} cannot be null or whitespace", paramName);
        return value;
    }

    public static int InRange(this int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max}");
        return value;
    }

    public static int GreaterThan(this int value, int min, string paramName)
    {
        if (value <= min)
            throw new ArgumentOutOfRangeException(paramName, $"Value must be greater than {min}");
        return value;
    }

    public static int GreaterThanOrEqual(this int value, int min, string paramName)
    {
        if (value < min)
            throw new ArgumentOutOfRangeException(paramName, $"Value must be greater than or equal to {min}");
        return value;
    }

    public static long GreaterThan(this long value, long min, string paramName)
    {
        if (value <= min)
            throw new ArgumentOutOfRangeException(paramName, $"Value must be greater than {min}");
        return value;
    }

    public static TimeSpan GreaterThanZero(this TimeSpan value, string paramName)
    {
        if (value <= TimeSpan.Zero)
            throw new ArgumentException($"{paramName} must be greater than zero", paramName);
        return value;
    }

    public static decimal InRange(this decimal value, decimal min, decimal max, string paramName)
    {
        if (value < min || value > max)
            throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max}");
        return value;
    }

    public static IEnumerable<T> NotEmpty<T>(this IEnumerable<T>? value, string paramName)
    {
        if (value == null || !value.Any())
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        return value;
    }

    public static Guid NotEmpty(this Guid value, string paramName)
    {
        if (value == Guid.Empty)
            throw new ArgumentException($"{paramName} cannot be empty GUID", paramName);
        return value;
    }

    public static T? ValidateIf<T>(this T? value, Func<T, bool> validator, string message)
    {
        if (value != null && !validator(value))
            throw new ArgumentException(message);
        return value;
    }

    public static string ValidateEmail(this string value, string paramName)
    {
        if (!value.IsValidEmail())
            throw new ArgumentException($"{paramName} must be a valid email address", paramName);
        return value;
    }

    public static string ValidateUrl(this string value, string paramName)
    {
        if (!value.IsValidUrl())
            throw new ArgumentException($"{paramName} must be a valid URL", paramName);
        return value;
    }

    public static string MaxLength(this string value, int maxLength, string paramName)
    {
        if (value.Length > maxLength)
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters", paramName);
        return value;
    }

    public static string MinLength(this string value, int minLength, string paramName)
    {
        if (value.Length < minLength)
            throw new ArgumentException($"{paramName} must be at least {minLength} characters", paramName);
        return value;
    }

    public static T[] NotEmptyArray<T>(this T[]? value, string paramName)
    {
        if (value == null || value.Length == 0)
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        return value;
    }

    public static Dictionary<K, V> NotEmptyDictionary<K, V>(this Dictionary<K, V>? value, string paramName) where K : notnull
    {
        if (value == null || value.Count == 0)
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        return value;
    }
}
