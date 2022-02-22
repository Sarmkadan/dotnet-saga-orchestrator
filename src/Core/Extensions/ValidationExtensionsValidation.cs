#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================================================

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Validation helpers for <see cref="ValidationExtensions"/> extension methods.
/// Provides comprehensive validation for validation operations used throughout the orchestrator.
/// </summary>
public static class ValidationExtensionsValidation
{
    /// <summary>
    /// Validates ValidationExtensions extension method behavior and returns a list of human-readable problems.
    /// </summary>
    /// <returns>A read-only list of validation problems, or empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var problems = new List<string>();

        // Validate NotNull<T> with non-null value
        try
        {
            var notNullResult = ValidationExtensions.NotNull(new object(), nameof(ValidationExtensions.NotNull));
            if (notNullResult is null)
            {
                problems.Add("NotNull<T> method is not working correctly - should return the validated object");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"NotNull<T> method threw exception: {ex.Message}");
        }

        // Validate NotNull<T> with null value
        try
        {
            ValidationExtensions.NotNull<object>(null, nameof(ValidationExtensions.NotNull));
            problems.Add("NotNull<T> method is not working correctly - should throw ArgumentNullException for null value");
        }
        catch (ArgumentNullException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"NotNull<T> method threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate NotNullOrEmpty with valid string
        try
        {
            var notNullOrEmptyResult = ValidationExtensions.NotNullOrEmpty("test", nameof(ValidationExtensions.NotNullOrEmpty));
            if (notNullOrEmptyResult != "test")
            {
                problems.Add("NotNullOrEmpty method is not working correctly - should return the validated string");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"NotNullOrEmpty method threw exception: {ex.Message}");
        }

        // Validate NotNullOrEmpty with null string
        try
        {
            ValidationExtensions.NotNullOrEmpty(null, nameof(ValidationExtensions.NotNullOrEmpty));
            problems.Add("NotNullOrEmpty method is not working correctly - should throw ArgumentNullException for null string");
        }
        catch (ArgumentNullException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"NotNullOrEmpty method threw wrong exception type for null: {ex.GetType().Name}");
        }

