#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics.CodeAnalysis;

namespace SagaOrchestrator.Infrastructure.Events;

/// <summary>
/// Contract for saga event observers that react to saga lifecycle events.
/// </summary>
/// <remarks>
/// <para>
/// Observers must adhere to the following contract:
/// <list type="bullet">
///   <item><description><b>Error Isolation:</b> Observer callbacks must never fail or roll back the saga transition. Any exceptions thrown by an observer must be caught and logged internally, allowing other observers to execute.</description></item>
///   <item><description><b>Async Contract:</b> Methods return <see cref="ValueTask"/> to allow callers to explicitly choose between awaited execution (ensuring completion) and fire-and-forget (optimizing for performance).</description></item>
///   <item><description><b>Idempotency:</b> Observers should handle duplicate events gracefully.</description></item>
/// </list>
/// </para>
/// <para>
/// For multiple observers, use <see cref="CompositeSagaEventObserver"/> to register and invoke them collectively without each caller managing a list.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Single observer usage
/// var observer = new SagaEventObserver(webhookHandler, eventBus, logger);
/// await observer.OnSagaCreatedAsync(sagaCreatedEvent);
///
/// // Multiple observers usage
/// var composite = new CompositeSagaEventObserver([metricsObserver, loggingObserver]);
/// await composite.OnSagaCreatedAsync(sagaCreatedEvent);
/// </code>
/// </example>
public interface ISagaEventObserver
{
    /// <summary>
    /// Called when a saga is created.
    /// </summary>
    /// <param name="@event">The saga created event.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="@event"/> is null.</exception>
    /// <remarks>
    /// This method should be exception-isolated - any exceptions thrown by implementations
    /// must be caught and logged internally, never propagated to the caller.
    /// </remarks>
    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Interface method")]
    ValueTask OnSagaCreatedAsync(SagaCreatedEvent @event);

    /// <summary>
    /// Called when a saga completes successfully.
    /// </summary>
    /// <param name="@event">The saga completed event.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="@event"/> is null.</exception>
    /// <remarks>
    /// This method should be exception-isolated - any exceptions thrown by implementations
    /// must be caught and logged internally, never propagated to the caller.
    /// </remarks>
    ValueTask OnSagaCompletedAsync(SagaCompletedEvent @event);

    /// <summary>
    /// Called when a saga fails.
    /// </summary>
    /// <param name="@event">The saga failed event.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="@event"/> is null.</exception>
    /// <remarks>
    /// This method should be exception-isolated - any exceptions thrown by implementations
    /// must be caught and logged internally, never propagated to the caller.
    /// </remarks>
    ValueTask OnSagaFailedAsync(SagaFailedEvent @event);

    /// <summary>
    /// Called when compensation starts for a saga.
    /// </summary>
    /// <param name="@event">The compensation started event.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="@event"/> is null.</exception>
    /// <remarks>
    /// This method should be exception-isolated - any exceptions thrown by implementations
    /// must be caught and logged internally, never propagated to the caller.
    /// </remarks>
    ValueTask OnCompensationStartedAsync(CompensationStartedEvent @event);
}