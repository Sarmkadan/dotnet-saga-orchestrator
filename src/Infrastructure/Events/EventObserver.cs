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
public interface ISagaEventObserver
{
    Task OnSagaCreatedAsync(SagaCreatedEvent @event);
    Task OnSagaCompletedAsync(SagaCompletedEvent @event);
    Task OnSagaFailedAsync(SagaFailedEvent @event);
    Task OnCompensationStartedAsync(CompensationStartedEvent @event);
}

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

    public async Task OnSagaCreatedAsync(SagaCreatedEvent @event)
    {
        _logger.LogInformation("Saga created event observed | SagaId: {SagaId}", @event.SagaId);

        var subscriptions = _webhookHandler.GetSubscriptions();
        var webhooks = subscriptions
            .Where(s => s.EventTypes.Contains(nameof(SagaCreatedEvent)))
            .ToList();

        foreach (var webhook in webhooks)
        {
            try
            {
                await _webhookHandler.SendWebhookAsync(webhook.Url, @event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for SagaCreated event | Url: {Url}", webhook.Url);
            }
        }
    }

    public async Task OnSagaCompletedAsync(SagaCompletedEvent @event)
    {
        _logger.LogInformation("Saga completed event observed | SagaId: {SagaId}", @event.SagaId);

        var subscriptions = _webhookHandler.GetSubscriptions();
        var webhooks = subscriptions
            .Where(s => s.EventTypes.Contains(nameof(SagaCompletedEvent)))
            .ToList();

        foreach (var webhook in webhooks)
        {
            try
            {
                await _webhookHandler.SendWebhookAsync(webhook.Url, @event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for SagaCompleted event | Url: {Url}", webhook.Url);
            }
        }
    }

    public async Task OnSagaFailedAsync(SagaFailedEvent @event)
    {
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
                await _webhookHandler.SendWebhookAsync(webhook.Url, @event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for SagaFailed event | Url: {Url}", webhook.Url);
            }
        }
    }

    public async Task OnCompensationStartedAsync(CompensationStartedEvent @event)
    {
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
                await _webhookHandler.SendWebhookAsync(webhook.Url, @event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send webhook for CompensationStarted event | Url: {Url}", webhook.Url);
            }
        }
    }
}
