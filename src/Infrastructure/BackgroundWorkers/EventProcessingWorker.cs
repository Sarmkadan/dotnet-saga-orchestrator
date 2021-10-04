#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Infrastructure.Events;

namespace SagaOrchestrator.Infrastructure.BackgroundWorkers;

/// <summary>
/// Background worker that processes and archives saga events.
/// Periodically polls the event bus and handles event persistence and cleanup.
/// </summary>
public class EventProcessingWorker : BackgroundService
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<EventProcessingWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);
    private readonly int _maxEventsToKeep = 10000;

    public EventProcessingWorker(
        IEventBus eventBus,
        ILogger<EventProcessingWorker> logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event Processing Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEventsAsync(stoppingToken).ConfigureAwait(false);
                await Task.Delay(_checkInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in event processing worker");
            }
        }

        _logger.LogInformation("Event Processing Worker stopped");
    }

    private async Task ProcessEventsAsync(CancellationToken stoppingToken)
    {
        var events = _eventBus.GetEventHistory();

        if (events.Count == 0)
            return;

        // Log event statistics
        var eventGroups = events
            .GroupBy(e => e.EventType)
            .ToDictionary(g => g.Key, g => g.Count());

        _logger.LogDebug("Event queue status | Total events: {Count}, Types: {Types}",
            events.Count,
            string.Join(", ", eventGroups.Select(kvp => $"{kvp.Key}:{kvp.Value}")));

        // Clean up old events if exceeding limit
        if (events.Count > _maxEventsToKeep)
        {
            _logger.LogWarning("Event history exceeds limit | Current: {Count}, Max: {Max}",
                events.Count, _maxEventsToKeep);
            _eventBus.ClearHistory();
        }

        // Process recent events (last 100)
        var recentEvents = events.TakeLast(100).ToList();
        foreach (var @event in recentEvents)
        {
            try
            {
                await ProcessEventAsync(@event, stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventId} of type {EventType}",
                    @event.EventId, @event.EventType);
            }
        }

        await Task.CompletedTask;
    }

    private async Task ProcessEventAsync(DomainEvent @event, CancellationToken cancellationToken)
    {
        // Handle specific event types
        switch (@event)
        {
            case SagaCreatedEvent sagaCreated:
                await HandleSagaCreatedAsync(sagaCreated).ConfigureAwait(false);
                break;

            case SagaCompletedEvent sagaCompleted:
                await HandleSagaCompletedAsync(sagaCompleted).ConfigureAwait(false);
                break;

            case SagaFailedEvent sagaFailed:
                await HandleSagaFailedAsync(sagaFailed).ConfigureAwait(false);
                break;

            case CompensationStartedEvent compensationStarted:
                await HandleCompensationStartedAsync(compensationStarted).ConfigureAwait(false);
                break;

            case CompensationCompletedEvent compensationCompleted:
                await HandleCompensationCompletedAsync(compensationCompleted).ConfigureAwait(false);
                break;

            default:
                _logger.LogDebug("Unhandled event type: {EventType}", @event.EventType);
                break;
        }
    }

    private async Task HandleSagaCreatedAsync(SagaCreatedEvent @event)
    {
        _logger.LogInformation(
            "Processing SagaCreated event | SagaId: {SagaId}, Name: {SagaName}",
            @event.SagaId, @event.SagaName);
        await Task.CompletedTask;
    }

    private async Task HandleSagaCompletedAsync(SagaCompletedEvent @event)
    {
        _logger.LogInformation(
            "Processing SagaCompleted event | SagaId: {SagaId}, Duration: {Duration}ms",
            @event.SagaId, @event.DurationMs);
        await Task.CompletedTask;
    }

    private async Task HandleSagaFailedAsync(SagaFailedEvent @event)
    {
        _logger.LogWarning(
            "Processing SagaFailed event | SagaId: {SagaId}, Error: {Error}",
            @event.SagaId, @event.ErrorMessage);
        await Task.CompletedTask;
    }

    private async Task HandleCompensationStartedAsync(CompensationStartedEvent @event)
    {
        _logger.LogWarning(
            "Processing CompensationStarted event | SagaId: {SagaId}, Strategy: {Strategy}",
            @event.SagaId, @event.CompensationStrategy);
        await Task.CompletedTask;
    }

    private async Task HandleCompensationCompletedAsync(CompensationCompletedEvent @event)
    {
        _logger.LogInformation(
            "Processing CompensationCompleted event | SagaId: {SagaId}, Steps: {Steps}",
            @event.SagaId, @event.CompensatedSteps);
        await Task.CompletedTask;
    }
}
