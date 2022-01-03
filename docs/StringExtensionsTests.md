# StringExtensionsTests

StringExtensionsTests is a unit test class responsible for validating the behavior of various string extension methods within the dotnet-saga-orchestrator project. These tests ensure that transformations such as case conversion, truncation, repetition, and slug generation function correctly under a variety of input conditions, including edge cases like empty strings, single characters, and repeated substrings.

## API

### ToSnakeCase_PascalCaseInput_InsertsUnderscoreBetweenWords
Validates that a PascalCase input string is correctly converted to snake_case format with underscores separating words.  
**Purpose:** Ensures proper word separation in snake_case conversion.  
**Parameters:** Implicitly tests with a PascalCase input string (e.g., "PascalCaseInput").  
**Return Value:** Asserts the output matches the expected snake_case string (e.g., "pascal_case_input").  
**Throws:** Does not throw; validates transformation logic.

### ToSnakeCase_SingleWord_ReturnsLowercase
Confirms that a single-word input string is returned in lowercase without underscores.  
**Purpose:** Tests single-word handling in snake_case conversion.  
**Parameters:** Implicitly tests with a single-word input (e.g., "Word").  
**Return Value:** Asserts the output is the lowercase version of the input (e.g., "word").  
**Throws:** Does not throw.

### ToKebabCase_PascalCaseInput_ReturnsHyphenSeparated
Verifies that PascalCase input is converted to kebab-case with hyphens separating words.  
**Purpose:** Ensures hyphen-separated formatting for kebab-case.  
**Parameters:** Implicitly tests with a PascalCase input (e.g., "PascalCaseInput").  
**Return Value:** Asserts the output uses hyphens (e.g., "pascal-case-input").  
**Throws:** Does not throw.

### ToCamelCase_PascalCase_LowercasesFirstCharacter
Tests that the first character of a PascalCase string is lowercased in camelCase conversion.  
**Purpose:** Validates first-character lowercase transformation.  
**Parameters:** Implicitly tests with a PascalCase input (e.g., "PascalCase").  
**Return Value:** Asserts the output starts with a lowercase letter (e.g., "pascalCase").  
**Throws:** Does not throw.

### ToCamelCase_SingleCharacter_ReturnsLowercase
Ensures a single-character input is returned in lowercase.  
**Purpose:** Tests single-character handling in camelCase conversion.  
**Parameters:** Implicitly tests with a single character (e.g., "A").  
**Return Value:** Asserts the output is lowercase (e.g., "a").  
**Throws:** Does not throw.

### Truncate_StringLongerThanMax_AppendsEllipsis
Confirms that strings exceeding a maximum length are truncated and appended with an ellipsis ("...").  
**Purpose:** Validates truncation logic for long strings.  
**Parameters:** Implicitly tests with a string longer than the specified max length.  
**Return Value:** Asserts the output is truncated and ends with "...".  
**Throws:** Does not throw.

### Truncate_StringShorterThanMax_ReturnsOriginalUnchanged
Ensures strings shorter than the maximum length are returned unmodified.  
**Purpose:** Tests that short strings are not altered.  
**Parameters:** Implicitly tests with a string shorter than the max length.  
**Return Value:** Asserts the output equals the original input.  
**Throws:** Does not throw.

### CountOccurrences_SubstringRepeatedMultipleTimes_ReturnsExactCount
Validates that the count of a repeated substring is accurately returned.  
**Purpose:** Ensures correct substring occurrence counting.  
**Parameters:** Implicitly tests with a string containing multiple instances of a substring.  
**Return Value:** Asserts the returned count matches the actual number of occurrences.  
**Throws:** Does not throw.

### CountOccurrences_SubstringNotPresent_ReturnsZero
Confirms that a count of zero is returned when a substring is not present.  
**Purpose:** Tests absence handling in substring counting.  
**Parameters:** Implicitly tests with a string that does not contain the target substring.  
**Return Value:** Asserts the output is zero.  
**Throws:** Does not throw.

### ToSlug_StringWithSpacesAndSpecialChars_ReturnsUrlFriendlySlug
Validates that strings with spaces and special characters are converted to URL-friendly slugs.  
**Purpose:** Ensures slug generation removes or replaces invalid characters.  
**Parameters:** Implicitly tests with a string containing spaces and special characters (e.g., "Hello World!").  
**Return Value:** Asserts the output is a lowercase, hyphen-separated slug (e.g., "hello-world").  
**Throws:** Does not throw.

