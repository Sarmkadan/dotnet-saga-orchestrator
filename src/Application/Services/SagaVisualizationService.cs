// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Provides real-time visualization snapshots and streaming state updates for saga execution.
/// </summary>
public interface ISagaVisualizationService
{
    /// <summary>Returns a point-in-time visualization snapshot for a specific saga.</summary>
    /// <param name="sagaId">The unique saga identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<SagaVisualizationSnapshot> GetSnapshotAsync(string sagaId, CancellationToken cancellationToken = default);

    /// <summary>Returns visualization snapshots for all sagas currently tracked in the system.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyList<SagaVisualizationSnapshot>> GetAllSnapshotsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams live state updates for a saga by polling at <paramref name="pollInterval"/>,
    /// invoking <paramref name="onUpdate"/> on each tick until a terminal state is reached or the token is cancelled.
    /// </summary>
    /// <param name="sagaId">The unique saga identifier.</param>
    /// <param name="onUpdate">Async callback invoked with each new snapshot.</param>
    /// <param name="pollInterval">Interval between state polls.</param>
    /// <param name="cancellationToken">Token to cancel the stream.</param>
    Task StreamLiveStateAsync(
        string sagaId,
        Func<SagaVisualizationSnapshot, Task> onUpdate,
        TimeSpan pollInterval,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Builds visualization snapshots from repository state and supports real-time live streaming
/// for saga execution monitoring.
/// </summary>
public class SagaVisualizationService : ISagaVisualizationService
{
    private readonly ISagaRepository _sagaRepository;
    private readonly ILogger<SagaVisualizationService> _logger;

    /// <summary>Initializes a new instance of <see cref="SagaVisualizationService"/>.</summary>
    public SagaVisualizationService(
        ISagaRepository sagaRepository,
        ILogger<SagaVisualizationService> logger)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<SagaVisualizationSnapshot> GetSnapshotAsync(
        string sagaId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must not be empty.", nameof(sagaId));

        try
        {
            var saga = await _sagaRepository.GetByIdAsync(sagaId);
            if (saga == null)
                throw new KeyNotFoundException($"Saga '{sagaId}' not found.");

            return BuildSnapshot(saga);
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build visualization snapshot for saga {SagaId}", sagaId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<SagaVisualizationSnapshot>> GetAllSnapshotsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sagas = await _sagaRepository.GetAllAsync();
            return sagas.Select(BuildSnapshot).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build saga visualization snapshots");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task StreamLiveStateAsync(
        string sagaId,
        Func<SagaVisualizationSnapshot, Task> onUpdate,
        TimeSpan pollInterval,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sagaId))
            throw new ArgumentException("Saga ID must not be empty.", nameof(sagaId));
        if (onUpdate == null)
            throw new ArgumentNullException(nameof(onUpdate));

        _logger.LogDebug("Starting live state stream for saga {SagaId} at {IntervalMs}ms interval",
            sagaId, pollInterval.TotalMilliseconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var snapshot = await GetSnapshotAsync(sagaId, cancellationToken);
                await onUpdate(snapshot);

                if (snapshot.IsTerminal)
                    break;

                await Task.Delay(pollInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during live state stream for saga {SagaId}; stopping stream", sagaId);
                break;
            }
        }

        _logger.LogDebug("Live state stream ended for saga {SagaId}", sagaId);
    }

    private static SagaVisualizationSnapshot BuildSnapshot(Saga saga)
    {
        var nodes = saga.Steps
            .OrderBy(s => s.Order)
            .Select((step, i) => new VisualizationNode
            {
                Index = i + 1,
                StepId = step.Id,
                Name = step.Name,
                Status = step.Status.ToString(),
                StartedAt = step.StartedAt,
                CompletedAt = step.CompletedAt,
                RetryCount = step.RetryCount,
                ErrorMessage = step.ErrorMessage,
                DurationMs = step.CompletedAt.HasValue && step.StartedAt.HasValue
                    ? (step.CompletedAt.Value - step.StartedAt.Value).TotalMilliseconds
                    : null
            })
            .ToList();

        var completedCount = saga.Steps.Count(s => s.Status == SagaStepStatus.Completed);
        var totalCount = saga.Steps.Count;
        var terminalAt = saga.CompletedAt ?? saga.FailedAt;
        var elapsed = (terminalAt ?? DateTime.UtcNow) - saga.StartedAt;

        return new SagaVisualizationSnapshot
        {
            SagaId = saga.Id,
            CorrelationId = saga.CorrelationId,
            SagaName = saga.Definition?.Name ?? saga.Id,
            Status = saga.Status.ToString(),
            Nodes = nodes,
            CompletedSteps = completedCount,
            TotalSteps = totalCount,
            ProgressPercent = totalCount > 0 ? (double)completedCount / totalCount * 100 : 0,
            ElapsedMs = elapsed.TotalMilliseconds,
            FailureReason = saga.FailureReason,
            CapturedAt = DateTime.UtcNow,
            IsTerminal = saga.Status is SagaStatus.Completed or SagaStatus.Compensated
                         or SagaStatus.Failed or SagaStatus.Aborted or SagaStatus.TimedOut
        };
    }
}

/// <summary>Point-in-time snapshot capturing a saga's execution state and ordered step graph.</summary>
public class SagaVisualizationSnapshot
{
    /// <summary>Unique identifier of the saga.</summary>
    [JsonPropertyName("sagaId")]
    public string SagaId { get; set; } = string.Empty;

