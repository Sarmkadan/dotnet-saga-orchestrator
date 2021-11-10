#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net.Http.Headers;
using Microsoft.Extensions.Http;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace SagaOrchestrator.Infrastructure.Http;

/// <summary>
/// Factory for creating HttpClient instances with resilience policies.
/// Implements retry, circuit breaker, and timeout policies for external service calls.
/// </summary>
public interface IHttpClientFactory
{
    HttpClient CreateClient(string name, HttpClientConfiguration config);
    Task<T> SendAsync<T>(HttpClient client, HttpRequestMessage request);
}

public class HttpClientFactory : IHttpClientFactory
{
    private readonly IAsyncPolicy<HttpResponseMessage> _resiliencePolicy;
    private readonly Dictionary<string, HttpClient> _cachedClients;

    public HttpClientFactory()
    {
        _cachedClients = new();
        _resiliencePolicy = CreateResiliencePolicy();
    }

    public HttpClient CreateClient(string name, HttpClientConfiguration config)
    {
        if (_cachedClients.TryGetValue(name, out var client))
            return client;

        var httpClient = new HttpClient(new PolicyHttpMessageHandler(_resiliencePolicy))
        {
            BaseAddress = new Uri(config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds)
        };

        // Add default headers
        if (!config.DefaultHeaders.IsEmpty())
        {
            foreach (var header in config.DefaultHeaders)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        // Add authorization header if provided
        if (!config.AuthToken.IsNullOrEmpty())
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config.AuthToken);
        }

        _cachedClients[name] = httpClient;
        return httpClient;
    }

    public async Task<T> SendAsync<T>(HttpClient client, HttpRequestMessage request)
    {
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(content)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    private IAsyncPolicy<HttpResponseMessage> CreateResiliencePolicy()
    {
        // Retry policy: 3 attempts with exponential backoff
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<IOException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Retry attempt {retryCount} after {timespan.TotalSeconds}s");
                });

        // Circuit breaker: break after 5 failures within 30 seconds
        var circuitBreakerPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<IOException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, timespan) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Circuit breaker opened for {timespan.TotalSeconds}s");
                });

        // Combine policies: retry first, then circuit breaker
        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }
}

public class HttpClientConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public string? AuthToken { get; set; }
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
}

public class PolicyHttpMessageHandler : DelegatingHandler
{
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;

    public PolicyHttpMessageHandler(IAsyncPolicy<HttpResponseMessage> policy)
    {
        _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = new Polly.Context();
        return await _policy.ExecuteAsync(
            async (ct) => await base.SendAsync(request, ct),
            cancellationToken);
    }
}
