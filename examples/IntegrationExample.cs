// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SagaOrchestrator.Application.Services;

/// <summary>
/// Example showing how to wire the saga orchestrator into an ASP.NET Core or Generic Host application.
/// </summary>
public class IntegrationExample
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Register the orchestrator in DI container
        services.AddSagaOrchestrator(options => {
            options.DefaultMaxRetries = 3;
        });

        // Register your custom services
        // services.AddTransient<IMyService, MyService>();
    }

    public static async Task RunSagaExample(IServiceProvider provider)
    {
        // Resolve orchestrator in your controller or background service
        var orchestration = provider.GetRequiredService<SagaOrchestrationService>();
        
        // ... saga execution logic ...
    }
}
