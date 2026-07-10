using System;
using System.Diagnostics;

namespace SagaOrchestrator.Benchmarks
{
    /// <summary>
    /// Extension methods that help orchestrate and measure the benchmark workflow.
    /// </summary>
    public static class SagaOrchestratorBenchmarksExtensions
    {
        /// <summary>
        /// Executes the full typical benchmark flow in the correct order.
        /// Calls the public members of <see cref="SagaOrchestratorBenchmarks"/> that are
        /// required to set up a saga, add a step, create the saga instance,
        /// start it and then execute the next step.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance. Cannot be null.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        public static void RunFullCycle(this SagaOrchestratorBenchmarks benchmarks)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);

            // The order mirrors the logical flow of a saga lifecycle.
            benchmarks.Setup();
            benchmarks.CreateDefinition();
            benchmarks.AddStep();
            benchmarks.CreateSaga();
            benchmarks.StartSaga();
            benchmarks.ExecuteNextStep();
        }

        /// <summary>
        /// Executes the supplied <paramref name="action"/> while measuring its elapsed time.
        /// Returns the <see cref="TimeSpan"/> representing the duration.
        /// </summary>
        /// <param name="_">The benchmark instance (unused).</param>
        /// <param name="action">The action to measure. Cannot be null.</param>
        /// <returns>The elapsed time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
        public static TimeSpan TimeAction(this SagaOrchestratorBenchmarks _, Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// Performs a warm‑up by invoking <see cref="SagaOrchestratorBenchmarks.ExecuteNextStep"/>
        /// a number of times. This can be useful to mitigate JIT warm‑up effects before
        /// measuring performance.
        /// </summary>
        /// <param name="benchmarks">The benchmark instance. Cannot be null.</param>
        /// <param name="iterations">How many warm‑up iterations to run. Default is 3.</param>
        /// <exception cref="ArgumentNullException"><paramref name="benchmarks"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="iterations"/> is negative.</exception>
        public static void WarmUp(this SagaOrchestratorBenchmarks benchmarks, int iterations = 3)
        {
            ArgumentNullException.ThrowIfNull(benchmarks);
            ArgumentOutOfRangeException.ThrowIfNegative(iterations);

            for (int i = 0; i < iterations; i++)
            {
                benchmarks.ExecuteNextStep();
            }
        }

        /// <summary>
        /// Attempts to execute the benchmark workflow by invoking the <see cref="ExecuteSagaSteps_Benchmark"/> method.
        /// Returns a boolean indicating whether the execution completed without throwing.
        /// </summary>
        /// <param name="_">The benchmark instance (unused).</param>
        /// <returns><see langword="true"/> if the benchmark completed successfully; otherwise, <see langword="false"/>.</returns>
        public static bool TryExecuteBenchmark(this SagaOrchestratorBenchmarks _)
        {
            ArgumentNullException.ThrowIfNull(_);

            try
            {
                var benchmarks = new SagaOrchestratorBenchmarks();
                benchmarks.Setup();
                benchmarks.CreateDefinition();
                benchmarks.AddStep();
                benchmarks.CreateSaga();
                benchmarks.StartSaga();
                benchmarks.ExecuteNextStep();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
