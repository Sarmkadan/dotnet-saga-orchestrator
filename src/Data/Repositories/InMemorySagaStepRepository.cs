#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// In-memory implementation of saga step repository.
/// </summary>
public class InMemorySagaStepRepository : ISagaStepRepository
{
    private readonly Dictionary<string, SagaStep> _steps = new();
    private readonly object _lockObject = new();

    public async Task<SagaStep?> GetByIdAsync(string id)
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _steps.TryGetValue(id, out var step) ? step : null;
        }
    }

    public async Task<SagaStep?> CreateAsync(SagaStep step)
    {
        if (step == null)
            throw new ArgumentNullException(nameof(step));

        await Task.Yield();

        lock (_lockObject)
        {
            if (_steps.ContainsKey(step.Id))
                throw new InvalidOperationException($"Step with ID '{step.Id}' already exists");

            _steps[step.Id] = step;
            return step;
        }
    }

    public async Task<SagaStep?> UpdateAsync(SagaStep step)
    {
        if (step == null)
            throw new ArgumentNullException(nameof(step));

        await Task.Yield();

        lock (_lockObject)
        {
            if (!_steps.ContainsKey(step.Id))
                return null;

            _steps[step.Id] = step;
            return step;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        await Task.Yield();

        lock (_lockObject)
        {
            return _steps.Remove(id);
        }
    }

    public async Task<List<SagaStep>> GetBySagaIdAsync(string sagaId)
    {
        if (string.IsNullOrEmpty(sagaId))
            return new List<SagaStep>();

        await Task.Yield();

        lock (_lockObject)
        {
            return _steps.Values.Where(s => s.SagaId == sagaId).OrderBy(s => s.Order).ToList();
        }
    }

    public async Task<List<SagaStep>> GetAllAsync()
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return new List<SagaStep>(_steps.Values);
        }
    }

    public async Task<SagaStep?> GetByOrderAsync(string sagaId, int order)
    {
        if (string.IsNullOrEmpty(sagaId))
            return null;

        await Task.Yield();

        lock (_lockObject)
        {
            return _steps.Values.FirstOrDefault(s => s.SagaId == sagaId && s.Order == order);
        }
    }

    public async Task<List<SagaStep>> GetByStatusAsync(SagaStepStatus status)
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _steps.Values.Where(s => s.Status == status).ToList();
        }
    }
}
