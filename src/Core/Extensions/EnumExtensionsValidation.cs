using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Validation extension methods for enum types.
/// Provides validation utilities to ensure enum values are valid and within expected ranges.
/// </summary>
public static class EnumExtensionsValidation
{
    /// <summary>
    /// Validates an enum value and returns a list of human-readable problems.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to validate.</param>
    /// <returns>A read-only list of validation problems; empty if the value is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Check if the value is defined in the enum type
        if (!Enum.IsDefined(typeof(T), value))
        {
            problems.Add($"Enum value '{value}' is not defined in type '{typeof(T).Name}'.");
        }

        // Skip default value check for flags enums as they can have zero value
        if (!typeof(T).IsDefined(typeof(FlagsAttribute), false))
        {
            // Check for default enum value (common issue for uninitialized enums)
            var underlyingTypeForDefaultCheck = Enum.GetUnderlyingType(typeof(T));
            var defaultValue = Activator.CreateInstance(underlyingTypeForDefaultCheck);
            var numericValue = Convert.ChangeType(value, underlyingTypeForDefaultCheck, CultureInfo.InvariantCulture);

            if (object.Equals(numericValue, defaultValue))
            {
                problems.Add($"Enum value '{value}' is the default value for type '{typeof(T).Name}'.");
            }
        }

        // For numeric enums, check if the value is within a reasonable range
        var underlyingType = Enum.GetUnderlyingType(typeof(T));
        if (underlyingType == typeof(int) || underlyingType == typeof(long) ||
            underlyingType == typeof(short) || underlyingType == typeof(byte))
        {
            var values = EnumExtensions.GetValues<T>().ToList();
            if (values.Count > 0)
            {
                var minValue = values.Min(v => (long)Convert.ChangeType(v, typeof(long), CultureInfo.InvariantCulture));
                var maxValue = values.Max(v => (long)Convert.ChangeType(v, typeof(long), CultureInfo.InvariantCulture));
                var currentValue = (long)Convert.ChangeType(value, typeof(long), CultureInfo.InvariantCulture);

                if (currentValue < minValue || currentValue > maxValue)
                {
                    problems.Add($"Enum value '{value}' ({currentValue}) is outside the valid range [{minValue}, {maxValue}] for type '{typeof(T).Name}'.");
                }
            }
        }

        // Check if the enum value has a Description attribute
        var field = typeof(T).GetField(value.ToString());
        var descriptionAttr = field is not null
            ? (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
            : null;

        if (descriptionAttr is null)
        {
            problems.Add($"Enum value '{value}' does not have a Description attribute.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an enum value is valid.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to check.</param>
    /// <returns>True if the enum value is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid<T>(this T value) where T : Enum
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that an enum value is valid, throwing an <see cref="ArgumentException"/> if it is not.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to validate.</param>
    /// <returns>The validated enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the enum value is not valid.</exception>
    public static T EnsureValid<T>(this T value) where T : Enum
    {
        var problems = value.Validate();

        return problems.Count == 0
            ? value
            : throw new ArgumentException(
                $"Enum value '{value}' of type '{typeof(T).Name}' is not valid. Problems:{Environment.NewLine} - " +
                string.Join($"{Environment.NewLine} - ", problems) +
                $"{Environment.NewLine}Consider using EnumExtensions.ParseEnum<T> or ensuring the value comes from a valid source.");
    }
}
