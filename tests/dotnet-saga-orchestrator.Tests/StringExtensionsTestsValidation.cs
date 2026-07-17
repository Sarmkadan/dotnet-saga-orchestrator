#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Reflection;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="StringExtensionsTests"/> instances.
/// Validates that the test class contains all expected test methods.
/// </summary>
public static class StringExtensionsTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="StringExtensionsTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this StringExtensionsTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate that all expected test methods exist
        var expectedTestMethods = new[]
        {
            nameof(StringExtensionsTests.ToSnakeCase_PascalCaseInput_InsertsUnderscoreBetweenWords),
            nameof(StringExtensionsTests.ToSnakeCase_SingleWord_ReturnsLowercase),
            nameof(StringExtensionsTests.ToKebabCase_PascalCaseInput_ReturnsHyphenSeparated),
            nameof(StringExtensionsTests.ToCamelCase_PascalCase_LowercasesFirstCharacter),
            nameof(StringExtensionsTests.ToCamelCase_SingleCharacter_ReturnsLowercase),
            nameof(StringExtensionsTests.Truncate_StringLongerThanMax_AppendsEllipsis),
            nameof(StringExtensionsTests.Truncate_StringShorterThanMax_ReturnsOriginalUnchanged),
            nameof(StringExtensionsTests.CountOccurrences_SubstringRepeatedMultipleTimes_ReturnsExactCount),
            nameof(StringExtensionsTests.CountOccurrences_SubstringNotPresent_ReturnsZero),
            nameof(StringExtensionsTests.ToSlug_StringWithSpacesAndSpecialChars_ReturnsUrlFriendlySlug),
            nameof(StringExtensionsTests.ToSlug_EmptyString_ReturnsEmptyString),
            nameof(StringExtensionsTests.RemovePrefix_PrefixPresent_RemovesPrefix),
            nameof(StringExtensionsTests.RemovePrefix_PrefixAbsent_ReturnsOriginalValue),
            nameof(StringExtensionsTests.RemoveSuffix_SuffixPresent_RemovesSuffix),
            nameof(StringExtensionsTests.NullIfEmpty_EmptyString_ReturnsNull),
            nameof(StringExtensionsTests.NullIfEmpty_NonEmptyString_ReturnsSameValue),
            nameof(StringExtensionsTests.Repeat_PositiveCount_ConcatenatesStringNTimes),
            nameof(StringExtensionsTests.Repeat_ZeroCount_ReturnsEmptyString),
            nameof(StringExtensionsTests.SplitAndTrim_StringWithSpacesAroundDelimiters_ReturnsTrimmedParts)
        };

        var testClassType = typeof(StringExtensionsTests);
        var actualTestMethods = testClassType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.Name.StartsWith("To", StringComparison.Ordinal) ||
                       m.Name.StartsWith("Truncate", StringComparison.Ordinal) ||
                       m.Name.StartsWith("CountOccurrences", StringComparison.Ordinal) ||
                       m.Name.StartsWith("ToSlug", StringComparison.Ordinal) ||
                       m.Name.StartsWith("Remove", StringComparison.Ordinal) ||
                       m.Name.StartsWith("NullIfEmpty", StringComparison.Ordinal) ||
                       m.Name.StartsWith("Repeat", StringComparison.Ordinal) ||
                       m.Name.StartsWith("SplitAndTrim", StringComparison.Ordinal))
            .Select(m => m.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var expectedMethod in expectedTestMethods)
        {
            if (!actualTestMethods.Contains(expectedMethod))
            {
                problems.Add($"Expected test method '{expectedMethod}' is missing from StringExtensionsTests class");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="StringExtensionsTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this StringExtensionsTests? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="StringExtensionsTests"/> instance is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this StringExtensionsTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
        $"StringExtensionsTests instance is not valid. Problems:{Environment.NewLine} - {string.Join($"{Environment.NewLine} - ", problems)}");
    }
}