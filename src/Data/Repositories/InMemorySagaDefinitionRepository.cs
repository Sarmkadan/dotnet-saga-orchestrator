// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// In-memory implementation of saga definition repository.
/// </summary>
public class InMemorySagaDefinitionRepository : ISagaDefinitionRepository
{
    private readonly Dictionary<string, SagaDefinition> _definitions = new();
    private readonly object _lockObject = new();

    public async Task<SagaDefinition?> GetByIdAsync(string id)
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _definitions.TryGetValue(id, out var def) ? def : null;
        }
    }

    public async Task<SagaDefinition?> GetByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        await Task.Yield();

        lock (_lockObject)
        {
            return _definitions.Values.FirstOrDefault(d => d.Name == name);
        }
    }

    public async Task<SagaDefinition?> CreateAsync(SagaDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        await Task.Yield();

        lock (_lockObject)
        {
            if (_definitions.ContainsKey(definition.Id))
                throw new InvalidOperationException($"Definition with ID '{definition.Id}' already exists");

            _definitions[definition.Id] = definition;
            return definition;
        }
    }

    public async Task<SagaDefinition?> UpdateAsync(SagaDefinition definition)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));

        await Task.Yield();

        lock (_lockObject)
        {
            if (!_definitions.ContainsKey(definition.Id))
                return null;

            _definitions[definition.Id] = definition;
            return definition;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        await Task.Yield();

        lock (_lockObject)
        {
            return _definitions.Remove(id);
        }
    }

    public async Task<List<SagaDefinition>> GetAllAsync()
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return new List<SagaDefinition>(_definitions.Values);
        }
    }

    public async Task<List<SagaDefinition>> GetActiveAsync()
    {
        await Task.Yield();

        lock (_lockObject)
        {
            return _definitions.Values.Where(d => d.IsActive).ToList();
        }
    }

    public async Task<List<SagaDefinition>> SearchAsync(Dictionary<string, object> criteria)
    {
        if (criteria == null)
            throw new ArgumentNullException(nameof(criteria));

        await Task.Yield();

        lock (_lockObject)
        {
            var results = _definitions.Values.AsEnumerable();

            if (criteria.TryGetValue("name", out var nameObj) && nameObj is string name)
            {
                results = results.Where(d => d.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            if (criteria.TryGetValue("activeOnly", out var activeObj) && activeObj is bool active)
            {
                results = results.Where(d => d.IsActive == active);
            }

            if (criteria.TryGetValue("version", out var versionObj) && versionObj is int version)
            {
                results = results.Where(d => d.Version == version);
            }

            return results.ToList();
        }
    }
}
