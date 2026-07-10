# EnumExtensions
Provides a set of static extension methods for working with .NET enumerations, offering utilities such as attribute‑based display names, parsing, flag manipulation, and enumeration of values and names.

## API
### GetDescription<T>
```csharp
public static string GetDescription<T>(this T value) where T : struct, Enum
```
- **Purpose**: Returns the string supplied by the `System.ComponentModel.DescriptionAttribute` on the enum member, if present; otherwise returns the member's name via `ToString()`.
- **Parameters**: `value` – the enum instance to query.
- **Return Value**: The description string, or the enum name if no description attribute exists.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enum type.
  - `InvalidOperationException` if `value` does not represent a defined enum member.

### GetDisplayName<T>
```csharp
public static string GetDisplayName<T>(this T value) where T : struct, Enum
```
- **Purpose**: Returns the string from the `System.ComponentModel.DataAnnotations.DisplayAttribute.Name` property on the enum member, if present; otherwise falls back to the description (see `GetDescription`) or the member name.
- **Parameters**: `value` – the enum instance to query.
- **Return Value**: The display name string, or a fallback as described.
- **Exceptions**: Same as `GetDescription<T>`.

### ParseEnum<T>
```csharp
public static T? ParseEnum<T>(this string input) where T : struct, Enum
```
- **Purpose**: Attempts to parse `input` into an enum value of type `T`, ignoring case and allowing whitespace.
- **Parameters**: `input` – the string to parse; may be `null`.
- **Return Value**: The parsed enum value, or `null` if parsing fails or `input` is `null`/empty.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enum type.

### GetValues<T>
```csharp
public static IEnumerable<T> GetValues<T>() where T : struct, Enum
```
- **Purpose**: Enumerates all defined constants of the enum type `T`.
- **Parameters**: None.
- **Return Value**: An `IEnumerable<T>` yielding each enum member.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enum type.

### GetNames<T>
```csharp
public static IEnumerable<string> GetNames<T>() where T : struct, Enum
```
- **Purpose**: Enumerates the names of all defined constants of the enum type `T`.
- **Parameters**: None.
- **Return Value**: An `IEnumerable<string>` containing each enum member's name.
- **Exceptions**: Same as `GetValues<T>`.

