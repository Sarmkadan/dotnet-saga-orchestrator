#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Extension methods for collections and enumerables.
/// Provides batch operations, filtering, and validation utilities.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Determines whether the specified collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to check.</param>
    /// <returns><see langword="true" /> if the collection is null or empty; otherwise, <see langword="false" />.</returns>
    public static bool IsEmpty<T>(this IEnumerable<T>? items)
    {
        return items is null || !items.Any();
    }

    /// <summary>
    /// Determines whether the specified collection is not null and contains at least one element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to check.</param>
    /// <returns><see langword="true" /> if the collection is not null and contains elements; otherwise, <see langword="false" />.</returns>
    public static bool IsNotEmpty<T>(this IEnumerable<T>? items)
    {
        return items is not null && items.Any();
    }

    /// <summary>
    /// Returns an empty enumerable if the input is null; otherwise returns the input.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to check.</param>
    /// <returns>An empty enumerable if <paramref name="items" /> is null; otherwise, <paramref name="items" />.</returns>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? items) =>
        items ?? Enumerable.Empty<T>();

    /// <summary>
    /// Splits the collection into batches of the specified size.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to batch.</param>
    /// <param name="batchSize">The maximum size of each batch.</param>
    /// <returns>An enumerable of batches, each containing up to <paramref name="batchSize" /> elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="batchSize" /> is less than 1.</exception>
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> items, int batchSize)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        var batch = new List<T>(batchSize);
        foreach (var item in items)
        {
            batch.Add(item);
            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    /// <summary>
    /// Returns distinct elements from the collection by using a specified key selector function to compare values.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="items">The collection to process.</param>
    /// <param name="selector">A function to extract the key for each element.</param>
    /// <returns>An enumerable that contains distinct elements from the source collection.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items" /> is <see langword="null" />.
    /// or
    /// <paramref name="selector" /> is <see langword="null" />.
    /// </exception>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> selector)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selector);

        var seen = new HashSet<TKey>();
        foreach (var item in items)
        {
            var key = selector(item);
            if (seen.Add(key))
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Returns the first element of the sequence, or a specified default value if the sequence is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to search.</param>
    /// <param name="defaultValue">The default value to return if the collection is empty.</param>
    /// <returns>The first element of the collection, or <paramref name="defaultValue" /> if the collection is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is <see langword="null" />.</exception>
    public static T FirstOrDefault<T>(this IEnumerable<T> items, T defaultValue)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.FirstOrDefault() is { } first ? first : defaultValue;
    }

    /// <summary>
    /// Returns a single element from the collection, or null if the collection is empty or contains more than one element.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to search.</param>
    /// <returns>The single element of the collection, or null if the collection is empty or contains multiple elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is <see langword="null" />.</exception>
    public static T? SingleOrDefaultSafe<T>(this IEnumerable<T> items) where T : class
    {
        ArgumentNullException.ThrowIfNull(items);
        var enumerable = items.Take(2).ToList();
        return enumerable.Count == 1 ? enumerable[0] : null;
    }

    /// <summary>
    /// Determines whether all elements in the collection match all of the specified predicates.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to check.</param>
    /// <param name="predicates">The predicates to apply to each element.</param>
    /// <returns><see langword="true" /> if all elements match all predicates; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items" /> is <see langword="null" />.
    /// or
    /// <paramref name="predicates" /> is <see langword="null" />.
    /// </exception>
    public static bool AllMatch<T>(this IEnumerable<T> items, params Func<T, bool>[] predicates)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicates);
        return items.All(item => predicates.All(p => p(item)));
    }

    /// <summary>
    /// Determines whether any element in the collection matches any of the specified predicates.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to check.</param>
    /// <param name="predicates">The predicates to apply to elements.</param>
    /// <returns><see langword="true" /> if any element matches any predicate; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items" /> is <see langword="null" />.
    /// or
    /// <paramref name="predicates" /> is <see langword="null" />.
    /// </exception>
    public static bool AnyMatch<T>(this IEnumerable<T> items, params Func<T, bool>[] predicates)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(predicates);
        return items.Any(item => predicates.Any(p => p(item)));
    }

    /// <summary>
    /// Converts a dictionary to a URL query string.
    /// </summary>
    /// <param name="parameters">The dictionary to convert.</param>
    /// <returns>A query string representation of the dictionary.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="parameters" /> is <see langword="null" />.</exception>
    public static string ToQueryString(this Dictionary<string, string> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        return parameters.IsEmpty() ? string.Empty : string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
    }

    /// <summary>
    /// Returns a sequence of tuples containing each element and its index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to enumerate with indices.</param>
    /// <returns>A sequence of (index, item) tuples.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is <see langword="null" />.</exception>
    public static IEnumerable<(int index, T item)> WithIndex<T>(this IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Select((item, index) => (index, item));
    }

    /// <summary>
    /// Flattens a sequence of sequences into a single sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections.</typeparam>
    /// <param name="items">The nested collections to flatten.</param>
    /// <returns>A single flattened sequence.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is <see langword="null" />.</exception>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.SelectMany(x => x);
    }

    /// <summary>
    /// Groups elements by a key selector and transforms values using a value selector.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="items">The collection to group.</param>
    /// <param name="keySelector">A function to extract the key for each element.</param>
    /// <param name="valueSelector">A function to extract the value for each element.</param>
    /// <returns>A dictionary mapping keys to lists of values.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items" /> is <see langword="null" />.
    /// or
    /// <paramref name="keySelector" /> is <see langword="null" />.
    /// or
    /// <paramref name="valueSelector" /> is <see langword="null" />.
    /// </exception>
    public static Dictionary<TKey, List<TValue>> GroupToDictionary<T, TKey, TValue>(
        this IEnumerable<T> items,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector) where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(keySelector);
        ArgumentNullException.ThrowIfNull(valueSelector);

        return items.GroupBy(keySelector).ToDictionary(g => g.Key, g => g.Select(valueSelector).ToList());
    }

    /// <summary>
    /// Returns the minimum element according to the specified key selector, or null if the collection is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the key used for comparison.</typeparam>
    /// <param name="items">The collection to search.</param>
    /// <param name="selector">A function to extract the key for each element.</param>
    /// <returns>The minimum element according to the key selector, or null if the collection is empty.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items" /> is <see langword="null" />.
    /// or
    /// <paramref name="selector" /> is <see langword="null" />.
    /// </exception>
    public static T? MinByOrDefault<T, TKey>(this IEnumerable<T> items, Func<T, TKey> selector) where T : class where TKey : IComparable<TKey>
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(selector);

        T? minItem = null;
        TKey? minValue = default;
        var isFirst = true;

        foreach (var item in items)
        {
            var value = selector(item);
            if (isFirst || (value is not null && value.CompareTo(minValue!) < 0))
            {
                minItem = item;
                minValue = value;
                isFirst = false;
            }
        }
        return minItem;
    }

    /// <summary>
    /// Returns a paginated subset of the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to paginate.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A sequence containing the items for the specified page.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="pageNumber" /> is less than 1.
    /// or
    /// <paramref name="pageSize" /> is less than 1.
    /// </exception>
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> items, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return items.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    }

    /// <summary>
    /// Returns a sliding window of the specified size over the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to window.</param>
    /// <param name="windowSize">The size of each window.</param>
    /// <returns>A sequence of windows, each containing <paramref name="windowSize" /> consecutive elements.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="windowSize" /> is less than 1.</exception>
    public static IEnumerable<IEnumerable<T>> Window<T>(this IEnumerable<T> items, int windowSize)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfLessThan(windowSize, 1);

        var list = items.ToList();
        for (int i = 0; i <= list.Count - windowSize; i++)
        {
            yield return list.Skip(i).Take(windowSize);
        }
    }

    /// <summary>
    /// Performs the specified action on each element of the collection with its index.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to enumerate.</param>
    /// <param name="action">The action to perform on each element.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items" /> is <see langword="null" />.
    /// or
    /// <paramref name="action" /> is <see langword="null" />.
    /// </exception>
    public static void ForEachIndexed<T>(this IEnumerable<T> items, Action<T, int> action)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(action);

        var index = 0;
        foreach (var item in items)
        {
            action(item, index++);
        }
    }

    /// <summary>
    /// Performs the specified async action on each element of the collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to enumerate.</param>
    /// <param name="action">The async action to perform on each element.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items" /> is <see langword="null" />.
    /// or
    /// <paramref name="action" /> is <see langword="null" />.
    /// </exception>
    public static async Task ForEachAsync<T>(this IEnumerable<T> items, Func<T, Task> action)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in items)
        {
            await action(item);
        }
    }

    /// <summary>
    /// Concatenates the sequence with another sequence if the other sequence is not null.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections.</typeparam>
    /// <param name="items">The original collection.</param>
    /// <param name="other">The collection to concatenate, or null.</param>
    /// <returns>A new sequence containing elements from both collections, or just the original if other is null.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is <see langword="null" />.</exception>
    public static IEnumerable<T> ConcatIfNotNull<T>(this IEnumerable<T> items, IEnumerable<T>? other)
    {
        ArgumentNullException.ThrowIfNull(items);
        return other is null ? items : items.Concat(other);
    }

    /// <summary>
    /// Returns a random element from the collection, or the default value if the collection is empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="items">The collection to select from.</param>
    /// <returns>A random element from the collection, or the default value if the collection is empty.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is <see langword="null" />.</exception>
    public static T? RandomOrDefault<T>(this IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var list = items as IList<T> ?? items.ToList();
        return list.IsEmpty() ? default : list[Random.Shared.Next(list.Count)];
    }
}