// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Metrics collection and reporting service for saga execution statistics.
/// Tracks performance, success rates, and system health metrics.
/// </summary>
public interface IMetricsService
{
    Task<SagaMetrics> GetMetricsAsync();
    Task<StepMetrics> GetStepMetricsAsync(string stepName);
    Task<PerformanceStats> GetPerformanceStatsAsync();
}

public class MetricsService : IMetricsService
{
    private readonly ISagaRepository _sagaRepository;
    private readonly ISagaStepRepository _stepRepository;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(
        ISagaRepository sagaRepository,
        ISagaStepRepository stepRepository,
        ILogger<MetricsService> logger)
    {
        _sagaRepository = sagaRepository ?? throw new ArgumentNullException(nameof(sagaRepository));
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SagaMetrics> GetMetricsAsync()
    {
        try
        {
            var allSagas = await _sagaRepository.GetAllAsync();
            var completed = allSagas.Count(s => s.Status.ToString() == "Completed");
            var failed = allSagas.Count(s => s.Status.ToString() == "Failed");
            var running = allSagas.Count(s => s.Status.ToString() == "Running");
            var compensated = allSagas.Count(s => s.Status.ToString() == "Compensated");

            var completedSagas = allSagas.Where(s => s.Status.ToString() == "Completed").ToList();
            var avgDuration = completedSagas.Any()
                ? completedSagas.Average(s => (s.CompletedAt ?? DateTime.UtcNow - s.CreatedAt).TotalSeconds)
                : 0;

            return new SagaMetrics
            {
                TotalSagas = allSagas.Count,
                CompletedSagas = completed,
                FailedSagas = failed,
                RunningSagas = running,
                CompensatedSagas = compensated,
                SuccessRate = allSagas.Count > 0 ? (double)completed / allSagas.Count * 100 : 0,
                FailureRate = allSagas.Count > 0 ? (double)failed / allSagas.Count * 100 : 0,
                AverageDurationSeconds = avgDuration,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating saga metrics");
            throw;
        }
    }

    public async Task<StepMetrics> GetStepMetricsAsync(string stepName)
    {
        try
        {
            var allSagas = await _sagaRepository.GetAllAsync();
            var allSteps = allSagas.SelectMany(s => s.Steps).Where(s => s.Name == stepName).ToList();

            var completed = allSteps.Count(s => s.Status.ToString() == "Completed");
            var failed = allSteps.Count(s => s.Status.ToString() == "Failed");
            var avgDuration = allSteps.Where(s => s.Status.ToString() == "Completed").Any()
                ? allSteps.Where(s => s.Status.ToString() == "Completed")
                    .Average(s => (s.CompletedAt ?? DateTime.UtcNow - s.StartedAt).TotalMilliseconds)
                : 0;

            return new StepMetrics
            {
                StepName = stepName,
                TotalExecutions = allSteps.Count,
                SuccessfulExecutions = completed,
                FailedExecutions = failed,
                SuccessRate = allSteps.Count > 0 ? (double)completed / allSteps.Count * 100 : 0,
                AverageDurationMs = avgDuration,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating step metrics for {StepName}", stepName);
            throw;
        }
    }

    public async Task<PerformanceStats> GetPerformanceStatsAsync()
    {
        try
        {
            var metrics = await GetMetricsAsync();
            var allSagas = await _sagaRepository.GetAllAsync();

            var completedSagas = allSagas.Where(s => s.Status.ToString() == "Completed").ToList();
            var minDuration = completedSagas.Any()
                ? completedSagas.Min(s => (s.CompletedAt ?? DateTime.UtcNow - s.CreatedAt).TotalSeconds)
                : 0;
            var maxDuration = completedSagas.Any()
                ? completedSagas.Max(s => (s.CompletedAt ?? DateTime.UtcNow - s.CreatedAt).TotalSeconds)
                : 0;

            return new PerformanceStats
            {
                AverageDurationSeconds = metrics.AverageDurationSeconds,
                MinDurationSeconds = minDuration,
                MaxDurationSeconds = maxDuration,
                MedianDurationSeconds = GetMedian(completedSagas.Select(s =>
                    (s.CompletedAt ?? DateTime.UtcNow - s.CreatedAt).TotalSeconds).ToList()),
                P95DurationSeconds = GetPercentile(completedSagas.Select(s =>
                    (s.CompletedAt ?? DateTime.UtcNow - s.CreatedAt).TotalSeconds).ToList(), 95),
                P99DurationSeconds = GetPercentile(completedSagas.Select(s =>
                    (s.CompletedAt ?? DateTime.UtcNow - s.CreatedAt).TotalSeconds).ToList(), 99),
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating performance stats");
            throw;
        }
    }

    private double GetMedian(List<double> values)
    {
        if (values.Count == 0) return 0;
        values.Sort();
        int mid = values.Count / 2;
        return values.Count % 2 == 0 ? (values[mid - 1] + values[mid]) / 2 : values[mid];
    }

    private double GetPercentile(List<double> values, int percentile)
    {
        if (values.Count == 0) return 0;
        values.Sort();
        int index = (int)Math.Ceiling(values.Count * percentile / 100.0) - 1;
        return values[Math.Max(0, Math.Min(index, values.Count - 1))];
    }
}

public class SagaMetrics
{
    public int TotalSagas { get; set; }
    public int CompletedSagas { get; set; }
    public int FailedSagas { get; set; }
    public int RunningSagas { get; set; }
    public int CompensatedSagas { get; set; }
    public double SuccessRate { get; set; }
    public double FailureRate { get; set; }
    public double AverageDurationSeconds { get; set; }
    public DateTime Timestamp { get; set; }
}

public class StepMetrics
{
    public string StepName { get; set; } = string.Empty;
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public double SuccessRate { get; set; }
    public double AverageDurationMs { get; set; }
    public DateTime Timestamp { get; set; }
}

public class PerformanceStats
{
    public double AverageDurationSeconds { get; set; }
    public double MinDurationSeconds { get; set; }
    public double MaxDurationSeconds { get; set; }
    public double MedianDurationSeconds { get; set; }
    public double P95DurationSeconds { get; set; }
    public double P99DurationSeconds { get; set; }
    public DateTime Timestamp { get; set; }
}
