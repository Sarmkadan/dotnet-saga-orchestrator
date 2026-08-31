#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// In-memory implementation of saga repository for development and testing.
/// </summary>
public class InMemorySagaRepository : ISagaRepository
{
    private static readonly JsonSerializerOptions CopyOptions = new() { WriteIndented = false };

    private readonly Dictionary<string, Saga> _sagas = new();
    private readonly Dictionary<string, string> _correlationIndex = new(); // correlationId -> sagaId
    private readonly object _lockObject = new();

    public InMemorySagaRepository()
    {
        _correlationIndex = new Dictionary<string, string>();
    }

    private static Saga? CopySaga(Saga? saga)
    {
        if (saga == null) return null;
        var json = JsonSerializer.Serialize(saga, CopyOptions);
        return JsonSerializer.Deserialize<Saga>(json);
    }

    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    public async Task<Saga?> GetByIdAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        await Task.Yield();

        lock (_lockObject)
        {
            if (_sagas.TryGetValue(id, out var saga))
                return CopySaga(saga);
            return null;
        }
    }

    /// <exception cref="ArgumentException">Thrown when <paramref name="correlationId"/> is null or empty.</exception>
    public async Task<Saga?> GetByCorrelationIdAsync(string correlationId)
    {
        ArgumentException.ThrowIfNullOrEmpty(correlationId);
        await Task.Yield();

        lock (_lockObject)
        {
            if (_correlationIndex.TryGetValue(correlationId, out var sagaId) &&
                _sagas.TryGetValue(sagaId, out var saga))
            {
                return CopySaga(saga);
            }
            return null;
        }
    }

    public async Task<Saga?> CreateAsync(Saga saga)
    {
        if (saga == null)
            throw new ArgumentNullException(nameof(saga));

        await Task.Yield();

        lock (_lockObject)
        {
            if (_sagas.ContainsKey(saga.Id))
                throw new InvalidOperationException($"Saga with ID '{saga.Id}' already exists");

            _sagas[saga.Id] = saga;

            if (!string.IsNullOrEmpty(saga.CorrelationId))
            {
                _correlationIndex[saga.CorrelationId] = saga.Id;
            }

            return saga;
        }
    }

    public async Task<Saga?> UpdateAsync(Saga saga)
    {
        if (saga == null)
            throw new ArgumentNullException(nameof(saga));

        await Task.Yield();

        lock (_lockObject)
        {
            if (!_sagas.ContainsKey(saga.Id))
                return null;

            var existingSaga = _sagas[saga.Id];
            var oldCorrelationId = existingSaga.CorrelationId;
            var newCorrelationId = saga.CorrelationId;

            _sagas[saga.Id] = saga;

            // Update correlation index if correlationId changed
            if (oldCorrelationId != newCorrelationId)
            {
                // Remove old mapping if it existed
                if (!string.IsNullOrEmpty(oldCorrelationId))
                {
                    _correlationIndex.Remove(oldCorrelationId);
                }

                // Add new mapping if it is non-empty
                if (!string.IsNullOrEmpty(newCorrelationId))
                {
                    _correlationIndex[newCorrelationId] = saga.Id;
                }
            }

            return saga;
        }
    }

    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    public async Task<bool> DeleteAsync(string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);

        await Task.Yield();

        lock (_lockObject)
        {
            if (_sagas.TryGetValue(id, out var saga))
            {
                // Remove from correlation index if present
                if (!string.IsNullOrEmpty(saga.CorrelationId))
                {
                    _correlationIndex.Remove(saga.CorrelationId);
                }

                return _sagas.Remove(id);
            }

            return false;
        }
    }

    public async Task<List<Saga>> GetAllAsync()
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _sagas.Values.Select(saga => CopySaga(saga)!).ToList();
        }
    }

    public async Task<List<Saga>> GetByStatusAsync(SagaStatus status)
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _sagas.Values.Where(s => s.Status == status).Select(saga => CopySaga(saga)!).ToList();
        }
    }

    public async Task<List<Saga>> SearchAsync(Dictionary<string, object> criteria)
    {
        if (criteria == null)
            throw new ArgumentNullException(nameof(criteria));

        await Task.Yield();

        lock (_lockObject)
        {
            var results = _sagas.Values.AsEnumerable();

            if (criteria.TryGetValue("status", out var statusObj) && statusObj is SagaStatus status)
            {
                results = results.Where(s => s.Status == status);
            }

            if (criteria.TryGetValue("definitionId", out var defIdObj) && defIdObj is string defId)
            {
                results = results.Where(s => s.Definition.Id == defId);
            }

            if (criteria.TryGetValue("startDateFrom", out var fromObj) && fromObj is DateTime fromDate)
            {
                results = results.Where(s => s.StartedAt >= fromDate);
            }

            if (criteria.TryGetValue("startDateTo", out var toObj) && toObj is DateTime toDate)
            {
                results = results.Where(s => s.StartedAt <= toDate);
            }

            return results.Select(saga => CopySaga(saga)!).ToList();
        }
    }
}
