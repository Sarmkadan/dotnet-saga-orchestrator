#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Net.Http.Headers;
using SagaOrchestrator.Core.Extensions;
using SagaOrchestrator.Core.Utilities;
using SagaOrchestrator.Infrastructure.Resilience;

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
    private readonly Dictionary<string, HttpClient> _cachedClients;

    public HttpClientFactory()
    {
        _cachedClients = new();
    }

    public HttpClient CreateClient(string name, HttpClientConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        config.Validate();
        if (_cachedClients.TryGetValue(name, out var client))
            return client;

        var httpClient = new HttpClient(new PolicyHttpMessageHandler(name, config.Policy))
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
}

/// <summary>
/// Configuration for a named <see cref="HttpClient"/>, including base address, timeout,
/// authentication, default headers, and the resilience policy applied to its requests.
/// </summary>
public class HttpClientConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public string? AuthToken { get; set; }
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();

    /// <summary>
    /// Retry and circuit-breaker configuration applied to requests sent through this client.
    /// Defaults to <see cref="PolicyOptions"/> defaults when not overridden.
    /// </summary>
    public PolicyOptions Policy { get; set; } = new();

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
            throw new ArgumentException("BaseUrl cannot be empty.", nameof(BaseUrl));
        if (TimeoutSeconds <= 0)
            throw new ArgumentException("TimeoutSeconds must be greater than zero.", nameof(TimeoutSeconds));
        if (Policy.MaxRetries < 0)
            throw new ArgumentException("MaxRetries cannot be negative.", nameof(Policy.MaxRetries));
    }
}

/// <summary>
/// Configuration surface for the resilience behavior applied by <see cref="PolicyHttpMessageHandler"/>:
/// retry attempts with exponential backoff and jitter, and circuit-breaker thresholds.
/// </summary>
public sealed class PolicyOptions
{
    /// <summary>Maximum number of retry attempts for a transient failure on a retryable request.</summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>Base delay, in milliseconds, used as the starting point for exponential backoff.</summary>
    public int BackoffBaseMs { get; init; } = 200;

    /// <summary>Upper bound, in milliseconds, that a computed backoff delay is capped to.</summary>
    public int MaxBackoffMs { get; init; } = 10_000;

    /// <summary>When true, applies random jitter to each computed backoff delay.</summary>
    public bool UseJitter { get; init; } = true;

    /// <summary>Consecutive transient failures on a named client that trip its circuit breaker open.</summary>
    public int CircuitBreakerFailureThreshold { get; init; } = 5;

    /// <summary>How long, in seconds, a tripped circuit breaker stays open before allowing a probe request.</summary>
    public int CircuitBreakerBreakDurationSeconds { get; init; } = 30;

    /// <summary>
    /// Builds the <see cref="RetryPolicy"/> described by this configuration.
    /// </summary>
    /// <returns>A configured <see cref="RetryPolicy"/> instance.</returns>
    public RetryPolicy ToRetryPolicy() =>
        new(MaxRetries, BackoffBaseMs, backoffMultiplier: 2.0, MaxBackoffMs, UseJitter);

    /// <summary>
    /// Builds the <see cref="ICircuitBreaker"/> described by this configuration.
    /// </summary>
    /// <returns>A configured <see cref="CircuitBreaker"/> instance.</returns>
    public ICircuitBreaker ToCircuitBreaker() =>
        new CircuitBreaker(CircuitBreakerFailureThreshold, CircuitBreakerBreakDurationSeconds);
}

/// <summary>
/// Marks an outgoing <see cref="HttpRequestMessage"/> as safe to retry automatically even though
/// its HTTP method is not inherently idempotent (for example, a saga step call that the caller
/// has verified is safe to re-issue, such as one guarded by an idempotency key).
/// </summary>
public static class HttpRequestRetryOptions
{
    /// <summary>
    /// The <see cref="HttpRequestOptions"/> key used to flag a request as explicitly retryable.
    /// </summary>
    public static readonly HttpRequestOptionsKey<bool> AllowRetryKey = new("AllowAutomaticRetry");

