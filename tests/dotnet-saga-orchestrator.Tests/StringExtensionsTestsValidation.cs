#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="StringExtensionsTests"/> instances.
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

        // Validate ToSnakeCase_PascalCaseInput_InsertsUnderscoreBetweenWords
        // This is a method signature, no validation needed beyond null check

        // Validate ToSnakeCase_SingleWord_ReturnsLowercase
        // This is a method signature, no validation needed beyond null check

        // Validate ToKebabCase_PascalCaseInput_ReturnsHyphenSeparated
        // This is a method signature, no validation needed beyond null check

        // Validate ToCamelCase_PascalCase_LowercasesFirstCharacter
        // This is a method signature, no validation needed beyond null check

        // Validate ToCamelCase_SingleCharacter_ReturnsLowercase
        // This is a method signature, no validation needed beyond null check

        // Validate Truncate_StringLongerThanMax_AppendsEllipsis
        // This is a method signature, no validation needed beyond null check

        // Validate Truncate_StringShorterThanMax_ReturnsOriginalUnchanged
        // This is a method signature, no validation needed beyond null check

        // Validate CountOccurrences_SubstringRepeatedMultipleTimes_ReturnsExactCount
        // This is a method signature, no validation needed beyond null check

        // Validate CountOccurrences_SubstringNotPresent_ReturnsZero
        // This is a method signature, no validation needed beyond null check

        // Validate ToSlug_StringWithSpacesAndSpecialChars_ReturnsUrlFriendlySlug
        // This is a method signature, no validation needed beyond null check

        // Validate ToSlug_EmptyString_ReturnsEmptyString
        // This is a method signature, no validation needed beyond null check

        // Validate RemovePrefix_PrefixPresent_RemovesPrefix
        // This is a method signature, no validation needed beyond null check

        // Validate RemovePrefix_PrefixAbsent_ReturnsOriginalValue
        // This is a method signature, no validation needed beyond null check

        // Validate RemoveSuffix_SuffixPresent_RemovesSuffix
        // This is a method signature, no validation needed beyond null check

        // Validate NullIfEmpty_EmptyString_ReturnsNull
        // This is a method signature, no validation needed beyond null check

        // Validate NullIfEmpty_NonEmptyString_ReturnsSameValue
        // This is a method signature, no validation needed beyond null check

        // Validate Repeat_PositiveCount_ConcatenatesStringNTimes
        // This is a method signature, no validation needed beyond null check

        // Validate Repeat_ZeroCount_ReturnsEmptyString
        // This is a method signature, no validation needed beyond null check

        // Validate SplitAndTrim_StringWithSpacesAroundDelimiters_ReturnsTrimmedParts
        // This is a method signature, no validation needed beyond null check

        // Validate Batch_CollectionOfTen_ProducesCorrectBatchCount
        // This is a method signature, no validation needed beyond null check

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
            $"StringExtensionsTests instance is not valid. Problems: {string.Join(", ", problems)}");
    }
}