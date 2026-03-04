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
/// Repository interface for saga step persistence operations.
/// </summary>
public interface ISagaStepRepository
{
    Task<SagaStep?> GetByIdAsync(string id);
    Task<SagaStep?> CreateAsync(SagaStep step);
    Task<SagaStep?> UpdateAsync(SagaStep step);
    Task<bool> DeleteAsync(string id);
    Task<List<SagaStep>> GetBySagaIdAsync(string sagaId);
    Task<List<SagaStep>> GetAllAsync();
    Task<SagaStep?> GetByOrderAsync(string sagaId, int order);
    Task<List<SagaStep>> GetByStatusAsync(Core.Domain.Enums.SagaStepStatus status);
}
