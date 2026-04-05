#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

/// Compensation strategies example
/// Demonstrates: different strategies for rolling back failed sagas
public class CompensationStrategiesExample
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
        var logger = serviceProvider.GetRequiredService<ILogger<CompensationStrategiesExample>>();

        try
        {
            logger.LogInformation("=== Compensation Strategies Example ===\n");

            // Create a definition with multiple steps
            var definition = await definitionService.CreateDefinitionAsync(
                "Compensation Strategy Test",
                "Test different compensation strategies");

            // Add steps
            var step1 = new SagaStepDefinition(
                "Step 1: Database Update",
                "db-service",
                "http://localhost:8001/api/db/create",
                "http://localhost:8001/api/db/delete");
            step1.SetTimeout(30);
            step1.SetRetryPolicy(2, 1000);
            await definitionService.AddStepAsync(definition.Id, step1);

            var step2 = new SagaStepDefinition(
                "Step 2: Cache Update",
                "cache-service",
                "http://localhost:8002/api/cache/set",
                "http://localhost:8002/api/cache/clear");
            step2.SetTimeout(30);
            step2.SetRetryPolicy(2, 1000);
            await definitionService.AddStepAsync(definition.Id, step2);

            var step3 = new SagaStepDefinition(
                "Step 3: Message Queue",
                "queue-service",
                "http://localhost:8003/api/queue/publish",
                "http://localhost:8003/api/queue/revoke");
            step3.SetTimeout(30);
            step3.SetRetryPolicy(2, 1000);
            await definitionService.AddStepAsync(definition.Id, step3);

            var step4 = new SagaStepDefinition(
                "Step 4: External Service",
                "external-service",
                "http://localhost:8004/api/external/call",
                "http://localhost:8004/api/external/rollback");
            step4.SetTimeout(30);
            step4.SetRetryPolicy(2, 1000);
            await definitionService.AddStepAsync(definition.Id, step4);

            var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

            // Demonstrate each strategy
            var strategies = new[]
            {
                (CompensationStrategy.ReverseOrder, "Reverse Order (LIFO)"),
                (CompensationStrategy.ForwardOrder, "Forward Order (FIFO)"),
                (CompensationStrategy.Parallel, "Parallel")
            };

            foreach (var (strategy, description) in strategies)
            {
                logger.LogInformation($"\n=== Testing: {description} ===\n");

                var saga = await orchestrationService.CreateSagaAsync(
                    retrievedDef,
                    maxRetries: 2,
                    timeoutSeconds: 300);

                logger.LogInformation($"Saga: {saga.Id}");
                logger.LogInformation($"Strategy: {strategy}\n");

                await orchestrationService.StartSagaAsync(saga.Id);

                // Execute first 3 steps (step 4 will fail and trigger compensation)
                for (int i = 0; i < 3; i++)
                {
                    var step = await orchestrationService.ExecuteNextStepAsync(saga.Id);
                    if (step != null)
                    {
                        logger.LogInformation($"✓ Executed: {step.Name}");
                    }
                }

                // Simulate failure in step 4
                var sagaBeforeFail = await orchestrationService.GetSagaAsync(saga.Id);
                logger.LogInformation("\nStep 4: Failed (simulated)\n");

                // Trigger compensation with strategy
                logger.LogInformation($"Triggering compensation with {description}:\n");

                switch (strategy)
                {
                    case CompensationStrategy.ReverseOrder:
                        logger.LogInformation("Compensation order (LIFO):");
                        logger.LogInformation("  1. Revoke Step 3: Message Queue");
                        logger.LogInformation("  2. Clear Step 2: Cache Update");
                        logger.LogInformation("  3. Delete Step 1: Database Update\n");
                        break;

                    case CompensationStrategy.ForwardOrder:
                        logger.LogInformation("Compensation order (FIFO):");
                        logger.LogInformation("  1. Delete Step 1: Database Update");
                        logger.LogInformation("  2. Clear Step 2: Cache Update");
                        logger.LogInformation("  3. Revoke Step 3: Message Queue\n");
                        break;

                    case CompensationStrategy.Parallel:
                        logger.LogInformation("Compensation order (Parallel):");
                        logger.LogInformation("  → Delete Step 1 (concurrent)");
                        logger.LogInformation("  → Clear Step 2 (concurrent)");
                        logger.LogInformation("  → Revoke Step 3 (concurrent)\n");
                        break;
                }

                // Execute compensation
                var compensatedSaga = await orchestrationService.CompensateSagaAsync(saga.Id, strategy);

                logger.LogInformation($"✓ Compensation complete");
                logger.LogInformation($"  Final Status: {compensatedSaga.Status}\n");
            }

            // Demonstrate Manual compensation
            logger.LogInformation("\n=== Testing: Manual Intervention ===\n");

            var manualSaga = await orchestrationService.CreateSagaAsync(
                retrievedDef,
                maxRetries: 2,
                timeoutSeconds: 300);

            logger.LogInformation($"Saga: {manualSaga.Id}");
            logger.LogInformation("Strategy: Manual\n");

            await orchestrationService.StartSagaAsync(manualSaga.Id);

            for (int i = 0; i < 2; i++)
            {
                var step = await orchestrationService.ExecuteNextStepAsync(manualSaga.Id);
                if (step != null)
                {
                    logger.LogInformation($"✓ Executed: {step.Name}");
                }
            }

            logger.LogInformation("\nStep 3: Failed (critical)\n");
            logger.LogInformation("Using Manual strategy - awaiting human intervention");
            logger.LogInformation("  1. Alert operations team");
            logger.LogInformation("  2. Review failure cause");
            logger.LogInformation("  3. Decide compensation approach");
            logger.LogInformation("  4. Execute manual compensation or restart\n");

            var manualCompensation = await orchestrationService.CompensateSagaAsync(
                manualSaga.Id,
                CompensationStrategy.Manual);

            logger.LogInformation($"Status: {manualCompensation.Status}");
            logger.LogInformation("Awaiting manual intervention...\n");

            logger.LogInformation("✓ Compensation strategies example completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in compensation strategies example");
        }
    }
}
