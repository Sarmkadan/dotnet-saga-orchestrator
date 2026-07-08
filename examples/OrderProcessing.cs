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

/// Complete order processing saga example
/// Demonstrates: order creation, inventory reservation, payment processing, and shipment
public class OrderProcessingExample
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
        var logger = serviceProvider.GetRequiredService<ILogger<OrderProcessingExample>>();

        try
        {
            logger.LogInformation("=== E-Commerce Order Processing Saga ===\n");

            // Create saga definition
            var definition = await definitionService.CreateDefinitionAsync(
                "Order Processing",
                "Complete order processing across inventory, payment, and shipping");

            logger.LogInformation($"✓ Created definition: {definition.Name}\n");

            // Add step 1: Reserve inventory
            var reserveStep = new SagaStepDefinition(
                "Reserve Inventory",
                "inventory-service",
                "http://localhost:5001/api/inventory/reserve",
                "http://localhost:5001/api/inventory/release");
            reserveStep.SetTimeout(30);
            reserveStep.SetRetryPolicy(3, 1000);
            await definitionService.AddStepAsync(definition.Id, reserveStep);
            logger.LogInformation("✓ Added step: Reserve Inventory");

            // Add step 2: Process payment
            var paymentStep = new SagaStepDefinition(
                "Process Payment",
                "payment-service",
                "http://localhost:5002/api/payments/charge",
                "http://localhost:5002/api/payments/refund");
            paymentStep.SetTimeout(30);
            paymentStep.SetRetryPolicy(2, 2000);
            await definitionService.AddStepAsync(definition.Id, paymentStep);
            logger.LogInformation("✓ Added step: Process Payment");

            // Add step 3: Create shipment
            var shipmentStep = new SagaStepDefinition(
                "Create Shipment",
                "shipping-service",
                "http://localhost:5003/api/shipments/create",
                "http://localhost:5003/api/shipments/cancel");
            shipmentStep.SetTimeout(60);
            shipmentStep.SetRetryPolicy(3, 1000);
            await definitionService.AddStepAsync(definition.Id, shipmentStep);
            logger.LogInformation("✓ Added step: Create Shipment\n");

            // Validate definition
            var validation = definitionService.ValidateDefinition(definition);
            if (!validation.IsValid)
            {
                logger.LogError("✗ Validation failed");
                return;
            }
            logger.LogInformation("✓ Definition validation passed\n");

            // Get updated definition with steps
            var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

            // Create saga instance
            var saga = await orchestrationService.CreateSagaAsync(
                retrievedDef,
                maxRetries: 3,
                timeoutSeconds: 300);
            logger.LogInformation($"✓ Created saga: {saga.Id}");
            logger.LogInformation($"  Correlation ID: {saga.CorrelationId}");
            logger.LogInformation($"  Status: {saga.Status}\n");

            // Start saga
            var startedSaga = await orchestrationService.StartSagaAsync(saga.Id);
            logger.LogInformation($"✓ Saga started");
            logger.LogInformation($"  Total steps: {startedSaga.Steps.Count}\n");

            // Execute steps
            logger.LogInformation("Executing steps...");
            for (int i = 0; i < startedSaga.Steps.Count; i++)
            {
                var step = await orchestrationService.ExecuteNextStepAsync(saga.Id);
                if (step != null)
                {
                    logger.LogInformation($"✓ Step {i + 1}: {step.Name} - {step.Status}");
                    if (step.CompletedAt.HasValue)
                    {
                        var duration = step.CompletedAt.Value - step.StartedAt;
                        logger.LogInformation($"  Duration: {duration?.TotalSeconds:F2}s");
                    }
                }
            }

            // Get final state
            var finalSaga = await orchestrationService.GetSagaAsync(saga.Id);
            logger.LogInformation($"\n=== Final Status: {finalSaga.Status} ===");
            logger.LogInformation($"Completed steps: {finalSaga.Steps.Count(s => s.Status == SagaStepStatus.Completed)}/{finalSaga.Steps.Count}");

            if (finalSaga.Status == SagaStatus.Completed)
            {
                logger.LogInformation("✓ Order processed successfully!");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred");
        }
    }
}
