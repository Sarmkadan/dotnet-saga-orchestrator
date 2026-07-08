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
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Service for managing saga definitions.
/// Handles creation, validation, versioning, and retrieval of saga workflows.
/// </summary>
public class SagaDefinitionService
{
    private readonly ISagaDefinitionRepository _repository;

    public SagaDefinitionService(ISagaDefinitionRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Creates a new saga definition
    /// </summary>
    public async Task<SagaDefinition> CreateDefinitionAsync(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required", nameof(description));

        var definition = new SagaDefinition(name, description);

        try
        {
            var created = await _repository.CreateAsync(definition);
            if (created == null)
                throw new SagaException("Failed to create saga definition");
            return created;
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error creating saga definition", ex);
        }
    }

    /// <summary>
    /// Adds a step to a saga definition
    /// </summary>
    public async Task<SagaDefinition> AddStepAsync(string definitionId, SagaStepDefinition stepDefinition)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition ID must be provided", nameof(definitionId));
        if (stepDefinition == null)
            throw new ArgumentNullException(nameof(stepDefinition));

        SagaDefinition definition;
        try
        {
            definition = await _repository.GetByIdAsync(definitionId)
                ?? throw new SagaException($"Definition '{definitionId}' not found");
        }
        catch (Exception ex) when (!(ex is SagaException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga definition for adding step", ex);
        }

        if (!stepDefinition.Validate())
            throw new InvalidSagaDefinitionException(definitionId, $"Step '{stepDefinition.Name}' validation failed");

        definition.AddStep(stepDefinition);

        try
        {
            var updated = await _repository.UpdateAsync(definition);
            if (updated == null)
                throw new SagaException("Failed to update saga definition");
            return updated;
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error updating saga definition after adding step", ex);
        }
    }

    /// <summary>
    /// Removes a step from a saga definition
    /// </summary>
    public async Task<SagaDefinition> RemoveStepAsync(string definitionId, string stepName)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition ID must be provided", nameof(definitionId));
        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentException("Step name must be provided", nameof(stepName));

        SagaDefinition definition;
        try
        {
            definition = await _repository.GetByIdAsync(definitionId)
                ?? throw new SagaException($"Definition '{definitionId}' not found");
        }
        catch (Exception ex) when (!(ex is SagaException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga definition for removing step", ex);
        }

        var step = definition.Steps.FirstOrDefault(s => s.Name == stepName);
        if (step == null)
            throw new SagaException($"Step '{stepName}' not found in definition");

        definition.Steps.Remove(step);

        // Reorder remaining steps
        for (int i = 0; i < definition.Steps.Count; i++)
        {
            definition.Steps[i].Order = i + 1;
        }

        try
        {
            var updated = await _repository.UpdateAsync(definition);
            if (updated == null)
                throw new SagaException("Failed to update saga definition");
            return updated;
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error updating saga definition after removing step", ex);
        }
    }

    /// <summary>
    /// Validates a saga definition
    /// </summary>
    public ValidationResult ValidateDefinition(SagaDefinition definition)
    {
        var errors = new List<string>();

        if (definition == null)
        {
            return new ValidationResult { IsValid = false, Errors = new[] { "Definition cannot be null" } };
        }

        if (string.IsNullOrWhiteSpace(definition.Name))
            errors.Add("Definition name is required");

        if (definition.Steps.Count == 0)
            errors.Add("At least one step is required");

        if (definition.Steps.Count > 50)
            errors.Add("Maximum 50 steps allowed per definition");

        foreach (var step in definition.Steps)
        {
            if (!step.Validate())
                errors.Add($"Step '{step.Name}' validation failed");
        }

        // Check for duplicate step names
        var duplicateNames = definition.Steps
            .GroupBy(s => s.Name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateNames.Any())
            errors.Add($"Duplicate step names found: {string.Join(", ", duplicateNames)}");

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors.ToArray()
        };
    }

