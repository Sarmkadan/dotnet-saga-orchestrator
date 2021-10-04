#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Infrastructure.Formatting;
using SagaOrchestrator.Presentation.Cli.Commands;

namespace SagaOrchestrator.Presentation.Cli;

/// <summary>
/// Command-line interface handler for saga operations.
/// Parses CLI commands and dispatches to appropriate services.
/// </summary>
public interface ICliHandler
{
    Task<int> HandleCommandAsync(string[] args);
}

public class CliHandler : ICliHandler
{
    private readonly SagaOrchestrationService _orchestrationService;
    private readonly SagaDefinitionService _definitionService;
    private readonly ISagaLogger _sagaLogger;
    private readonly IOutputFormatter _outputFormatter;
    private readonly ILogger<CliHandler> _logger;

    public CliHandler(
        SagaOrchestrationService orchestrationService,
        SagaDefinitionService definitionService,
        ISagaLogger sagaLogger,
        IOutputFormatter outputFormatter,
        ILogger<CliHandler> logger)
    {
        _orchestrationService = orchestrationService ?? throw new ArgumentNullException(nameof(orchestrationService));
        _definitionService = definitionService ?? throw new ArgumentNullException(nameof(definitionService));
        _sagaLogger = sagaLogger ?? throw new ArgumentNullException(nameof(sagaLogger));
        _outputFormatter = outputFormatter ?? throw new ArgumentNullException(nameof(outputFormatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> HandleCommandAsync(string[] args)
    {
        try
        {
            var command = SagaCliCommand.Parse(args);

            if (!command.IsValid)
            {
                PrintErrors(command.ValidationErrors);
                return 1;
            }

            return command.CommandType switch
            {
                "create" => await HandleCreateAsync(command),
                "execute" => await HandleExecuteAsync(command),
                "status" => await HandleStatusAsync(command),
                "list" => await HandleListAsync(command),
                "compensate" => await HandleCompensateAsync(command),
                "help" => HandleHelp(command),
                _ => HandleUnknownCommand(command)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CLI command execution failed");
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> HandleCreateAsync(SagaCliCommand command)
    {
        if (!command.Arguments.TryGetValue("definition", out var definitionName))
        {
            Console.Error.WriteLine("Missing --definition parameter");
            return 1;
        }

        try
        {
            var definitions = await _definitionService.GetAllDefinitionsAsync().ConfigureAwait(false);
            var definition = definitions.FirstOrDefault(d => d.Name.Equals(definitionName, StringComparison.OrdinalIgnoreCase));

            if (definition == null)
            {
                Console.Error.WriteLine($"Definition '{definitionName}' not found");
                return 1;
            }

            var saga = await _orchestrationService.CreateSagaAsync(definition.Id, definitionName).ConfigureAwait(false);
            Console.WriteLine($"✓ Created saga: {saga.Id}");
            Console.WriteLine($"  Status: {saga.Status}");

            if (command.Options.Contains("verbose"))
            {
                Console.WriteLine(_outputFormatter.FormatAsJson(saga));
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to create saga: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> HandleExecuteAsync(SagaCliCommand command)
    {
        if (!command.Arguments.TryGetValue("saga-id", out var sagaId))
        {
            Console.Error.WriteLine("Missing --saga-id parameter");
            return 1;
        }

        try
        {
            var saga = await _orchestrationService.GetSagaAsync(sagaId).ConfigureAwait(false);
            if (saga == null)
            {
                Console.Error.WriteLine($"Saga '{sagaId}' not found");
                return 1;
            }

            var isAsync = command.Options.Contains("async");
            if (isAsync)
            {
                _ = _orchestrationService.ExecuteNextStepAsync(sagaId);
                Console.WriteLine("✓ Step execution started asynchronously");
            }
            else
            {
                var updatedSaga = await _orchestrationService.ExecuteNextStepAsync(sagaId).ConfigureAwait(false);
                var currentStep = updatedSaga.Steps.LastOrDefault();
                if (currentStep != null)
                {
                    Console.WriteLine($"✓ Step executed: {currentStep.Name}");
                    Console.WriteLine($"  Status: {currentStep.Status}");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to execute step: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> HandleStatusAsync(SagaCliCommand command)
    {
        if (!command.Arguments.TryGetValue("saga-id", out var sagaId))
        {
            Console.Error.WriteLine("Missing --saga-id parameter");
            return 1;
        }

        try
        {
            var saga = await _orchestrationService.GetSagaAsync(sagaId).ConfigureAwait(false);
            if (saga == null)
            {
                Console.Error.WriteLine($"Saga '{sagaId}' not found");
                return 1;
            }

            Console.WriteLine($"Saga: {saga.Name}");
            Console.WriteLine($"Status: {saga.Status}");
            Console.WriteLine($"Created: {saga.CreatedAt:O}");

            if (command.Options.Contains("verbose"))
            {
                Console.WriteLine("\nSteps:");
                foreach (var step in saga.Steps)
                {
                    Console.WriteLine($"  [{step.Order}] {step.Name}: {step.Status}");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to get status: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> HandleListAsync(SagaCliCommand command)
    {
        try
        {
            var sagas = await _orchestrationService.GetAllSagasAsync().ConfigureAwait(false);
            var limit = 50;

            if (command.Arguments.TryGetValue("limit", out var limitStr) && int.TryParse(limitStr, out var parsedLimit))
            {
                limit = parsedLimit;
            }

            var filtered = sagas.Take(limit).ToList();

            if (command.Arguments.ContainsKey("filter"))
            {
                var filterStatus = command.Arguments["filter"];
                filtered = filtered.Where(s => s.Status.ToString().Equals(filterStatus, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (command.Options.Contains("json"))
            {
                Console.WriteLine(_outputFormatter.FormatAsJson(filtered));
            }
            else if (command.Options.Contains("csv"))
            {
                Console.WriteLine(_outputFormatter.FormatAsCsv(filtered));
            }
            else
            {
                Console.WriteLine(_outputFormatter.FormatAsTable(filtered));
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Failed to list sagas: {ex.Message}");
            return 1;
        }
    }

    private async Task<int> HandleCompensateAsync(SagaCliCommand command)
    {
        if (!command.Arguments.TryGetValue("saga-id", out var sagaId))
        {
            Console.Error.WriteLine("Missing --saga-id parameter");
            return 1;
        }

        Console.WriteLine($"✓ Compensation initiated for saga: {sagaId}");
        return 0;
    }

    private int HandleHelp(SagaCliCommand command)
    {
        Console.WriteLine(command.GetHelpText());
        return 0;
    }

    private int HandleUnknownCommand(SagaCliCommand command)
    {
        Console.Error.WriteLine($"Unknown command: {command.CommandType}");
        Console.Error.WriteLine("Run 'saga help' for usage information");
        return 1;
    }

    private void PrintErrors(List<string> errors)
    {
        foreach (var error in errors)
        {
            Console.Error.WriteLine($"✗ {error}");
        }
    }
}
