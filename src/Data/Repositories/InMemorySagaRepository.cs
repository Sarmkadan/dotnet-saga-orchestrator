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
    private readonly Dictionary<string, Saga> _sagas = new();
    private readonly object _lockObject = new();

    private Saga CopySaga(Saga saga)
    {
        if (saga == null) return null;
        var options = new JsonSerializerOptions { WriteIndented = false };
        var json = JsonSerializer.Serialize(saga, options);
        return JsonSerializer.Deserialize<Saga>(json);
    }

    public async Task<Saga?> GetByIdAsync(string id)
    {
        await Task.Yield();

        lock (_lockObject)
        {
            if (_sagas.TryGetValue(id, out var saga))
                return CopySaga(saga);
            return null;
        }
    }

    public async Task<Saga?> GetByCorrelationIdAsync(string correlationId)
    {
        await Task.Yield();

        lock (_lockObject)
        {
            var saga = _sagas.Values.FirstOrDefault(s => s.CorrelationId == correlationId);
            return CopySaga(saga);
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

            _sagas[saga.Id] = saga;
            return saga;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        await Task.Yield();

        lock (_lockObject)
        {
            return _sagas.Remove(id);
        }
    }

    public async Task<List<Saga>> GetAllAsync()
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _sagas.Values.Select(CopySaga).ToList();
        }
    }

    public async Task<List<Saga>> GetByStatusAsync(SagaStatus status)
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _sagas.Values.Where(s => s.Status == status).Select(CopySaga).ToList();
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

            return results.Select(CopySaga).ToList();
        }
    }
}
