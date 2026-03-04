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
using SagaOrchestrator.Core.Utilities;

/// Advanced retry policies example
/// Demonstrates: exponential backoff, custom retry configurations per step
public class AdvancedRetriesExample
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
        var logger = serviceProvider.GetRequiredService<ILogger<AdvancedRetriesExample>>();

        try
        {
            logger.LogInformation("=== Advanced Retry Policies Example ===\n");

            var definition = await definitionService.CreateDefinitionAsync(
                "Retry Policy Test",
                "Testing various retry configurations");

            // Step 1: Aggressive retries for unreliable service
            var unreliableStep = new SagaStepDefinition(
                "Unreliable Service Call",
                "unreliable-service",
                "http://localhost:7001/api/unreliable",
                "http://localhost:7001/api/unreliable/undo");

            // Configure aggressive retry: 5 attempts with exponential backoff
            unreliableStep.SetRetryPolicy(
                maxRetries: 5,
                initialDelayMs: 500);

            logger.LogInformation("Step 1: Unreliable Service");
            logger.LogInformation("  Max Retries: 5");
            logger.LogInformation("  Initial Delay: 500ms");
            logger.LogInformation("  Backoff: Exponential (500ms, 1s, 2s, 4s, 8s)\n");

            await definitionService.AddStepAsync(definition.Id, unreliableStep);

            // Step 2: Conservative retries for sensitive service
            var sensitiveStep = new SagaStepDefinition(
                "Sensitive Service Call",
                "sensitive-service",
                "http://localhost:7002/api/sensitive",
                "http://localhost:7002/api/sensitive/undo");

            // Configure conservative retry: 2 attempts with longer delay
            sensitiveStep.SetRetryPolicy(
                maxRetries: 2,
                initialDelayMs: 2000);

            logger.LogInformation("Step 2: Sensitive Service");
            logger.LogInformation("  Max Retries: 2");
            logger.LogInformation("  Initial Delay: 2000ms");
            logger.LogInformation("  Backoff: Exponential (2s, 4s)\n");

            await definitionService.AddStepAsync(definition.Id, sensitiveStep);

            // Step 3: Fast retries for responsive service
            var responsiveStep = new SagaStepDefinition(
                "Responsive Service Call",
                "responsive-service",
                "http://localhost:7003/api/responsive",
                "http://localhost:7003/api/responsive/undo");

            // Configure fast retries: many attempts with short delay
            responsiveStep.SetRetryPolicy(
                maxRetries: 10,
                initialDelayMs: 100);

            logger.LogInformation("Step 3: Responsive Service");
            logger.LogInformation("  Max Retries: 10");
            logger.LogInformation("  Initial Delay: 100ms");
            logger.LogInformation("  Backoff: Exponential (100ms, 200ms, 400ms, ...)\n");

            await definitionService.AddStepAsync(definition.Id, responsiveStep);

            var validation = definitionService.ValidateDefinition(definition);
            if (!validation.IsValid)
            {
                logger.LogError("✗ Validation failed");
                return;
            }

            var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

            var saga = await orchestrationService.CreateSagaAsync(
                retrievedDef,
                maxRetries: 3,
                timeoutSeconds: 600);

            logger.LogInformation("=== Saga Configuration ===");
            logger.LogInformation($"Saga ID: {saga.Id}");
            logger.LogInformation($"Max Retries: 3");
            logger.LogInformation($"Timeout: 600 seconds\n");

            await orchestrationService.StartSagaAsync(saga.Id);
            logger.LogInformation("✓ Saga started\n");

            // Execute steps with detailed logging
            logger.LogInformation("=== Executing Steps ===\n");

            for (int i = 0; i < 3; i++)
            {
                var step = await orchestrationService.ExecuteNextStepAsync(saga.Id);

                if (step != null)
                {
                    logger.LogInformation($"Step {i + 1}: {step.Name}");
                    logger.LogInformation($"  Status: {step.Status}");
                    logger.LogInformation($"  Retry Count: {step.RetryCount}");

                    if (step.CompletedAt.HasValue && step.StartedAt.HasValue)
                    {
                        var duration = step.CompletedAt.Value - step.StartedAt;
                        logger.LogInformation($"  Duration: {duration?.TotalSeconds:F2}s");
                    }

                    if (!string.IsNullOrEmpty(step.ErrorMessage))
                    {
                        logger.LogInformation($"  Last Error: {step.ErrorMessage}");
                    }

                    logger.LogInformation();
                }
            }

            var finalSaga = await orchestrationService.GetSagaAsync(saga.Id);

            logger.LogInformation("=== Execution Summary ===\n");
            logger.LogInformation($"Final Status: {finalSaga.Status}");

            foreach (var step in finalSaga.Steps)
            {
                logger.LogInformation($"\n{step.Name}:");
                logger.LogInformation($"  Status: {step.Status}");
                logger.LogInformation($"  Total Attempts: {step.RetryCount + 1}");
            }

            logger.LogInformation("\n✓ Advanced retry example completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in retry example");
        }
    }
}
