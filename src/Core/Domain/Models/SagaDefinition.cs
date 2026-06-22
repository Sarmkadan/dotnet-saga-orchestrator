#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Defines the structure and sequence of steps in a saga workflow.
/// </summary>
public class SagaDefinition
{
    /// <summary>Gets or sets the unique identifier of the saga definition.</summary>
    [JsonPropertyName("id")]
    public string Id { get; set; }

    /// <summary>Gets or sets the name of the saga definition.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>Gets or sets the description of the saga definition.</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; }

    /// <summary>Gets or sets the version of the saga definition.</summary>
    [JsonPropertyName("version")]
    public int Version { get; set; }

    /// <summary>Gets or sets the list of steps in the saga definition.</summary>
    [JsonPropertyName("steps")]
    public List<SagaStepDefinition> Steps { get; set; } = new();

    /// <summary>Gets or sets the time the saga definition was created.</summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets a value indicating whether the saga definition is active.</summary>
    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the compensation strategy used when a saga fails.</summary>
    [JsonPropertyName("compensationStrategy")]
    public CompensationStrategy CompensationStrategy { get; set; }

    // Constructor
    public SagaDefinition()
    {
        Id = Guid.NewGuid().ToString();
        Name = "Undefined Saga";
        Description = "No description provided";
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        CompensationStrategy = CompensationStrategy.ReverseOrder;
    }

    /// <summary>
    /// Creates a named saga definition.
    /// </summary>
    /// <param name="name">The name of the saga definition.</param>
    /// <param name="description">The description of the saga definition.</param>
    public SagaDefinition(string name, string description)
    {
        Id = Guid.NewGuid().ToString();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Version = 1;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
        CompensationStrategy = CompensationStrategy.ReverseOrder;
    }

    /// <summary>
    /// Adds a step to the saga definition.
    /// </summary>
    /// <param name="stepDefinition">The step definition to add.</param>
    public void AddStep(SagaStepDefinition stepDefinition)
    {
        if (stepDefinition == null)
            throw new ArgumentNullException(nameof(stepDefinition));

        stepDefinition.Order = Steps.Count + 1;
        Steps.Add(stepDefinition);
    }

    /// <summary>
    /// Validates the saga definition structure.
    /// </summary>
    /// <returns>True if the definition is valid, false otherwise.</returns>
    public bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (Steps.Count == 0)
            return false;

        for (int i = 0; i < Steps.Count; i++)
        {
            if (!Steps[i].Validate())
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets step definition by name.
    /// </summary>
    /// <param name="stepName">The name of the step.</param>
    /// <returns>The step definition if found, null otherwise.</returns>
    public SagaStepDefinition? GetStepByName(string stepName)
    {
        return Steps.Find(s => s.Name == stepName);
    }

    /// <summary>
    /// Gets step definition by order.
    /// </summary>
    /// <param name="order">The order index of the step.</param>
    /// <returns>The step definition if found, null otherwise.</returns>
    public SagaStepDefinition? GetStepByOrder(int order)
    {
        return Steps.Find(s => s.Order == order);
    }
}
