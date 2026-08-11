#nullable enable
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
        ArgumentNullException.ThrowIfNull(saga);
        return SagaResponse.FromSaga(saga);
    }

    public List<SagaResponse> MapToResponses(List<Saga> sagas)
    {
        ArgumentNullException.ThrowIfNull(sagas);
        return sagas.Select(MapToResponse).ToList();
    }

    public SagaStepResponse MapStepToResponse(SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        return SagaStepResponse.FromStep(step);
    }
}

/// <summary>
/// Extended, detail-oriented step response shape (duration in milliseconds,
/// retry/timeout policy fields) kept for consumers that need more detail than
/// the standard <see cref="SagaStepResponse"/> DTO exposes.
/// </summary>
public class SagaStepResponseDetail
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