### ToSlug_EmptyString_ReturnsEmptyString
Confirms that an empty string input returns an empty string.  
**Purpose:** Tests empty input handling in slug generation.  
**Parameters:** Implicitly tests with an empty string.  
**Return Value:** Asserts the output is an empty string.  
**Throws:** Does not throw.

### RemovePrefix_PrefixPresent_RemovesPrefix
Validates that a specified prefix is removed from the input string when present.  
**Purpose:** Ensures prefix removal logic.  
**Parameters:** Implicitly tests with a string that starts with the target prefix.  
**Return Value:** Asserts the output excludes the prefix.  
**Throws:** Does not throw.

### RemovePrefix_PrefixAbsent_ReturnsOriginalValue
Confirms that the original string is returned if the prefix is not present.  
**Purpose:** Tests absence handling in prefix removal.  
**Parameters:** Implicitly tests with a string that does not start with the target prefix.  
**Return Value:** Asserts the output equals the original input.  
**Throws:** Does not throw.

### RemoveSuffix_SuffixPresent_RemovesSuffix
Validates that a specified suffix is removed from the input string when present.  
**Purpose:** Ensures suffix removal logic.  
**Parameters:** Implicitly tests with a string ending with the target suffix.  
**Return Value:** Asserts the output excludes the suffix.  
**Throws:** Does not throw.

### NullIfEmpty_EmptyString_ReturnsNull
Confirms that an empty string input returns null.  
**Purpose:** Tests empty string handling in null conversion.  
**Parameters:** Implicitly tests with an empty string.  
**Return Value:** Asserts the output is null.  
**Throws:** Does not throw.

### NullIfEmpty_NonEmptyString_ReturnsSameValue
Ensures non-empty strings are returned unchanged.  
**Purpose:** Tests non-empty input handling in null conversion.  
**Parameters:** Implicitly tests with a non-empty string.  
**Return Value:** Asserts the output equals the original input.  
**Throws:** Does not throw.

### Repeat_PositiveCount_ConcatenatesStringNTimes
Validates that a string is repeated and concatenated N times for a positive count.  
**Purpose:** Ensures correct string repetition logic.  
**Parameters:** Implicitly tests with a positive integer count.  
**Return Value:** Asserts the output is the input string repeated N times.  
**Throws:** Does not throw.

### Repeat_ZeroCount_ReturnsEmptyString
Confirms that a zero count results in an empty string.  
**Purpose:** Tests zero count handling in repetition.  
**Parameters:** Implicitly tests with a count of zero.  
**Return Value:** Asserts the output is an empty string.  
**Throws:** Does not throw.

### SplitAndTrim_StringWithSpacesAroundDelimiters_ReturnsTrimmedParts
Validates that splitting a string by delimiters and trimming whitespace around parts produces clean results.  
**Purpose:** Ensures split and trim logic for strings with irregular spacing.  
**Parameters:** Implicitly tests with a string containing spaces around delimiters (e.g., " a , b , c ").  
**Return Value:** Asserts the output array contains trimmed, non-whitespace parts (e.g., ["a", "b", "c"]).  
**Throws:** Does not throw.

### Batch_CollectionOfTen_ProducesCorrectBatchCount
Confirms that batching a collection of ten items produces the expected number of batches based on a specified batch size.  
**Purpose:** Validates batch partitioning logic.  
**Parameters:** Implicitly tests with a collection of ten elements and a defined batch size.  
**Return Value:** Asserts the output contains the correct number of batches.  
**Throws:** Does not throw.

## Usage

```csharp
// Example 1: Converting a PascalCase string to snake_case
string input = "PascalCaseInput";
string result = input.ToSnakeCase();
// Result: "pascal_case_input"
```

```csharp
// Example 2: Generating a URL-friendly slug from a title
string title = "Hello World! Welcome to 2023";
string slug = title.ToSlug();
// Result: "hello-world-welcome-to-2023"
```

## Notes

- **Edge Cases:** Methods like `Repeat` may throw `ArgumentOutOfRangeException` if passed a negative count, though this is not tested here. Similarly, `Truncate` may behave unpredictably with negative or zero max lengths.  
- **Thread Safety:** All extension methods are stateless and do not modify shared resources, making them inherently thread-safe.  
- **Null Handling:** Methods such as `NullIfEmpty` explicitly handle empty strings but may not account for null inputs unless explicitly validated in the implementation.  
- **Batch Behavior:** The `Batch` method assumes the input collection is non-null and the batch size is positive; invalid inputs may cause runtime exceptions.
