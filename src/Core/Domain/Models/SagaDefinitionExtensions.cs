#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides extension methods for <see cref="SagaDefinition"/> to simplify common operations.
/// </summary>
public static class SagaDefinitionExtensions
{
	/// <summary>
	/// Creates a new SagaDefinition with the specified name and description.
	/// </summary>
	/// <param name="name">The name of the saga definition.</param>
	/// <param name="description">The description of the saga definition.</param>
	/// <param name="compensationStrategy">The compensation strategy to use (defaults to ReverseOrder).</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> or <paramref name="description"/> is null.</exception>
	/// <returns>A new SagaDefinition instance.</returns>
	public static SagaDefinition Create(string name, string description, CompensationStrategy compensationStrategy = CompensationStrategy.ReverseOrder)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(description);

		return new SagaDefinition
		{
			Name = name,
			Description = description,
			CompensationStrategy = compensationStrategy,
			Version = 1,
			CreatedAt = DateTime.UtcNow,
			IsActive = true
		};
	}

	/// <summary>
	/// Adds multiple steps to the saga definition at once.
	/// </summary>
	/// <param name="sagaDefinition">The saga definition to add steps to.</param>
	/// <param name="stepDefinitions">The collection of step definitions to add.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaDefinition"/> or <paramref name="stepDefinitions"/> is null.</exception>
	public static void AddSteps(this SagaDefinition sagaDefinition, IEnumerable<SagaStepDefinition> stepDefinitions)
	{
		ArgumentNullException.ThrowIfNull(sagaDefinition);
		ArgumentNullException.ThrowIfNull(stepDefinitions);

		foreach (var stepDefinition in stepDefinitions)
		{
			sagaDefinition.AddStep(stepDefinition);
		}
	}

	/// <summary>
	/// Gets the total number of steps in the saga definition.
	/// </summary>
	/// <param name="sagaDefinition">The saga definition.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaDefinition"/> is null.</exception>
	/// <returns>The count of steps, or 0 if the saga definition has no steps.</returns>
	public static int GetStepCount(this SagaDefinition sagaDefinition)
	{
		ArgumentNullException.ThrowIfNull(sagaDefinition);
		return sagaDefinition.Steps?.Count ?? 0;
	}

	/// <summary>
	/// Checks if the saga definition contains a step with the specified name.
	/// </summary>
	/// <param name="sagaDefinition">The saga definition.</param>
	/// <param name="stepName">The name of the step to find.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaDefinition"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown when <paramref name="stepName"/> is null or whitespace.</exception>
	/// <returns>True if the step exists, false otherwise.</returns>
	public static bool ContainsStep(this SagaDefinition sagaDefinition, string stepName)
	{
		ArgumentNullException.ThrowIfNull(sagaDefinition);
		ArgumentException.ThrowIfNullOrWhiteSpace(stepName, nameof(stepName));

		return sagaDefinition.GetStepByName(stepName) != null;
	}

	/// <summary>
	/// Gets the first step in the saga definition.
	/// </summary>
	/// <param name="sagaDefinition">The saga definition.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaDefinition"/> is null.</exception>
	/// <returns>The first step definition, or null if the saga has no steps.</returns>
	public static SagaStepDefinition? GetFirstStep(this SagaDefinition sagaDefinition)
	{
		ArgumentNullException.ThrowIfNull(sagaDefinition);
		return sagaDefinition.Steps?.FirstOrDefault();
	}

	/// <summary>
	/// Gets the last step in the saga definition.
	/// </summary>
	/// <param name="sagaDefinition">The saga definition.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaDefinition"/> is null.</exception>
	/// <returns>The last step definition, or null if the saga has no steps.</returns>
	public static SagaStepDefinition? GetLastStep(this SagaDefinition sagaDefinition)
	{
		ArgumentNullException.ThrowIfNull(sagaDefinition);
		return sagaDefinition.Steps?.LastOrDefault();
	}
}
