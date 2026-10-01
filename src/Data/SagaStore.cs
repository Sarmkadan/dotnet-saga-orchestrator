#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// High-level store that provides saga snapshot persistence, history tracking,
/// and aggregate queries on top of the underlying repositories.
/// </summary>
public class SagaStore : IDisposable
{
    private readonly ISagaRepository _sagaRepository;
    private readonly ISagaStepRepository _stepRepository;
    private readonly ILogger<SagaStore> _logger;
    private readonly ConcurrentDictionary<string, List<string>> _snapshotHistory = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SagaStore"/> class.
    /// </summary>
    public SagaStore(
        ISagaRepository sagaRepository,
        ISagaStepRepository stepRepository,
        ILogger<SagaStore> logger)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Saves a point-in-time snapshot of the saga state for audit and debugging.
    /// </summary>
    /// <param name="saga">The saga to snapshot.</param>
    public async Task SaveSnapshotAsync(Saga saga)
    {
        ArgumentNullException.ThrowIfNull(saga);

        var snapshot = JsonSerializer.Serialize(saga);
        var history = _snapshotHistory.GetOrAdd(saga.Id, _ => new List<string>());

        lock (history)
        {
            history.Add(snapshot);
        }

        await _sagaRepository.UpdateAsync(saga);
        _logger.LogDebug("Snapshot saved for saga {SagaId}, total snapshots: {Count}", saga.Id, history.Count);
    }

    /// <summary>
    /// Retrieves a saga with all its steps fully loaded.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>The saga with steps populated, or null if not found.</returns>
    public async Task<Saga?> LoadFullSagaAsync(string sagaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        var saga = await _sagaRepository.GetByIdAsync(sagaId);
        if (saga == null) return null;

        saga.Steps = await _stepRepository.GetBySagaIdAsync(sagaId);
        return saga;
    }

    /// <summary>
    /// Returns execution statistics: total, completed, failed, and average duration.
    /// </summary>
    /// <returns>A dictionary of metric names to values.</returns>
    public async Task<Dictionary<string, long>> GetStatisticsAsync()
    {
        var all = await _sagaRepository.GetAllAsync();
        var completed = all.Count(s => s.Status == SagaStatus.Completed);
        var failed = all.Count(s => s.Status is SagaStatus.Failed or SagaStatus.Compensated);

        var avgDurationMs = all
            .Where(s => s.CompletedAt.HasValue && s.CreatedAt != default)
            .Select(s => (long)(s.CompletedAt!.Value - s.CreatedAt).TotalMilliseconds)
            .DefaultIfEmpty(0)
            .Average();

        return new Dictionary<string, long>
        {
            ["total"] = all.Count,
            ["completed"] = completed,
            ["failed"] = failed,
            ["avg_duration_ms"] = (long)avgDurationMs
        };
    }

    /// <summary>
    /// Returns the snapshot history for a saga as serialized JSON strings.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>List of JSON snapshots in chronological order.</returns>
    public List<string> GetSnapshotHistory(string sagaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        if (_snapshotHistory.TryGetValue(sagaId, out var history))
        {
            lock (history)
            {
                return new List<string>(history);
            }
        }

        return new List<string>();
    }

    /// <summary>
    /// Purges snapshot history for completed or compensated sagas older than the given threshold.
    /// </summary>
    /// <param name="olderThan">Age threshold for purging.</param>
    /// <returns>Number of sagas whose history was purged.</returns>
    public async Task<int> PurgeOldSnapshotsAsync(TimeSpan olderThan)
    {
        var cutoff = DateTime.UtcNow - olderThan;
        var all = await _sagaRepository.GetAllAsync();
        var purged = 0;

        foreach (var saga in all.Where(s =>
            s.Status is SagaStatus.Completed or SagaStatus.Compensated &&
            (s.CompletedAt ?? s.StartedAt) < cutoff))
        {
            if (_snapshotHistory.TryRemove(saga.Id, out _))
                purged++;
        }

        _logger.LogInformation("Purged snapshot history for {Count} sagas older than {Threshold}", purged, olderThan);
        return purged;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _snapshotHistory.Clear();
    }
}
