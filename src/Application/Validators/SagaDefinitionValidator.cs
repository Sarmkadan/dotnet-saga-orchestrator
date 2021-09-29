#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Application.Validators;

/// <summary>
/// Validation service for saga definitions.
/// Ensures definitions are valid before execution with comprehensive checks.
/// </summary>
public interface ISagaDefinitionValidator
{
    Task ValidateAsync(SagaDefinition definition);
    Task<List<string>> ValidateAndGetErrorsAsync(SagaDefinition definition);
}

public class SagaDefinitionValidator : ISagaDefinitionValidator
{
    public async Task ValidateAsync(SagaDefinition definition)
    {
        var errors = await ValidateAndGetErrorsAsync(definition);
        if (errors.Count > 0)
        {
            throw new InvalidSagaDefinitionException(
                $"Saga definition '{definition.Name}' is invalid: {string.Join(", ", errors)}");
        }
    }

    public async Task<List<string>> ValidateAndGetErrorsAsync(SagaDefinition definition)
    {
        return await Task.Run(() =>
        {
            var errors = new List<string>();

            // Check definition properties
            if (string.IsNullOrWhiteSpace(definition.Name))
                errors.Add("Definition name cannot be null or empty");

            if (definition.Name.Length > 255)
                errors.Add("Definition name cannot exceed 255 characters");

            // Check steps
            if (definition.Steps == null || definition.Steps.Count == 0)
                errors.Add("Definition must contain at least one step");

            if (definition.Steps?.Count > 100)
                errors.Add("Definition cannot have more than 100 steps");

            // Validate each step
            if (definition.Steps != null)
            {
                for (int i = 0; i < definition.Steps.Count; i++)
                {
                    var step = definition.Steps[i];
                    var stepErrors = ValidateStep(step, i + 1);
                    errors.AddRange(stepErrors);
                }

                // Check step order integrity
                var orderErrors = ValidateStepOrder(definition.Steps);
                errors.AddRange(orderErrors);
            }

            return errors;
        });
    }

    private List<string> ValidateStep(SagaStepDefinition step, int stepNumber)
    {
        var errors = new List<string>();
        var stepPrefix = $"Step {stepNumber}";

        if (string.IsNullOrWhiteSpace(step.Name))
            errors.Add($"{stepPrefix}: Name cannot be null or empty");

        if (step.Name?.Length > 255)
            errors.Add($"{stepPrefix}: Name cannot exceed 255 characters");

        if (string.IsNullOrWhiteSpace(step.ServiceName))
            errors.Add($"{stepPrefix}: Service name cannot be null or empty");

        if (string.IsNullOrWhiteSpace(step.Action))
            errors.Add($"{stepPrefix}: Action URL cannot be null or empty");

        if (!Uri.IsWellFormedUriString(step.Action, UriKind.Absolute))
            errors.Add($"{stepPrefix}: Action URL is not valid");

        if (step.TimeoutSeconds <= 0)
            errors.Add($"{stepPrefix}: Timeout must be greater than 0");

        if (step.TimeoutSeconds > 3600)
            errors.Add($"{stepPrefix}: Timeout cannot exceed 3600 seconds (1 hour)");

        if (step.MaxRetries < 0)
            errors.Add($"{stepPrefix}: Max retries cannot be negative");

        if (step.MaxRetries > 10)
            errors.Add($"{stepPrefix}: Max retries cannot exceed 10");

        if (!string.IsNullOrWhiteSpace(step.Compensation) && !Uri.IsWellFormedUriString(step.Compensation, UriKind.Absolute))
            errors.Add($"{stepPrefix}: Compensation URL is not valid if provided");

        return errors;
    }

    private List<string> ValidateStepOrder(List<SagaStepDefinition> steps)
    {
        var errors = new List<string>();

        // Check for duplicate order numbers
        var orderGroups = steps.GroupBy(s => s.Order).Where(g => g.Count() > 1);
        foreach (var group in orderGroups)
        {
            errors.Add($"Multiple steps have the same order: {group.Key}");
        }

        // Check for gaps in order (but allow non-sequential)
        var orders = steps.Select(s => s.Order).OrderBy(o => o).ToList();
        if (orders.Count > 0 && orders.First() != 1)
            errors.Add("Step ordering should start from 1");

        return errors;
    }
}

/// <summary>
/// Validation service for saga creation requests.
/// </summary>
public interface ISagaRequestValidator
{
    Task ValidateCreateSagaAsync(CreateSagaRequest request);
}

public class SagaRequestValidator : ISagaRequestValidator
{
    public async Task ValidateCreateSagaAsync(CreateSagaRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.DefinitionId))
            errors.Add("DefinitionId is required");

        if (request.DefinitionId?.Length > 255)
            errors.Add("DefinitionId cannot exceed 255 characters");

        if (!string.IsNullOrEmpty(request.Data) && request.Data.Length > 10000)
            errors.Add("Saga data cannot exceed 10000 characters");

        if (errors.Count > 0)
            throw new ArgumentException($"Invalid saga creation request: {string.Join(", ", errors)}");

        await Task.CompletedTask;
    }
}
