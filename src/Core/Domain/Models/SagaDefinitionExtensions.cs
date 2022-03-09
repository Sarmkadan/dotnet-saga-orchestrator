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
    /// <returns>A new SagaDefinition instance.</returns>
    public static SagaDefinition Create(string name, string description, CompensationStrategy compensationStrategy = CompensationStrategy.ReverseOrder)
    {
        return new SagaDefinition
        {
            Name = name ?? throw new ArgumentNullException(nameof(name)),
            Description = description ?? throw new ArgumentNullException(nameof(description)),
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
    public static void AddSteps(this SagaDefinition sagaDefinition, IEnumerable<SagaStepDefinition> stepDefinitions)
    {
        if (sagaDefinition == null)
            throw new ArgumentNullException(nameof(sagaDefinition));

        if (stepDefinitions == null)
            throw new ArgumentNullException(nameof(stepDefinitions));

        foreach (var stepDefinition in stepDefinitions)
        {
            sagaDefinition.AddStep(stepDefinition);
        }
    }

    /// <summary>
    /// Gets the total number of steps in the saga definition.
    /// </summary>
    /// <param name="sagaDefinition">The saga definition.</param>
    /// <returns>The count of steps, or 0 if null.</returns>
    public static int GetStepCount(this SagaDefinition sagaDefinition)
    {
        return sagaDefinition?.Steps?.Count ?? 0;
    }

    /// <summary>
    /// Checks if the saga definition contains a step with the specified name.
    /// </summary>
    /// <param name="sagaDefinition">The saga definition.</param>
    /// <param name="stepName">The name of the step to find.</param>
    /// <returns>True if the step exists, false otherwise.</returns>
    public static bool ContainsStep(this SagaDefinition sagaDefinition, string stepName)
    {
        if (sagaDefinition == null)
            throw new ArgumentNullException(nameof(sagaDefinition));

        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentException("Step name cannot be null or whitespace.", nameof(stepName));

        return sagaDefinition.GetStepByName(stepName) != null;
    }

    /// <summary>
    /// Gets the first step in the saga definition.
    /// </summary>
    /// <param name="sagaDefinition">The saga definition.</param>
    /// <returns>The first step definition, or null if the saga has no steps.</returns>
    public static SagaStepDefinition? GetFirstStep(this SagaDefinition sagaDefinition)
    {
        return sagaDefinition?.Steps?.FirstOrDefault();
    }

    /// <summary>
    /// Gets the last step in the saga definition.
    /// </summary>
    /// <param name="sagaDefinition">The saga definition.</param>
    /// <returns>The last step definition, or null if the saga has no steps.</returns>
    public static SagaStepDefinition? GetLastStep(this SagaDefinition sagaDefinition)
    {
        return sagaDefinition?.Steps?.LastOrDefault();
    }
}