    /// <summary>Correlation identifier used for cross-service tracing.</summary>
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>Human-readable saga name derived from its definition.</summary>
    [JsonPropertyName("sagaName")]
    public string SagaName { get; set; } = string.Empty;

    /// <summary>Current saga status as a string label.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>Ordered list of step nodes representing the state graph.</summary>
    [JsonPropertyName("nodes")]
    public List<VisualizationNode> Nodes { get; set; } = new();

    /// <summary>Number of steps that have completed successfully.</summary>
    [JsonPropertyName("completedSteps")]
    public int CompletedSteps { get; set; }

    /// <summary>Total number of steps in this saga.</summary>
    [JsonPropertyName("totalSteps")]
    public int TotalSteps { get; set; }

    /// <summary>Completion percentage in the range 0–100.</summary>
    [JsonPropertyName("progressPercent")]
    public double ProgressPercent { get; set; }

    /// <summary>Total elapsed time in milliseconds since the saga started.</summary>
    [JsonPropertyName("elapsedMs")]
    public double ElapsedMs { get; set; }

    /// <summary>Failure reason message, populated only when the saga has failed.</summary>
    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; set; }

    /// <summary>UTC timestamp when this snapshot was captured.</summary>
    [JsonPropertyName("capturedAt")]
    public DateTime CapturedAt { get; set; }

    /// <summary>
    /// Indicates the saga has reached a terminal state and no further state changes are expected.
    /// </summary>
    [JsonPropertyName("isTerminal")]
    public bool IsTerminal { get; set; }
}

/// <summary>Represents a single step node in the saga state graph with its current execution details.</summary>
public class VisualizationNode
{
    /// <summary>One-based position of this step within the saga's ordered sequence.</summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>Unique identifier of the step instance.</summary>
    [JsonPropertyName("stepId")]
    public string StepId { get; set; } = string.Empty;

    /// <summary>Step name as declared in the saga definition.</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Current execution status of the step.</summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the step began executing, or null if not yet started.</summary>
    [JsonPropertyName("startedAt")]
    public DateTime? StartedAt { get; set; }

    /// <summary>UTC timestamp when the step finished (success or failure), or null if still in progress.</summary>
    [JsonPropertyName("completedAt")]
    public DateTime? CompletedAt { get; set; }

    /// <summary>Number of retry attempts made for this step.</summary>
    [JsonPropertyName("retryCount")]
    public int RetryCount { get; set; }

    /// <summary>Error message captured when the step failed, or null on success.</summary>
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>Execution duration in milliseconds, or null if the step has not completed.</summary>
    [JsonPropertyName("durationMs")]
    public double? DurationMs { get; set; }
}
