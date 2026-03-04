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

        var created = await _repository.CreateAsync(definition);
        if (created == null)
            throw new SagaException("Failed to create saga definition");

        return created;
    }

    /// <summary>
    /// Adds a step to a saga definition
    /// </summary>
    public async Task<SagaDefinition> AddStepAsync(string definitionId, SagaStepDefinition stepDefinition)
    {
        var definition = await _repository.GetByIdAsync(definitionId)
            ?? throw new SagaException($"Definition '{definitionId}' not found");

        if (stepDefinition == null)
            throw new ArgumentNullException(nameof(stepDefinition));

        if (!stepDefinition.Validate())
            throw new InvalidSagaDefinitionException(definitionId, $"Step '{stepDefinition.Name}' validation failed");

        definition.AddStep(stepDefinition);

        var updated = await _repository.UpdateAsync(definition);
        if (updated == null)
            throw new SagaException("Failed to update saga definition");

        return updated;
    }

    /// <summary>
    /// Removes a step from a saga definition
    /// </summary>
    public async Task<SagaDefinition> RemoveStepAsync(string definitionId, string stepName)
    {
        var definition = await _repository.GetByIdAsync(definitionId)
            ?? throw new SagaException($"Definition '{definitionId}' not found");

        var step = definition.Steps.FirstOrDefault(s => s.Name == stepName);
        if (step == null)
            throw new SagaException($"Step '{stepName}' not found in definition");

        definition.Steps.Remove(step);

        // Reorder remaining steps
        for (int i = 0; i < definition.Steps.Count; i++)
        {
            definition.Steps[i].Order = i + 1;
        }

        var updated = await _repository.UpdateAsync(definition);
        if (updated == null)
            throw new SagaException("Failed to update saga definition");

        return updated;
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
        return await _repository.GetByIdAsync(definitionId)
            ?? throw new SagaException($"Definition '{definitionId}' not found");
    }

    /// <summary>
    /// Gets a saga definition by name
    /// </summary>
    public async Task<SagaDefinition?> GetDefinitionByNameAsync(string name)
    {
        return await _repository.GetByNameAsync(name);
    }

    /// <summary>
    /// Lists all saga definitions
    /// </summary>
    public async Task<List<SagaDefinition>> ListDefinitionsAsync(bool activeOnly = false)
    {
        var definitions = await _repository.GetAllAsync();

        if (activeOnly)
            definitions = definitions.Where(d => d.IsActive).ToList();

        return definitions.OrderByDescending(d => d.CreatedAt).ToList();
    }

    /// <summary>
    /// Activates a saga definition
    /// </summary>
    public async Task<SagaDefinition> ActivateDefinitionAsync(string definitionId)
    {
        var definition = await _repository.GetByIdAsync(definitionId)
            ?? throw new SagaException($"Definition '{definitionId}' not found");

        definition.IsActive = true;

        var updated = await _repository.UpdateAsync(definition);
        if (updated == null)
            throw new SagaException("Failed to activate definition");

        return updated;
    }

    /// <summary>
    /// Deactivates a saga definition
    /// </summary>
    public async Task<SagaDefinition> DeactivateDefinitionAsync(string definitionId)
    {
        var definition = await _repository.GetByIdAsync(definitionId)
            ?? throw new SagaException($"Definition '{definitionId}' not found");

        definition.IsActive = false;

        var updated = await _repository.UpdateAsync(definition);
        if (updated == null)
            throw new SagaException("Failed to deactivate definition");

        return updated;
    }

    /// <summary>
    /// Clones a saga definition for versioning
    /// </summary>
    public async Task<SagaDefinition> CloneDefinitionAsync(string sourceDefinitionId)
    {
        var source = await _repository.GetByIdAsync(sourceDefinitionId)
            ?? throw new SagaException($"Definition '{sourceDefinitionId}' not found");

        var clone = new SagaDefinition(source.Name, source.Description)
        {
            Version = source.Version + 1,
            CompensationStrategy = source.CompensationStrategy
        };

        foreach (var step in source.Steps)
        {
            clone.AddStep(step.Clone());
        }

        var created = await _repository.CreateAsync(clone);
        if (created == null)
            throw new SagaException("Failed to clone saga definition");

        return created;
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
