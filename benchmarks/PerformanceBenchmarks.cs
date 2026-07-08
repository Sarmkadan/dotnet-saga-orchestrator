using System;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Benchmarks
{
    [MemoryDiagnoser]
    public class SagaOrchestratorBenchmarks
    {
        private ServiceProvider _serviceProvider = null!;
        private SagaDefinitionService _definitionService = null!;
        private SagaOrchestrationService _orchestrationService = null!;

        private SagaDefinition _definition = null!;
        private string _definitionId = null!;
        private string _sagaId = null!;

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

        [Benchmark]
        public void CreateDefinition()
        {
            _definitionService
                .CreateDefinitionAsync("BenchDef", "Temporary definition")
                .GetAwaiter()
                .GetResult();
        }

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

        [Benchmark]
        public void CreateSaga()
        {
            _orchestrationService
                .CreateSagaAsync(_definition)
                .GetAwaiter()
                .GetResult();
        }

        [Benchmark]
        public void StartSaga()
        {
            _orchestrationService
                .StartSagaAsync(_sagaId)
                .GetAwaiter()
                .GetResult();
        }

        [Benchmark]
        public void ExecuteNextStep()
        {
            // Use CancellationToken.None for simplicity
            _orchestrationService
                .ExecuteNextStepAsync(_sagaId, CancellationToken.None)
                .GetAwaiter()
                .GetResult();
        }

        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SagaOrchestratorBenchmarks>();
            Console.WriteLine(summary);
        }
    }
}
