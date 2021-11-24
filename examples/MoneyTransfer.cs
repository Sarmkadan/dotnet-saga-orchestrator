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

/// Financial money transfer saga example
/// Demonstrates: account validation, debit, credit, and compensation
public class MoneyTransferExample
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
        var logger = serviceProvider.GetRequiredService<ILogger<MoneyTransferExample>>();

        try
        {
            logger.LogInformation("=== Financial Money Transfer Saga ===\n");

            // Create definition
            var definition = await definitionService.CreateDefinitionAsync(
                "Money Transfer",
                "Transfer funds between bank accounts with validation and ledger updates");

            // Step 1: Validate accounts
            var validateStep = new SagaStepDefinition(
                "Validate Accounts",
                "account-service",
                "http://localhost:5001/api/accounts/validate",
                "http://localhost:5001/api/accounts/unlock");
            validateStep.SetTimeout(15);
            validateStep.SetRetryPolicy(3, 500);
            await definitionService.AddStepAsync(definition.Id, validateStep);

            // Step 2: Debit source account
            var debitStep = new SagaStepDefinition(
                "Debit Source Account",
                "ledger-service",
                "http://localhost:5002/api/ledger/debit",
                "http://localhost:5002/api/ledger/credit");
            debitStep.SetTimeout(30);
            debitStep.SetRetryPolicy(5, 1000);
            await definitionService.AddStepAsync(definition.Id, debitStep);

            // Step 3: Credit destination account
            var creditStep = new SagaStepDefinition(
                "Credit Destination Account",
                "ledger-service",
                "http://localhost:5002/api/ledger/credit",
                "http://localhost:5002/api/ledger/debit");
            creditStep.SetTimeout(30);
            creditStep.SetRetryPolicy(5, 1000);
            await definitionService.AddStepAsync(definition.Id, creditStep);

            logger.LogInformation("✓ Created definition with 3 steps\n");

            // Validate
            var validation = definitionService.ValidateDefinition(definition);
            if (!validation.IsValid)
            {
                logger.LogError("✗ Validation failed");
                return;
            }

            var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

            // Create and execute saga
            var saga = await orchestrationService.CreateSagaAsync(
                retrievedDef,
                maxRetries: 5,
                timeoutSeconds: 300);

            logger.LogInformation($"✓ Transfer saga created: {saga.Id}");
            logger.LogInformation($"  From: Account-123");
            logger.LogInformation($"  To: Account-456");
            logger.LogInformation($"  Amount: $1,000.00\n");

            await orchestrationService.StartSagaAsync(saga.Id);
            logger.LogInformation("✓ Transfer initiated\n");

            // Execute all steps
            logger.LogInformation("Processing transfer...");
            for (int i = 0; i < 3; i++)
            {
                var step = await orchestrationService.ExecuteNextStepAsync(saga.Id);
                if (step != null)
                {
                    logger.LogInformation($"✓ {step.Name}: {step.Status}");
                }
            }

            var finalSaga = await orchestrationService.GetSagaAsync(saga.Id);

            if (finalSaga.Status == SagaStatus.Completed)
            {
                logger.LogInformation("\n✓ Transfer completed successfully!");
                logger.LogInformation("  Source account debited");
                logger.LogInformation("  Destination account credited");
            }
            else if (finalSaga.Status == SagaStatus.Failed)
            {
                logger.LogInformation("\n✗ Transfer failed - initiating compensation");
                await orchestrationService.CompensateSagaAsync(saga.Id);
                logger.LogInformation("✓ Compensation completed - funds restored");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transfer error");
        }
    }
}
