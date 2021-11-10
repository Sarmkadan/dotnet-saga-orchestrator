#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Infrastructure.Debugging;

namespace SagaOrchestrator.Configuration;

/// <summary>
/// Extension methods for registering the distributed saga debugger with the
/// dependency injection container.
/// </summary>
public static class DebuggerServiceExtensions
{
    /// <summary>
    /// Registers the <see cref="ISagaDebugger"/> service and its <see cref="DebuggerOptions"/>
    /// configuration as singletons.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The debugger requires that <c>AddSagaOrchestrator()</c> has already been called so that
    /// <c>ISagaRepository</c>, <c>ISagaStepRepository</c>, and <c>SagaEventPublisher</c> are
    /// resolvable from the container.
    /// </para>
    /// <para>
    /// Time-travel and auto-capture are opt-in; see <see cref="DebuggerOptions"/> for all flags.
    /// In production, keep <see cref="DebuggerOptions.IsEnabled"/> as <c>false</c> (the default)
    /// to eliminate any runtime overhead.
    /// </para>
    /// </remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configure">
    /// Optional delegate for customising <see cref="DebuggerOptions"/>.
    /// When omitted the default options are used (debugger disabled).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// <code>
    /// // Development – full debugging with time-travel:
    /// services.AddSagaOrchestrator()
    ///         .AddSagaDebugger(opts =>
    ///         {
    ///             opts.IsEnabled              = true;
    ///             opts.EnableTimeTravel       = true;
    ///             opts.AutoCaptureOnStepTransition = true;
    ///             opts.MaxSnapshotsPerSaga    = 100;
    ///         });
    ///
    /// // Production – debugger disabled (default), zero overhead:
    /// services.AddSagaOrchestrator()
    ///         .AddSagaDebugger();
    /// </code>
    /// </example>
    public static IServiceCollection AddSagaDebugger(
        this IServiceCollection services,
        Action<DebuggerOptions>? configure = null)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

        var options = new DebuggerOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<ISagaDebugger, SagaDebuggerService>();

        return services;
    }

    /// <summary>
    /// Registers the <see cref="ISagaDebugger"/> service using a pre-built
    /// <see cref="DebuggerOptions"/> instance produced by <see cref="DebuggerOptionsBuilder"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="options">
    /// A fully validated <see cref="DebuggerOptions"/> object, typically the output of
    /// <see cref="DebuggerOptionsBuilder.Build"/>.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="options"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// <code>
    /// var debuggerOptions = new DebuggerOptionsBuilder()
    ///     .Enable()
    ///     .WithMaxSnapshotsPerSaga(200)
    ///     .WithAutoCapture(onStepTransition: true, onCompensation: true)
    ///     .WithTimeTravel(enabled: true)
    ///     .Build();
    ///
    /// services.AddSagaOrchestrator()
    ///         .AddSagaDebugger(debuggerOptions);
    /// </code>
    /// </example>
    public static IServiceCollection AddSagaDebugger(
        this IServiceCollection services,
        DebuggerOptions options)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (options  is null) throw new ArgumentNullException(nameof(options));

        services.AddSingleton(options);
        services.AddSingleton<ISagaDebugger, SagaDebuggerService>();

        return services;
    }
}
