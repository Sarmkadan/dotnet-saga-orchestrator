# CacheKeyBuilder

Utility class for constructing and parsing Redis-style cache keys used by the Saga Orchestrator to organize and retrieve cached data. Keys are structured hierarchically with prefixes and identifiers to support efficient querying of related entities (e.g., all sagas, sagas by status, definitions by name). The class provides methods to build keys for specific entities, validate key formats, extract identifiers, and retrieve all supported key prefixes.

## API

### `public static string BuildSagaKey(string sagaId)`
Constructs a cache key for a specific saga instance.
- **Parameters**: `sagaId` – Unique identifier of the saga.
- **Returns**: A string in the format `"saga:{sagaId}"`.
- **Throws**: `ArgumentException` if `sagaId` is null or whitespace.

### `public static string BuildDefinitionKey(string definitionId)`
Constructs a cache key for a specific saga definition.
- **Parameters**: `definitionId` – Unique identifier of the saga definition.
- **Returns**: A string in the format `"definition:{definitionId}"`.
- **Throws**: `ArgumentException` if `definitionId` is null or whitespace.

### `public static string BuildAllSagasKey()`
Constructs a cache key representing the collection of all saga instances.
- **Returns**: The string `"sagas:all"`.

### `public static string BuildAllDefinitionsKey()`
Constructs a cache key representing the collection of all saga definitions.
- **Returns**: The string `"definitions:all"`.

### `public static string BuildSagasByStatusKey(string status)`
Constructs a cache key for retrieving all sagas filtered by a specific status.
- **Parameters**: `status` – The status to filter by (e.g., `"running"`, `"completed"`).
- **Returns**: A string in the format `"sagas:status:{status}"`.
- **Throws**: `ArgumentException` if `status` is null or whitespace.

### `public static string BuildDefinitionByNameKey(string name)`
Constructs a cache key for retrieving a saga definition by its name.
- **Parameters**: `name` – The name of the saga definition.
- **Returns**: A string in the format `"definition:name:{name}"`.
- **Throws**: `ArgumentException` if `name` is null or whitespace.

### `public static string BuildCompensationKey(string sagaId)`
Constructs a cache key for storing compensation data associated with a saga.
- **Parameters**: `sagaId` – Unique identifier of the saga.
- **Returns**: A string in the format `"saga:{sagaId}:compensation"`.

### `public static string BuildEventHistoryKey(string sagaId)`
Constructs a cache key for storing the event history of a saga.
- **Parameters**: `sagaId` – Unique identifier of the saga.
- **Returns**: A string in the format `"saga:{sagaId}:events"`.

### `public static string BuildServiceKey(string serviceName)`
Constructs a cache key for storing service-specific metadata.
- **Parameters**: `serviceName` – Name of the service.
- **Returns**: A string in the format `"service:{serviceName}"`.
- **Throws**: `ArgumentException` if `serviceName` is null or whitespace.

### `public static string BuildHealthCheckKey()`
Constructs a cache key for health check status.
- **Returns**: The string `"health:check"`.

### `public static string BuildMetricsKey()`
Constructs a cache key for storing system metrics.
- **Returns**: The string `"metrics:system"`.

### `public static string BuildWebhookKey(string webhookId)`
Constructs a cache key for a specific webhook.
- **Parameters**: `webhookId` – Unique identifier of the webhook.
- **Returns**: A string in the format `"webhook:{webhookId}"`.
- **Throws**: `ArgumentException` if `webhookId` is null or whitespace.

### `public static string BuildRateLimitKey(string clientId)`
Constructs a cache key for rate limiting a client.
- **Parameters**: `clientId` – Identifier of the client.
- **Returns**: A string in the format `"rate:limit:{clientId}"`.
- **Throws**: `ArgumentException` if `clientId` is null or whitespace.

### `public static string BuildUserCacheKey(string userId)`
Constructs a cache key for user-specific data.
- **Parameters**: `userId` – Unique identifier of the user.
- **Returns**: A string in the format `"user:{userId}"`.
- **Throws**: `ArgumentException` if `userId` is null or whitespace.

### `public static string BuildSessionKey(string sessionId)`
Constructs a cache key for a user session.
- **Parameters**: `sessionId` – Unique identifier of the session.
- **Returns**: A string in the format `"session:{sessionId}"`.
- **Throws**: `ArgumentException` if `sessionId` is null or whitespace.

### `public static string GenerateTempKey()`
Generates a temporary, unique cache key.
- **Returns**: A string in the format `"temp:{Guid.NewGuid()}"`.

### `public static bool IsSagaKey(string key)`
Determines whether the given key is a valid saga key.
- **Parameters**: `key` – The cache key to validate.
- **Returns**: `true` if the key matches the pattern `"saga:{sagaId}"`; otherwise, `false`.
- **Throws**: `ArgumentException` if `key` is null or whitespace.

### `public static bool IsDefinitionKey(string key)`
Determines whether the given key is a valid definition key.
- **Parameters**: `key` – The cache key to validate.
- **Returns**: `true` if the key matches the pattern `"definition:{definitionId}"`; otherwise, `false`.
- **Throws**: `ArgumentException` if `key` is null or whitespace.

### `public static string ExtractIdFromKey(string key)`
Extracts the identifier from a valid saga or definition key.
- **Parameters**: `key` – The cache key from which to extract the identifier.
- **Returns**: The identifier part of the key (e.g., `"123"` from `"saga:123"`).
- **Throws**:
  - `ArgumentException` if `key` is null or whitespace.
  - `ArgumentException` if the key does not match the expected pattern.

### `public static Dictionary<string, string> GetAllPrefixes()`
Retrieves all supported key prefixes used by the cache system.
- **Returns**: A dictionary mapping descriptive names (e.g., `"Saga"`, `"Definition"`) to their corresponding key prefixes (e.g., `"saga:"`, `"definition:"`).

## Usage

### Example 1: Building and Validating Keys
