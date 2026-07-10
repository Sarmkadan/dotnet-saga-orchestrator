# DateTimeExtensions

A collection of extension methods that add common date‑and‑time utilities to the `System.DateTime` and `System.TimeSpan` types. The methods are pure, stateless, and safe to call from any thread.

## API

### IsExpired
**Purpose**  
Determines whether a given `DateTime` has passed a specified expiry interval relative to the current UTC time.  
**Parameters**  
- `this DateTime dateTime` – The instant to test.  
- `TimeSpan expiry` – The duration after which `dateTime` is considered expired.  
**Return value**  
`true` if `dateTime.Add(expiry)` is earlier than `DateTime.UtcNow`; otherwise `false`.  
**Exceptions**  
None.

### TimeUntil
**Purpose**  
Calculates the amount of time remaining until a future `DateTime`.  
**Parameters**  
- `this DateTime dateTime` – The target instant.  
**Return value**  
A `TimeSpan` representing `dateTime - DateTime.UtcNow`. The value may be negative if `dateTime` is in the past.  
**Exceptions**  
None.

### ElapsedSince
**Purpose**  
Calculates the amount of time that has elapsed since a past `DateTime`.  
**Parameters**  
- `this DateTime dateTime` – The starting instant.  
**Return value**  
A `TimeSpan` representing `DateTime.UtcNow - dateTime`. The value may be negative if `dateTime` is in the future.  
**Exceptions**  
None.

### RoundDownToSecond
**Purpose**  
Truncates a `DateTime` to whole‑second precision.  
**Parameters**  
- `this DateTime dateTime` – The value to truncate.  
**Return value**  
A new `DateTime` with the tick fraction removed (seconds retained, milliseconds and finer set to zero), preserving the original `Kind`.  
**Exceptions**  
None.

### RoundDownToMinute
**Purpose**  
Truncates a `DateTime` to whole‑minute precision.  
**Parameters**  
- `this DateTime dateTime` – The value to truncate.  
**Return value**  
A new `DateTime` with seconds and finer set to zero, preserving the original `Kind`.  
**Exceptions**  
None.

### RoundDownToHour
**Purpose**  
Truncates a `DateTime` to whole‑hour precision.  
**Parameters**  
- `this DateTime dateTime` – The value to truncate.  
**Return value**  
A new `DateTime` with minutes, seconds, and finer set to zero, preserving the original `Kind`.  
**Exceptions**  
None.

### IsWithinRange
**Purpose**  
Checks whether a `DateTime` falls inside a closed interval defined by two bounds.  
**Parameters**  
- `this DateTime dateTime` – The value to test.  
- `DateTime start` – The inclusive lower bound.  
- `DateTime end` – The inclusive upper bound.  
**Return value**  
`true` if `dateTime` is greater than or equal to `start` and less than or equal to `end`; otherwise `false`.  
**Exceptions**  
None.

### ToRelativeTime
**Purpose**  
Produces a human‑readable string that describes how far a `DateTime` is from the current UTC time (e.g., “2 minutes ago”, “in 3 hours”).  
**Parameters**  
- `this DateTime dateTime` – The instant to format.  
**Return value**  
A localized‑friendly relative time string.  
**Exceptions**  
None.

### ToIso8601String
**Purpose**  
Converts a `DateTime` to an ISO 8601‑compliant string suitable for round‑trip serialization.  
**Parameters**  
- `this DateTime dateTime` – The value to format.  
**Return value**  
A string in the format `"yyyy-MM-ddTHH:mm:ss.fffffffK"` (the “o” standard format specifier).  
**Exceptions**  
None.

### ToUnixTimestamp
**Purpose**  
Represents a `DateTime` as seconds elapsed since the Unix epoch (1970‑01‑01T00:00:00Z).  
**Parameters**  
- `this DateTime dateTime` – The value to convert; it is treated as UTC if its `Kind` is `Utc`, otherwise it is converted to UTC.  
**Return value**  
A `long` containing the total number of whole seconds.  
**Exceptions**  
None.

### FromUnixTimestamp
**Purpose**  
Creates a `DateTime` from a Unix timestamp expressed in seconds.  
**Parameters**  
- `long unixTimestamp` – The number of seconds since 1970‑01‑01T00:00:00Z.  
**Return value**  
A `DateTime` with `Kind` set to `Utc` representing the corresponding instant.  
**Exceptions**  
- `ArgumentOutOfRangeException` if the resulting `DateTime` would be outside the supported range of `DateTime`.

### AddBusinessDays
**Purpose**  
Adds a number of business days (Monday‑Friday) to a `DateTime`, skipping weekends.  
**Parameters**  
- `this DateTime dateTime` – The starting instant.  
- `int businessDays` – The number of business days to add; may be negative to subtract.  
**Return value**  
A new `DateTime` shifted by the specified number of business days, preserving the original time‑of‑day and `Kind`.  
**Exceptions**  
- `ArgumentOutOfRangeException` if the resulting `DateTime` would be outside the supported range.

