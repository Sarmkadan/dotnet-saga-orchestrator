using System;
using System.Threading.Tasks;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides extension methods for the <see cref="CircuitBreakerTests"/> class to aggregate test execution.
/// </summary>
public static class CircuitBreakerTestsExtensions
{
    /// <summary>
    /// Runs basic successful execution tests.
    /// </summary>
    /// <param name="instance">The test instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public static async Task RunBasicExecutionTestsAsync(this CircuitBreakerTests instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        await instance.ExecuteAsync_SuccessfulAction_RecordsSuccessAndReturnsTrue();
        await instance.ExecuteAsync_GenericSuccessfulAction_ReturnsValue();
        await instance.ExecuteAsync_SuccessfulAction_IncrementSuccess();
        await instance.ExecuteAsync_DifferentIdentifiers_MaintainIndependentState();
    }

    /// <summary>
    /// Runs circuit state and failure handling tests.
    /// </summary>
    /// <param name="instance">The test instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public static async Task RunCircuitStateTestsAsync(this CircuitBreakerTests instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        await instance.ExecuteAsync_FailingAction_ThrowsAndRecordsFailure();
        await instance.ExecuteAsync_MultipleFailures_OpensCircuit();
        await instance.ExecuteAsync_WhenCircuitOpen_ReturnsFalse();
        await instance.ExecuteAsync_WhenCircuitOpen_GenericThrowsException();
        await instance.ExecuteAsync_SuccessInHalfOpenClosesCircuit();
        await instance.ExecuteAsync_FailureInHalfOpenReopensCircuit();
    }

    /// <summary>
    /// Runs management and recovery tests.
    /// </summary>
    /// <param name="instance">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public static void RunManagementTests(this CircuitBreakerTests instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        instance.GetState_UnknownIdentifier_ReturnsClosed();
        instance.Reset_ClearsMetricsForIdentifier();
        instance.EvictStaleEntries_RemovesUnusedClosedCircuits();
    }
}
