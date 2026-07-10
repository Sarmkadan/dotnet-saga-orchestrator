# StringExtensions

`StringExtensions` provides a collection of static utility methods for common string operations, validation, and format conversions. It is designed to reduce boilerplate null-checking and manipulation code across the `dotnet-saga-orchestrator` project by offering concise, intention-revealing extension methods.

## API

### `IsNullOrEmpty`

Indicates whether the specified string is `null` or an empty string (`""`).

- **Parameters:** `string? value`
- **Returns:** `bool` — `true` if the string is `null` or empty; otherwise `false`.
- **Throws:** Never.

### `IsNullOrWhiteSpace`

Indicates whether the specified string is `null`, empty, or consists exclusively of white-space characters.

- **Parameters:** `string? value`
- **Returns:** `bool` — `true` if the string is `null`, empty, or white-space only; otherwise `false`.
- **Throws:** Never.

### `NullIfEmpty`

Returns `null` if the string is empty or `null`; otherwise returns the original string. White-space is not considered empty.

- **Parameters:** `string? value`
- **Returns:** `string?` — `null` when the input is `null` or `""`; the unchanged input otherwise.
- **Throws:** Never.

### `ToTitleCase`

Converts the string to title case (first letter of each word capitalized) using the current culture.

- **Parameters:** `string value`
- **Returns:** `string` — the title-cased string.
- **Throws:** `ArgumentNullException` if `value` is `null`.

### `ToCamelCase`

Converts the string to camelCase by lowercasing the first character and leaving the remainder unchanged.

- **Parameters:** `string value`
- **Returns:** `string` — the camel-cased string.
- **Throws:** `ArgumentNullException` if `value` is `null`.

### `ToSnakeCase`

Converts the string to snake_case by inserting underscores between word boundaries and lowercasing all characters.

- **Parameters:** `string value`
- **Returns:** `string` — the snake-cased string.
- **Throws:** `ArgumentNullException` if `value` is `null`.

### `ToKebabCase`

Converts the string to kebab-case by inserting hyphens between word boundaries and lowercasing all characters.

- **Parameters:** `string value`
- **Returns:** `string` — the kebab-cased string.
- **Throws:** `ArgumentNullException` if `value` is `null`.

### `Truncate`

Truncates the string to a specified maximum length, appending an optional suffix (default `"..."`) when truncation occurs. If the string is shorter than or equal to the maximum length, it is returned unchanged.

- **Parameters:**
  - `string value`
  - `int maxLength` — the maximum allowed length.
  - `string suffix = "..."` — the string appended when truncation occurs.
- **Returns:** `string` — the truncated string with suffix if truncation was necessary; otherwise the original string.
- **Throws:**
  - `ArgumentNullException` if `value` is `null`.
  - `ArgumentOutOfRangeException` if `maxLength` is less than zero.

### `CountOccurrences`

Counts the number of non-overlapping occurrences of a substring within the string using ordinal comparison.

- **Parameters:**
  - `string value`
  - `string substring`
- **Returns:** `int` — the number of times `substring` appears in `value`.
- **Throws:**
  - `ArgumentNullException` if `value` is `null`.
  - `ArgumentException` if `substring` is `null` or empty.

### `RemovePrefix`

Removes a specified prefix from the start of the string if it matches exactly, using ordinal comparison. If the prefix is not present, the string is returned unchanged.

- **Parameters:**
  - `string value`
  - `string prefix`
- **Returns:** `string` — the string without the prefix, or the original string if the prefix was not found.
- **Throws:**
  - `ArgumentNullException` if `value` is `null`.
  - `ArgumentException` if `prefix` is `null` or empty.

### `RemoveSuffix`

Removes a specified suffix from the end of the string if it matches exactly, using ordinal comparison. If the suffix is not present, the string is returned unchanged.

- **Parameters:**
  - `string value`
  - `string suffix`
