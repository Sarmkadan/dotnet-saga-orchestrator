#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SagaOrchestrator.Core.Extensions;
using SagaOrchestrator.Infrastructure.Events;
using SagaOrchestrator.Infrastructure.Http;
using IHttpClientFactory = SagaOrchestrator.Infrastructure.Http.IHttpClientFactory;

namespace SagaOrchestrator.Infrastructure.Integration;

/// <summary>
/// Webhook handler for sending saga events to external systems.
/// Manages webhook subscriptions and reliable delivery of events.
/// </summary>
public interface IWebhookHandler
{
    Task SubscribeWebhookAsync(string url, string[] eventTypes);
    Task UnsubscribeWebhookAsync(string url);
    Task SendWebhookAsync<T>(string url, T @event) where T : DomainEvent;
    List<WebhookSubscription> GetSubscriptions();
}

public class WebhookHandler : IWebhookHandler
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEventBus _eventBus;
    private readonly ILogger<WebhookHandler> _logger;
    private readonly List<WebhookSubscription> _subscriptions;
    private readonly object _lock = new();

    public WebhookHandler(
        IHttpClientFactory httpClientFactory,
        IEventBus eventBus,
        ILogger<WebhookHandler> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscriptions = new();
    }

    public async Task SubscribeWebhookAsync(string url, string[] eventTypes)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Webhook URL cannot be null or empty", nameof(url));

        url.ValidateUrl(nameof(url));

        lock (_lock)
        {
            var existing = _subscriptions.FirstOrDefault(s => s.Url == url);
            if (existing != null)
            {
                existing.EventTypes = eventTypes;
                existing.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                _subscriptions.Add(new WebhookSubscription
                {
                    Id = Guid.NewGuid().ToString(),
                    Url = url,
                    EventTypes = eventTypes,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
            }
        }

        _logger.LogInformation("Webhook subscribed | Url: {Url}, Events: {EventCount}", url, eventTypes.Length);

        // Subscribe to event bus for all event types
        await _eventBus.PublishAsync(new WebhookRegisteredEvent
        {
            WebhookUrl = url,
            EventTypes = string.Join(", ", eventTypes)
        });
    }

    public async Task UnsubscribeWebhookAsync(string url)
    {
        lock (_lock)
        {
            var subscription = _subscriptions.FirstOrDefault(s => s.Url == url);
            if (subscription != null)
            {
                subscription.IsActive = false;
                _subscriptions.Remove(subscription);
            }
        }

        _logger.LogInformation("Webhook unsubscribed | Url: {Url}", url);
    }

    public async Task SendWebhookAsync<T>(string url, T @event) where T : DomainEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            var config = new HttpClientConfiguration
            {
                BaseUrl = url,
                TimeoutSeconds = 10
            };

            var client = _httpClientFactory.CreateClient("webhook", config);
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(@event),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            _logger.LogInformation(
                "Webhook delivered successfully | Url: {Url}, EventType: {EventType}",
                url, @event.EventType);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Failed to deliver webhook | Url: {Url}, EventType: {EventType}",
                url, @event.EventType);
            throw;
        }
    }

    public List<WebhookSubscription> GetSubscriptions()
    {
        lock (_lock)
        {
            return _subscriptions.Where(s => s.IsActive).ToList();
        }
    }
}

public class WebhookSubscription
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string[] EventTypes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsActive { get; set; }
    public int DeliveryCount { get; set; }
    public int FailureCount { get; set; }
}

public class WebhookRegisteredEvent : DomainEvent
{
    public string WebhookUrl { get; set; } = string.Empty;
    public string EventTypes { get; set; } = string.Empty;
}
