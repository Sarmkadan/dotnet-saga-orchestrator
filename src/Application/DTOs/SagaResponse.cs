#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Application.DTOs;

/// <summary>
/// Response model for saga operations.
/// </summary>
public class SagaResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("definitionId")]
    public string DefinitionId { get; set; } = string.Empty;

    [JsonPropertyName("definitionName")]
    public string DefinitionName { get; set; } = string.Empty;

    [JsonPropertyName("startedAt")]
    public DateTime StartedAt { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; set; }

    [JsonPropertyName("stepCount")]
    public int StepCount { get; set; }

    [JsonPropertyName("completedSteps")]
    public int CompletedSteps { get; set; }

    [JsonPropertyName("failedSteps")]
    public int FailedSteps { get; set; }

    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    [JsonPropertyName("steps")]
    public List<SagaStepResponse> Steps { get; set; } = new();

    /// <summary>
    /// Creates response from saga domain model
    /// </summary>
    public static SagaResponse FromSaga(Saga saga)
    {
        if (saga == null)
            throw new ArgumentNullException(nameof(saga));

        var response = new SagaResponse
        {
            Id = saga.Id,
            CorrelationId = saga.CorrelationId,
            Status = saga.Status.ToString(),
            DefinitionId = saga.Definition.Id,
            DefinitionName = saga.Definition.Name,
            StartedAt = saga.StartedAt,
            CompletedAt = saga.CompletedAt,
            FailureReason = saga.FailureReason,
            StepCount = saga.Steps.Count,
            CompletedSteps = saga.Steps.Count(s => s.Status == Core.Domain.Enums.SagaStepStatus.Completed),
            FailedSteps = saga.Steps.Count(s => s.Status == Core.Domain.Enums.SagaStepStatus.Failed),
            RetryCount = saga.RetryCount
        };

        foreach (var step in saga.Steps)
        {
            response.Steps.Add(SagaStepResponse.FromStep(step));
        }

        return response;
    }
}

/// <summary>
/// Response model for saga steps.
/// </summary>
public class SagaStepResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("serviceName")]
    public string ServiceName { get; set; } = string.Empty;

    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; set; }

    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan? Duration
    {
        get
        {
            if (StartedAt == null || CompletedAt == null)
                return null;
            return CompletedAt - StartedAt;
        }
    }

    /// <summary>
    /// Creates response from step domain model
    /// </summary>
    public static SagaStepResponse FromStep(SagaStep step)
    {
        if (step == null)
            throw new ArgumentNullException(nameof(step));

        return new SagaStepResponse
        {
            Id = step.Id,
            Name = step.Name,
            Order = step.Order,
            Status = step.Status.ToString(),
            ServiceName = step.ServiceUrl,
            StartedAt = step.StartedAt,
            CompletedAt = step.CompletedAt,
            ErrorMessage = step.ErrorMessage,
            RetryCount = step.RetryCount
        };
    }
}
