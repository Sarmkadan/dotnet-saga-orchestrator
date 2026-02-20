// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Application.DTOs;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Application.Mappers;

/// <summary>
/// Maps saga domain models to response DTOs for API responses.
/// Provides consistent serialization format for client consumption.
/// </summary>
public interface ISagaResponseMapper
{
    SagaResponse MapToResponse(Saga saga);
    List<SagaResponse> MapToResponses(List<Saga> sagas);
    SagaStepResponse MapStepToResponse(SagaStep step);
}

public class SagaResponseMapper : ISagaResponseMapper
{
    public SagaResponse MapToResponse(Saga saga)
    {
        return new SagaResponse
        {
            Id = saga.Id,
            Name = saga.Name,
            DefinitionId = saga.DefinitionId,
            Status = saga.Status.ToString(),
            CreatedAt = saga.CreatedAt,
            CompletedAt = saga.CompletedAt,
            TotalSteps = saga.Steps.Count,
            CompletedSteps = saga.Steps.Count(s => s.Status.ToString() == "Completed"),
            FailedSteps = saga.Steps.Count(s => s.Status.ToString() == "Failed"),
            Steps = saga.Steps.Select(MapStepToResponse).ToList(),
            Data = saga.Data,
            TimeoutSeconds = saga.TimeoutSeconds,
            CompensationStrategy = saga.CompensationStrategy.ToString()
        };
    }

    public List<SagaResponse> MapToResponses(List<Saga> sagas)
    {
        return sagas.Select(MapToResponse).ToList();
    }

    public SagaStepResponse MapStepToResponse(SagaStep step)
    {
        return new SagaStepResponse
        {
            Id = step.Id,
            Name = step.Name,
            Status = step.Status.ToString(),
            Order = step.Order,
            StartedAt = step.StartedAt,
            CompletedAt = step.CompletedAt,
            DurationMs = step.CompletedAt.HasValue
                ? (long)(step.CompletedAt.Value - step.StartedAt).TotalMilliseconds
                : 0,
            RetryCount = step.RetryCount,
            MaxRetries = step.MaxRetries,
            TimeoutSeconds = step.TimeoutSeconds,
            ServiceName = step.ServiceName,
            Error = step.Error
        };
    }
}

public class SagaStepResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long DurationMs { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public int TimeoutSeconds { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string? Error { get; set; }
}
