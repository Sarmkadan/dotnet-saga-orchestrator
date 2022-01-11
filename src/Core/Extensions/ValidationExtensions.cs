#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Validation extension methods for common validation patterns.
/// Provides fluent validation API for parameter checking.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that the specified value is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static T NotNull<T>(this T? value, string paramName) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    /// <summary>
    /// Validates that the specified string is not null or empty.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is empty.</exception>
    public static string NotNullOrEmpty(this string? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (value.Length == 0)
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified string is not null or whitespace.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is whitespace.</exception>
    public static string NotNullOrWhiteSpace(this string? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be whitespace", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified integer is within the specified range [min, max].
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside the specified range.</exception>
    public static int InRange(this int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max} (inclusive). Actual: {value}.");
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified integer is greater than the specified minimum value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum value (exclusive).</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not greater than <paramref name="min"/>.</exception>
    public static int GreaterThan(this int value, int min, string paramName)
    {
        if (value <= min)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Value must be greater than {min}. Actual: {value}.");
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified integer is greater than or equal to the specified minimum value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is less than <paramref name="min"/>.</exception>
    public static int GreaterThanOrEqual(this int value, int min, string paramName)
    {
        if (value < min)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Value must be greater than or equal to {min}. Actual: {value}.");
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified long integer is greater than the specified minimum value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum value (exclusive).</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not greater than <paramref name="min"/>.</exception>
    public static long GreaterThan(this long value, long min, string paramName)
    {
        if (value <= min)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Value must be greater than {min}. Actual: {value}.");
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified TimeSpan is greater than zero.
    /// </summary>
    /// <param name="value">The TimeSpan to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not greater than zero.</exception>
    public static TimeSpan GreaterThanZero(this TimeSpan value, string paramName)
    {
        if (value <= TimeSpan.Zero)
        {
            throw new ArgumentException($"{paramName} must be greater than zero. Actual: {value}.", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified decimal is within the specified range [min, max].
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated value.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside the specified range.</exception>
    public static decimal InRange(this decimal value, decimal min, decimal max, string paramName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Value must be between {min} and {max} (inclusive). Actual: {value}.");
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified enumerable is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the enumerable.</typeparam>
    /// <param name="value">The enumerable to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated enumerable.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is empty.</exception>
    public static IEnumerable<T> NotEmpty<T>(this IEnumerable<T>? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (!value.Any())
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified GUID is not empty.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated GUID.</returns>
    /// <exception cref="ArgumentException"><paramref name="value"/> is empty.</exception>
    public static Guid NotEmpty(this Guid value, string paramName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{paramName} cannot be empty GUID", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates the value using the specified validator function if the value is not null.
    /// </summary>
    /// <typeparam name="T">The type of the value being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="validator">The validation function.</param>
    /// <param name="message">The error message to throw if validation fails.</param>
    /// <returns>The validated value, or null if validation fails.</returns>
    /// <exception cref="ArgumentException">Validation failed.</exception>
    public static T? ValidateIf<T>(this T? value, Func<T, bool> validator, string message)
    {
        if (value != null && !validator(value))
        {
            throw new ArgumentException(message);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified string is a properly formatted email address.
    /// </summary>
    /// <param name="value">The email address to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated email address.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a valid email address.</exception>
    public static string ValidateEmail(this string value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (!value.IsValidEmail())
        {
            throw new ArgumentException($"{paramName} must be a valid email address. Actual: '{value}'.", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified string is a properly formatted absolute HTTP or HTTPS URL.
    /// </summary>
    /// <param name="value">The URL to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated URL.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not a valid URL.</exception>
    public static string ValidateUrl(this string value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (!value.IsValidUrl())
        {
            throw new ArgumentException($"{paramName} must be a valid absolute HTTP/HTTPS URL. Actual: '{value}'.", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified string does not exceed the maximum length.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="maxLength">The maximum allowed length (inclusive).</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> exceeds the maximum length.</exception>
    public static string MaxLength(this string value, int maxLength, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (value.Length > maxLength)
        {
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters. Actual: {value.Length}.", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified string has at least the minimum length.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="minLength">The minimum required length (inclusive).</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is shorter than the minimum length.</exception>
    public static string MinLength(this string value, int minLength, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (value.Length < minLength)
        {
            throw new ArgumentException($"{paramName} must be at least {minLength} characters. Actual: {value.Length}.", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified array is not null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="value">The array to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated array.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is empty.</exception>
    public static T[] NotEmptyArray<T>(this T[]? value, string paramName)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (value.Length == 0)
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
        return value;
    }

    /// <summary>
    /// Validates that the specified dictionary is not null or empty.
    /// </summary>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    /// <param name="value">The dictionary to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    /// <returns>The validated dictionary.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="value"/> is empty.</exception>
    public static Dictionary<K, V> NotEmptyDictionary<K, V>(this Dictionary<K, V>? value, string paramName) where K : notnull
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        if (value.Count == 0)
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
        return value;
    }
}
