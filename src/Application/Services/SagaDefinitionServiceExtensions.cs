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
	/// Creates a new saga definition and immediately activates it.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <param name="name">The name of the saga definition.</param>
	/// <param name="description">The description of the saga definition.</param>
	/// <returns>The activated saga definition.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/>, <paramref name="name"/>, or <paramref name="description"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="description"/> is whitespace.</exception>
	public static async Task<SagaDefinition> CreateAndActivateDefinitionAsync(
		this SagaDefinitionService service,
		string name,
		string description)
	{
		ArgumentNullException.ThrowIfNull(service);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(description);

		var definition = await service.CreateDefinitionAsync(name, description);
		return await service.ActivateDefinitionAsync(definition.Id);
	}

	/// <summary>
	/// Adds multiple steps to a saga definition in a single operation.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <param name="definitionId">The ID of the saga definition.</param>
	/// <param name="stepDefinitions">The collection of step definitions to add.</param>
	/// <returns>The updated saga definition.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="stepDefinitions"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="definitionId"/> is whitespace.</exception>
	public static async Task<SagaDefinition> AddStepsAsync(
		this SagaDefinitionService service,
		string definitionId,
		IEnumerable<SagaStepDefinition> stepDefinitions)
	{
		ArgumentNullException.ThrowIfNull(service);
		ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
		ArgumentNullException.ThrowIfNull(stepDefinitions);

		SagaDefinition definition = await service.GetDefinitionAsync(definitionId);

		foreach (var stepDefinition in stepDefinitions)
		{
			definition = await service.AddStepAsync(definition.Id, stepDefinition);
		}

		return definition;
	}

	/// <summary>
	/// Removes multiple steps from a saga definition in a single operation.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <param name="definitionId">The ID of the saga definition.</param>
	/// <param name="stepNames">The collection of step names to remove.</param>
	/// <returns>The updated saga definition.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="stepNames"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="definitionId"/> or <paramref name="stepNames"/> is whitespace.</exception>
	public static async Task<SagaDefinition> RemoveStepsAsync(
		this SagaDefinitionService service,
		string definitionId,
		IEnumerable<string> stepNames)
	{
		ArgumentNullException.ThrowIfNull(service);
		ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
		ArgumentNullException.ThrowIfNull(stepNames);

		SagaDefinition definition = await service.GetDefinitionAsync(definitionId);

		foreach (var stepName in stepNames)
		{
			definition = await service.RemoveStepAsync(definition.Id, stepName);
		}

		return definition;
	}

	/// <summary>
	/// Checks if a saga definition with the given name already exists.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <param name="name">The name of the saga definition to check.</param>
	/// <returns><see langword="true"/> if a definition with the given name exists; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is whitespace.</exception>
	public static async Task<bool> DefinitionExistsAsync(
		this SagaDefinitionService service,
		string name)
	{
		ArgumentNullException.ThrowIfNull(service);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		return await service.GetDefinitionByNameAsync(name) is not null;
	}

	/// <summary>
	/// Gets a saga definition by name or creates a new one if it doesn't exist.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <param name="name">The name of the saga definition.</param>
	/// <param name="description">The description of the saga definition.</param>
	/// <param name="activateIfCreated">Whether to activate the definition if it's created.</param>
	/// <returns>The existing or newly created saga definition.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/>, <paramref name="name"/>, or <paramref name="description"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="description"/> is whitespace.</exception>
	public static async Task<SagaDefinition> GetOrCreateDefinitionAsync(
		this SagaDefinitionService service,
		string name,
		string description,
		bool activateIfCreated = false)
	{
		ArgumentNullException.ThrowIfNull(service);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		ArgumentException.ThrowIfNullOrWhiteSpace(description);

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
	/// Validates a saga definition and throws if invalid.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <param name="definition">The saga definition to validate.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="definition"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidSagaDefinitionException">Thrown when the definition is invalid.</exception>
	public static void ValidateOrThrow(this SagaDefinitionService service, SagaDefinition definition)
	{
		ArgumentNullException.ThrowIfNull(service);
		ArgumentNullException.ThrowIfNull(definition);

		var validationResult = service.ValidateDefinition(definition);

		if (!validationResult.IsValid)
		{
			throw new InvalidSagaDefinitionException(
				definition.Id,
				$"Validation failed: {string.Join(", ", validationResult.Errors)}");
		}
	}

	/// <summary>
	/// Gets all active saga definitions.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <returns>A list of active saga definitions.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
	public static async Task<List<SagaDefinition>> GetActiveDefinitionsAsync(this SagaDefinitionService service)
	{
		ArgumentNullException.ThrowIfNull(service);

		return await service.ListDefinitionsAsync(activeOnly: true);
	}

	/// <summary>
	/// Gets all inactive saga definitions.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <returns>A list of inactive saga definitions.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
	public static async Task<List<SagaDefinition>> GetInactiveDefinitionsAsync(this SagaDefinitionService service)
	{
		ArgumentNullException.ThrowIfNull(service);

		var allDefinitions = await service.ListDefinitionsAsync();
		return allDefinitions.Where(d => !d.IsActive).ToList();
	}

	/// <summary>
	/// Gets a saga definition by name and validates it.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <param name="name">The name of the saga definition to retrieve and validate.</param>
	/// <returns>The validated saga definition.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is whitespace.</exception>
	/// <exception cref="SagaException">Thrown when the definition is not found.</exception>
	public static async Task<SagaDefinition> GetAndValidateDefinitionAsync(
		this SagaDefinitionService service,
		string name)
	{
		ArgumentNullException.ThrowIfNull(service);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		var definition = await service.GetDefinitionByNameAsync(name)
			?? throw new SagaException($"Definition '{name}' not found");

		service.ValidateOrThrow(definition);
		return definition;
	}

	/// <summary>
	/// Creates a new version of an existing saga definition by cloning.
	/// </summary>
	/// <param name="service">The saga definition service.</param>
	/// <param name="sourceDefinitionId">The ID of the source definition to clone.</param>
	/// <param name="activateNewVersion">Whether to activate the new version.</param>
	/// <returns>The cloned saga definition.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="service"/> or <paramref name="sourceDefinitionId"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="sourceDefinitionId"/> is whitespace.</exception>
	public static async Task<SagaDefinition> CreateNewVersionAsync(
		this SagaDefinitionService service,
		string sourceDefinitionId,
		bool activateNewVersion = false)
	{
		ArgumentNullException.ThrowIfNull(service);
		ArgumentException.ThrowIfNullOrWhiteSpace(sourceDefinitionId);

		var source = await service.GetDefinitionAsync(sourceDefinitionId);
		var cloned = await service.CloneDefinitionAsync(source.Id);

		if (activateNewVersion)
		{
			return await service.ActivateDefinitionAsync(cloned.Id);
		}

		return cloned;
	}
}
