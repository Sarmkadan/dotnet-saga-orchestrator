#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Infrastructure configuration settings for the saga orchestrator.
/// This record defines the configuration options that can be serialized to/from JSON.
/// </summary>
/// <param name="EnableCaching">Whether to enable caching infrastructure.</param>
/// <param name="EnableHttpClients">Whether to enable HTTP client infrastructure.</param>
/// <param name="EnableEventBus">Whether to enable event bus infrastructure.</param>
/// <param name="EnableFormatting">Whether to enable formatting infrastructure.</param>
/// <param name="EnableLogging">Whether to enable logging infrastructure.</param>
/// <param name="EnableIntegration">Whether to enable integration infrastructure.</param>
/// <param name="EnableRateLimiting">Whether to enable rate limiting infrastructure.</param>
/// <param name="EnableBackgroundWorkers">Whether to enable background workers infrastructure.</param>
public sealed record InfrastructureConfiguration(
    bool EnableCaching = true,
    bool EnableHttpClients = true,
    bool EnableEventBus = true,
    bool EnableFormatting = true,
    bool EnableLogging = true,
    bool EnableIntegration = true,
    bool EnableRateLimiting = true,
    bool EnableBackgroundWorkers = true)
{
    /// <summary>
    /// Creates a default infrastructure configuration with all features enabled.
    /// </summary>
    public static InfrastructureConfiguration Default { get; } = new();

    /// <summary>
    /// Configures infrastructure services based on these settings.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public IServiceCollection ConfigureServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (EnableCaching)
        {
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Caching.ICacheService, global::SagaOrchestrator.Infrastructure.Caching.CacheService>();
        }

        if (EnableHttpClients)
        {
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Http.IHttpClientFactory, global::SagaOrchestrator.Infrastructure.Http.HttpClientFactory>();
            services.AddHttpClient();
        }

        if (EnableEventBus)
        {
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Events.IEventBus, global::SagaOrchestrator.Infrastructure.Events.EventBus>();
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Events.ISagaEventObserver, global::SagaOrchestrator.Infrastructure.Events.SagaEventObserver>();
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Events.CompositeSagaEventObserver>();
        }

        if (EnableFormatting)
        {
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Serialization.ISagaSerializer, global::SagaOrchestrator.Infrastructure.Serialization.SagaJsonSerializer>();
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Formatting.IOutputFormatter, global::SagaOrchestrator.Infrastructure.Formatting.OutputFormatter>();
        }

        if (EnableLogging)
        {
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Logging.ISagaLogger, global::SagaOrchestrator.Infrastructure.Logging.SagaLogger>();
        }

        if (EnableIntegration)
        {
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Integration.IWebhookHandler, global::SagaOrchestrator.Infrastructure.Integration.WebhookHandler>();
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.Integration.IServiceRegistry, global::SagaOrchestrator.Infrastructure.Integration.ServiceRegistry>();
        }

        if (EnableRateLimiting)
        {
            services.AddSingleton<global::SagaOrchestrator.Infrastructure.RateLimiting.IRateLimiter, global::SagaOrchestrator.Infrastructure.RateLimiting.TokenBucketRateLimiter>();
        }

        services.AddScoped<global::SagaOrchestrator.Infrastructure.Context.IRequestContext, global::SagaOrchestrator.Infrastructure.Context.RequestContext>();
        services.AddScoped<global::SagaOrchestrator.Infrastructure.Context.IRequestContextProvider, global::SagaOrchestrator.Infrastructure.Context.RequestContextProvider>();

        if (EnableBackgroundWorkers)
        {
            services.AddHostedService<global::SagaOrchestrator.Infrastructure.BackgroundWorkers.SagaTimeoutWorker>();
            services.AddHostedService<global::SagaOrchestrator.Infrastructure.BackgroundWorkers.CompensationWorker>();
        }

        return services;
    }
}