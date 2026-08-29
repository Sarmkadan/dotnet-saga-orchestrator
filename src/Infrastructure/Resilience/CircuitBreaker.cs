#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using SagaOrchestrator.Core.Extensions;
using SagaOrchestrator.Infrastructure.Logging;

using System.Threading;

namespace SagaOrchestrator.Infrastructure.Resilience;

/// <summary>
/// Circuit breaker pattern implementation for fault tolerance.
/// Prevents cascading failures by stopping requests to failing services.
/// </summary>
public interface ICircuitBreaker
{
    /// <summary>
    /// Executes the action under the breaker for the given identifier.
    /// </summary>
    /// <param name="action">The action to guard.</param>
    /// <param name="identifier">The logical target the breaker tracks (e.g. a service name).</param>
    /// <returns><c>true</c> if the action ran; <c>false</c> if the breaker was open and rejected it.</returns>
    Task<bool> ExecuteAsync(Func<Task> action, string identifier);

    /// <summary>
    /// Executes the action under the breaker and returns its result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="action">The action to guard.</param>
    /// <param name="identifier">The logical target the breaker tracks.</param>
    /// <returns>The action result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the breaker is open.</exception>
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, string identifier);

    /// <summary>Gets the current state of the breaker for the given identifier.</summary>
    /// <param name="identifier">The logical target the breaker tracks.</param>
    /// <returns>The current <see cref="CircuitBreakerState"/>.</returns>
    CircuitBreakerState GetState(string identifier);

    /// <summary>Clears all recorded state for the given identifier, returning it to closed.</summary>
    /// <param name="identifier">The logical target to reset.</param>
    void Reset(string identifier);
}

/// <summary>
/// The states of a circuit breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>Normal operation; requests flow through and failures are counted.</summary>
    Closed,

    /// <summary>The breaker has tripped; requests are rejected immediately until the open window elapses.</summary>
    Open,

    /// <summary>A single trial request is allowed through to test whether the target has recovered.</summary>
    HalfOpen
}

/// <summary>
/// Default per-identifier circuit breaker implementation. See <see cref="ICircuitBreaker"/>.
/// </summary>
public class CircuitBreaker : ICircuitBreaker
{
    private readonly ConcurrentDictionary<string, CircuitBreakerMetrics> _metrics;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly object _lock = new();
    private readonly ISagaLogger? _logger;

    /// <summary>
    /// Initializes a new circuit breaker.
    /// </summary>
    /// <param name="failureThreshold">Consecutive failures that trip the breaker open.</param>
    /// <param name="timeoutSeconds">How long the breaker stays open before allowing a half-open probe.</param>
    /// <param name="logger">The saga logger (optional).</param>
    public CircuitBreaker(int failureThreshold = 5, int timeoutSeconds = 60, ISagaLogger? logger = null)
    {
        _failureThreshold = failureThreshold.GreaterThan(0, nameof(failureThreshold));
        _timeout = TimeSpan.FromSeconds(timeoutSeconds);
        _logger = logger;
        _metrics = new();
    }

