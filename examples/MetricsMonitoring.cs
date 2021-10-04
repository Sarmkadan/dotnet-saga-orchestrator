#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Models;

/// Metrics and monitoring example
/// Demonstrates: gathering performance metrics, health checks, and real-time monitoring
public class MetricsMonitoringExample
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });
        services.AddSagaOrchestrator();

        var serviceProvider = services.BuildServiceProvider();
        var definitionService = serviceProvider.GetRequiredService<SagaDefinitionService>();
        var orchestrationService = serviceProvider.GetRequiredService<SagaOrchestrationService>();
        var healthCheckService = serviceProvider.GetRequiredService<HealthCheckService>();
        var metricsService = serviceProvider.GetRequiredService<MetricsService>();
        var logger = serviceProvider.GetRequiredService<ILogger<MetricsMonitoringExample>>();

        try
        {
            logger.LogInformation("=== Metrics and Monitoring Example ===\n");

            // Check system health
            logger.LogInformation("System Health Check:");
            var health = await healthCheckService.GetHealthAsync().ConfigureAwait(false);
            logger.LogInformation("  Service Status: {ServiceStatus}", health.ServiceStatus);
            logger.LogInformation("  Active Sagas: {ActiveSagaCount}", health.ActiveSagaCount);
            logger.LogInformation("  Uptime: {Uptime}\n", health.Uptime);

            // Create a test saga for metrics
            var definition = await definitionService.CreateDefinitionAsync(
                "Metrics Test",
                "Saga for testing metrics");

            var step1 = new SagaStepDefinition(
                "Step 1",
                "service1",
                "http://localhost:5001/api/step1",
                "http://localhost:5001/api/step1/undo");
            step1.SetTimeout(30);
            step1.SetRetryPolicy(2, 1000);
            await definitionService.AddStepAsync(definition.Id, step1).ConfigureAwait(false);

            var step2 = new SagaStepDefinition(
                "Step 2",
                "service2",
                "http://localhost:5002/api/step2",
                "http://localhost:5002/api/step2/undo");
            step2.SetTimeout(30);
            step2.SetRetryPolicy(2, 1000);
            await definitionService.AddStepAsync(definition.Id, step2).ConfigureAwait(false);

            var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id).ConfigureAwait(false);

            // Create and execute multiple sagas
            logger.LogInformation("Creating test sagas...");
            var sagaIds = new List<string>();

            for (int i = 0; i < 5; i++)
            {
                var saga = await orchestrationService.CreateSagaAsync(
                    retrievedDef,
                    maxRetries: 3,
                    timeoutSeconds: 300);

                await orchestrationService.StartSagaAsync(saga.Id).ConfigureAwait(false);
                sagaIds.Add(saga.Id);

                // Execute steps
                for (int j = 0; j < 2; j++)
                {
                    await orchestrationService.ExecuteNextStepAsync(saga.Id).ConfigureAwait(false);
                }

                logger.LogInformation($"✓ Completed saga {i + 1}/5: {saga.Id}");
            }

            logger.LogInformation("\n✓ All test sagas completed\n");

            // Gather and display metrics
            logger.LogInformation("=== Performance Metrics ===\n");

            var metrics = metricsService.GetMetrics();

            logger.LogInformation("Saga Statistics:");
            logger.LogInformation("  Total Sagas: {TotalSagas}", metrics.TotalSagas);
            logger.LogInformation("  Completed: {CompletedSagas}", metrics.CompletedSagas);
            logger.LogInformation("  Failed: {FailedSagas}", metrics.FailedSagas);
            logger.LogInformation("  Success Rate: {SuccessRate}", metrics.SuccessRate);
            logger.LogInformation("  Failure Rate: {FailureRate}\n", metrics.FailureRate);

            logger.LogInformation("Duration Metrics (ms):");
            logger.LogInformation("  Minimum: {MinDurationMs}", metrics.MinDurationMs);
            logger.LogInformation("  Maximum: {MaxDurationMs}", metrics.MaxDurationMs);
            logger.LogInformation("  Average: {AverageDurationMs}", metrics.AverageDurationMs);
            logger.LogInformation("  Median: {MedianDurationMs}\n", metrics.MedianDurationMs);

            // Performance percentiles
            logger.LogInformation("Performance Percentiles:");
            if (metrics.P50DurationMs.HasValue)
                logger.LogInformation("  P50 (Median): {P50DurationMs}ms", metrics.P50DurationMs);
            if (metrics.P95DurationMs.HasValue)
                logger.LogInformation("  P95: {P95DurationMs}ms", metrics.P95DurationMs);
            if (metrics.P99DurationMs.HasValue)
                logger.LogInformation("  P99: {P99DurationMs}ms\n", metrics.P99DurationMs);

            // List all sagas with their details
            logger.LogInformation("=== Completed Sagas ===\n");

            var allSagas = await orchestrationService.ListSagasAsync().ConfigureAwait(false);

            foreach (var saga in allSagas)
            {
                var duration = saga.CompletedAt.HasValue && saga.StartedAt.HasValue
                    ? (saga.CompletedAt.Value - saga.StartedAt.Value).TotalSeconds
                    : 0;

                logger.LogInformation("Saga: {Id}", saga.Id);
                logger.LogInformation("  Status: {Status}", saga.Status);
                logger.LogInformation("  Created: {CreatedAt}", saga.CreatedAt);
                logger.LogInformation("  Duration: {Duration}s", duration);
                logger.LogInformation("  Steps: {Count}", saga.Steps.Count);
                logger.LogInformation($"  Completed: {saga.Steps.Count(s => s.Status == SagaStepStatus.Completed)}");
            }

            logger.LogInformation("\n✓ Monitoring example completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during monitoring");
        }
    }
}
