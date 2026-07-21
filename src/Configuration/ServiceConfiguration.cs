#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Infrastructure.Logging;
using SagaOrchestrator.Infrastructure.Serialization;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Configures dependency injection for all saga orchestration services and repositories.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registers all saga orchestrator services with the dependency injection container.
    /// Uses in-memory repositories by default for development.
    /// </summary>
    public static IServiceCollection AddSagaOrchestrator(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddLogging();

        // Register repositories (in-memory; swap for database-backed implementations as needed)
        services.AddSingleton<ISagaRepository, InMemorySagaRepository>();
        services.AddSingleton<ISagaStepRepository, InMemorySagaStepRepository>();
        services.AddSingleton<ICompensationTransactionRepository, InMemoryCompensationTransactionRepository>();
        services.AddSingleton<ISagaDefinitionRepository, InMemorySagaDefinitionRepository>();

        // Default options unless the caller already configured them via the overload
        services.TryAddSingleton(new SagaOptions());

        // Register services
        services.AddSingleton<ISagaLogger, SagaLogger>();
        services.AddSingleton<ISagaSerializer, SagaJsonSerializer>();
        services.AddSingleton<SagaDefinitionService>();
        services.AddSingleton<CompensationService>();
        services.AddSingleton<SagaOrchestrationService>();
        services.AddSingleton<IMetricsService, MetricsService>();

        return services;
    }

    /// <summary>
    /// Registers all saga orchestrator services with the dependency injection container,
    /// applying custom configuration to the resulting <see cref="SagaOptions"/> instance.
    /// </summary>
    public static IServiceCollection AddSagaOrchestrator(this IServiceCollection services, Action<SagaOptions> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        var options = new SagaOptions();
        configureOptions(options);
        services.AddSingleton(options);

        return services.AddSagaOrchestrator();
    }

    /// <summary>
    /// Registers only the repository layer for dependency injection.
    /// Useful for advanced scenarios with custom service configuration.
    /// </summary>
    public static IServiceCollection AddSagaRepositories(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddSingleton<ISagaRepository, InMemorySagaRepository>();
        services.AddSingleton<ISagaStepRepository, InMemorySagaStepRepository>();
        services.AddSingleton<ICompensationTransactionRepository, InMemoryCompensationTransactionRepository>();
        services.AddSingleton<ISagaDefinitionRepository, InMemorySagaDefinitionRepository>();

        return services;
    }

    /// <summary>
    /// Registers only the service layer for dependency injection.
    /// Repositories must be registered separately.
    /// </summary>
    public static IServiceCollection AddSagaServices(this IServiceCollection services)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddLogging();
        services.AddSingleton<ISagaLogger, SagaLogger>();
        services.AddSingleton<SagaDefinitionService>();
        services.AddSingleton<CompensationService>();
        services.AddSingleton<SagaOrchestrationService>();

        return services;
    }
}