### StartOfDay
**Purpose**  
Returns the midnight that begins the day of a given `DateTime`.  
**Parameters**  
- `this DateTime dateTime` – The input instant.  
**Return value**  
A `DateTime` set to 00:00:00 of the same day, preserving the original `Kind`.  
**Exceptions**  
None.

### EndOfDay
**Purpose**  
Returns the last tick of the day that a given `DateTime` belongs to.  
**Parameters**  
- `this DateTime dateTime` – The input instant.  
**Return value**  
A `DateTime` set to 23:59:59.9999999 of the same day, preserving the original `Kind`.  
**Exceptions**  
None.

### StartOfMonth
**Purpose**  
Returns the first moment of the month that a given `DateTime` belongs to.  
**Parameters**  
- `this DateTime dateTime` – The input instant.  
**Return value**  
A `DateTime` set to 00:00:00 on the first day of the month, preserving the original `Kind`.  
**Exceptions**  
None.

### EndOfMonth
**Purpose**  
Returns the last tick of the month that a given `DateTime` belongs to.  
**Parameters**  
- `this DateTime dateTime` – The input instant.  
**Return value**  
A `DateTime` set to 23:59:59.9999999 on the last day of the month, preserving the original `Kind`.  
**Exceptions**  
None.

### StartOfYear
**Purpose**  
Returns the first moment of the year that a given `DateTime` belongs to.  
**Parameters**  
- `this DateTime dateTime` – The input instant.  
**Return value**  
A `DateTime` set to 00:00:00 on January 1 of the same year, preserving the original `Kind`.  
**Exceptions**  
None.

### EndOfYear
**Purpose**  
Returns the last tick of the year that a given `DateTime` belongs to.  
**Parameters**  
- `this DateTime dateTime` – The input instant.  
**Return value**  
A `DateTime` set to 23:59:59.9999999 on December 31 of the same year, preserving the original `Kind`.  
**Exceptions**  
None.

### FormatDuration
**Purpose**  
Produces a compact, readable string representation of a `TimeSpan`.  
**Parameters**  
- `this TimeSpan timeSpan` – The interval to format.  
**Return value**  
A string in the format `"[d.]hh:mm:ss"` where the days component is omitted if zero. Fractional seconds are truncated to whole seconds.  
**Exceptions**  
None.

### Measure
**Purpose**  
Measures the execution time of an action using a high‑resolution stopwatch.  
**Parameters**  
- `Action action` – The delegate to execute; must not be `null`.  
**Return value**  
A `TimeSpan` indicating the elapsed time for the invocation of `action`.  
**Exceptions**  
- `ArgumentNullException` if `action` is `null`.  

## Usage

```csharp
using System;
using static DateTimeExtensions;   // assuming the class is imported as a static namespace

var now = DateTime.UtcNow;

// Example 1: Determine if a token issued 10 minutes ago is still valid (5‑minute expiry).
DateTime issued = now.AddMinutes(-10);
bool stillValid = !issued.IsExpired(TimeSpan.FromMinutes(5)); // false, token expired

// Example 2: Calculate the start of the next business day, skipping weekends.
DateTime today = new DateTime(2025, 11, 02, 14, 30, 0, DateTimeKind.Utc); // a Sunday
DateTime nextBizDay = today.AddBusinessDays(1); // results in 2025-11-03 14:30:00Z (Monday)
```

```csharp
using System;
using DateTimeExtensions;   // instance‑style usage

var deadline = new DateTime(2025, 12, 31, 23, 59, 59, DateTimeKind.Utc);

// How much time remains until the deadline?
TimeSpan remaining = deadline.TimeUntil();
Console.WriteLine($"Time left: {remaining}");

// Convert a Unix timestamp to a DateTime and format it as ISO 8601.
long unix = 1735689600; // 2025-01-01T00:00:00Z
DateTime fromUnix = DateTimeExtensions.FromUnixTimestamp(unix);
string iso = fromUnix.ToIso8601String();
Console.WriteLine(iso); // "2025-01-01T00:00:00Z"
```

## Notes

- All extension methods are **pure**: they do not modify the source `DateTime` or `TimeSpan` instance and have no side effects.  
- The methods rely on `DateTime.UtcNow` for “now‑based” calculations; if the source `DateTime` has a different `Kind`, it is implicitly converted to UTC where required (e.g., `ToUnixTimestamp`, `IsExpired`).  
- `AddBusinessDays` excludes only Saturdays and Sundays; it does **not** account for public holidays.  
- `Measure` starts and stops a `System.Diagnostics.Stopwatch` around the supplied `Action`; the action is executed synchronously, and any exception thrown by the action is propagated unchanged.  
- Because the methods contain no static mutable state, they are thread‑safe and can be invoked concurrently from any number of threads without additional synchronization.  
- Passing a `DateTime` with an `Kind` of `Unspecified` to methods that treat the value as UTC (`ToUnixTimestamp`, `FromUnixTimestamp`, `IsExpired`) will assume the value represents UTC; callers should ensure the intended semantics or convert the value explicitly beforehand.  
- The `FormatDuration` method truncates sub‑second precision; for higher‑resolution formatting, use the standard `TimeSpan` format strings directly.
