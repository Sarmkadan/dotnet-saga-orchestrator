#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Immutable;

namespace SagaOrchestrator.Infrastructure.Events;

/// <summary>
/// A composite observer that delegates to multiple <see cref="ISagaEventObserver"/> instances.
/// </summary>
/// <remarks>
/// This allows multiple observers (metrics, logging, timeline entries, etc.) to be registered
/// and invoked collectively without each caller needing to iterate over a list of observers.
/// </remarks>
public class CompositeSagaEventObserver : ISagaEventObserver
{
    private readonly ImmutableArray<ISagaEventObserver> _observers;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeSagaEventObserver"/> class.
    /// </summary>
    /// <param name="observers">The observers to compose. Must not be null or contain null values.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="observers"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="observers"/> contains null values.</exception>
    public CompositeSagaEventObserver(IEnumerable<ISagaEventObserver> observers)
    {
        ArgumentNullException.ThrowIfNull(observers);

        var observerList = observers.ToList();
        if (observerList.Any(o => o is null))
        {
            throw new ArgumentException("Observer collection must not contain null values", nameof(observers));
        }

        _observers = observerList.ToImmutableArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeSagaEventObserver"/> class with an array of observers.
    /// </summary>
    /// <param name="observers">The observers to compose. Must not be null or contain null values.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="observers"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="observers"/> contains null values.</exception>
    public CompositeSagaEventObserver(params ISagaEventObserver[] observers)
    {
        ArgumentNullException.ThrowIfNull(observers);

        if (observers.Any(o => o is null))
        {
            throw new ArgumentException("Observer array must not contain null values", nameof(observers));
        }

        _observers = observers.ToImmutableArray();
    }

    /// <summary>
    /// Gets the number of observers in this composite.
    /// </summary>
    public int ObserverCount => _observers.Length;

    /// <summary>
    /// Called when a saga is created.
    /// </summary>
    /// <param name="@event">The saga created event.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="@event"/> is null.</exception>
    /// <remarks>
    /// Invokes <see cref="ISagaEventObserver.OnSagaCreatedAsync"/> on all composed observers.
    /// Errors from individual observers are caught and logged internally, ensuring one faulty observer
    /// does not prevent other observers from executing.
    /// </remarks>
    public async ValueTask OnSagaCreatedAsync(SagaCreatedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var tasks = new List<ValueTask>(_observers.Length);
        foreach (var observer in _observers)
        {
            try
            {
                tasks.Add(observer.OnSagaCreatedAsync(@event));
            }
            catch (Exception ex)
            {
                // Log error but continue with other observers
                // Note: This catches synchronous exceptions from observer constructors/property access
                Console.Error.WriteLine($"Error preparing OnSagaCreatedAsync for observer {observer.GetType().Name}: {ex.Message}");
            }
        }

        // Await all tasks, but don't let one failure stop others
        foreach (var task in tasks)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Errors are intentionally swallowed to keep the composite resilient
                // Individual observer implementations should already handle their own errors
                Console.Error.WriteLine($"Error in OnSagaCreatedAsync for observer: {ex.Message}");
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
    /// Invokes <see cref="ISagaEventObserver.OnSagaCompletedAsync"/> on all composed observers.
    /// Errors from individual observers are caught and logged internally, ensuring one faulty observer
    /// does not prevent other observers from executing.
    /// </remarks>
    public async ValueTask OnSagaCompletedAsync(SagaCompletedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var tasks = new List<ValueTask>(_observers.Length);
        foreach (var observer in _observers)
        {
            try
            {
                tasks.Add(observer.OnSagaCompletedAsync(@event));
            }
            catch (Exception ex)
            {
                // Log error but continue with other observers
                Console.Error.WriteLine($"Error preparing OnSagaCompletedAsync for observer {observer.GetType().Name}: {ex.Message}");
            }
        }

        // Await all tasks, but don't let one failure stop others
        foreach (var task in tasks)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Errors are intentionally swallowed to keep the composite resilient
                Console.Error.WriteLine($"Error in OnSagaCompletedAsync for observer: {ex.Message}");
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
    /// Invokes <see cref="ISagaEventObserver.OnSagaFailedAsync"/> on all composed observers.
    /// Errors from individual observers are caught and logged internally, ensuring one faulty observer
    /// does not prevent other observers from executing.
    /// </remarks>
    public async ValueTask OnSagaFailedAsync(SagaFailedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var tasks = new List<ValueTask>(_observers.Length);
        foreach (var observer in _observers)
        {
            try
            {
                tasks.Add(observer.OnSagaFailedAsync(@event));
            }
            catch (Exception ex)
            {
                // Log error but continue with other observers
                Console.Error.WriteLine($"Error preparing OnSagaFailedAsync for observer {observer.GetType().Name}: {ex.Message}");
            }
        }

        // Await all tasks, but don't let one failure stop others
        foreach (var task in tasks)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Errors are intentionally swallowed to keep the composite resilient
                Console.Error.WriteLine($"Error in OnSagaFailedAsync for observer: {ex.Message}");
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
    /// Invokes <see cref="ISagaEventObserver.OnCompensationStartedAsync"/> on all composed observers.
    /// Errors from individual observers are caught and logged internally, ensuring one faulty observer
    /// does not prevent other observers from executing.
    /// </remarks>
    public async ValueTask OnCompensationStartedAsync(CompensationStartedEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var tasks = new List<ValueTask>(_observers.Length);
        foreach (var observer in _observers)
        {
            try
            {
                tasks.Add(observer.OnCompensationStartedAsync(@event));
            }
            catch (Exception ex)
            {
                // Log error but continue with other observers
                Console.Error.WriteLine($"Error preparing OnCompensationStartedAsync for observer {observer.GetType().Name}: {ex.Message}");
            }
        }

        // Await all tasks, but don't let one failure stop others
        foreach (var task in tasks)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Errors are intentionally swallowed to keep the composite resilient
                Console.Error.WriteLine($"Error in OnCompensationStartedAsync for observer: {ex.Message}");
            }
        }
    }
}