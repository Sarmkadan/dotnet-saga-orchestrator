// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Basic usage example demonstrating minimal setup and execution of a simple saga.
/// </summary>
public class BasicUsage
{
    public static async Task Main(string[] args)
    {
        // 1. Setup Dependency Injection
        var services = new ServiceCollection();
        services.AddSagaOrchestrator();
        var provider = services.BuildServiceProvider();

        // 2. Get Services
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();
        var orchestration = provider.GetRequiredService<SagaOrchestrationService>();

        // 3. Create Saga Definition
        var definition = await definitionService.CreateDefinitionAsync("SimpleSaga", "A simple 1-step saga");

        // 4. Define and Add Step
        var step = new SagaStepDefinition(
            "PrintMessage",
            "logging-service",
            "http://logging/info",
            "http://logging/compensate");
        await definitionService.AddStepAsync(definition.Id, step);

        // 5. Create and Run Saga
        var saga = await orchestration.CreateSagaAsync(definition);
        await orchestration.StartSagaAsync(saga.Id);
        await orchestration.ExecuteNextStepAsync(saga.Id);

        Console.WriteLine($"Saga {saga.Id} executed.");
    }
}
