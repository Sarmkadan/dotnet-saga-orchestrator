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
/// In-memory implementation of compensation transaction repository.
/// </summary>
public class InMemoryCompensationTransactionRepository : ICompensationTransactionRepository
{
    private readonly Dictionary<string, CompensationTransaction> _compensations = new();
    private readonly object _lockObject = new();

    public async Task<CompensationTransaction?> GetByIdAsync(string id)
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _compensations.TryGetValue(id, out var comp) ? comp : null;
        }
    }

    public async Task<CompensationTransaction?> CreateAsync(CompensationTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        await Task.Yield();

        lock (_lockObject)
        {
            if (_compensations.ContainsKey(transaction.Id))
                throw new InvalidOperationException($"Compensation with ID '{transaction.Id}' already exists");

            _compensations[transaction.Id] = transaction;
            return transaction;
        }
    }

    public async Task<CompensationTransaction?> UpdateAsync(CompensationTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        await Task.Yield();

        lock (_lockObject)
        {
            if (!_compensations.ContainsKey(transaction.Id))
                return null;

            _compensations[transaction.Id] = transaction;
            return transaction;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        await Task.Yield();

        lock (_lockObject)
        {
            return _compensations.Remove(id);
        }
    }

    public async Task<List<CompensationTransaction>> GetBySagaIdAsync(string sagaId)
    {
        if (string.IsNullOrEmpty(sagaId))
            return new List<CompensationTransaction>();

        await Task.Yield();

        lock (_lockObject)
        {
            return _compensations.Values
                .Where(c => c.SagaId == sagaId)
                .OrderByDescending(c => c.Order)
                .ToList();
        }
    }

    public async Task<List<CompensationTransaction>> GetAllAsync()
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return new List<CompensationTransaction>(_compensations.Values);
        }
    }

    public async Task<List<CompensationTransaction>> GetByStatusAsync(CompensationStatus status)
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _compensations.Values.Where(c => c.Status == status).ToList();
        }
    }
}
