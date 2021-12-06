#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Extension methods for SagaDefinitionService providing additional convenience methods
/// </summary>
public static class SagaDefinitionServiceExtensions
{
    /// <summary>
    /// Creates a new saga definition and immediately activates it
    /// </summary>
    public static async Task<SagaDefinition> CreateAndActivateDefinitionAsync(this SagaDefinitionService service, string name, string description)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        var definition = await service.CreateDefinitionAsync(name, description);
        return await service.ActivateDefinitionAsync(definition.Id);
    }

    /// <summary>
    /// Adds multiple steps to a saga definition in a single operation
    /// </summary>
    public static async Task<SagaDefinition> AddStepsAsync(this SagaDefinitionService service, string definitionId, IEnumerable<SagaStepDefinition> stepDefinitions)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition ID must be provided", nameof(definitionId));
        if (stepDefinitions is null)
            throw new ArgumentNullException(nameof(stepDefinitions));

        SagaDefinition definition = await service.GetDefinitionAsync(definitionId);

        foreach (var stepDefinition in stepDefinitions)
        {
            definition = await service.AddStepAsync(definition.Id, stepDefinition);
        }

        return definition;
    }

    /// <summary>
    /// Removes multiple steps from a saga definition in a single operation
    /// </summary>
    public static async Task<SagaDefinition> RemoveStepsAsync(this SagaDefinitionService service, string definitionId, IEnumerable<string> stepNames)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition ID must be provided", nameof(definitionId));
        if (stepNames is null)
            throw new ArgumentNullException(nameof(stepNames));

        SagaDefinition definition = await service.GetDefinitionAsync(definitionId);

        foreach (var stepName in stepNames)
        {
            definition = await service.RemoveStepAsync(definition.Id, stepName);
        }

        return definition;
    }

    /// <summary>
    /// Checks if a saga definition with the given name already exists
    /// </summary>
    public static async Task<bool> DefinitionExistsAsync(this SagaDefinitionService service, string name)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be provided", nameof(name));

        return await service.GetDefinitionByNameAsync(name) is not null;
    }

    /// <summary>
    /// Gets a saga definition by name or creates a new one if it doesn't exist
    /// </summary>
    public static async Task<SagaDefinition> GetOrCreateDefinitionAsync(this SagaDefinitionService service, string name, string description, bool activateIfCreated = false)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be provided", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description must be provided", nameof(description));

        var existing = await service.GetDefinitionByNameAsync(name);

        if (existing is not null)
        {
            return existing;
        }

        var created = await service.CreateDefinitionAsync(name, description);

        if (activateIfCreated)
        {
            return await service.ActivateDefinitionAsync(created.Id);
        }

        return created;
    }

    /// <summary>
    /// Validates a saga definition and throws if invalid
    /// </summary>
    public static void ValidateOrThrow(this SagaDefinitionService service, SagaDefinition definition)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (definition is null)
            throw new ArgumentNullException(nameof(definition));

        var validationResult = service.ValidateDefinition(definition);

        if (!validationResult.IsValid)
        {
            throw new InvalidSagaDefinitionException(
                definition.Id,
                $"Validation failed: {string.Join(", ", validationResult.Errors)}"
            );
        }
    }

    /// <summary>
    /// Gets all active saga definitions
    /// </summary>
    public static async Task<List<SagaDefinition>> GetActiveDefinitionsAsync(this SagaDefinitionService service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        return await service.ListDefinitionsAsync(activeOnly: true);
    }

    /// <summary>
    /// Gets all inactive saga definitions
    /// </summary>
    public static async Task<List<SagaDefinition>> GetInactiveDefinitionsAsync(this SagaDefinitionService service)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        var allDefinitions = await service.ListDefinitionsAsync();
        return allDefinitions.Where(d => !d.IsActive).ToList();
    }

    /// <summary>
    /// Gets a saga definition by name and validates it
    /// </summary>
    public static async Task<SagaDefinition> GetAndValidateDefinitionAsync(this SagaDefinitionService service, string name)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be provided", nameof(name));

        var definition = await service.GetDefinitionByNameAsync(name)
            ?? throw new SagaException($"Definition '{name}' not found");

        service.ValidateOrThrow(definition);
        return definition;
    }

    /// <summary>
    /// Creates a new version of an existing saga definition by cloning
    /// </summary>
    public static async Task<SagaDefinition> CreateNewVersionAsync(this SagaDefinitionService service, string sourceDefinitionId, bool activateNewVersion = false)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));
        if (string.IsNullOrWhiteSpace(sourceDefinitionId))
            throw new ArgumentException("Source definition ID must be provided", nameof(sourceDefinitionId));

        var source = await service.GetDefinitionAsync(sourceDefinitionId);
        var cloned = await service.CloneDefinitionAsync(source.Id);

        if (activateNewVersion)
        {
            return await service.ActivateDefinitionAsync(cloned.Id);
        }

        return cloned;
    }
}