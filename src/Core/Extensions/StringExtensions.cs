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
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);

    public static string? NullIfEmpty(this string? value) => string.IsNullOrEmpty(value) ? null : value;

    public static string ToTitleCase(this string value)
    {
        if (value.IsNullOrEmpty()) return value;
        var textInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(value.ToLowerInvariant());
    }

    public static string ToCamelCase(this string value)
    {
        if (value.IsNullOrEmpty() || value.Length < 2) return value;
        return char.ToLowerInvariant(value[0]) + value.Substring(1);
    }

    public static string ToSnakeCase(this string value)
    {
        if (value.IsNullOrEmpty()) return value;

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

    public static string ToKebabCase(this string value)
    {
        return value.ToSnakeCase().Replace('_', '-');
    }

    // Truncate string to max length with ellipsis
    public static string Truncate(this string value, int maxLength, string ellipsis = "...")
    {
        if (value.IsNullOrEmpty() || value.Length <= maxLength) return value;
        return value.Substring(0, maxLength - ellipsis.Length) + ellipsis;
    }

    // Count occurrences of substring
    public static int CountOccurrences(this string value, string substring)
    {
        if (value.IsNullOrEmpty() || substring.IsNullOrEmpty()) return 0;
        return (value.Length - value.Replace(substring, string.Empty).Length) / substring.Length;
    }

    // Remove prefix safely
    public static string RemovePrefix(this string value, string prefix)
    {
        if (value.IsNullOrEmpty()) return value;
        return value.StartsWith(prefix) ? value.Substring(prefix.Length) : value;
    }

    // Remove suffix safely
    public static string RemoveSuffix(this string value, string suffix)
    {
        if (value.IsNullOrEmpty()) return value;
        return value.EndsWith(suffix) ? value.Substring(0, value.Length - suffix.Length) : value;
    }

    // Verify email format (basic validation)
    public static bool IsValidEmail(this string value)
    {
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

    // Verify URL format
    public static bool IsValidUrl(this string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    // Create slug from text (for URLs)
    public static string ToSlug(this string value)
    {
        if (value.IsNullOrEmpty()) return string.Empty;

        var slug = value.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-");

        var allowed = "abcdefghijklmnopqrstuvwxyz0123456789-";
        var result = new System.Text.StringBuilder();

        foreach (var c in slug)
        {
            if (allowed.Contains(c))
                result.Append(c);
        }

        return result.ToString().Trim('-');
    }

    // Repeat string n times
    public static string Repeat(this string value, int count)
    {
        if (count <= 0) return string.Empty;
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < count; i++)
        {
            result.Append(value);
        }
        return result.ToString();
    }

    // Split and trim each part
    public static string[] SplitAndTrim(this string value, params char[] separators)
    {
        return value?.Split(separators)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray() ?? [];
    }
}
