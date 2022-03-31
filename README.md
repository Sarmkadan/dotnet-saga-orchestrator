## EnumExtensions

The `EnumExtensions` static class provides a comprehensive set of extension methods for handling and manipulating enums. It simplifies tasks like retrieving descriptions and display names, parsing strings to enums, performing flag checks, and navigating enum values.

### Usage Example

```csharp
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SagaOrchestrator.Core.Extensions;

public enum Status
{
    [Description("Pending Approval")]
    Pending,
    [Display(Name = "In Progress")]
    Processing,
    Completed
}

[Flags]
public enum Permissions
{
    Read = 1,
    Write = 2,
    Execute = 4
}

// Get descriptions and display names
var status = Status.Pending;
Console.WriteLine(status.GetDescription()); // Pending Approval
Console.WriteLine(Status.Processing.GetDisplayName()); // In Progress

// Parse strings
var parsed = "Completed".ParseEnum<Status>(); // Status.Completed

// Get all values and names
var values = EnumExtensions.GetValues<Status>(); // [Pending, Processing, Completed]
var names = EnumExtensions.GetNames<Status>(); // ["Pending", "Processing", "Completed"]

// Get dictionary
var dict = EnumExtensions.GetEnumDictionary<Status>();

// Check definition and navigate
Console.WriteLine(Status.Pending.IsDefined()); // True
Console.WriteLine(status.GetNext()); // Processing
Console.WriteLine(status.GetPrevious()); // Completed

// Get numeric value
Console.WriteLine(status.GetNumericValue()); // 0

// Flags and range
var perms = Permissions.Read | Permissions.Write;
Console.WriteLine(perms.HasAnyFlag(Permissions.Read)); // True
Console.WriteLine(EnumExtensions.IsFlagsEnum<Permissions>()); // True
Console.WriteLine(perms.FormatFlags()); // "Read, Write"
var range = EnumExtensions.GetRange<Status>(); // (Pending, Completed)

// Get from numeric value
var fromValue = EnumExtensions.GetEnumFromValue<Status>(1); // Status.Processing
```


## StringExtensions

The `StringExtensions` static class provides a comprehensive set of extension methods for string manipulation and validation. These utilities simplify common string operations like checking for null/empty, case conversions, substring counting, and formatting.

### Usage Example

```csharp
using SagaOrchestrator.Core.Extensions;

var originalString = "  Hello World  ";
var trimmedString = originalString.Trim();
var isEmpty = string.IsNullOrEmpty(trimmedString); // False

// Check if null/empty/whitespace
Console.WriteLine(originalString.IsNullOrEmpty()); // False
Console.WriteLine(originalString.IsNullOrWhiteSpace()); // True
Console.WriteLine("".IsNullOrEmpty()); // True
Console.WriteLine("   ".IsNullOrWhiteSpace()); // True

// Convert to title/camel/snake/kebab case
Console.WriteLine("helloWorld".ToTitleCase()); // HelloWorld
Console.WriteLine("HelloWorld".ToCamelCase()); // helloWorld
Console.WriteLine("HelloWorld".ToSnakeCase()); // hello_world
Console.WriteLine("HelloWorld".ToKebabCase()); // hello-world

// Truncate and append ellipsis
Console.WriteLine("This is a very long string".Truncate(10)); // This is a...

// Count occurrences of substring
Console.WriteLine("hello world, hello universe".CountOccurrences("hello")); // 2

// Remove prefix/suffix
Console.WriteLine("https://example.com/path".RemovePrefix("https://")); // example.com/path
Console.WriteLine("example.txt".RemoveSuffix(".txt")); // example

// Validate email and URL
Console.WriteLine("user@example.com".IsValidEmail()); // True
Console.WriteLine("https://example.com".IsValidUrl()); // True

// Create slug
Console.WriteLine("Hello World!".ToSlug()); // hello-world

// Repeat string
Console.WriteLine("abc".Repeat(3)); // abcabcabc

// Split and trim
Console.WriteLine("  a,  b,   c  ".SplitAndTrim(',').Length); // 3

// Null if empty
Console.WriteLine("".NullIfEmpty()); // 
Console.WriteLine("test".NullIfEmpty()); // test
```

## SagaStepDefinition