    /// <summary>
    /// Gets a saga definition by ID
    /// </summary>
    public async Task<SagaDefinition> GetDefinitionAsync(string definitionId)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition ID must be provided", nameof(definitionId));

        try
        {
            return await _repository.GetByIdAsync(definitionId)
                ?? throw new SagaException($"Definition '{definitionId}' not found");
        }
        catch (Exception ex) when (!(ex is SagaException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga definition", ex);
        }
    }

    /// <summary>
    /// Gets a saga definition by name
    /// </summary>
    public async Task<SagaDefinition?> GetDefinitionByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be provided", nameof(name));

        try
        {
            return await _repository.GetByNameAsync(name);
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga definition by name", ex);
        }
    }

    /// <summary>
    /// Lists all saga definitions
    /// </summary>
    public async Task<List<SagaDefinition>> ListDefinitionsAsync(bool activeOnly = false)
    {
        try
        {
            var definitions = await _repository.GetAllAsync();

            if (activeOnly)
                definitions = definitions.Where(d => d.IsActive).ToList();

            return definitions.OrderByDescending(d => d.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error listing saga definitions", ex);
        }
    }

    /// <summary>
    /// Activates a saga definition
    /// </summary>
    public async Task<SagaDefinition> ActivateDefinitionAsync(string definitionId)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition ID must be provided", nameof(definitionId));

        SagaDefinition definition;
        try
        {
            definition = await _repository.GetByIdAsync(definitionId)
                ?? throw new SagaException($"Definition '{definitionId}' not found");
        }
        catch (Exception ex) when (!(ex is SagaException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga definition for activation", ex);
        }

        definition.IsActive = true;

        try
        {
            var updated = await _repository.UpdateAsync(definition);
            if (updated == null)
                throw new SagaException("Failed to activate definition");
            return updated;
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error activating saga definition", ex);
        }
    }

    /// <summary>
    /// Deactivates a saga definition
    /// </summary>
    public async Task<SagaDefinition> DeactivateDefinitionAsync(string definitionId)
    {
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Definition ID must be provided", nameof(definitionId));

        SagaDefinition definition;
        try
        {
            definition = await _repository.GetByIdAsync(definitionId)
                ?? throw new SagaException($"Definition '{definitionId}' not found");
        }
        catch (Exception ex) when (!(ex is SagaException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving saga definition for deactivation", ex);
        }

        definition.IsActive = false;

        try
        {
            var updated = await _repository.UpdateAsync(definition);
            if (updated == null)
                throw new SagaException("Failed to deactivate definition");
            return updated;
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error deactivating saga definition", ex);
        }
    }

    /// <summary>
    /// Clones a saga definition for versioning
    /// </summary>
    public async Task<SagaDefinition> CloneDefinitionAsync(string sourceDefinitionId)
    {
        if (string.IsNullOrWhiteSpace(sourceDefinitionId))
            throw new ArgumentException("Source definition ID must be provided", nameof(sourceDefinitionId));

        SagaDefinition source;
        try
        {
            source = await _repository.GetByIdAsync(sourceDefinitionId)
                ?? throw new SagaException($"Definition '{sourceDefinitionId}' not found");
        }
        catch (Exception ex) when (!(ex is SagaException))
        {
            throw new DotnetSagaOrchestratorException("Error retrieving source saga definition for cloning", ex);
        }

        var clone = new SagaDefinition(source.Name, source.Description)
        {
            Version = source.Version + 1,
            CompensationStrategy = source.CompensationStrategy
        };

        foreach (var step in source.Steps)
        {
            clone.AddStep(step.Clone());
        }

        try
        {
            var created = await _repository.CreateAsync(clone);
            if (created == null)
                throw new SagaException("Failed to clone saga definition");
            return created;
        }
        catch (Exception ex)
        {
            throw new DotnetSagaOrchestratorException("Error cloning saga definition", ex);
        }
    }
}

/// <summary>
/// Result of validation operation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string[] Errors { get; set; } = [];
}
