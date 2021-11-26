using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Benchmarks
{
    /// <summary>
    /// A set of benchmarks for the Saga Orchestrator.
    /// </summary>
    [MemoryDiagnoser]
    public class SagaOrchestratorBenchmarks
    {
        private ServiceProvider _serviceProvider = null!;
        private SagaDefinitionService _definitionService = null!;
        private SagaOrchestrationService _orchestrationService = null!;

        private SagaDefinition _definition = null!;
        private string _definitionId = null!;
        private string _sagaId = null!;

        /// <summary>
        /// Sets up the benchmark by building a minimal DI container with the orchestrator services,
        /// creating a baseline definition and saga, and storing them for reuse in benchmarks.
        /// </summary>
        [GlobalSetup]
        public void Setup()
        {
            // Build a minimal DI container with the orchestrator services
            var services = new ServiceCollection();
            services.AddSagaOrchestrator(); // extension method from ServiceConfiguration
            _serviceProvider = services.BuildServiceProvider();

            _definitionService = _serviceProvider.GetRequiredService<SagaDefinitionService>();
            _orchestrationService = _serviceProvider.GetRequiredService<SagaOrchestrationService>();

            // Create a baseline definition and saga that will be reused in benchmarks
            _definition = _definitionService
                .CreateDefinitionAsync("BenchmarkDefinition", "Definition used for benchmarks")
                .GetAwaiter()
                .GetResult();

            _definitionId = _definition.Id;

            var saga = _orchestrationService
                .CreateSagaAsync(_definition)
                .GetAwaiter()
                .GetResult();

            _sagaId = saga.Id;
        }

        /// <summary>
        /// Creates a new definition with the specified name and description.
        /// </summary>
        /// <param name="name">The name of the definition.</param>
        /// <param name="description">The description of the definition.</param>
        [Benchmark]
        public void CreateDefinition()
        {
            _definitionService
                .CreateDefinitionAsync("BenchDef", "Temporary definition")
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Adds a new step to the specified definition.
        /// </summary>
        /// <param name="definitionId">The ID of the definition to add the step to.</param>
        /// <param name="step">The step to add.</param>
        [Benchmark]
        public void AddStep()
        {
            var step = new SagaStepDefinition
            {
                // Minimal configuration – adjust as needed for realistic workloads
                // (Assuming there are parameterless properties; otherwise set required ones)
            };

            _definitionService
                .AddStepAsync(_definitionId, step)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Creates a new saga with the specified definition.
        /// </summary>
        /// <param name="definition">The definition to use for the saga.</param>
        [Benchmark]
        public void CreateSaga()
        {
            _orchestrationService
                .CreateSagaAsync(_definition)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Starts the specified saga.
        /// </summary>
        /// <param name="sagaId">The ID of the saga to start.</param>
        [Benchmark]
        public void StartSaga()
        {
            _orchestrationService
                .StartSagaAsync(_sagaId)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Executes the next step in the specified saga.
        /// </summary>
        /// <param name="sagaId">The ID of the saga to execute the next step in.</param>
        /// <param name="cancellationToken">The cancellation token to use.</param>
        [Benchmark]
        public void ExecuteNextStep()
        {
            // Use CancellationToken.None for simplicity
            _orchestrationService
                .ExecuteNextStepAsync(_sagaId, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Runs the benchmarks and prints the results to the console.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SagaOrchestratorBenchmarks>();
            Console.WriteLine(summary);
        }
    }
}
