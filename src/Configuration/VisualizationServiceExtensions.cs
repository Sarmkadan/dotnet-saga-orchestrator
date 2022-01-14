#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Infrastructure.Visualization;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Extension methods for registering saga visualization services with the dependency injection container.
/// </summary>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
public static class VisualizationServiceExtensions
{
    /// <summary>
    /// Registers the saga visualization service and ASCII state renderer as singletons.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// services.AddSagaOrchestrator()
    ///         .AddSagaVisualization();
    /// </code>
    /// </example>
    public static IServiceCollection AddSagaVisualization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ISagaVisualizationService, SagaVisualizationService>();
        services.AddSingleton<ISagaStateRenderer, SagaStateRenderer>();

        return services;
    }
}