The `SagaStepDefinition` class defines the configuration and behavior of a step in a saga workflow. It encapsulates all necessary parameters for executing a step, including service endpoints, timeout and retry configurations, compensation logic, and metadata for custom orchestration behaviors.

### Usage Example

```csharp
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Utilities;

// Create a basic step definition
var step = new SagaStepDefinition(
    "Process Payment",
    "PaymentService",
    "https://payments.example.com/api/process",
    "https://payments.example.com/api/compensate"
);

step.Description = "Process customer payment transaction";
step.Order = 1;
step.TimeoutSeconds = 60;
step.MaxRetries = 5;
step.RetryDelayMilliseconds = 2000;
step.IsAsync = true;
step.HttpMethod = "POST";

// Configure compensation
step.SetCompensable(true, "https://payments.example.com/api/compensate");

// Set custom retry policy
step.SetRetryPolicy(new RetryPolicy(
    maxRetries: 3,
    initialDelayMs: 1000,
    maxDelayMs: 10000,
    backoffMultiplier: 2.0,
    jitterFactor: 0.1
));

// Add metadata
step.Metadata.Add("circuitBreakerThreshold", "3");
step.Metadata.Add("requiresTransaction", "true");

// Validate configuration
bool isValid = step.Validate();

// Adjust timeout dynamically
step.SetTimeout(30);
```

### Usage Example

```csharp
using SagaOrchestrator.Core.Extensions;

// Create a deadline 2 hours from now
var deadline = DateTime.UtcNow.AddHours(2);

// Check if deadline has expired
Console.WriteLine(deadline.IsExpired()); // False
Console.WriteLine(DateTime.UtcNow.AddMinutes(-5).IsExpired()); // True

// Calculate remaining time until deadline
var remaining = deadline.TimeUntil();
Console.WriteLine($"Time remaining: {remaining.FormatDuration()}"); // Time remaining: 2 hours

// Calculate elapsed time since start
var start = DateTime.UtcNow.AddMinutes(-15);
var elapsed = start.ElapsedSince();
Console.WriteLine($"Elapsed: {elapsed.FormatDuration()}"); // Elapsed: 15 minutes

// Round down to specific time units
var now = DateTime.UtcNow;
Console.WriteLine(now.RoundDownToSecond()); // Current time rounded to nearest second
Console.WriteLine(now.RoundDownToMinute()); // Current time rounded to nearest minute
Console.WriteLine(now.RoundDownToHour()); // Current time rounded to nearest hour

// Check if date is within range
var rangeStart = DateTime.UtcNow.AddDays(-1);
var rangeEnd = DateTime.UtcNow.AddDays(1);
Console.WriteLine(now.IsWithinRange(rangeStart, rangeEnd)); // True

// Convert to ISO 8601 and Unix timestamp
Console.WriteLine(now.ToIso8601String()); // 2025-07-15T12:34:56.789Z
Console.WriteLine(now.ToUnixTimestamp()); // 1752544496

// Convert back from Unix timestamp
var fromTimestamp = DateTimeExtensions.FromUnixTimestamp(1752544496);
Console.WriteLine(fromTimestamp); // 2025-07-15T12:34:56Z

// Add business days (excludes weekends)
var monday = new DateTime(2025, 7, 14); // Monday
Console.WriteLine(monday.AddBusinessDays(3)); // 2025-07-17 (Thursday)
Console.WriteLine(monday.AddBusinessDays(5)); // 2025-07-21 (Monday)

// Get start/end of time periods
Console.WriteLine(now.StartOfDay()); // Today at 00:00:00
Console.WriteLine(now.EndOfDay()); // Today at 23:59:59.9999999
Console.WriteLine(now.StartOfMonth()); // First day of month at 00:00:00
Console.WriteLine(now.EndOfMonth()); // Last day of month at 23:59:59.9999999
Console.WriteLine(now.StartOfYear()); // January 1st at 00:00:00
Console.WriteLine(now.EndOfYear()); // December 31st at 23:59:59.999

// Format time spans
var duration = TimeSpan.FromMinutes(90);
Console.WriteLine(duration.FormatDuration()); // 1 hours

// Measure execution time
var executionTime = DateTimeExtensions.Measure(() => {
    Thread.Sleep(100);
});
Console.WriteLine($"Execution took: {executionTime.TotalMilliseconds}ms");

// Relative time formatting
Console.WriteLine(DateTime.UtcNow.AddMinutes(-2).ToRelativeTime()); // 2 minutes ago
Console.WriteLine(DateTime.UtcNow.AddHours(-1).ToRelativeTime()); // 1 hours ago
```

