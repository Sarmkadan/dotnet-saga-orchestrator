// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Repository interface for saga definition persistence.
/// </summary>
public interface ISagaDefinitionRepository
{
    Task<SagaDefinition?> GetByIdAsync(string id);
    Task<SagaDefinition?> GetByNameAsync(string name);
    Task<SagaDefinition?> CreateAsync(SagaDefinition definition);
    Task<SagaDefinition?> UpdateAsync(SagaDefinition definition);
    Task<bool> DeleteAsync(string id);
    Task<List<SagaDefinition>> GetAllAsync();
    Task<List<SagaDefinition>> GetActiveAsync();
    Task<List<SagaDefinition>> SearchAsync(Dictionary<string, object> criteria);
}
