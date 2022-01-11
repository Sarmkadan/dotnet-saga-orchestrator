using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Extension methods for enum types.
/// Provides conversion, description, and validation utilities for enums.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Gets the description from the <see cref="DescriptionAttribute"/> attached to an enum value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>The description if available; otherwise, the enum value's string representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string GetDescription<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var field = value.GetType().GetField(value.ToString());
        if (field is null)
            return value.ToString();

        var attr = (DescriptionAttribute?)Attribute.GetCustomAttribute(
            field, typeof(DescriptionAttribute));

        return attr?.Description ?? value.ToString();
    }

    /// <summary>
    /// Gets the display name from the <see cref="DisplayAttribute"/> attached to an enum value.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value.</param>
    /// <returns>The display name if available; otherwise, the enum value's string representation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string GetDisplayName<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var displayAttr = value.GetType()
            .GetField(value.ToString())?
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .FirstOrDefault() as DisplayAttribute;

        return displayAttr?.GetName() ?? value.ToString();
    }

    /// <summary>
    /// Parses a string into an enum value (case-insensitive).
    /// </summary>
    /// <typeparam name="T">The enum type to parse into.</typeparam>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed enum value, or null if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static T? ParseEnum<T>(this string value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            return (T?)Enum.Parse(typeof(T), value, ignoreCase: true);
        }
        catch (ArgumentException)
        {
            return default;
        }
    }

    /// <summary>
    /// Gets all defined values of the enum type.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>An enumerable of all enum values.</returns>
    /// <remarks>
    /// Consider using <see cref="GetValues{T}()"/> instead of <see cref="Enum.GetValues(Type)"/> for better AOT compatibility.
    /// </remarks>
    public static IEnumerable<T> GetValues<T>() where T : Enum =>
        Enum.GetValues(typeof(T)).Cast<T>();

    /// <summary>
    /// Gets all names of the enum type.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>An enumerable of all enum names.</returns>
    public static IEnumerable<string> GetNames<T>() where T : Enum =>
        Enum.GetNames(typeof(T));

    /// <summary>
    /// Creates a dictionary mapping enum values to their descriptions.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>A dictionary with enum values as keys and descriptions as values.</returns>
    public static Dictionary<T, string> GetEnumDictionary<T>() where T : Enum
    {
        var result = new Dictionary<T, string>();
        foreach (var value in GetValues<T>())
        {
            result[value] = value.GetDescription();
        }
        return result;
    }

    /// <summary>
    /// Determines whether the enum value is defined in the enum type.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to check.</param>
    /// <returns>True if the value is defined; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsDefined<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        return Enum.IsDefined(typeof(T), value);
    }

    /// <summary>
    /// Gets the next enum value in a circular manner.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The current enum value.</param>
    /// <returns>The next enum value, wrapping around to the first value if at the end.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the enum has no values.</exception>
    public static T GetNext<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var values = GetValues<T>().ToList();
        if (values.Count == 0)
            throw new InvalidOperationException("Enum type has no defined values.");

        var currentIndex = values.IndexOf(value);
        return values[(currentIndex + 1) % values.Count];
    }

    /// <summary>
    /// Gets the previous enum value in a circular manner.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The current enum value.</param>
    /// <returns>The previous enum value, wrapping around to the last value if at the beginning.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the enum has no values.</exception>
    public static T GetPrevious<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        var values = GetValues<T>().ToList();
        if (values.Count == 0)
            throw new InvalidOperationException("Enum type has no defined values.");

        var currentIndex = values.IndexOf(value);
        return values[currentIndex == 0 ? values.Count - 1 : currentIndex - 1];
    }

    /// <summary>
    /// Converts the enum value to its underlying numeric representation.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to convert.</param>
    /// <returns>The numeric representation of the enum value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when conversion fails.</exception>
    public static object GetNumericValue<T>(this T value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            return Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T)));
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException)
        {
            throw new InvalidOperationException("Cannot convert enum to numeric value", ex);
        }
    }

    /// <summary>
    /// Determines whether the enum value has any of the specified flags set.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The enum value to check.</param>
    /// <param name="flags">The flags to check for.</param>
    /// <returns>True if any of the flags are set; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool HasAnyFlag<T>(this T value, T flags) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(flags);

        var valueNumeric = value.GetNumericValue();
        var flagsNumeric = flags.GetNumericValue();

        if (valueNumeric is not int intVal || flagsNumeric is not int intFlags)
            return false;

        return (intVal & intFlags) != 0;
    }

    /// <summary>
    /// Converts a numeric value to the corresponding enum value.
    /// </summary>
    /// <typeparam name="T">The enum type to convert to.</typeparam>
    /// <param name="value">The numeric value to convert.</param>
    /// <returns>The enum value, or null if conversion fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static T? GetEnumFromValue<T>(object value) where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        try
        {
            return (T?)Enum.ToObject(typeof(T), value);
        }
        catch (Exception ex) when (ex is ArgumentException or OverflowException)
        {
            return default;
        }
    }

    /// <summary>
    /// Formats a flags enum value with a custom separator.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="value">The flags enum value to format.</param>
    /// <param name="separator">The separator to use between flag names.</param>
    /// <returns>A formatted string of flag names, or the original string representation if not a flags enum.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string FormatFlags<T>(this T value, string separator = ", ") where T : Enum
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!typeof(T).IsDefined(typeof(FlagsAttribute), false))
            return value.ToString();

        var names = new List<string>();
        foreach (var enumValue in GetValues<T>())
        {
            if (value.HasAnyFlag(enumValue))
                names.Add(enumValue.ToString());
        }

        return string.Join(separator, names);
    }

    /// <summary>
    /// Determines whether the enum type is decorated with the <see cref="FlagsAttribute"/>.
    /// </summary>
    /// <typeparam name="T">The enum type to check.</typeparam>
    /// <returns>True if the enum is a flags enum; otherwise, false.</returns>
    public static bool IsFlagsEnum<T>() where T : Enum =>
        typeof(T).IsDefined(typeof(FlagsAttribute), false);

    /// <summary>
    /// Gets the minimum and maximum values of the enum type.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <returns>A tuple containing the minimum and maximum enum values.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the enum has no values.</exception>
    public static (T min, T max) GetRange<T>() where T : Enum
    {
        var values = GetValues<T>().ToList();
        if (values.Count == 0)
            throw new InvalidOperationException("Enum type has no defined values.");

        return (values.First(), values.Last());
    }
}