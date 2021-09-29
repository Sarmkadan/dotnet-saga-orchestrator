#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Extensions;

/// <summary>
/// Extension methods for enum types.
/// Provides conversion, description, and validation utilities for enums.
/// </summary>
public static class EnumExtensions
{
    // Get enum description from DescriptionAttribute
    public static string GetDescription<T>(this T value) where T : Enum
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null) return value.ToString();

        var attr = (System.ComponentModel.DescriptionAttribute?)
            System.Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute));

        return attr?.Description ?? value.ToString();
    }

    // Get enum display name
    public static string GetDisplayName<T>(this T value) where T : Enum
    {
        var displayAttr = value.GetType()
            .GetField(value.ToString())?
            .GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
            .FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;

        return displayAttr?.GetName() ?? value.ToString();
    }

    // Parse enum from string (case-insensitive)
    public static T? ParseEnum<T>(this string value) where T : Enum
    {
        try
        {
            return (T?)Enum.Parse(typeof(T), value, ignoreCase: true);
        }
        catch
        {
            return null;
        }
    }

    // Get all enum values
    public static IEnumerable<T> GetValues<T>() where T : Enum =>
        Enum.GetValues(typeof(T)).Cast<T>();

    // Get all enum names
    public static IEnumerable<string> GetNames<T>() where T : Enum =>
        Enum.GetNames(typeof(T));

    // Get enum members with descriptions as key-value pairs
    public static Dictionary<T, string> GetEnumDictionary<T>() where T : Enum
    {
        var result = new Dictionary<T, string>();
        foreach (var value in GetValues<T>())
        {
            result[value] = value.GetDescription();
        }
        return result;
    }

    // Check if value is defined in enum
    public static bool IsDefined<T>(this T value) where T : Enum =>
        Enum.IsDefined(typeof(T), value);

    // Get next enum value (circular)
    public static T GetNext<T>(this T value) where T : Enum
    {
        var values = GetValues<T>().ToList();
        var currentIndex = values.IndexOf(value);
        return values[(currentIndex + 1) % values.Count];
    }

    // Get previous enum value (circular)
    public static T GetPrevious<T>(this T value) where T : Enum
    {
        var values = GetValues<T>().ToList();
        var currentIndex = values.IndexOf(value);
        return values[currentIndex == 0 ? values.Count - 1 : currentIndex - 1];
    }

    // Convert enum to numeric value
    public static object GetNumericValue<T>(this T value) where T : Enum =>
        Convert.ChangeType(value, Enum.GetUnderlyingType(typeof(T)))
            ?? throw new InvalidOperationException("Cannot convert enum to numeric value");

    // HasFlag safe operation
    public static bool HasAnyFlag<T>(this T value, T flags) where T : Enum =>
        value.GetNumericValue() is int intVal &&
        flags.GetNumericValue() is int intFlags &&
        (intVal & intFlags) != 0;

    // Get enum from numeric value
    public static T? GetEnumFromValue<T>(object value) where T : Enum
    {
        try
        {
            return (T?)Enum.ToObject(typeof(T), value);
        }
        catch
        {
            return null;
        }
    }

    // Format enum with custom separator
    public static string FormatFlags<T>(this T value, string separator = ", ") where T : Enum
    {
        if (!typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Any())
            return value.ToString();

        var names = new List<string>();
        foreach (var enumValue in GetValues<T>())
        {
            if (value.HasAnyFlag(enumValue))
                names.Add(enumValue.ToString());
        }

        return string.Join(separator, names);
    }

    // Is enum a flags enum
    public static bool IsFlagsEnum<T>() where T : Enum =>
        typeof(T).GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;

    // Get enum range (min to max)
    public static (T min, T max) GetRange<T>() where T : Enum
    {
        var values = GetValues<T>().ToList();
        return (values.First(), values.Last());
    }
}