## ValidationExtensions

The `ValidationExtensions` static class provides a comprehensive set of extension methods for parameter validation using a fluent API. These utilities simplify common validation patterns like null checks, range validation, string length constraints, email/URL validation, and collection validation in a clean, chainable way.

### Usage Example

```csharp
using SagaOrchestrator.Core.Extensions;

// Validate method parameters with fluent syntax
public void ProcessOrder(
    string customerEmail,
    int quantity,
    DateTime deadline,
    List<string> items,
    decimal discountRate)
{
    // Null checks
    customerEmail.NotNullOrEmpty(nameof(customerEmail))
        .ValidateEmail(nameof(customerEmail));
    
    // Range validation
    quantity.InRange(1, 100, nameof(quantity));
    discountRate.InRange(0, 1, nameof(discountRate));
    
    // Time validation
    deadline.GreaterThan(DateTime.UtcNow, nameof(deadline));
    TimeSpan.FromHours(2).GreaterThanZero(nameof(deadline));
    
    // Collection validation
    items.NotEmpty(nameof(items));
    
    // Guid validation
    var orderId = Guid.NewGuid();
    orderId.NotEmpty(nameof(orderId));
    
    // Array validation
    var tags = new string[] { "urgent", "priority" };
    tags.NotEmptyArray(nameof(tags));
    
    // Dictionary validation
    var metadata = new Dictionary<string, string> { { "key", "value" } };
    metadata.NotEmptyDictionary(nameof(metadata));
    
    // Conditional validation
    var optionalValue = "test@example.com";
    optionalValue.ValidateIf(
        v => v.IsValidEmail(),
        "Optional value must be a valid email if provided");
    
    // String length constraints
    var username = "user123";
    username.MinLength(5, nameof(username))
        .MaxLength(20, nameof(username));
    
    // URL validation
    var callbackUrl = "https://example.com/api/callback";
    callbackUrl.ValidateUrl(nameof(callbackUrl));
}

// ... existing ValidationExtensions usage example ...

// Example usage
ProcessOrder(
    "user@example.com",
    5,
    DateTime.UtcNow.AddHours(1),
    new List<string> { "item1", "item2" },
    0.15m);
```

## EnumExtensions

The `EnumExtensions` static class provides a comprehensive set of extension methods for handling and manipulating enums. It simplifies tasks like retrieving descriptions and display names, parsing strings to enums, performing flag checks, and navigating enum values.

### Usage Example

```csharp
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SagaOrchestrator.Core.Extensions;

public enum Status
{
    [Description("Pending Approval")]
    Pending,
    [Display(Name = "In Progress")]
    Processing,
    Completed
}

[Flags]
public enum Permissions
{
    Read = 1,
    Write = 2,
    Execute = 4
}

// Get descriptions and display names
var status = Status.Pending;
Console.WriteLine(status.GetDescription()); // Pending Approval
Console.WriteLine(Status.Processing.GetDisplayName()); // In Progress

// Parse strings
var parsed = "Completed".ParseEnum<Status>(); // Status.Completed

// Get all values and names
var values = EnumExtensions.GetValues<Status>(); // [Pending, Processing, Completed]
var names = EnumExtensions.GetNames<Status>(); // ["Pending", "Processing", "Completed"]

// Get dictionary
var dict = EnumExtensions.GetEnumDictionary<Status>();

// Check definition and navigate
Console.WriteLine(Status.Pending.IsDefined()); // True
Console.WriteLine(status.GetNext()); // Processing
Console.WriteLine(status.GetPrevious()); // Completed

// Get numeric value
Console.WriteLine(status.GetNumericValue()); // 0

// Flags and range
var perms = Permissions.Read | Permissions.Write;
Console.WriteLine(perms.HasAnyFlag(Permissions.Read)); // True
Console.WriteLine(EnumExtensions.IsFlagsEnum<Permissions>()); // True
Console.WriteLine(perms.FormatFlags()); // "Read, Write"
var range = EnumExtensions.GetRange<Status>(); // (Pending, Completed)

// Get from numeric value
var fromValue = EnumExtensions.GetEnumFromValue<Status>(1); // Status.Processing
```

