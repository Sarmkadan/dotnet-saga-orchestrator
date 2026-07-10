#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace SagaOrchestrator.Presentation.Cli.Commands;

/// <summary>
/// Provides validation helpers for <see cref="SagaCliCommand"/> instances.
/// Validates command structure, required parameters, and value formats.
/// </summary>
public static class SagaCliCommandValidation
{
    /// <summary>
    /// Validates a <see cref="SagaCliCommand"/> and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The command to validate.</param>
    /// <returns>A read-only list of validation error messages. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaCliCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate CommandType
        if (string.IsNullOrWhiteSpace(value.CommandType))
        {
            errors.Add("CommandType cannot be null or whitespace.");
        }
        else if (value.CommandType is not ("create" or "execute" or "status" or "list" or "compensate" or "help"))
        {
            errors.Add($"Invalid CommandType '{value.CommandType}'. Must be one of: create, execute, status, list, compensate, help.");
        }

        // Validate Arguments dictionary
        if (value.Arguments is null)
        {
            errors.Add("Arguments dictionary cannot be null.");
        }

        // Validate Options list
        if (value.Options is null)
        {
            errors.Add("Options list cannot be null.");
        }

        // Command-specific validation
        if (value.CommandType == "create")
        {
            ValidateCreateCommand(value, errors);
        }
        else if (value.CommandType == "execute")
        {
            ValidateExecuteCommand(value, errors);
        }
        else if (value.CommandType == "status")
        {
            ValidateStatusCommand(value, errors);
        }
        else if (value.CommandType == "compensate")
        {
            ValidateCompensateCommand(value, errors);
        }
        else if (value.CommandType == "list")
        {
            ValidateListCommand(value, errors);
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SagaCliCommand"/> is valid.
    /// </summary>
    /// <param name="value">The command to check.</param>
    /// <returns>True if the command is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static bool IsValid(this SagaCliCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SagaCliCommand"/> is valid.
    /// </summary>
    /// <param name="value">The command to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the command is invalid, containing a list of validation errors.</exception>
    public static void EnsureValid(this SagaCliCommand value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SagaCliCommand is invalid. Validation errors: {string.Join(" ", errors)}",
                nameof(value));
        }
    }

    private static void ValidateCreateCommand(SagaCliCommand command, List<string> errors)
    {
        if (command.Arguments is null)
        {
            errors.Add("Arguments cannot be null for create command.");
            return;
        }

        if (!command.Arguments.ContainsKey("definition"))
        {
            errors.Add("--definition parameter is required for create command.");
        }
        else if (string.IsNullOrWhiteSpace(command.Arguments["definition"]))
        {
            errors.Add("--definition parameter cannot be null or whitespace.");
        }

        // Validate optional --data parameter if present
        if (command.Arguments.TryGetValue("data", out var data) && !string.IsNullOrWhiteSpace(data))
        {
            try
            {
                // Basic JSON validation - just check if it's not empty and can be parsed as JSON
                if (data.Trim().Length == 0)
                {
                    errors.Add("--data parameter cannot be empty if provided.");
                }
                else if (!IsValidJson(data))
                {
                    errors.Add("--data parameter must contain valid JSON.");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"--data parameter validation failed: {ex.Message}");
            }
        }
    }

    private static void ValidateExecuteCommand(SagaCliCommand command, List<string> errors)
    {
        if (command.Arguments is null)
        {
            errors.Add("Arguments cannot be null for execute command.");
            return;
        }

        if (!command.Arguments.ContainsKey("saga-id"))
        {
            errors.Add("--saga-id parameter is required for execute command.");
        }
        else if (string.IsNullOrWhiteSpace(command.Arguments["saga-id"]))
        {
            errors.Add("--saga-id parameter cannot be null or whitespace.");
        }
        else if (!IsValidGuid(command.Arguments["saga-id"]))
        {
            errors.Add("--saga-id parameter must be a valid GUID.");
        }

        // Validate optional --async parameter
        if (command.Options is not null && command.Options.Contains("async"))
        {
            // No additional validation needed for boolean flag
        }
    }

    private static void ValidateStatusCommand(SagaCliCommand command, List<string> errors)
    {
        if (command.Arguments is null)
        {
            errors.Add("Arguments cannot be null for status command.");
            return;
        }

        if (!command.Arguments.ContainsKey("saga-id"))
        {
            errors.Add("--saga-id parameter is required for status command.");
        }
        else if (string.IsNullOrWhiteSpace(command.Arguments["saga-id"]))
        {
            errors.Add("--saga-id parameter cannot be null or whitespace.");
        }
        else if (!IsValidGuid(command.Arguments["saga-id"]))
        {
            errors.Add("--saga-id parameter must be a valid GUID.");
        }

        // Validate optional --verbose parameter
        if (command.Options is not null && command.Options.Contains("verbose"))
        {
            // No additional validation needed for boolean flag
        }
    }

    private static void ValidateCompensateCommand(SagaCliCommand command, List<string> errors)
    {
        if (command.Arguments is null)
        {
            errors.Add("Arguments cannot be null for compensate command.");
            return;
        }

        if (!command.Arguments.ContainsKey("saga-id"))
        {
            errors.Add("--saga-id parameter is required for compensate command.");
        }
        else if (string.IsNullOrWhiteSpace(command.Arguments["saga-id"]))
        {
            errors.Add("--saga-id parameter cannot be null or whitespace.");
        }
        else if (!IsValidGuid(command.Arguments["saga-id"]))
        {
            errors.Add("--saga-id parameter must be a valid GUID.");
        }

        // Validate optional --strategy parameter if present
        if (command.Arguments.TryGetValue("strategy", out var strategy) && !string.IsNullOrWhiteSpace(strategy))
        {
            var validStrategies = new[] { "reverse", "forward", "compensate", "none" };
            if (!validStrategies.Contains(strategy.ToLowerInvariant()))
            {
                errors.Add($"--strategy parameter must be one of: {string.Join(", ", validStrategies)}.");
            }
        }
    }

    private static void ValidateListCommand(SagaCliCommand command, List<string> errors)
    {
        // --limit parameter validation
        if (command.Arguments?.TryGetValue("limit", out var limitStr) == true && !string.IsNullOrWhiteSpace(limitStr))
        {
            if (!int.TryParse(limitStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var limit) || limit <= 0)
            {
                errors.Add("--limit parameter must be a positive integer.");
            }
        }

        // --filter parameter validation
        if (command.Arguments?.TryGetValue("filter", out var filter) == true && !string.IsNullOrWhiteSpace(filter))
        {
            var validFilters = new[] { "pending", "inprogress", "completed", "failed", "compensating", "compensated" };
            if (!validFilters.Contains(filter.ToLowerInvariant()))
            {
                errors.Add($"--filter parameter must be one of: {string.Join(", ", validFilters)}.");
            }
        }
    }

    private static bool IsValidGuid(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            _ = Guid.Parse(input.Trim());
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        // Basic JSON validation - check if it starts with { or [ and ends with } or ]
        var trimmed = json.Trim();
        if ((trimmed.StartsWith('{') && trimmed.EndsWith('}')) ||
            (trimmed.StartsWith('[') && trimmed.EndsWith(']'))
        )
        {
            return true;
        }

        return false;
    }
}