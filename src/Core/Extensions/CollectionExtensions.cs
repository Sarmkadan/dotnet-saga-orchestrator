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
    // Check if collection is null or empty
    public static bool IsEmpty<T>(this IEnumerable<T>? items) =>
        items?.Any() != true;

    public static bool IsNotEmpty<T>(this IEnumerable<T>? items) =>
        items?.Any() == true;

    // Safe access with default
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? items) =>
        items ?? Enumerable.Empty<T>();

    // Batch/chunk a collection
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> items, int batchSize)
    {
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
            yield return batch;
    }

    // Distinct by selector
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> selector)
    {
        var seen = new HashSet<TKey>();
        foreach (var item in items)
        {
            var key = selector(item);
            if (seen.Add(key))
                yield return item;
        }
    }

    // First or default with fallback
    public static T FirstOrDefault<T>(this IEnumerable<T> items, T defaultValue) =>
        items?.FirstOrDefault() ?? defaultValue;

    // Single or default without exception
    public static T? SingleOrDefaultSafe<T>(this IEnumerable<T> items) where T : class
    {
        var enumerable = items.Take(2).ToList();
        return enumerable.Count == 1 ? enumerable[0] : null;
    }

    // Check if any item matches all predicates
    public static bool AllMatch<T>(this IEnumerable<T> items, params Func<T, bool>[] predicates) =>
        items.All(item => predicates.All(p => p(item)));

    // Check if any item matches any predicate
    public static bool AnyMatch<T>(this IEnumerable<T> items, params Func<T, bool>[] predicates) =>
        items.Any(item => predicates.Any(p => p(item)));

    // Convert dictionary to query string
    public static string ToQueryString(this Dictionary<string, string> parameters)
    {
        if (parameters.IsEmpty()) return string.Empty;
        return string.Join("&", parameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
    }

    // Safe enumerable access with index
    public static IEnumerable<(int index, T item)> WithIndex<T>(this IEnumerable<T> items)
    {
        return items.Select((item, index) => (index, item));
    }

    // Flatten nested collections
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> items) =>
        items.SelectMany(x => x);

    // Group and transform
    public static Dictionary<TKey, List<TValue>> GroupToDictionary<T, TKey, TValue>(
        this IEnumerable<T> items,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector) where TKey : notnull =>
        items.GroupBy(keySelector).ToDictionary(g => g.Key, g => g.Select(valueSelector).ToList());

    // Min/Max safe operations
    public static T? MinByOrDefault<T, TKey>(this IEnumerable<T> items, Func<T, TKey> selector) where T : class where TKey : IComparable<TKey>
    {
        T? minItem = null;
        TKey? minValue = default;
        var isFirst = true;

        foreach (var item in items)
        {
            var value = selector(item);
            if (isFirst || (value?.CompareTo(minValue!) ?? 1) < 0)
            {
                minItem = item;
                minValue = value;
                isFirst = false;
            }
        }
        return minItem;
    }

    // Paginate a collection
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> items, int pageNumber, int pageSize) =>
        items.Skip((pageNumber - 1) * pageSize).Take(pageSize);

    // Chunk/window collection
    public static IEnumerable<IEnumerable<T>> Window<T>(this IEnumerable<T> items, int windowSize)
    {
        var list = items.ToList();
        for (int i = 0; i <= list.Count - windowSize; i++)
        {
            yield return list.Skip(i).Take(windowSize);
        }
    }

    // ForEach with index
    public static void ForEachIndexed<T>(this IEnumerable<T> items, Action<T, int> action)
    {
        var index = 0;
        foreach (var item in items)
        {
            action(item, index++);
        }
    }

    // Async ForEach
    public static async Task ForEachAsync<T>(this IEnumerable<T> items, Func<T, Task> action)
    {
        foreach (var item in items)
        {
            await action(item);
        }
    }

    // Safe concat
    public static IEnumerable<T> ConcatIfNotNull<T>(this IEnumerable<T> items, IEnumerable<T>? other) =>
        other == null ? items : items.Concat(other);

    // Random selection
    public static T? RandomOrDefault<T>(this IEnumerable<T> items)
    {
        var list = items as IList<T> ?? items.ToList();
        return list.IsEmpty() ? default : list[new Random().Next(list.Count)];
    }
}
