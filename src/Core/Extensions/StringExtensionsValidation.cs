#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Mail;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Validation helpers for <see cref="StringExtensions"/> extension methods.
/// Provides comprehensive validation for string operations used throughout the orchestrator.
/// </summary>
public static class StringExtensionsValidation
{
    /// <summary>
    /// Validates string extension method behavior and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The string to validate extension methods against.</param>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate IsNullOrEmpty
        if (value.IsNullOrEmpty() is false)
        {
            problems.Add("IsNullOrEmpty method is not working correctly - should return false for non-null/empty strings");
        }

        // Validate IsNullOrWhiteSpace
        if (value.IsNullOrWhiteSpace() is false)
        {
            problems.Add("IsNullOrWhiteSpace method is not working correctly - should return false for non-null/whitespace strings");
        }

        // Validate NullIfEmpty
        var nullResult = string.Empty.NullIfEmpty();
        if (nullResult is not null)
        {
            problems.Add("NullIfEmpty method is not working correctly - should return null for empty strings");
        }

        // Validate ToTitleCase
        try
        {
            var titleCaseResult = "test".ToTitleCase();
            if (titleCaseResult != "Test")
            {
                problems.Add("ToTitleCase method is not working correctly - expected 'Test' for input 'test'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToTitleCase method threw exception: {ex.Message}");
        }

        // Validate ToCamelCase
        try
        {
            var camelCaseResult = "TestString".ToCamelCase();
            if (camelCaseResult != "testString")
            {
                problems.Add("ToCamelCase method is not working correctly - expected 'testString' for input 'TestString'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToCamelCase method threw exception: {ex.Message}");
        }

        // Validate ToSnakeCase
        try
        {
            var snakeCaseResult = "TestString".ToSnakeCase();
            if (snakeCaseResult != "test_string")
            {
                problems.Add("ToSnakeCase method is not working correctly - expected 'test_string' for input 'TestString'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToSnakeCase method threw exception: {ex.Message}");
        }

        // Validate ToKebabCase
        try
        {
            var kebabCaseResult = "TestString".ToKebabCase();
            if (kebabCaseResult != "test-string")
            {
                problems.Add("ToKebabCase method is not working correctly - expected 'test-string' for input 'TestString'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToKebabCase method threw exception: {ex.Message}");
        }

        // Validate Truncate
        try
        {
            var truncateResult = "Hello World".Truncate(5);
            if (truncateResult != "Hello...")
            {
                problems.Add("Truncate method is not working correctly - expected 'Hello...' for input 'Hello World' with maxLength 5");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Truncate method threw exception: {ex.Message}");
        }

        // Validate CountOccurrences
        try
        {
            var countResult = "Hello World".CountOccurrences("l");
            if (countResult != 3)
            {
                problems.Add("CountOccurrences method is not working correctly - expected 3 for input 'Hello World' with substring 'l'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"CountOccurrences method threw exception: {ex.Message}");
        }

        // Validate RemovePrefix
        try
        {
            var removePrefixResult = "Hello World".RemovePrefix("Hello ");
            if (removePrefixResult != "World")
            {
                problems.Add("RemovePrefix method is not working correctly - expected 'World' for input 'Hello World' with prefix 'Hello '");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"RemovePrefix method threw exception: {ex.Message}");
        }

        // Validate RemoveSuffix
        try
        {
            var removeSuffixResult = "Hello World".RemoveSuffix(" World");
            if (removeSuffixResult != "Hello")
            {
                problems.Add("RemoveSuffix method is not working correctly - expected 'Hello' for input 'Hello World' with suffix ' World'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"RemoveSuffix method threw exception: {ex.Message}");
        }

        // Validate IsValidEmail
        try
        {
            var isValidEmailResult = "test@example.com".IsValidEmail();
            if (isValidEmailResult is false)
            {
                problems.Add("IsValidEmail method is not working correctly - expected true for valid email 'test@example.com'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"IsValidEmail method threw exception: {ex.Message}");
        }

        // Validate IsValidUrl
        try
        {
            var isValidUrlResult = "https://example.com".IsValidUrl();
            if (isValidUrlResult is false)
            {
                problems.Add("IsValidUrl method is not working correctly - expected true for valid URL 'https://example.com'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"IsValidUrl method threw exception: {ex.Message}");
        }

        // Validate ToSlug
        try
        {
            var toSlugResult = "Hello World Test".ToSlug();
            if (toSlugResult != "hello-world-test")
            {
                problems.Add("ToSlug method is not working correctly - expected 'hello-world-test' for input 'Hello World Test'");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ToSlug method threw exception: {ex.Message}");
        }

        // Validate Repeat
        try
        {
            var repeatResult = "abc".Repeat(3);
            if (repeatResult != "abcabcabc")
            {
                problems.Add("Repeat method is not working correctly - expected 'abcabcabc' for input 'abc' with count 3");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"Repeat method threw exception: {ex.Message}");
        }

        // Validate SplitAndTrim
        try
        {
            var splitResult = "Hello, World, Test".SplitAndTrim(new char[] { ',' });
            if (splitResult.Length != 3 || splitResult[0] != "Hello" || splitResult[1] != "World" || splitResult[2] != "Test")
            {
                problems.Add("SplitAndTrim method is not working correctly - expected array with 3 elements ['Hello', 'World', 'Test']");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"SplitAndTrim method threw exception: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified string extension methods are working correctly.
    /// </summary>
    /// <param name="value">The string to validate extension methods against.</param>
    /// <returns><c>true</c> if the extension methods are working correctly; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValid(this string? value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified string extension methods are working correctly, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The string to validate extension methods against.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Extension methods are not working correctly.</exception>
    public static void EnsureValid(this string? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"StringExtensions validation failed:{Environment.NewLine}  - {
                    string.Join($"{Environment.NewLine}  - ", problems)
                }");
        }
    }
}