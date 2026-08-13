#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Presentation.Cli.Commands;

/// <summary>
/// Represents a CLI command for saga operations with full argument parsing.
/// Supports create, execute, status, list, and compensate commands with validation.
/// </summary>
public class SagaCliCommand
{
    /// <summary>Gets or sets the verb of the command (create, execute, status, list, compensate, help).</summary>
    public string CommandType { get; set; } = string.Empty;

    /// <summary>Gets or sets the parsed <c>--key=value</c> named arguments.</summary>
    public Dictionary<string, string> Arguments { get; set; } = new();

    /// <summary>Gets or sets the parsed flag-style options (<c>--flag</c> or <c>-f</c>).</summary>
    public List<string> Options { get; set; } = new();

    /// <summary>Gets a value indicating whether the command passed validation for its verb.</summary>
    public bool IsValid { get; private set; }

    /// <summary>Gets or sets the validation errors accumulated while parsing the command.</summary>
    public List<string> ValidationErrors { get; set; } = new();

    /// <summary>
    /// Parses raw CLI arguments into a structured, validated command.
    /// </summary>
    /// <param name="args">The raw argument array, where <c>args[0]</c> is the command verb.</param>
    /// <returns>A parsed command; inspect <see cref="IsValid"/> and <see cref="ValidationErrors"/> before use.</returns>
    public static SagaCliCommand Parse(string[] args)
    {
        var command = new SagaCliCommand();

        if (args.Length == 0)
        {
            command.ValidationErrors.Add("No command specified");
            return command;
        }

        command.CommandType = args[0].ToLowerInvariant();

        // Parse named arguments and options
        for (int i = 1; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("--"))
            {
                var parts = arg.Substring(2).Split('=');
                if (parts.Length == 2)
                {
                    command.Arguments[parts[0]] = parts[1];
                }
                else
                {
                    command.Options.Add(parts[0]);
                }
            }
            else if (arg.StartsWith("-"))
            {
                command.Options.Add(arg.Substring(1));
            }
        }

        // Validate command
        command.IsValid = command.ValidateCommand();
        return command;
    }

    private bool ValidateCommand()
    {
        return CommandType switch
        {
            "create" => ValidateCreateCommand(),
            "execute" => ValidateExecuteCommand(),
            "status" => ValidateStatusCommand(),
            "list" => true,
            "compensate" => ValidateCompensateCommand(),
            "help" => true,
            _ => false
        };
    }

    private bool ValidateCreateCommand()
    {
        if (!Arguments.ContainsKey("definition"))
        {
            ValidationErrors.Add("--definition parameter required for create command");
            return false;
        }
        return true;
    }

    private bool ValidateExecuteCommand()
    {
        if (!Arguments.ContainsKey("saga-id"))
        {
            ValidationErrors.Add("--saga-id parameter required for execute command");
            return false;
        }
        return true;
    }

    private bool ValidateStatusCommand()
    {
        if (!Arguments.ContainsKey("saga-id"))
        {
            ValidationErrors.Add("--saga-id parameter required for status command");
            return false;
        }
        return true;
    }

    private bool ValidateCompensateCommand()
    {
        if (!Arguments.ContainsKey("saga-id"))
        {
            ValidationErrors.Add("--saga-id parameter required for compensate command");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Returns a string representation of the command.
    /// </summary>
    public override string ToString()
    {
        var argsFormatted = "";
        foreach (var kvp in Arguments)
        {
            if (argsFormatted.Length > 0) argsFormatted += ", ";
            argsFormatted += kvp.Key + "=" + kvp.Value;
        }
        var optionsFormatted = string.Join(", ", Options);
        var errorsFormatted = string.Join(", ", ValidationErrors);

        return $"SagaCliCommand {{ CommandType = {CommandType}, Arguments = [{argsFormatted}], Options = [{optionsFormatted}], ValidationErrors = [{errorsFormatted}] }}";
    }

    /// <summary>
    /// Returns the human-readable usage/help text describing every supported command and option.
    /// </summary>
    /// <returns>The formatted help text.</returns>
    public string GetHelpText()
    {
        return @"
Saga Orchestrator CLI - Distributed Saga Management

USAGE:
  saga <command> [options]

COMMANDS:
  create       Create a new saga from a definition
               --definition <name>    Definition identifier
               --data <json>         Optional saga data

  execute      Execute the next step in a saga
               --saga-id <id>        Saga identifier
               --async               Execute asynchronously

  status       Get current saga status
               --saga-id <id>        Saga identifier
               --verbose             Show detailed step information

  list         List all sagas
               --limit <number>      Maximum results (default: 50)
               --filter <status>     Filter by status

  compensate   Initiate compensation for a saga
               --saga-id <id>        Saga identifier
               --strategy <type>     Compensation strategy

  help         Display this help message

EXAMPLES:
  saga create --definition OrderProcessing
  saga execute --saga-id ddf3c1ec-5b23-4905-b78c-ecc897d15443
  saga status --saga-id ddf3c1ec-5b23-4905-b78c-ecc897d15443 --verbose
  saga list --filter Completed
  saga compensate --saga-id ddf3c1ec-5b23-4905-b78c-ecc897d15443 --strategy Reverse
";
    }
}
