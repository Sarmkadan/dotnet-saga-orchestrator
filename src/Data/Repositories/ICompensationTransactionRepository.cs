// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories;

/// <summary>
/// Repository interface for compensation transaction persistence.
/// </summary>
public interface ICompensationTransactionRepository
{
    Task<CompensationTransaction?> GetByIdAsync(string id);
    Task<CompensationTransaction?> CreateAsync(CompensationTransaction transaction);
    Task<CompensationTransaction?> UpdateAsync(CompensationTransaction transaction);
    Task<bool> DeleteAsync(string id);
    Task<List<CompensationTransaction>> GetBySagaIdAsync(string sagaId);
    Task<List<CompensationTransaction>> GetAllAsync();
    Task<List<CompensationTransaction>> GetByStatusAsync(Core.Domain.Enums.CompensationStatus status);
}
