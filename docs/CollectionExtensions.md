# CollectionExtensions
The `CollectionExtensions` class provides a set of extension methods for working with collections in C#. These methods offer various functionalities such as checking for emptiness, batching, distinct selection, and more, making it easier to manipulate and process data in collections.

## API
* `IsEmpty<T>(IEnumerable<T> collection)`: Checks if the given collection is empty. Returns `true` if the collection is empty, `false` otherwise. Throws `ArgumentNullException` if the collection is null.
* `IsNotEmpty<T>(IEnumerable<T> collection)`: Checks if the given collection is not empty. Returns `true` if the collection is not empty, `false` otherwise. Throws `ArgumentNullException` if the collection is null.
* `EmptyIfNull<T>(IEnumerable<T> collection)`: Returns an empty collection if the given collection is null, otherwise returns the original collection.
* `Batch<T>(IEnumerable<T> collection, int batchSize)`: Batches the given collection into chunks of the specified size. Returns an enumerable of batches.
* `DistinctBy<T, TKey>(IEnumerable<T> collection, Func<T, TKey> keySelector)`: Returns a distinct collection based on the specified key selector. Throws `ArgumentNullException` if the collection or key selector is null.
* `FirstOrDefault<T>(IEnumerable<T> collection)`: Returns the first element of the collection, or the default value if the collection is empty. Throws `ArgumentNullException` if the collection is null.
* `SingleOrDefaultSafe<T>(IEnumerable<T> collection)`: Returns the single element of the collection, or the default value if the collection is empty or contains more than one element. Throws `ArgumentNullException` if the collection is null.
* `AllMatch<T>(IEnumerable<T> collection, Func<T, bool> predicate)`: Checks if all elements in the collection match the specified predicate. Returns `true` if all elements match, `false` otherwise. Throws `ArgumentNullException` if the collection or predicate is null.
* `AnyMatch<T>(IEnumerable<T> collection, Func<T, bool> predicate)`: Checks if any element in the collection matches the specified predicate. Returns `true` if any element matches, `false` otherwise. Throws `ArgumentNullException` if the collection or predicate is null.
* `ToQueryString(IEnumerable<T> collection)`: Converts the collection to a query string. Throws `ArgumentNullException` if the collection is null.
* `WithIndex<T>(IEnumerable<T> collection)`: Returns an enumerable of tuples containing the index and value of each element in the collection.
* `Flatten<T>(IEnumerable<IEnumerable<T>> collection)`: Flattens a collection of collections into a single collection. Throws `ArgumentNullException` if the collection is null.
* `GroupToDictionary<T, TKey, TValue>(IEnumerable<T> collection, Func<T, TKey> keySelector, Func<T, TValue> valueSelector)`: Groups the collection by the specified key selector and returns a dictionary with the grouped values. Throws `ArgumentNullException` if the collection, key selector, or value selector is null.
* `MinByOrDefault<T, TKey>(IEnumerable<T> collection, Func<T, TKey> keySelector)`: Returns the minimum element in the collection based on the specified key selector, or the default value if the collection is empty. Throws `ArgumentNullException` if the collection or key selector is null.
* `Paginate<T>(IEnumerable<T> collection, int pageSize)`: Paginates the collection into chunks of the specified size. Returns an enumerable of pages.
* `Window<T>(IEnumerable<T> collection, int windowSize)`: Returns an enumerable of windows of the specified size over the collection.
* `ForEachIndexed<T>(IEnumerable<T> collection, Action<int, T> action)`: Performs the specified action on each element in the collection with its index. Throws `ArgumentNullException` if the collection or action is null.
* `ForEachAsync<T>(IEnumerable<T> collection, Func<T, Task> action)`: Asynchronously performs the specified action on each element in the collection. Throws `ArgumentNullException` if the collection or action is null.
* `ConcatIfNotNull<T>(IEnumerable<T> collection1, IEnumerable<T> collection2)`: Concatenates the two collections if the second collection is not null, otherwise returns the first collection.
* `RandomOrDefault<T>(IEnumerable<T> collection)`: Returns a random element from the collection, or the default value if the collection is empty. Throws `ArgumentNullException` if the collection is null.

## Usage
The following examples demonstrate how to use the `CollectionExtensions` class:
```csharp
// Example 1: Using IsEmpty and Batch
var numbers = new List<int> { 1, 2, 3, 4, 5 };
if (numbers.IsEmpty())
{
    Console.WriteLine("The list is empty.");
}
else
{
    var batches = numbers.Batch(2);
    foreach (var batch in batches)
    {
        Console.WriteLine(string.Join(", ", batch));
    }
}

// Example 2: Using DistinctBy and GroupToDictionary
var people = new List<Person>
{
    new Person { Name = "John", Age = 25 },
    new Person { Name = "Jane", Age = 25 },
    new Person { Name = "Bob", Age = 30 },
};
var distinctPeople = people.DistinctBy(p => p.Age);
var peopleByAge = people.GroupToDictionary(p => p.Age, p => p.Name);
foreach (var group in peopleByAge)
{
    Console.WriteLine($"Age {group.Key}: {string.Join(", ", group.Value)}");
}
```

## Notes
When using the `CollectionExtensions` class, be aware of the following edge cases:
* If a collection is null, most methods will throw an `ArgumentNullException`.
* If a collection is empty, methods like `FirstOrDefault` and `SingleOrDefaultSafe` will return the default value.
* Methods like `Batch` and `Paginate` will return an empty enumerable if the collection is empty.
* Methods like `GroupToDictionary` and `MinByOrDefault` will throw an `ArgumentNullException` if the key selector or value selector is null.
* The `ForEachIndexed` and `ForEachAsync` methods will throw an `ArgumentNullException` if the action is null.
* The `RandomOrDefault` method will return the default value if the collection is empty.
As for thread-safety, most methods in the `CollectionExtensions` class are thread-safe, but methods that modify the collection or use external state may not be. It is recommended to use these methods in a thread-safe manner or to synchronize access to the collection as needed.
