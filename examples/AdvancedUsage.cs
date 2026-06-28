// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

/// <summary>
/// Advanced usage demonstrating configuration, retry policies, and custom options.
/// </summary>
public class AdvancedUsage
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection();
        // Configure with custom options
        services.AddSagaOrchestrator(options => {
            options.DefaultMaxRetries = 3;
            options.DefaultStepTimeoutSeconds = 30;
        });
        var provider = services.BuildServiceProvider();

        var definitionService = provider.GetRequiredService<SagaDefinitionService>();
        var orchestration = provider.GetRequiredService<SagaOrchestrationService>();

        var definition = await definitionService.CreateDefinitionAsync("AdvancedSaga", "Saga with retries");

        // Step with custom retry policy
        var step = new SagaStepDefinition("UnreliableTask", "service", "url", "compUrl");
        step.SetRetryPolicy(maxRetries: 5, delayMilliseconds: 1000);
        step.SetTimeout(60);
        await definitionService.AddStepAsync(definition.Id, step);

        var saga = await orchestration.CreateSagaAsync(definition);
        await orchestration.StartSagaAsync(saga.Id);
        
        // Execute and handle result
        var result = await orchestration.ExecuteNextStepAsync(saga.Id);
        
        if (result != null) {
            Console.WriteLine($"Step status: {result.Status}");
        }
    }
}
