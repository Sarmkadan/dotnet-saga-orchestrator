# ValidationExtensions

A static utility class providing strongly-typed validation methods for common scenarios such as null checks, string constraints, numeric ranges, collections, and structured data formats like email and URLs. Designed for use in domain models, DTOs, and service layers where input validation is required before processing.

## API

### `public static T NotNull<T>(T? value, string paramName)`

Validates that a nullable value is not `null`.
- **Parameters**:
  - `value` – The value to validate.
  - `paramName` – The name of the parameter being validated, used in exceptions.
- **Return value**: The original non-null value of type `T`.
- **Throws**: `ArgumentNullException` if `value` is `null`.

---

### `public static string NotNullOrEmpty(string? value, string paramName)`

Validates that a string is not `null` or empty.
- **Parameters**:
  - `value` – The string to validate.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original non-null, non-empty string.
- **Throws**: `ArgumentException` if `value` is `null` or empty.

---

### `public static string NotNullOrWhiteSpace(string? value, string paramName)`

Validates that a string is not `null`, empty, or whitespace-only.
- **Parameters**:
  - `value` – The string to validate.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original non-null, non-empty, non-whitespace string.
- **Throws**: `ArgumentException` if `value` is `null`, empty, or consists only of whitespace.

---

### `public static int InRange(int value, int min, int max, string paramName)`

Validates that an integer lies within the specified inclusive range.
- **Parameters**:
  - `value` – The integer to validate.
  - `min` – The minimum allowed value (inclusive).
  - `max` – The maximum allowed value (inclusive).
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original `value`.
- **Throws**: `ArgumentOutOfRangeException` if `value < min` or `value > max`.

---

### `public static int GreaterThan(int value, int min, string paramName)`

Validates that an integer is strictly greater than a minimum value.
- **Parameters**:
  - `value` – The integer to validate.
  - `min` – The minimum value (exclusive).
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original `value`.
- **Throws**: `ArgumentOutOfRangeException` if `value <= min`.

---
### `public static int GreaterThanOrEqual(int value, int min, string paramName)`

Validates that an integer is greater than or equal to a minimum value.
- **Parameters**:
  - `value` – The integer to validate.
  - `min` – The minimum value (inclusive).
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original `value`.
- **Throws**: `ArgumentOutOfRangeException` if `value < min`.

---
### `public static long GreaterThan(long value, long min, string paramName)`

Validates that a long integer is strictly greater than a minimum value.
- **Parameters**:
  - `value` – The long integer to validate.
  - `min` – The minimum value (exclusive).
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original `value`.
- **Throws**: `ArgumentOutOfRangeException` if `value <= min`.

---
### `public static TimeSpan GreaterThanZero(TimeSpan value, string paramName)`

Validates that a `TimeSpan` is strictly greater than zero.
- **Parameters**:
  - `value` – The `TimeSpan` to validate.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original `value`.
- **Throws**: `ArgumentOutOfRangeException` if `value <= TimeSpan.Zero`.

---
### `public static decimal InRange(decimal value, decimal min, decimal max, string paramName)`

Validates that a decimal lies within the specified inclusive range.
- **Parameters**:
  - `value` – The decimal to validate.
  - `min` – The minimum allowed value (inclusive).
  - `max` – The maximum allowed value (inclusive).
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original `value`.
- **Throws**: `ArgumentOutOfRangeException` if `value < min` or `value > max`.

---
### `public static IEnumerable<T> NotEmpty<T>(IEnumerable<T>? collection, string paramName)`

Validates that an enumerable is not `null` and contains at least one element.
- **Parameters**:
  - `collection` – The enumerable to validate.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original non-null, non-empty enumerable.
- **Throws**: `ArgumentNullException` if `collection` is `null`.
- **Throws**: `ArgumentException` if `collection` is empty.

---
### `public static Guid NotEmpty(Guid value, string paramName)`

Validates that a `Guid` is not empty (i.e., not `Guid.Empty`).
- **Parameters**:
  - `value` – The `Guid` to validate.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original non-empty `Guid`.
- **Throws**: `ArgumentException` if `value == Guid.Empty`.

---
### `public static T? ValidateIf<T>(T? value, Func<T, bool> predicate, string paramName)`

Conditionally validates a value using a custom predicate; returns the value only if the predicate passes.
- **Parameters**:
  - `value` – The value to validate.
  - `predicate` – A function that returns `true` if the value is valid.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original `value` if `predicate(value)` is `true`; otherwise `null`.
- **Throws**: `ArgumentException` if `predicate(value)` is `false`.

---
### `public static string ValidateEmail(string? email, string paramName)`

Validates that a string is a syntactically valid email address.
- **Parameters**:
  - `email` – The email string to validate.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original email string if valid.
- **Throws**: `ArgumentException` if `email` is `null`, empty, whitespace-only, or not a valid email format.

---
### `public static string ValidateUrl(string? url, string paramName)`

Validates that a string is a syntactically valid absolute HTTP/HTTPS URL.
- **Parameters**:
  - `url` – The URL string to validate.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original URL string if valid.
- **Throws**: `ArgumentException` if `url` is `null`, empty, whitespace-only, or not a valid absolute HTTP/HTTPS URL.

---
### `public static string MaxLength(string? value, int maxLength, string paramName)`

Validates that a string’s length does not exceed a maximum value.
- **Parameters**:
  - `value` – The string to validate.
  - `maxLength` – The maximum allowed length (inclusive).
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original string.
- **Throws**: `ArgumentException` if `value` is `null`, empty, whitespace-only, or its length exceeds `maxLength`.

---
### `public static string MinLength(string? value, int minLength, string paramName)`

Validates that a string’s length is at least a minimum value.
- **Parameters**:
  - `value` – The string to validate.
  - `minLength` – The minimum allowed length (inclusive).
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original string.
- **Throws**: `ArgumentException` if `value` is `null`, empty, whitespace-only, or its length is less than `minLength`.

---
### `public static T[] NotEmptyArray<T>(T[]? array, string paramName)`

Validates that an array is not `null` and contains at least one element.
- **Parameters**:
  - `array` – The array to validate.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original non-null, non-empty array.
- **Throws**: `ArgumentNullException` if `array` is `null`.
- **Throws**: `ArgumentException` if `array` is empty.

---
### `public static Dictionary<K, V> NotEmptyDictionary<K, V>(Dictionary<K, V>? dictionary, string paramName)`

Validates that a dictionary is not `null` and contains at least one entry.
- **Parameters**:
  - `dictionary` – The dictionary to validate.
  - `paramName` – The name of the parameter being validated.
- **Return value**: The original non-null, non-empty dictionary.
- **Throws**: `ArgumentNullException` if `dictionary` is `null`.
- **Throws**: `ArgumentException` if `dictionary` is empty.

## Usage
