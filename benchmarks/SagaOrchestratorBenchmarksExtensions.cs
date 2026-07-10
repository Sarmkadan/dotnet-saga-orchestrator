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
        public static void RunFullCycle(this SagaOrchestratorBenchmarks benchmarks)
        {
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
        public static TimeSpan TimeAction(this SagaOrchestratorBenchmarks _, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

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
        /// <param name="benchmarks">The benchmark instance.</param>
        /// <param name="iterations">How many warm‑up iterations to run. Default is 3.</param>
        public static void WarmUp(this SagaOrchestratorBenchmarks benchmarks, int iterations = 3)
        {
            if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations));

            for (int i = 0; i < iterations; i++)
            {
                benchmarks.ExecuteNextStep();
            }
        }

        /// <summary>
        /// Runs the benchmark's <c>Main</c> method inside a try/catch block and returns
        /// a boolean indicating whether it completed without throwing.
        /// </summary>
        public static bool TryRunMain(this SagaOrchestratorBenchmarks _)
        {
            try
            {
                SagaOrchestratorBenchmarks.Main();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
