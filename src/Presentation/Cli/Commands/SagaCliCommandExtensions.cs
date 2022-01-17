#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SagaOrchestrator.Presentation.Cli.Commands;

/// <summary>
/// Extension methods for <see cref="SagaCliCommand"/> that provide practical functionality
/// for working with saga CLI commands in a fluent and type-safe manner.
/// </summary>
public static class SagaCliCommandExtensions
{
    /// <summary>
    /// Gets the value of a command argument by key, or returns null if the argument doesn't exist.
    /// </summary>
    /// <param name="command">The saga command instance.</param>
    /// <param name="key">The argument key to retrieve.</param>
    /// <returns>The argument value if found; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
    public static string? GetArgument(this SagaCliCommand command, string key)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return command.Arguments.TryGetValue(key, out var value) ? value : null;
    }

    /// <summary>
    /// Gets the value of a command argument by key, or returns a default value if the argument doesn't exist.
    /// </summary>
    /// <param name="command">The saga command instance.</param>
    /// <param name="key">The argument key to retrieve.</param>
    /// <param name="defaultValue">The default value to return if the argument doesn't exist.</param>
    /// <returns>The argument value if found; otherwise, the default value.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
    public static string GetArgument(this SagaCliCommand command, string key, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrEmpty(key);

        return command.Arguments.TryGetValue(key, out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Gets the value of a command argument by key and parses it as an integer.
    /// </summary>
    /// <param name="command">The saga command instance.</param>
    /// <param name="key">The argument key to retrieve.</param>
    /// <returns>The parsed integer value if found and valid; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
    public static int? GetIntArgument(this SagaCliCommand command, string key)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (command.Arguments.TryGetValue(key, out var value) &&
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// Gets the value of a command argument by key and parses it as a boolean.
    /// Accepts values: "true", "false", "1", "0", "yes", "no" (case-insensitive).
    /// </summary>
    /// <param name="command">The saga command instance.</param>
    /// <param name="key">The argument key to retrieve.</param>
    /// <returns>The parsed boolean value if found and valid; otherwise, null.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
    public static bool? GetBooleanArgument(this SagaCliCommand command, string key)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (command.Arguments.TryGetValue(key, out var value))
        {
            var normalized = value.Trim().ToLowerInvariant();
            return normalized switch
            {
                "true" or "1" or "yes" => true,
                "false" or "0" or "no" => false,
                _ => null
            };
        }

        return null;
    }

    /// <summary>
    /// Checks if the command has a specific option.
    /// </summary>
    /// <param name="command">The saga command instance.</param>
    /// <param name="optionName">The option name to check (without leading dashes).</param>
    /// <returns>True if the option exists; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
    public static bool HasOption(this SagaCliCommand command, string optionName)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrEmpty(optionName);

        return command.Options.Contains(optionName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets all command arguments as a read-only collection.
    /// </summary>
    /// <param name="command">The saga command instance.</param>
    /// <returns>A read-only collection of all command arguments.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
    public static IReadOnlyDictionary<string, string> GetArguments(this SagaCliCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return command.Arguments.AsReadOnly();
    }

    /// <summary>
    /// Gets all command options as a read-only collection.
    /// </summary>
    /// <param name="command">The saga command instance.</param>
    /// <returns>A read-only collection of all command options.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
    public static IReadOnlyList<string> GetOptions(this SagaCliCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        return command.Options.AsReadOnly();
    }

    /// <summary>
    /// Validates that required arguments are present for the command type.
    /// Adds validation errors to the command's ValidationErrors collection if requirements aren't met.
    /// </summary>
    /// <param name="command">The saga command instance.</param>
    /// <returns>True if validation passes; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
    public static bool ValidateRequiredArguments(this SagaCliCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        return command.CommandType switch
        {
            "create" => ValidateCreateArguments(command),
            "execute" => ValidateExecuteArguments(command),
            "status" => ValidateStatusArguments(command),
            "compensate" => ValidateCompensateArguments(command),
            _ => true
        };
    }

    /// <summary>
    /// Gets a formatted string representation of the command for logging purposes.
    /// </summary>
    /// <param name="command">The saga command instance.</param>
    /// <returns>A formatted string representation of the command.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="command"/> is null.</exception>
    public static string ToLogString(this SagaCliCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var args = command.Arguments.Count > 0
            ? string.Join(", ", command.Arguments.Select(kvp => $"{kvp.Key}={kvp.Value}"))
            : "none";

        var options = command.Options.Count > 0
            ? string.Join(", ", command.Options)
            : "none";

        return $"Command: {command.CommandType}, Args: [{args}], Options: [{options}], Valid: {command.IsValid}";
    }

    private static bool ValidateCreateArguments(SagaCliCommand command)
    {
        var isValid = true;

        if (!command.Arguments.ContainsKey("definition"))
        {
            command.ValidationErrors.Add("--definition parameter is required for create command");
            isValid = false;
        }

        return isValid;
    }

    private static bool ValidateExecuteArguments(SagaCliCommand command)
    {
        var isValid = true;

        if (!command.Arguments.ContainsKey("saga-id"))
        {
            command.ValidationErrors.Add("--saga-id parameter is required for execute command");
            isValid = false;
        }

        return isValid;
    }

    private static bool ValidateStatusArguments(SagaCliCommand command)
    {
        var isValid = true;

        if (!command.Arguments.ContainsKey("saga-id"))
        {
            command.ValidationErrors.Add("--saga-id parameter is required for status command");
            isValid = false;
        }

        return isValid;
    }

    private static bool ValidateCompensateArguments(SagaCliCommand command)
    {
        var isValid = true;

        if (!command.Arguments.ContainsKey("saga-id"))
        {
            command.ValidationErrors.Add("--saga-id parameter is required for compensate command");
            isValid = false;
        }

        return isValid;
    }
}