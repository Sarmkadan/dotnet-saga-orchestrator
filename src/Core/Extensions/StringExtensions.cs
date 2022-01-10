#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Extension methods for string manipulation and validation.
/// Provides common string operations used throughout the orchestrator.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Determines whether the specified string is null or empty.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns><c>true</c> if the value is null or empty; otherwise, <c>false</c>.</returns>
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// Determines whether the specified string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="value">The string to test.</param>
    /// <returns><c>true</c> if the value is null, empty, or white-space; otherwise, <c>false</c>.</returns>
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Returns null if the string is empty; otherwise, returns the original string.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The original string if not empty; otherwise, null.</returns>
    public static string? NullIfEmpty(this string? value) => string.IsNullOrEmpty(value) ? null : value;

    /// <summary>
    /// Converts the specified string to title case using the current culture.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string converted to title case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToTitleCase(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length == 0)
        {
            return value;
        }

        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLowerInvariant());
    }

    /// <summary>
    /// Converts the specified string to camel case.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string converted to camel case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToCamelCase(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.Length < 2
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
    }

    /// <summary>
    /// Converts the specified PascalCase or camelCase string to snake_case.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string converted to snake_case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToSnakeCase(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length == 0)
        {
            return value;
        }

        var result = new System.Text.StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            if (char.IsUpper(value[i]) && i > 0)
            {
                result.Append('_');
            }

            result.Append(char.ToLowerInvariant(value[i]));
        }

        return result.ToString();
    }

    /// <summary>
    /// Converts the specified PascalCase or camelCase string to kebab-case.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The string converted to kebab-case.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToKebabCase(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.ToSnakeCase().Replace('_', '-');
    }

    /// <summary>
    /// Truncates the string to the specified maximum length, adding an ellipsis if truncated.
    /// </summary>
    /// <param name="value">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <param name="ellipsis">The ellipsis string to append when truncating. Defaults to "...".</param>
    /// <returns>The truncated string, or the original string if it's shorter than maxLength.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is less than 0.</exception>
    public static string Truncate(this string value, int maxLength, string ellipsis = "...")
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..Math.Min(maxLength, value.Length)] + ellipsis;
    }

    /// <summary>
    /// Counts the occurrences of a substring within the string.
    /// </summary>
    /// <param name="value">The string to search.</param>
    /// <param name="substring">The substring to count.</param>
    /// <returns>The number of occurrences of the substring.</returns>
    /// <exception cref="ArgumentNullException">Either <paramref name="value"/> or <paramref name="substring"/> is null.</exception>
    public static int CountOccurrences(this string value, string substring)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(substring);

        if (substring.Length == 0 || value.Length < substring.Length)
        {
            return 0;
        }

        int count = 0;
        int index = 0;
        while ((index = value.IndexOf(substring, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += substring.Length;
        }

        return count;
    }

    /// <summary>
    /// Removes the specified prefix from the string if it exists.
    /// </summary>
    /// <param name="value">The string to process.</param>
    /// <param name="prefix">The prefix to remove.</param>
    /// <returns>The string with the prefix removed, or the original string if the prefix was not found.</returns>
    /// <exception cref="ArgumentNullException">Either <paramref name="value"/> or <paramref name="prefix"/> is null.</exception>
    public static string RemovePrefix(this string value, string prefix)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(prefix);

        return value.StartsWith(prefix, StringComparison.Ordinal)
            ? value[prefix.Length..]
            : value;
    }

    /// <summary>
    /// Removes the specified suffix from the string if it exists.
    /// </summary>
    /// <param name="value">The string to process.</param>
    /// <param name="suffix">The suffix to remove.</param>
    /// <returns>The string with the suffix removed, or the original string if the suffix was not found.</returns>
    /// <exception cref="ArgumentNullException">Either <paramref name="value"/> or <paramref name="suffix"/> is null.</exception>
    public static string RemoveSuffix(this string value, string suffix)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(suffix);

        return value.EndsWith(suffix, StringComparison.Ordinal)
            ? value[..^suffix.Length]
            : value;
    }

    /// <summary>
    /// Validates that the string is a properly formatted email address.
    /// </summary>
    /// <param name="value">The email address to validate.</param>
    /// <returns><c>true</c> if the string is a valid email address; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValidEmail(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates that the string is a properly formatted absolute HTTP or HTTPS URL.
    /// </summary>
    /// <param name="value">The URL to validate.</param>
    /// <returns><c>true</c> if the string is a valid absolute HTTP/HTTPS URL; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValidUrl(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Creates a URL-friendly slug from the specified text.
    /// </summary>
    /// <param name="value">The text to convert to a slug.</param>
    /// <returns>A URL-friendly slug string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static string ToSlug(this string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length == 0)
        {
            return string.Empty;
        }

        var slug = value
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-");

        var allowed = "abcdefghijklmnopqrstuvwxyz0123456789-";
        var result = new System.Text.StringBuilder(slug.Length);

        foreach (var c in slug)
        {
            if (allowed.Contains(c))
            {
                result.Append(c);
            }
        }

        return result.ToString().Trim('-');
    }

    /// <summary>
    /// Repeats the string the specified number of times.
    /// </summary>
    /// <param name="value">The string to repeat.</param>
    /// <param name="count">The number of times to repeat the string.</param>
    /// <returns>A new string containing the repeated value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 0.</exception>
    public static string Repeat(this string value, int count)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        return count == 0
            ? string.Empty
            : string.Concat(Enumerable.Repeat(value, count));
    }

    /// <summary>
    /// Splits the string using the specified separators and trims each resulting substring.
    /// </summary>
    /// <param name="value">The string to split.</param>
    /// <param name="separators">The character separators to use.</param>
    /// <returns>An array of trimmed, non-empty substrings.</returns>
    public static string[] SplitAndTrim(this string? value, params char[] separators)
    {
        if (value is null)
        {
            return [];
        }

        return value
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToArray();
    }
}