    /// <summary>
    /// Flags <paramref name="request"/> as explicitly safe to retry on transient failure.
    /// </summary>
    /// <param name="request">The request to flag.</param>
    /// <returns>The same request instance, for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static HttpRequestMessage AllowRetry(this HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Options.Set(AllowRetryKey, true);
        return request;
    }

    /// <summary>
    /// Determines whether <paramref name="request"/> has been explicitly flagged as retryable.
    /// </summary>
    /// <param name="request">The request to inspect.</param>
    /// <returns>True if the request was flagged via <see cref="AllowRetry"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    public static bool IsRetryAllowed(this HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Options.TryGetValue(AllowRetryKey, out var allowed) && allowed;
    }
}

/// <summary>
/// Delegating handler that applies retry-with-jitter and per-client circuit-breaker policies
/// to outgoing HTTP requests. Automatic retries only apply to inherently idempotent methods
/// (GET, HEAD, OPTIONS, PUT, DELETE) or requests explicitly flagged via
/// <see cref="HttpRequestRetryOptions.AllowRetry"/> - non-idempotent calls such as saga step
/// POSTs are sent at most once so compensation logic cannot double-execute.
/// </summary>
public class PolicyHttpMessageHandler : DelegatingHandler
{
    private static readonly HashSet<HttpMethod> IdempotentMethods = new()
    {
        HttpMethod.Get,
        HttpMethod.Head,
        HttpMethod.Options,
        HttpMethod.Put,
        HttpMethod.Delete
    };

    private readonly string _clientName;
    private readonly RetryPolicy _retryPolicy;
    private readonly ICircuitBreaker _circuitBreaker;

    /// <summary>
    /// Creates a handler that applies the given <paramref name="policy"/> to requests
    /// issued for the named client <paramref name="clientName"/>.
    /// </summary>
    /// <param name="clientName">The logical name of the client this handler serves; used as the circuit-breaker identifier.</param>
    /// <param name="policy">The retry and circuit-breaker configuration to apply.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="clientName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is null.</exception>
    public PolicyHttpMessageHandler(string clientName, PolicyOptions policy)
    {
        ArgumentException.ThrowIfNullOrEmpty(clientName);
        ArgumentNullException.ThrowIfNull(policy);

        _clientName = clientName;
        _retryPolicy = policy.ToRetryPolicy();
        _circuitBreaker = policy.ToCircuitBreaker();
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
    /// <exception cref="HttpRequestException">Thrown when the circuit breaker for this client is open.</exception>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var canAutoRetry = IsAutoRetryable(request);

        try
        {
            return await _circuitBreaker.ExecuteAsync(
                () => SendWithRetryAsync(request, canAutoRetry, cancellationToken),
                _clientName);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Circuit breaker is open", StringComparison.Ordinal))
        {
            throw new HttpRequestException(
                $"Circuit breaker is open for client '{_clientName}'; request rejected without attempting delivery.",
                ex);
        }
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(
        HttpRequestMessage request,
        bool canAutoRetry,
        CancellationToken cancellationToken)
    {
        var attempt = 1;

        while (true)
        {
            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException) when (canAutoRetry && attempt <= _retryPolicy.MaxRetries)
            {
                await DelayBeforeRetryAsync(attempt, cancellationToken);
                attempt++;
                continue;
            }

            if (!IsTransientFailure(response) || !canAutoRetry || attempt > _retryPolicy.MaxRetries)
                return response;

            // Transient failure on a retryable request with attempts remaining: back off and retry.
            response.Dispose();
            await DelayBeforeRetryAsync(attempt, cancellationToken);
            attempt++;
        }
    }

    private async Task DelayBeforeRetryAsync(int attempt, CancellationToken cancellationToken)
    {
        var delayMs = _retryPolicy.CalculateDelay(attempt);
        if (delayMs > 0)
            await Task.Delay(delayMs, cancellationToken);
    }

    private static bool IsAutoRetryable(HttpRequestMessage request) =>
        IdempotentMethods.Contains(request.Method) || request.IsRetryAllowed();

    private static bool IsTransientFailure(HttpResponseMessage response) =>
        (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout;
}
