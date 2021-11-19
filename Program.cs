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

// Setup dependency injection
var services = new ServiceCollection();
services.AddLogging(config =>
{
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});
services.AddSagaOrchestrator();

var serviceProvider = services.BuildServiceProvider();

// Get services
var definitionService = serviceProvider.GetRequiredService<SagaDefinitionService>();
var orchestrationService = serviceProvider.GetRequiredService<SagaOrchestrationService>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    logger.LogInformation("=== Saga Orchestrator Demo ===");

    // Create a saga definition
    logger.LogInformation("Creating saga definition...");
    var definition = await definitionService.CreateDefinitionAsync(
        "Order Processing Saga",
        "Distributed saga for processing orders across multiple microservices");

    // Add steps to the definition
    var step1 = new SagaStepDefinition(
        "Reserve Inventory",
        "inventory-service",
        "http://inventory-service/reserve",
        "http://inventory-service/release");
    step1.SetTimeout(30);
    step1.SetRetryPolicy(3, 1000);
    await definitionService.AddStepAsync(definition.Id, step1);

    var step2 = new SagaStepDefinition(
        "Process Payment",
        "payment-service",
        "http://payment-service/charge",
        "http://payment-service/refund");
    step2.SetTimeout(30);
    step2.SetRetryPolicy(3, 1000);
    await definitionService.AddStepAsync(definition.Id, step2);

    var step3 = new SagaStepDefinition(
        "Create Shipment",
        "shipping-service",
        "http://shipping-service/create",
        "http://shipping-service/cancel");
    step3.SetTimeout(30);
    step3.SetRetryPolicy(3, 1000);
    await definitionService.AddStepAsync(definition.Id, step3);

    logger.LogInformation($"✓ Created saga definition: {definition.Name} ({definition.Id})");
    logger.LogInformation($"  Steps: {definition.Steps.Count}");

    // Validate definition
    var validation = definitionService.ValidateDefinition(definition);
    if (!validation.IsValid)
    {
        logger.LogError($"✗ Definition validation failed: {string.Join(", ", validation.Errors)}");
        return 1;
    }
    logger.LogInformation("✓ Definition validation passed");

    // Retrieve updated definition with steps
    var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

    // Create a saga from the definition
    logger.LogInformation("\nCreating saga instance...");
    var saga = await orchestrationService.CreateSagaAsync(retrievedDef, maxRetries: 3, timeoutSeconds: 300);
    logger.LogInformation($"✓ Created saga: {saga.Id}");
    logger.LogInformation($"  Status: {saga.Status}");
    logger.LogInformation($"  Correlation ID: {saga.CorrelationId}");

    // Start the saga
    logger.LogInformation("\nStarting saga execution...");
    var startedSaga = await orchestrationService.StartSagaAsync(saga.Id);
    logger.LogInformation($"✓ Saga started");
    logger.LogInformation($"  Status: {startedSaga.Status}");
    logger.LogInformation($"  Total steps: {startedSaga.Steps.Count}");

    // Execute steps sequentially
    logger.LogInformation("\nExecuting saga steps...");
    for (int i = 0; i < startedSaga.Steps.Count; i++)
    {
        logger.LogInformation($"  Executing step {i + 1}/{startedSaga.Steps.Count}...");
        var step = await orchestrationService.ExecuteNextStepAsync(saga.Id);

        if (step != null)
        {
            logger.LogInformation($"    ✓ {step.Name}: {step.Status}");
        }
    }

    // Get final saga state
    var finalSaga = await orchestrationService.GetSagaAsync(saga.Id);
    logger.LogInformation($"\n✓ Saga execution completed!");
    logger.LogInformation($"  Final Status: {finalSaga.Status}");
    logger.LogInformation($"  Completed Steps: {finalSaga.Steps.Count(s => s.Status == SagaStepStatus.Completed)}/{finalSaga.Steps.Count}");
    logger.LogInformation($"  Completed At: {finalSaga.CompletedAt:O}");

    // List all sagas
    logger.LogInformation("\nListing all sagas...");
    var allSagas = await orchestrationService.ListSagasAsync();
    logger.LogInformation($"✓ Total sagas in system: {allSagas.Count}");
    foreach (var s in allSagas)
    {
        logger.LogInformation($"  - {s.Id}: {s.Status} (created {s.StartedAt:O})");
    }

    logger.LogInformation("\n=== Demo Complete ===");
    return 0;
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during saga orchestration");
    return 1;
}
// TODO: add configurable timeout for saga compensation steps