### GetEnumDictionary<T>
```csharp
public static Dictionary<T, string> GetEnumDictionary<T>() where T : struct, Enum
```
- **Purpose**: Creates a dictionary mapping each enum value to its string representation (the member's name).
- **Parameters**: None.
- **Return Value**: A `Dictionary<T, string>` where the key is the enum value and the value is its name.
- **Exceptions**: Same as `GetValues<T>`.

### IsDefined<T>
```csharp
public static bool IsDefined<T>(this T value) where T : struct, Enum
```
- **Purpose**: Determines whether `value` corresponds to a defined member of the enum type `T`.
- **Parameters**: `value` – the enum instance to test.
- **Return Value**: `true` if `value` is a defined enum constant; otherwise `false`.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enum type.

### GetNext<T>
```csharp
public static T GetNext<T>(this T value) where T : struct, Enum
```
- **Purpose**: Returns the enum constant that follows `value` in the order defined by the enum declaration. If `value` is the last defined member, throws.
- **Parameters**: `value` – the enum instance for which to find the successor.
- **Return Value**: The next enum constant.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enum type.
  - `InvalidOperationException` if `value` is not a defined member or is the last member.

### GetPrevious<T>
```csharp
public static T GetPrevious<T>(this T value) where T : struct, Enum
```
- **Purpose**: Returns the enum constant that precedes `value` in the declaration order. If `value` is the first defined member, throws.
- **Parameters**: `value` – the enum instance for which to find the predecessor.
- **Return Value**: The previous enum constant.
- **Exceptions**: Same as `GetNext<T>`.

### GetNumericValue<T>
```csharp
public static object GetNumericValue<T>(this T value) where T : struct, Enum
```
- **Purpose**: Returns the underlying integral value of the enum member as a boxed `object`.
- **Parameters**: `value` – the enum instance.
- **Return Value**: The numeric value (`byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, or `ulong`) depending on the enum's underlying type.
- **Exceptions**: Same as `IsDefined<T>`.

### HasAnyFlag<T>
```csharp
public static bool HasAnyFlag<T>(this T value, T flags) where T : struct, Enum
```
- **Purpose**: Returns `true` if any of the bits set in `flags` are also set in `value`. Intended for use with `[Flags]` enums.
- **Parameters**: 
  - `value` – the enum instance to test.
  - `flags` – the enum value containing bits to check.
- **Return Value**: `true` if at least one flag matches; otherwise `false`.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enum type.
  - `InvalidOperationException` if `T` is not decorated with the `[Flags]` attribute.

### GetEnumFromValue<T>
```csharp
public static T? GetEnumFromValue<T>(this int value) where T : struct, Enum
```
- **Purpose**: Looks up the enum constant whose underlying integral value equals `value`. Returns `null` if no such constant exists.
- **Parameters**: `value` – the integral value to match.
- **Return Value**: The corresponding enum member, or `null` if not found.
- **Exceptions**: Same as `IsDefined<T>`.

### FormatFlags<T>
```csharp
public static string FormatFlags<T>(this T value) where T : struct, Enum
```
- **Purpose**: Produces a comma‑separated string of the flag names that are set in `value`. For non‑flags enums behaves like `ToString()`.
- **Parameters**: `value` – the enum instance containing flags.
- **Return Value**: A string representation of the set flags, or the enum name if no flags are set.
- **Exceptions**: Same as `HasAnyFlag<T>`.

### IsFlagsEnum<T>
```csharp
public static bool IsFlagsEnum<T>() where T : struct, Enum
```
- **Purpose**: Indicates whether the enum type `T` is decorated with the `[System.FlagsAttribute]`.
- **Parameters**: None.
- **Return Value**: `true` if `T` has the `Flags` attribute; otherwise `false`.
- **Exceptions**: Same as `GetValues<T>`.

### GetRange<T>
```csharp
public static (T min, T max) GetRange<T>() where T : struct, Enum
```
- **Purpose**: Returns the smallest and largest defined enum constants of type `T` according to their underlying integral values.
- **Parameters**: None.
- **Return Value**: A value tuple `(min, max)` where `min` is the enum member with the lowest numeric value and `max` the one with the highest.
- **Exceptions**: 
  - `ArgumentException` if `T` is not an enum type.
  - `InvalidOperationException` if the enum has no defined members.

## Usage
```csharp
public enum Priority
{
    [Description("Low importance")]
    Low = 1,
    [Description("Medium importance")]
    Medium = 2,
    [Description("High importance")]
    High = 3
}

// Retrieve the description attribute value
string desc = Priority.Medium.GetDescription(); // "Medium importance"

// Get all defined values as an array
Priority[] all = Priority.GetValues().ToArray(); // [Low, Medium, High]
```

```csharp
[Flags]
public enum Permissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Execute = 4
}

Permissions user = Permissions.Read | Permissions.Write;

// Check if any of the specified flags are set
bool canWriteOrExecute = user.HasAnyFlag(Permissions.Write | Permissions.Execute); // true

// Produce a readable flag list
string flagList = user.FormatFlags(); // "Read, Write"
```

## Notes
- All methods are pure static extensions; they contain no internal state and are therefore thread‑safe for concurrent invocation.
- Generic type parameter `T` must be an enum; supplying a non‑enum type results in an `ArgumentException` (or `InvalidOperationException` where noted).
- Methods that return a nullable enum (`ParseEnum`, `GetEnumFromValue`) yield `null` when the input does not map to a defined member, avoiding exceptions for invalid strings or values.
- Flag‑related methods (`HasAnyFlag`, `FormatFlags`, `IsFlagsEnum`) assume the enum is decorated with `[Flags]`; using them on non‑flags enums will throw an `InvalidOperationException` to prevent misleading results.
- Order‑sensitive methods (`GetNext`, `GetPrevious`, `GetRange`) rely on the declaration order of enum constants, not their numeric values, except for `GetRange` which uses numeric extremes.
- If an enum defines duplicate underlying values (e.g., two members with the same integer), `GetNext`/`GetPrevious` treat the first occurrence in declaration order as the distinct member; `GetRange` will still return the correct numeric min and max based on those values.
