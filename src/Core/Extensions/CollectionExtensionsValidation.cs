#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Validation extension methods for collections.
/// Provides validation utilities to check collection state and content.
/// </summary>
public static class CollectionExtensionsValidation
{
    /// <summary>
    /// Validates a collection for common issues like null elements, empty strings, out-of-range values, and default dates.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to validate.</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var problems = new List<string>();

        // Validate collection structure
        if (source.IsEmpty())
        {
            problems.Add("Collection is null or empty");
            return problems.AsReadOnly();
        }

        // Check for null elements
        var nullElements = source.Where(x => x is null).ToList();
        if (nullElements.Count > 0)
        {
            problems.Add($"Collection contains {nullElements.Count} null element(s)");
        }

        // Type-specific validations
        if (typeof(T) == typeof(string))
        {
            var stringCollection = source.Cast<string>()!;
            var emptyStrings = stringCollection.Where(s => !string.IsNullOrWhiteSpace(s) && string.IsNullOrEmpty(s)).ToList();
            var whitespaceStrings = stringCollection.Where(s => s is not null && string.IsNullOrWhiteSpace(s)).ToList();

            if (emptyStrings.Count > 0)
            {
                problems.Add($"Collection contains {emptyStrings.Count} empty string(s)");
            }

            if (whitespaceStrings.Count > 0)
            {
                problems.Add($"Collection contains {whitespaceStrings.Count} whitespace-only string(s)");
            }
        }
        else if (typeof(T) == typeof(int) || typeof(T) == typeof(long) || typeof(T) == typeof(double) || typeof(T) == typeof(float))
        {
            var numericCollection = source.Cast<object>()!;
            var defaultValues = numericCollection.Where(x => x is not null && x.Equals(default(T))).ToList();

            if (defaultValues.Count > 0)
            {
                problems.Add($"Collection contains {defaultValues.Count} default/zero value(s)");
            }
        }
        else if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTimeOffset))
        {
            var dateCollection = source.Cast<DateTimeOffset>()!;
            var defaultDates = dateCollection.Where(d => d == default).ToList();
            var minDates = dateCollection.Where(d => d == DateTimeOffset.MinValue).ToList();

            if (defaultDates.Count > 0)
            {
                problems.Add($"Collection contains {defaultDates.Count} default date(s) (DateTimeOffset.MinValue)");
            }

            if (minDates.Count > 0)
            {
                problems.Add($"Collection contains {minDates.Count} minimum date(s) (DateTimeOffset.MinValue)");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates a dictionary for common issues like null keys, empty values, and out-of-range values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to validate.</param>
    /// <returns>A list of human-readable validation problems, or empty list if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        var problems = new List<string>();

        if (dictionary.IsEmpty())
        {
            problems.Add("Dictionary is null or empty");
            return problems.AsReadOnly();
        }

        // Check for null keys
        var nullKeys = dictionary.Keys.Where(k => k is null).ToList();
        if (nullKeys.Count > 0)
        {
            problems.Add($"Dictionary contains {nullKeys.Count} null key(s)");
        }

        // Check for null values
        var nullValues = dictionary.Values.Where(v => v is null).ToList();
        if (nullValues.Count > 0)
        {
            problems.Add($"Dictionary contains {nullValues.Count} null value(s)");
        }

        // Type-specific validations
        if (typeof(TValue) == typeof(string))
        {
            var stringValues = dictionary.Values.Cast<string>()!;
            var emptyStrings = stringValues.Where(s => string.IsNullOrEmpty(s)).ToList();
            var whitespaceStrings = stringValues.Where(s => s is not null && string.IsNullOrWhiteSpace(s)).ToList();

            if (emptyStrings.Count > 0)
            {
                problems.Add($"Dictionary contains {emptyStrings.Count} empty string value(s)");
            }

            if (whitespaceStrings.Count > 0)
            {
                problems.Add($"Dictionary contains {whitespaceStrings.Count} whitespace-only string value(s)");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a collection is valid.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to check.</param>
    /// <returns><see langword="true"/> if the collection is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    public static bool IsValid<T>(this IEnumerable<T> source)
    {
        return source.Validate().Count == 0;
    }

    /// <summary>
    /// Determines whether a dictionary is valid.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to check.</param>
    /// <returns><see langword="true"/> if the dictionary is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/>.</exception>
    public static bool IsValid<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
        => dictionary.Validate().Count == 0;

    /// <summary>
    /// Ensures that a collection is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="source">The collection to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The collection is in an invalid state.</exception>
    public static void EnsureValid<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var problems = source.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Collection is invalid:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }

    /// <summary>
    /// Ensures that a dictionary is valid, throwing an exception if not.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The dictionary is in an invalid state.</exception>
    public static void EnsureValid<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        var problems = dictionary.Validate();

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"Dictionary is invalid:{Environment.NewLine}  - {string.Join($"{Environment.NewLine}  - ", problems)}");
        }
    }
}