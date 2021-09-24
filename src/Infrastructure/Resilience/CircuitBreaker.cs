// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Infrastructure.Resilience;

/// <summary>
/// Circuit breaker pattern implementation for fault tolerance.
/// Prevents cascading failures by stopping requests to failing services.
/// </summary>
public interface ICircuitBreaker
{
    Task<bool> ExecuteAsync(Func<Task> action, string identifier);
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, string identifier);
    CircuitBreakerState GetState(string identifier);
    void Reset(string identifier);
}

public enum CircuitBreakerState
{
    Closed,      // Normal operation
    Open,        // Failing, block requests
    HalfOpen     // Testing if service recovered
}

public class CircuitBreaker : ICircuitBreaker
{
    private readonly Dictionary<string, CircuitBreakerMetrics> _metrics;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly object _lock = new();

    public CircuitBreaker(int failureThreshold = 5, int timeoutSeconds = 60)
    {
        _failureThreshold = failureThreshold.GreaterThan(0, nameof(failureThreshold));
        _timeout = TimeSpan.FromSeconds(timeoutSeconds);
        _metrics = new();
    }

    public async Task<bool> ExecuteAsync(Func<Task> action, string identifier)
    {
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

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string identifier)
    {
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

    public CircuitBreakerState GetState(string identifier)
    {
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

    public void Reset(string identifier)
    {
        lock (_lock)
        {
            _metrics.Remove(identifier);
        }
    }

    private bool CanExecute(string identifier)
    {
        lock (_lock)
        {
            if (!_metrics.TryGetValue(identifier, out var metrics))
            {
                _metrics[identifier] = new CircuitBreakerMetrics();
                return true;
            }

            if (metrics.State == CircuitBreakerState.Closed)
                return true;

            if (metrics.State == CircuitBreakerState.Open)
            {
                if (DateTime.UtcNow - metrics.LastFailureTime >= _timeout)
                {
                    metrics.State = CircuitBreakerState.HalfOpen;
                    metrics.FailureCount = 0;
                    return true;
                }
                return false;
            }

            // HalfOpen - allow one request
            return true;
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
                    metrics.State = CircuitBreakerState.Closed;
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

            if (metrics.FailureCount >= _failureThreshold)
                metrics.State = CircuitBreakerState.Open;
            else if (metrics.State == CircuitBreakerState.HalfOpen)
                metrics.State = CircuitBreakerState.Open;
        }
    }

    private class CircuitBreakerMetrics
    {
        public CircuitBreakerState State { get; set; } = CircuitBreakerState.Closed;
        public int FailureCount { get; set; }
        public int SuccessCount { get; set; }
        public DateTime LastFailureTime { get; set; }
    }
}
