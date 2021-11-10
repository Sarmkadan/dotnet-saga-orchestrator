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
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("steps")]
    public List<SagaStepDefinition> Steps { get; set; } = new();

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

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
    /// Creates a named saga definition
    /// </summary>
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
    /// Adds a step to the saga definition
    /// </summary>
    public void AddStep(SagaStepDefinition stepDefinition)
    {
        if (stepDefinition == null)
            throw new ArgumentNullException(nameof(stepDefinition));

        stepDefinition.Order = Steps.Count + 1;
        Steps.Add(stepDefinition);
    }

    /// <summary>
    /// Validates the saga definition structure
    /// </summary>
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
    /// Gets step definition by name
    /// </summary>
    public SagaStepDefinition? GetStepByName(string stepName)
    {
        return Steps.Find(s => s.Name == stepName);
    }

    /// <summary>
    /// Gets step definition by order
    /// </summary>
    public SagaStepDefinition? GetStepByOrder(int order)
    {
        return Steps.Find(s => s.Order == order);
    }
}
