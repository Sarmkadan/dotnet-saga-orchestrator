#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Infrastructure.BackgroundWorkers;
using SagaOrchestrator.Infrastructure.Caching;
using SagaOrchestrator.Infrastructure.Context;
using SagaOrchestrator.Infrastructure.Events;
using SagaOrchestrator.Infrastructure.Formatting;
using SagaOrchestrator.Infrastructure.Http;
using SagaOrchestrator.Infrastructure.Integration;
using SagaOrchestrator.Infrastructure.Logging;
using SagaOrchestrator.Infrastructure.RateLimiting;
using SagaOrchestrator.Infrastructure.Serialization;
using SagaOrchestrator.Presentation.Cli;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Dependency injection configuration for infrastructure components.
/// Registers caching, HTTP clients, event bus, formatters, and background workers.
/// </summary>
public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Caching
        services.AddSingleton<ICacheService, CacheService>();

        // HTTP client factory with resilience
        services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
        services.AddHttpClient();

        // Event bus and observers
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<ISagaEventObserver, SagaEventObserver>();

        // Serialization
        services.AddSingleton<ISagaSerializer, SagaJsonSerializer>();

        // Formatting and output
        services.AddSingleton<IOutputFormatter, OutputFormatter>();

        // Logging
        services.AddSingleton<ISagaLogger, SagaLogger>();

        // Integration
        services.AddSingleton<IWebhookHandler, WebhookHandler>();
        services.AddSingleton<IServiceRegistry, ServiceRegistry>();

        // Rate limiting
        services.AddSingleton<IRateLimiter, TokenBucketRateLimiter>();

        // Request context
        services.AddScoped<IRequestContext, RequestContext>();
        services.AddScoped<IRequestContextProvider, RequestContextProvider>();

        // CLI
        services.AddScoped<ICliHandler, CliHandler>();

        // Background workers
        services.AddHostedService<SagaTimeoutWorker>();
        services.AddHostedService<CompensationWorker>();

        return services;
    }

    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddSingleton<ICacheService, CacheService>();
        return services;
    }

    public static IServiceCollection AddEventHandling(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<ISagaEventObserver, SagaEventObserver>();
        return services;
    }

    public static IServiceCollection AddIntegration(this IServiceCollection services)
    {
        services.AddSingleton<IHttpClientFactory, HttpClientFactory>();
        services.AddSingleton<IWebhookHandler, WebhookHandler>();
        services.AddSingleton<IServiceRegistry, ServiceRegistry>();
        services.AddHttpClient();
        return services;
    }

    public static IServiceCollection AddFormatting(this IServiceCollection services)
    {
        services.AddSingleton<ISagaSerializer, SagaJsonSerializer>();
        services.AddSingleton<IOutputFormatter, OutputFormatter>();
        return services;
    }

    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<SagaTimeoutWorker>();
        services.AddHostedService<CompensationWorker>();
        return services;
    }
}