        // Validate NotNullOrEmpty with empty string
        try
        {
            ValidationExtensions.NotNullOrEmpty(string.Empty, nameof(ValidationExtensions.NotNullOrEmpty));
            problems.Add("NotNullOrEmpty method is not working correctly - should throw ArgumentException for empty string");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"NotNullOrEmpty method threw wrong exception type for empty: {ex.GetType().Name}");
        }

        // Validate NotNullOrWhiteSpace with valid string
        try
        {
            var notNullOrWhiteSpaceResult = ValidationExtensions.NotNullOrWhiteSpace("test", nameof(ValidationExtensions.NotNullOrWhiteSpace));
            if (notNullOrWhiteSpaceResult != "test")
            {
                problems.Add("NotNullOrWhiteSpace method is not working correctly - should return the validated string");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"NotNullOrWhiteSpace method threw exception: {ex.Message}");
        }

        // Validate NotNullOrWhiteSpace with whitespace string
        try
        {
            ValidationExtensions.NotNullOrWhiteSpace("   ", nameof(ValidationExtensions.NotNullOrWhiteSpace));
            problems.Add("NotNullOrWhiteSpace method is not working correctly - should throw ArgumentException for whitespace string");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"NotNullOrWhiteSpace method threw wrong exception type for whitespace: {ex.GetType().Name}");
        }

        // Validate InRange for int with valid value
        try
        {
            var inRangeResult = ValidationExtensions.InRange(5, 1, 10, nameof(ValidationExtensions.InRange));
            if (inRangeResult != 5)
            {
                problems.Add("InRange method for int is not working correctly - should return the validated value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"InRange method for int threw exception: {ex.Message}");
        }

        // Validate InRange for int with out of range value
        try
        {
            ValidationExtensions.InRange(0, 1, 10, nameof(ValidationExtensions.InRange));
            problems.Add("InRange method for int is not working correctly - should throw ArgumentOutOfRangeException for out of range value");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"InRange method for int threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate GreaterThan for int with valid value
        try
        {
            var greaterThanResult = ValidationExtensions.GreaterThan(5, 3, nameof(ValidationExtensions.GreaterThan));
            if (greaterThanResult != 5)
            {
                problems.Add("GreaterThan method for int is not working correctly - should return the validated value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GreaterThan method for int threw exception: {ex.Message}");
        }

        // Validate GreaterThan for int with invalid value
        try
        {
            ValidationExtensions.GreaterThan(3, 5, nameof(ValidationExtensions.GreaterThan));
            problems.Add("GreaterThan method for int is not working correctly - should throw ArgumentOutOfRangeException for value not greater than min");
        }
        catch (ArgumentOutOfRangeException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"GreaterThan method for int threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate GreaterThanOrEqual for int with valid value
        try
        {
            var greaterThanOrEqualResult = ValidationExtensions.GreaterThanOrEqual(5, 5, nameof(ValidationExtensions.GreaterThanOrEqual));
            if (greaterThanOrEqualResult != 5)
            {
                problems.Add("GreaterThanOrEqual method for int is not working correctly - should return the validated value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GreaterThanOrEqual method for int threw exception: {ex.Message}");
        }

        // Validate GreaterThan for long with valid value
        try
        {
            var greaterThanLongResult = ValidationExtensions.GreaterThan(5L, 3L, nameof(ValidationExtensions.GreaterThan));
            if (greaterThanLongResult != 5L)
            {
                problems.Add("GreaterThan method for long is not working correctly - should return the validated value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GreaterThan method for long threw exception: {ex.Message}");
        }

        // Validate GreaterThanZero for TimeSpan with valid value
        try
        {
            var greaterThanZeroResult = ValidationExtensions.GreaterThanZero(TimeSpan.FromSeconds(1), nameof(ValidationExtensions.GreaterThanZero));
            if (greaterThanZeroResult != TimeSpan.FromSeconds(1))
            {
                problems.Add("GreaterThanZero method for TimeSpan is not working correctly - should return the validated value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"GreaterThanZero method for TimeSpan threw exception: {ex.Message}");
        }

        // Validate GreaterThanZero for TimeSpan with zero value
        try
        {
            ValidationExtensions.GreaterThanZero(TimeSpan.Zero, nameof(ValidationExtensions.GreaterThanZero));
            problems.Add("GreaterThanZero method for TimeSpan is not working correctly - should throw ArgumentException for zero TimeSpan");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"GreaterThanZero method for TimeSpan threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate InRange for decimal with valid value
        try
        {
            var decimalInRangeResult = ValidationExtensions.InRange(5.5m, 1.0m, 10.0m, nameof(ValidationExtensions.InRange));
            if (decimalInRangeResult != 5.5m)
            {
                problems.Add("InRange method for decimal is not working correctly - should return the validated value");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"InRange method for decimal threw exception: {ex.Message}");
        }

        // Validate NotEmpty for IEnumerable<T> with valid collection
        try
        {
            var notEmptyResult = ValidationExtensions.NotEmpty(new[] { 1, 2, 3 }, nameof(ValidationExtensions.NotEmpty));
            if (notEmptyResult is null)
            {
                problems.Add("NotEmpty method for IEnumerable<T> is not working correctly - should return the validated collection");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"NotEmpty method for IEnumerable<T> threw exception: {ex.Message}");
        }

        // Validate NotEmpty for IEnumerable<T> with empty collection
        try
        {
            ValidationExtensions.NotEmpty(Array.Empty<int>(), nameof(ValidationExtensions.NotEmpty));
            problems.Add("NotEmpty method for IEnumerable<T> is not working correctly - should throw ArgumentException for empty collection");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"NotEmpty method for IEnumerable<T> threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate NotEmpty for Guid with valid Guid
        try
        {
            var notEmptyGuidResult = ValidationExtensions.NotEmpty(Guid.NewGuid(), nameof(ValidationExtensions.NotEmpty));
            if (notEmptyGuidResult == Guid.Empty)
            {
                problems.Add("NotEmpty method for Guid is not working correctly - should return the validated Guid");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"NotEmpty method for Guid threw exception: {ex.Message}");
        }

        // Validate NotEmpty for Guid with empty Guid
        try
        {
            ValidationExtensions.NotEmpty(Guid.Empty, nameof(ValidationExtensions.NotEmpty));
            problems.Add("NotEmpty method for Guid is not working correctly - should throw ArgumentException for empty Guid");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"NotEmpty method for Guid threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate ValidateIf with valid value
        try
        {
            var validateIfResult = ValidationExtensions.ValidateIf(5, x => x > 0, "Value must be positive");
            if (validateIfResult != 5)
            {
                problems.Add("ValidateIf method is not working correctly - should return the validated value when validation passes");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateIf method threw exception: {ex.Message}");
        }

        // Validate ValidateIf with invalid value
        try
        {
            ValidationExtensions.ValidateIf(0, x => x > 0, "Value must be positive");
            problems.Add("ValidateIf method is not working correctly - should throw ArgumentException for invalid value");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateIf method threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate ValidateEmail with valid email
        try
        {
            var validateEmailResult = ValidationExtensions.ValidateEmail("test@example.com", nameof(ValidationExtensions.ValidateEmail));
            if (validateEmailResult != "test@example.com")
            {
                problems.Add("ValidateEmail method is not working correctly - should return the validated email");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateEmail method threw exception: {ex.Message}");
        }

        // Validate ValidateUrl with valid URL
        try
        {
            var validateUrlResult = ValidationExtensions.ValidateUrl("https://example.com", nameof(ValidationExtensions.ValidateUrl));
            if (validateUrlResult != "https://example.com")
            {
                problems.Add("ValidateUrl method is not working correctly - should return the validated URL");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"ValidateUrl method threw exception: {ex.Message}");
        }

        // Validate MaxLength with valid string
        try
        {
            var maxLengthResult = ValidationExtensions.MaxLength("test", 10, nameof(ValidationExtensions.MaxLength));
            if (maxLengthResult != "test")
            {
                problems.Add("MaxLength method is not working correctly - should return the validated string");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"MaxLength method threw exception: {ex.Message}");
        }

        // Validate MaxLength with too long string
        try
        {
            ValidationExtensions.MaxLength(new string('a', 11), 10, nameof(ValidationExtensions.MaxLength));
            problems.Add("MaxLength method is not working correctly - should throw ArgumentException for string exceeding max length");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"MaxLength method threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate MinLength with valid string
        try
        {
            var minLengthResult = ValidationExtensions.MinLength("test", 3, nameof(ValidationExtensions.MinLength));
            if (minLengthResult != "test")
            {
                problems.Add("MinLength method is not working correctly - should return the validated string");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"MinLength method threw exception: {ex.Message}");
        }

        // Validate MinLength with too short string
        try
        {
            ValidationExtensions.MinLength("ab", 3, nameof(ValidationExtensions.MinLength));
            problems.Add("MinLength method is not working correctly - should throw ArgumentException for string below min length");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"MinLength method threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate NotEmptyArray with valid array
        try
        {
            var notEmptyArrayResult = ValidationExtensions.NotEmptyArray(new[] { 1, 2, 3 }, nameof(ValidationExtensions.NotEmptyArray));
            if (notEmptyArrayResult is null)
            {
                problems.Add("NotEmptyArray method is not working correctly - should return the validated array");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"NotEmptyArray method threw exception: {ex.Message}");
        }

        // Validate NotEmptyArray with empty array
        try
        {
            ValidationExtensions.NotEmptyArray(Array.Empty<int>(), nameof(ValidationExtensions.NotEmptyArray));
            problems.Add("NotEmptyArray method is not working correctly - should throw ArgumentException for empty array");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"NotEmptyArray method threw wrong exception type: {ex.GetType().Name}");
        }

        // Validate NotEmptyDictionary with valid dictionary
        try
        {
            var notEmptyDictResult = ValidationExtensions.NotEmptyDictionary(new Dictionary<int, string> { { 1, "one" } }, nameof(ValidationExtensions.NotEmptyDictionary));
            if (notEmptyDictResult is null)
            {
                problems.Add("NotEmptyDictionary method is not working correctly - should return the validated dictionary");
            }
        }
        catch (Exception ex)
        {
            problems.Add($"NotEmptyDictionary method threw exception: {ex.Message}");
        }

        // Validate NotEmptyDictionary with empty dictionary
        try
        {
            ValidationExtensions.NotEmptyDictionary(new Dictionary<int, string>(), nameof(ValidationExtensions.NotEmptyDictionary));
            problems.Add("NotEmptyDictionary method is not working correctly - should throw ArgumentException for empty dictionary");
        }
        catch (ArgumentException)
        {
            // Expected behavior
        }
        catch (Exception ex)
        {
            problems.Add($"NotEmptyDictionary method threw wrong exception type: {ex.GetType().Name}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the ValidationExtensions extension methods are working correctly.
    /// </summary>
    /// <returns><c>true</c> if the extension methods are working correctly; otherwise, <c>false</c>.</returns>
    public static bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the ValidationExtensions extension methods are working correctly, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <exception cref="ArgumentException">Extension methods are not working correctly.</exception>
    public static void EnsureValid()
    {
        var problems = Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ValidationExtensions validation failed with {problems.Count} problem(s):{Environment.NewLine} - ".Replace("\n", "\n- ") +
                string.Join(Environment.NewLine + "- ", problems)
            );
        }
    }
}