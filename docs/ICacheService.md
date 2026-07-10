# ICacheService

A service interface for managing in-memory cache operations, providing asynchronous and synchronous methods to store, retrieve, remove, and inspect cached values with expiration support.

## API

### `GetAsync<T>`
Retrieves a cached value of type `T` asynchronously.
- **Parameters**:
  - `key` (string): The cache key to look up.
- **Return value**: A `Task<T?>` resolving to the cached value if it exists and is not expired; otherwise `null`.
- **Exceptions**: Throws `ArgumentNullException` if `key` is `null`.

### `SetAsync<T>`
Stores a value of type `T` in the cache asynchronously.
- **Parameters**:
  - `key` (string): The cache key under which the value is stored.
  - `value` (T): The value to cache.
  - `expiryTime` (DateTime): The absolute expiration time for the entry.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `key` is `null`.

### `RemoveAsync`
Removes a cached entry asynchronously.
- **Parameters**:
  - `key` (string): The cache key to remove.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: Throws `ArgumentNullException` if `key` is `null`.

### `ClearAsync`
Removes all cached entries asynchronously.
- **Return value**: A `Task` representing the asynchronous operation.

### `ExistsAsync`
Checks whether a cached entry exists and is not expired asynchronously.
- **Parameters**:
  - `key` (string): The cache key to check.
- **Return value**: A `Task<bool>` resolving to `true` if the entry exists and is not expired; otherwise `false`.
- **Exceptions**: Throws `ArgumentNullException` if `key` is `null`.

### `GetCacheSize`
Gets the current number of entries in the cache.
- **Return value**: An `int` representing the count of active (non-expired) entries.

### `Dispose`
Releases all resources used by the cache service.
- **Return value**: None.

### `Value`
Gets the cached value.
- **Return value**: An `object?` representing the stored value, or `null` if the entry is expired or absent.

### `ExpiryTime`
Gets the absolute expiration time of the current cache entry.
- **Return value**: A `DateTime` indicating when the entry expires.

### `CacheEntry`
Gets the current cache entry metadata.
- **Return value**: A `CacheEntry` object containing the value and expiration details.

### `IsExpired`
Indicates whether the current cache entry has expired.
- **Return value**: A `bool` set to `true` if the entry is expired; otherwise `false`.

## Usage
