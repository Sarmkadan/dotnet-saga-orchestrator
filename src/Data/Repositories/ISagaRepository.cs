#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Repository interface for saga persistence operations.
/// </summary>
public interface ISagaRepository
{
    Task<Saga?> GetByIdAsync(string id);
    Task<Saga?> GetByCorrelationIdAsync(string correlationId);
    Task<Saga?> CreateAsync(Saga saga);
    Task<Saga?> UpdateAsync(Saga saga);
    Task<bool> DeleteAsync(string id);
    Task<List<Saga>> GetAllAsync();
    Task<List<Saga>> GetByStatusAsync(Core.Domain.Enums.SagaStatus status);
    Task<List<Saga>> SearchAsync(Dictionary<string, object> criteria);
}
