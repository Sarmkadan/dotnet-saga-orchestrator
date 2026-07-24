#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SagaOrchestrator.Infrastructure.Integration;

namespace SagaOrchestrator.Infrastructure.Events;

/// <summary>
/// Observer pattern implementation for saga domain events.
/// Subscribes to events and triggers side effects like webhook delivery.
/// </summary>
/// <remarks>
/// Implements the <see cref="ISagaEventObserver"/> contract with proper error isolation and async handling.
/// </remarks>
public class SagaEventObserver : ISagaEventObserver
{
    private readonly IWebhookHandler _webhookHandler;
    private readonly IEventBus _eventBus;
    private readonly ILogger<SagaEventObserver> _logger;

    public SagaEventObserver(
        IWebhookHandler webhookHandler,
        IEventBus eventBus,
        ILogger<SagaEventObserver> logger)
    {
        _webhookHandler = webhookHandler ?? throw new ArgumentNullException(nameof(webhookHandler));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Called when a saga is created.
    /// </summary>
    /// <param name="@event">The saga created event.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="@event"/> is null.</exception>
    /// <remarks>
    /// Implements error isolation by catching and logging any exceptions from webhook delivery.
    /// The method returns <see cref="ValueTask"/> to allow callers to choose awaited or fire-and-forget execution.
    /// </remarks>
    public async ValueTask OnSagaCreatedAsync(SagaCreatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogInformation("Saga created event observed | SagaId: {SagaId}", @event.SagaId);

        var subscriptions = _webhookHandler.GetSubscriptions();
        var webhooks = subscriptions
            .Where(s => s.EventTypes.Contains(nameof(SagaCreatedEvent)))
            .ToList();

        foreach (var webhook in webhooks)
        {
            try
            {
                await _webhookHandler.SendWebhookAsync(webhook.Url, @event).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for SagaCreated event | Url: {Url}", webhook.Url);
            }
        }
    }

    /// <summary>
    /// Called when a saga completes successfully.
    /// </summary>
    /// <param name="@event">The saga completed event.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="@event"/> is null.</exception>
    /// <remarks>
    /// Implements error isolation by catching and logging any exceptions from webhook delivery.
    /// The method returns <see cref="ValueTask"/> to allow callers to choose awaited or fire-and-forget execution.
    /// </remarks>
    public async ValueTask OnSagaCompletedAsync(SagaCompletedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogInformation("Saga completed event observed | SagaId: {SagaId}", @event.SagaId);

        var subscriptions = _webhookHandler.GetSubscriptions();
        var webhooks = subscriptions
            .Where(s => s.EventTypes.Contains(nameof(SagaCompletedEvent)))
            .ToList();

        foreach (var webhook in webhooks)
        {
            try
            {
                await _webhookHandler.SendWebhookAsync(webhook.Url, @event).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for SagaCompleted event | Url: {Url}", webhook.Url);
            }
        }
    }

    /// <summary>
    /// Called when a saga fails.
    /// </summary>
    /// <param name="@event">The saga failed event.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="@event"/> is null.</exception>
    /// <remarks>
    /// Implements error isolation by catching and logging any exceptions from webhook delivery.
    /// The method returns <see cref="ValueTask"/> to allow callers to choose awaited or fire-and-forget execution.
    /// </remarks>
    public async ValueTask OnSagaFailedAsync(SagaFailedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogError("Saga failed event observed | SagaId: {SagaId}, Error: {Error}",
            @event.SagaId, @event.ErrorMessage);

        var subscriptions = _webhookHandler.GetSubscriptions();
        var webhooks = subscriptions
            .Where(s => s.EventTypes.Contains(nameof(SagaFailedEvent)))
            .ToList();

        foreach (var webhook in webhooks)
        {
            try
            {
                await _webhookHandler.SendWebhookAsync(webhook.Url, @event).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for SagaFailed event | Url: {Url}", webhook.Url);
            }
        }
    }

    /// <summary>
    /// Called when compensation starts for a saga.
    /// </summary>
    /// <param name="@event">The compensation started event.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="@event"/> is null.</exception>
    /// <remarks>
    /// Implements error isolation by catching and logging any exceptions from webhook delivery.
    /// The method returns <see cref="ValueTask"/> to allow callers to choose awaited or fire-and-forget execution.
    /// </remarks>
    public async ValueTask OnCompensationStartedAsync(CompensationStartedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        _logger.LogWarning("Compensation started event observed | SagaId: {SagaId}, Strategy: {Strategy}",
            @event.SagaId, @event.CompensationStrategy);

        var subscriptions = _webhookHandler.GetSubscriptions();
        var webhooks = subscriptions
            .Where(s => s.EventTypes.Contains(nameof(CompensationStartedEvent)))
            .ToList();

        foreach (var webhook in webhooks)
        {
            try
            {
                await _webhookHandler.SendWebhookAsync(webhook.Url, @event).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for CompensationStarted event | Url: {Url}", webhook.Url);
            }
        }
    }
}