    /// <inheritdoc />
    public async Task<bool> ExecuteAsync(Func<Task> action, string identifier)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        try
        {
            var canExecute = CanExecute(identifier);
            if (!canExecute)
                return false;

            await action();
            RecordSuccess(identifier);
            return true;
        }
        catch (Exception)
        {
            RecordFailure(identifier);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string identifier)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        try
        {
            var canExecute = CanExecute(identifier);
            if (!canExecute)
                throw new InvalidOperationException($"Circuit breaker is open for {identifier}");

            var result = await action();
            RecordSuccess(identifier);
            return result;
        }
        catch (Exception)
        {
            RecordFailure(identifier);
            throw;
        }
    }

    /// <inheritdoc />
    public CircuitBreakerState GetState(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        lock (_lock)
        {
            if (!_metrics.TryGetValue(identifier, out var metrics))
                return CircuitBreakerState.Closed;

            if (metrics.State == CircuitBreakerState.Open)
            {
                if (DateTime.UtcNow - metrics.LastFailureTime >= _timeout)
                    return CircuitBreakerState.HalfOpen;
                return CircuitBreakerState.Open;
            }

            return metrics.State;
        }
    }

    /// <inheritdoc />
    public void Reset(string identifier)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);
        lock (_lock)
        {
            _metrics.TryRemove(identifier, out _);
            _logger?.LogCircuitBreakerStateChanged(
                identifier,
                "Reset",
                null);
        }
    }

    private bool CanExecute(string identifier)
    {
        lock (_lock)
        {
            if (!_metrics.TryGetValue(identifier, out var metrics))
            {
                _metrics[identifier] = new CircuitBreakerMetrics { LastAccessedAt = DateTime.UtcNow };
                return true;
            }

            metrics.LastAccessedAt = DateTime.UtcNow;

            if (metrics.State == CircuitBreakerState.Closed)
                return true;

            if (metrics.State == CircuitBreakerState.Open)
            {
                if (DateTime.UtcNow - metrics.LastFailureTime >= _timeout)
                {
                    metrics.State = CircuitBreakerState.HalfOpen;
                    metrics.FailureCount = 0;
                    _logger?.LogCircuitBreakerStateChanged(
                        identifier,
                        "Open -> HalfOpen",
                        new { OpenDurationSeconds = (DateTime.UtcNow - metrics.LastFailureTime).TotalSeconds });
                    return true;
                }
                return false;
            }

            // HalfOpen - allow only one request at a time
            if (metrics.State == CircuitBreakerState.HalfOpen)
            {
                // Use Interlocked to atomically check and set the execution flag
                // This ensures only one thread can execute the probe request
                if (Interlocked.CompareExchange(ref metrics.ExecutionInProgress, 1, 0) == 0)
                {
                    return true;
                }
                return false;
            }

            return false;
        }
    }

    private void RecordSuccess(string identifier)
    {
        lock (_lock)
        {
            if (_metrics.TryGetValue(identifier, out var metrics))
            {
                metrics.FailureCount = 0;
                metrics.SuccessCount++;
                if (metrics.State == CircuitBreakerState.HalfOpen)
                {
                    metrics.State = CircuitBreakerState.Closed;
                    _logger?.LogCircuitBreakerStateChanged(
                        identifier,
                        "HalfOpen -> Closed",
                        null);
                }

                // Reset the execution flag when probe succeeds
                metrics.ExecutionInProgress = 0;
            }
        }
    }

    private void RecordFailure(string identifier)
    {
        lock (_lock)
        {
            if (!_metrics.TryGetValue(identifier, out var metrics))
            {
                metrics = new CircuitBreakerMetrics();
                _metrics[identifier] = metrics;
            }

            metrics.FailureCount++;
            metrics.LastFailureTime = DateTime.UtcNow;
            metrics.ExecutionInProgress = 0; // Reset execution flag on failure

            if (metrics.State == CircuitBreakerState.HalfOpen)
            {
                metrics.State = CircuitBreakerState.Open;
                _logger?.LogCircuitBreakerStateChanged(
                    identifier,
                    "HalfOpen -> Open",
                    null);
            }
            else if (metrics.State == CircuitBreakerState.Closed && metrics.FailureCount >= _failureThreshold)
            {
                metrics.State = CircuitBreakerState.Open;
                _logger?.LogCircuitBreakerStateChanged(
                    identifier,
                    "Closed -> Open",
                    new { FailureCount = metrics.FailureCount, Threshold = _failureThreshold });
            }
        }
    }

    private class CircuitBreakerMetrics
    {
        public CircuitBreakerState State { get; set; } = CircuitBreakerState.Closed;
        public int FailureCount { get; set; }
        public int SuccessCount { get; set; }
        public DateTime LastFailureTime { get; set; }
        public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
        public int ExecutionInProgress;
    }

    /// <summary>
    /// Removes metrics entries that haven't been accessed within the specified time window.
    /// Call periodically to prevent unbounded growth of the metrics dictionary.
    /// </summary>
    public int EvictStaleEntries(TimeSpan maxIdleTime)
    {
        var cutoff = DateTime.UtcNow - maxIdleTime;
        var staleKeys = _metrics
            .Where(kvp => kvp.Value.LastAccessedAt < cutoff && kvp.Value.State == CircuitBreakerState.Closed)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in staleKeys)
        {
            _metrics.TryRemove(key, out _);
        }

        return staleKeys.Count;
    }
}
