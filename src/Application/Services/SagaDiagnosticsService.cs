using SagaOrchestrator.Services;

namespace SagaOrchestrator.Services;

/// <summary>
/// Provides diagnostic checks over saga executions, such as detecting sagas that have stopped making progress.
/// </summary>
public class SagaDiagnosticsService
{
    /// <summary>
    /// Finds sagas that have not progressed within the specified threshold.
    /// </summary>
    /// <param name="threshold">The minimum time without progress after which a saga is considered stuck.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task FindStuckSagasAsync(TimeSpan threshold)
    {
        // implement me
    }
}