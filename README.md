// existing content ...

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
Console.WriteLine(originalString.IsNullOrWhiteSpace()); // False
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
Console.WriteLine("".NullIfEmpty()); // null
Console.WriteLine("test".NullIfEmpty()); // test
```
```