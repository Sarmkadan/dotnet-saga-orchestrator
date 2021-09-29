#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.DTOs;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Health check service for monitoring saga orchestrator and dependent services.
/// Provides system status, metrics, and diagnostics.
/// </summary>
public interface IHealthCheckService
{
    Task<HealthCheckResponse> CheckHealthAsync();
    Task<Dictionary<string, object>> GetMetricsAsync();
}

public class HealthCheckService : IHealthCheckService
{
    private readonly ISagaRepository _sagaRepository;
    private readonly IServiceRegistry _serviceRegistry;
    private readonly ILogger<HealthCheckService> _logger;
    private readonly DateTime _startTime;

    public HealthCheckService(
        ISagaRepository sagaRepository,
        IServiceRegistry serviceRegistry,
        ILogger<HealthCheckService> logger)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _startTime = DateTime.UtcNow;
    }

    public async Task<HealthCheckResponse> CheckHealthAsync()
    {
        try
        {
            var allSagas = await _sagaRepository.GetAllAsync();
            var activeSagas = allSagas.Count(s => s.Status.ToString() == "Running" || s.Status.ToString() == "Compensating");
            var services = await _serviceRegistry.GetAllServicesAsync();
            var unhealthyServices = services.Count(s => !s.IsHealthy);

            var status = unhealthyServices > 0 ? "degraded" : "healthy";

            return new HealthCheckResponse
            {
                Status = status,
                Timestamp = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - _startTime,
                ActiveSagas = activeSagas,
                TotalSagas = allSagas.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return new HealthCheckResponse
            {
                Status = "unhealthy",
                Timestamp = DateTime.UtcNow,
                Uptime = DateTime.UtcNow - _startTime
            };
        }
    }

    public async Task<Dictionary<string, object>> GetMetricsAsync()
    {
        try
        {
            var allSagas = await _sagaRepository.GetAllAsync();
            var completed = allSagas.Count(s => s.Status.ToString() == "Completed");
            var failed = allSagas.Count(s => s.Status.ToString() == "Failed");
            var running = allSagas.Count(s => s.Status.ToString() == "Running");
            var services = await _serviceRegistry.GetAllServicesAsync();

            var metrics = new Dictionary<string, object>
            {
                ["totalSagas"] = allSagas.Count,
                ["completedSagas"] = completed,
                ["failedSagas"] = failed,
                ["runningSagas"] = running,
                ["successRate"] = allSagas.Count > 0 ? (double)completed / allSagas.Count * 100 : 0,
                ["registeredServices"] = services.Count,
                ["healthyServices"] = services.Count(s => s.IsHealthy),
                ["unhealthyServices"] = services.Count(s => !s.IsHealthy),
                ["uptime"] = (DateTime.UtcNow - _startTime).TotalSeconds,
                ["averageCompletionTime"] = CalculateAverageCompletionTime(allSagas.Where(s => s.Status.ToString() == "Completed").ToList()),
                ["memoryUsage"] = GC.GetTotalMemory(false) / (1024 * 1024), // MB
                ["timestamp"] = DateTime.UtcNow
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get metrics");
            return new();
        }
    }

    private double CalculateAverageCompletionTime(List<Core.Domain.Models.Saga> completedSagas)
    {
        if (completedSagas.Count == 0)
            return 0;

        var totalMs = completedSagas.Sum(s => (s.CompletedAt ?? DateTime.UtcNow - s.CreatedAt).TotalMilliseconds);
        return totalMs / completedSagas.Count;
    }
}
