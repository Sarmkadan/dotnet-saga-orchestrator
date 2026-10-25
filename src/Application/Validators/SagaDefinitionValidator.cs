#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Application.DTOs;
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
    private const int MaxNameLength = 255;
    private const int MaxStepCount = 100;
    private const int MaxTimeoutSeconds = 3600;
    private const int MaxRetryCount = 10;

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

            if (definition.Name.Length > MaxNameLength)
                errors.Add($"Definition name cannot exceed {MaxNameLength} characters");

            // Check steps
            if (definition.Steps == null || definition.Steps.Count == 0)
                errors.Add("Definition must contain at least one step");

            if (definition.Steps?.Count > MaxStepCount)
                errors.Add($"Definition cannot have more than {MaxStepCount} steps");

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

        if (step.Name?.Length > MaxNameLength)
            errors.Add($"{stepPrefix}: Name cannot exceed {MaxNameLength} characters");

        if (string.IsNullOrWhiteSpace(step.ServiceName))
            errors.Add($"{stepPrefix}: Service name cannot be null or empty");

        if (string.IsNullOrWhiteSpace(step.ServiceUrl))
            errors.Add($"{stepPrefix}: Action URL cannot be null or empty");

        if (!Uri.IsWellFormedUriString(step.ServiceUrl, UriKind.Absolute))
            errors.Add($"{stepPrefix}: Action URL is not valid");

        if (step.TimeoutSeconds <= 0)
            errors.Add($"{stepPrefix}: Timeout must be greater than 0");

        if (step.TimeoutSeconds > MaxTimeoutSeconds)
            errors.Add($"{stepPrefix}: Timeout cannot exceed {MaxTimeoutSeconds} seconds (1 hour)");

        if (step.MaxRetries < 0)
            errors.Add($"{stepPrefix}: Max retries cannot be negative");

        if (step.MaxRetries > MaxRetryCount)
            errors.Add($"{stepPrefix}: Max retries cannot exceed {MaxRetryCount}");

        if (!string.IsNullOrWhiteSpace(step.CompensationUrl) && !Uri.IsWellFormedUriString(step.CompensationUrl, UriKind.Absolute))
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
    private const int MaxDefinitionIdLength = 255;
    private const int MaxSagaDataLength = 10000;

    public async Task ValidateCreateSagaAsync(CreateSagaRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.DefinitionId))
            errors.Add("DefinitionId is required");

        if (request.DefinitionId?.Length > MaxDefinitionIdLength)
            errors.Add($"DefinitionId cannot exceed {MaxDefinitionIdLength} characters");

        if (!string.IsNullOrEmpty(request.Data) && request.Data.Length > MaxSagaDataLength)
            errors.Add($"Saga data cannot exceed {MaxSagaDataLength} characters");

        if (errors.Count > 0)
            throw new ArgumentException($"Invalid saga creation request: {string.Join(", ", errors)}");

        await Task.CompletedTask;
    }
}