- **Returns:** `string` — the string without the suffix, or the original string if the suffix was not found.
- **Throws:**
  - `ArgumentNullException` if `value` is `null`.
  - `ArgumentException` if `suffix` is `null` or empty.

### `IsValidEmail`

Determines whether the string represents a valid email address according to a standard pattern.

- **Parameters:** `string value`
- **Returns:** `bool` — `true` if the string matches the email pattern; otherwise `false`.
- **Throws:** Never. Returns `false` for `null` input.

### `IsValidUrl`

Determines whether the string represents a valid absolute URL using `Uri.TryCreate` with `UriKind.Absolute`.

- **Parameters:** `string value`
- **Returns:** `bool` — `true` if the string is a valid absolute URL; otherwise `false`.
- **Throws:** Never. Returns `false` for `null` input.

### `ToSlug`

Converts the string to a URL-friendly slug by lowercasing, replacing non-alphanumeric characters with hyphens, collapsing consecutive hyphens, and trimming leading/trailing hyphens.

- **Parameters:** `string value`
- **Returns:** `string` — the slugified string.
- **Throws:** `ArgumentNullException` if `value` is `null`.

### `Repeat`

Repeats the string a specified number of times.

- **Parameters:**
  - `string value`
  - `int count`
- **Returns:** `string` — the concatenated result of repeating `value` `count` times. Returns `string.Empty` if `count` is zero.
- **Throws:**
  - `ArgumentNullException` if `value` is `null`.
  - `ArgumentOutOfRangeException` if `count` is negative.

### `SplitAndTrim`

Splits the string by a specified separator and trims each resulting element. Empty entries are removed.

- **Parameters:**
  - `string value`
  - `char separator`
- **Returns:** `string[]` — an array of trimmed, non-empty substrings.
- **Throws:** `ArgumentNullException` if `value` is `null`.

## Usage

### Example 1: Processing Saga Input

```csharp
string rawInput = "  USER_REGISTRATION_SAGA  ";
string sagaName = rawInput.Trim().ToSnakeCase(); // "user_registration_saga"
string slug = sagaName.ToSlug();                 // "user-registration-saga"

if (!sagaName.RemovePrefix("user_").IsNullOrEmpty())
{
    string[] parts = sagaName.SplitAndTrim('_');
    Console.WriteLine($"Parts: {string.Join(", ", parts)}");
}
```

### Example 2: Formatting and Validating Contact Data

```csharp
string? email = GetUserEmail(); // may return null or empty
string? url = GetCallbackUrl();

if (email.NullIfEmpty() is string validEmail && validEmail.IsValidEmail())
{
    string truncated = validEmail.Truncate(30);
    Console.WriteLine($"Contact: {truncated}");
}

if (url.IsValidUrl())
{
    string domain = url.RemovePrefix("https://").RemoveSuffix("/");
    Console.WriteLine($"Domain: {domain}");
}
```

## Notes

- All methods that accept a `string value` parameter and throw `ArgumentNullException` treat `null` as an invalid argument. Methods that return `bool` and accept `string?` gracefully handle `null` by returning `false`.
- `NullIfEmpty` treats white-space strings as non-empty; use `IsNullOrWhiteSpace` separately if white-space should also yield `null`.
- `Truncate` accounts for the length of the suffix when determining whether truncation is necessary. If `maxLength` is less than the suffix length, the suffix itself may be truncated or the result may be the suffix alone, depending on implementation specifics.
- `CountOccurrences` performs ordinal, case-sensitive matching and does not count overlapping occurrences (e.g., searching for `"aa"` in `"aaa"` yields `1`).
- `RemovePrefix` and `RemoveSuffix` perform exact, case-sensitive, ordinal matching. They remove only the first matching prefix or last matching suffix respectively.
- `IsValidUrl` validates only absolute URIs; relative URLs return `false`.
- All methods are static and stateless, making them inherently thread-safe. No shared mutable state is used.
