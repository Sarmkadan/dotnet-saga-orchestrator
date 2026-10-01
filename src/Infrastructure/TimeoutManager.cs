#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SagaOrchestrator.Configuration;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Manages per-saga timeout enforcement using timers. When a saga exceeds its
/// allowed execution time, the registered callback fires to trigger abort/compensation.
/// </summary>
public class TimeoutManager : IDisposable
{
    private readonly ConcurrentDictionary<string, Timer> _timers = new();
    private readonly ConcurrentDictionary<string, DateTime> _registrationTimes = new();
    private readonly ILogger<TimeoutManager> _logger;
    private readonly SagaOptions _options;
    private Action<string>? _onTimeout;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutManager"/> class.
    /// </summary>
    public TimeoutManager(
        ILogger<TimeoutManager> logger,
        IOptions<SagaOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new SagaOptions();
    }

    /// <summary>
    /// Sets the callback that is invoked when a saga times out.
    /// </summary>
    /// <param name="onTimeout">Callback receiving the saga identifier.</param>
    public void SetTimeoutCallback(Action<string> onTimeout)
    {
        _onTimeout = onTimeout ?? throw new ArgumentNullException(nameof(onTimeout));
    }

    /// <summary>
    /// Registers a saga for timeout monitoring. If the saga does not complete
    /// within the specified duration, the timeout callback fires.
    /// </summary>
    /// <param name="sagaId">The saga identifier to monitor.</param>
    /// <param name="timeout">Maximum allowed execution duration.</param>
    public void RegisterSaga(string sagaId, TimeSpan timeout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        if (timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be positive.");

        ObjectDisposedException.ThrowIf(_disposed, this);

        var timer = new Timer(
            OnTimerElapsed,
            sagaId,
            timeout,
            Timeout.InfiniteTimeSpan);

        if (!_timers.TryAdd(sagaId, timer))
        {
            timer.Dispose();
            _logger.LogWarning("Saga {SagaId} is already registered for timeout", sagaId);
            return;
        }

        _registrationTimes.TryAdd(sagaId, DateTime.UtcNow);
        _logger.LogDebug("Saga {SagaId} registered with timeout {Timeout}", sagaId, timeout);
    }

    /// <summary>
    /// Removes a saga from timeout monitoring, typically after successful completion.
    /// </summary>
    /// <param name="sagaId">The saga identifier to unregister.</param>
    public void UnregisterSaga(string sagaId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);

        if (_timers.TryRemove(sagaId, out var timer))
        {
            timer.Dispose();
            _registrationTimes.TryRemove(sagaId, out _);
            _logger.LogDebug("Saga {SagaId} unregistered from timeout monitoring", sagaId);
        }
    }

    /// <summary>
    /// Returns the identifiers of all sagas currently being monitored for timeout.
    /// </summary>
    /// <returns>List of monitored saga identifiers.</returns>
    public List<string> GetMonitoredSagas()
    {
        return _timers.Keys.ToList();
    }

    /// <summary>
    /// Returns how long a saga has been running since registration.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <returns>Elapsed time, or null if the saga is not monitored.</returns>
    public TimeSpan? GetElapsedTime(string sagaId)
    {
        if (_registrationTimes.TryGetValue(sagaId, out var registered))
            return DateTime.UtcNow - registered;
        return null;
    }

    private void OnTimerElapsed(object? state)
    {
        if (state is not string sagaId) return;

        _logger.LogWarning("Saga {SagaId} has timed out", sagaId);
        UnregisterSaga(sagaId);

        try
        {
            _onTimeout?.Invoke(sagaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in timeout callback for saga {SagaId}", sagaId);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var kvp in _timers)
        {
            kvp.Value.Dispose();
        }
        _timers.Clear();
        _registrationTimes.Clear();
    }
}
