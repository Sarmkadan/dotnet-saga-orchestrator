using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Benchmarks;

/// <summary>
/// Performance benchmarks for the Saga Orchestrator
/// </summary>
[MemoryDiagnoser]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SagaOrchestratorBenchmarks
{
    private ServiceProvider _serviceProvider;
    private SagaDefinitionService _definitionService;
    private SagaOrchestrationService _orchestrationService;
    private ISagaRepository _sagaRepository;
    private ISagaStepRepository _stepRepository;

    [Params(1, 5, 10, 20)]
    public int SagaStepCount { get; set; }

    [Params(1, 10, 100)]
    public int IterationCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        // Configure minimal logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning));

        // Register application services
        services.AddSingleton<SagaDefinitionService>();
        services.AddSingleton<SagaOrchestrationService>();

        // Use in-memory repositories for benchmarking
        services.AddSingleton<ISagaDefinitionRepository, InMemorySagaDefinitionRepository>();
        services.AddSingleton<ISagaRepository, InMemorySagaRepository>();
        services.AddSingleton<ISagaStepRepository, InMemorySagaStepRepository>();
        services.AddSingleton<CompensationService>();

        _serviceProvider = services.BuildServiceProvider();

        _definitionService = _serviceProvider.GetRequiredService<SagaDefinitionService>();
        _orchestrationService = _serviceProvider.GetRequiredService<SagaOrchestrationService>();
        _sagaRepository = _serviceProvider.GetRequiredService<ISagaRepository>();
        _stepRepository = _serviceProvider.GetRequiredService<ISagaStepRepository>();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }

    [Benchmark]
    public async Task CreateSagaDefinition_Benchmark()
    {
        for (int i = 0; i < IterationCount; i++)
        {
            var definition = await _definitionService.CreateDefinitionAsync(
                $"Order Processing Benchmark {i}",
                "Benchmark test saga definition");

            // Add steps
            for (int step = 0; step < SagaStepCount; step++)
            {
                await _definitionService.AddStepAsync(definition.Id, new SagaStepDefinition(
                    $"Step {step}",
                    $"service-{step}",
                    $"http://service-{step}/execute",
                    $"http://service-{step}/compensate"));
            }
        }
    }

    [Benchmark]
    public async Task CreateSagaInstance_Benchmark()
    {
        // Pre-create a definition
        var definition = await _definitionService.CreateDefinitionAsync(
            "Benchmark Definition",
            "Benchmark test saga definition");

        for (int step = 0; step < SagaStepCount; step++)
        {
            await _definitionService.AddStepAsync(definition.Id, new SagaStepDefinition(
                $"Step {step}",
                $"service-{step}",
                $"http://service-{step}/execute",
                $"http://service-{step}/compensate"));
        }

        // Benchmark saga creation
        for (int i = 0; i < IterationCount; i++)
        {
            await _orchestrationService.CreateSagaAsync(definition);
        }
    }

    [Benchmark]
    public async Task ExecuteSagaSteps_Benchmark()
    {
        // Pre-create a definition
        var definition = await _definitionService.CreateDefinitionAsync(
            "Benchmark Definition",
            "Benchmark test saga definition");

        for (int step = 0; step < SagaStepCount; step++)
        {
            await _definitionService.AddStepAsync(definition.Id, new SagaStepDefinition(
                $"Step {step}",
                $"service-{step}",
                $"http://service-{step}/execute",
                $"http://service-{step}/compensate"));
        }

        // Create and start saga
        var saga = await _orchestrationService.CreateSagaAsync(definition);
        saga = await _orchestrationService.StartSagaAsync(saga.Id);

        // Benchmark step execution
        for (int i = 0; i < IterationCount; i++)
        {
            var executedStep = await _orchestrationService.ExecuteNextStepAsync(saga.Id);
            if (executedStep == null)
            {
                // Saga completed, create a new one
                saga = await _orchestrationService.CreateSagaAsync(definition);
                saga = await _orchestrationService.StartSagaAsync(saga.Id);
            }
        }
    }

    [Benchmark]
    public async Task ListSagas_Benchmark()
    {
        // Pre-create multiple sagas
        for (int i = 0; i < IterationCount; i++)
        {
            var definition = await _definitionService.CreateDefinitionAsync(
                $"Definition {i}",
                $"Test definition {i}");

            var saga = await _orchestrationService.CreateSagaAsync(definition);
            saga = await _orchestrationService.StartSagaAsync(saga.Id);

            // Execute some steps
            for (int step = 0; step < Math.Min(5, SagaStepCount); step++)
            {
                await _orchestrationService.ExecuteNextStepAsync(saga.Id);
            }
        }

        // Benchmark listing
        await _orchestrationService.ListSagasAsync();
    }

    [Benchmark]
    public async Task GetSagaById_Benchmark()
    {
        // Pre-create a saga
        var definition = await _definitionService.CreateDefinitionAsync(
            "Benchmark Definition",
            "Benchmark test saga definition");

        for (int step = 0; step < SagaStepCount; step++)
        {
            await _definitionService.AddStepAsync(definition.Id, new SagaStepDefinition(
                $"Step {step}",
                $"service-{step}",
                $"http://service-{step}/execute",
                $"http://service-{step}/compensate"));
        }

        var saga = await _orchestrationService.CreateSagaAsync(definition);
        saga = await _orchestrationService.StartSagaAsync(saga.Id);

        // Benchmark retrieval
        for (int i = 0; i < IterationCount; i++)
        {
            _ = await _orchestrationService.GetSagaAsync(saga.Id);
        }
    }
}

/// <summary>
/// In-memory repository implementations for benchmarking
/// </summary>
public class InMemorySagaDefinitionRepository : ISagaDefinitionRepository
{
    private readonly List<SagaDefinition> _definitions = [];
    private int _counter = 0;

    public Task<SagaDefinition> CreateAsync(SagaDefinition entity)
    {
        entity.Id = $"def-{++_counter}";
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _definitions.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<SagaDefinition?> GetByIdAsync(string id)
    {
        var definition = _definitions.FirstOrDefault(d => d.Id == id);
        return Task.FromResult(definition);
    }

    public Task<SagaDefinition?> GetByNameAsync(string name)
    {
        var definition = _definitions.FirstOrDefault(d => d.Name == name);
        return Task.FromResult(definition);
    }

    public Task<List<SagaDefinition>> GetAllAsync()
    {
        return Task.FromResult(_definitions.ToList());
    }

    public Task<SagaDefinition?> UpdateAsync(SagaDefinition entity)
    {
        var existing = _definitions.FirstOrDefault(d => d.Id == entity.Id);
        if (existing != null)
        {
            _definitions.Remove(existing);
            _definitions.Add(entity);
            entity.UpdatedAt = DateTime.UtcNow;
        }
        return Task.FromResult(existing);
    }
}

public class InMemorySagaRepository : ISagaRepository
{
    private readonly List<Saga> _sagas = [];
    private int _counter = 0;

    public Task<Saga> CreateAsync(Saga entity)
    {
        entity.Id = $"saga-{++_counter}";
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _sagas.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<Saga?> GetByIdAsync(string id)
    {
        var saga = _sagas.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(saga);
    }

    public Task<List<Saga>> GetAllAsync()
    {
        return Task.FromResult(_sagas.ToList());
    }

    public Task<Saga?> UpdateAsync(Saga entity)
    {
        var existing = _sagas.FirstOrDefault(s => s.Id == entity.Id);
        if (existing != null)
        {
            _sagas.Remove(existing);
            _sagas.Add(entity);
            entity.UpdatedAt = DateTime.UtcNow;
        }
        return Task.FromResult(existing);
    }
}

public class InMemorySagaStepRepository : ISagaStepRepository
{
    private readonly List<SagaStep> _steps = [];
    private int _counter = 0;

    public Task<SagaStep> CreateAsync(SagaStep entity)
    {
        entity.Id = $"step-{++_counter}";
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _steps.Add(entity);
        return Task.FromResult(entity);
    }

    public Task<SagaStep?> GetByIdAsync(string id)
    {
        var step = _steps.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(step);
    }

    public Task<List<SagaStep>> GetAllAsync()
    {
        return Task.FromResult(_steps.ToList());
    }

    public Task<SagaStep?> UpdateAsync(SagaStep entity)
    {
        var existing = _steps.FirstOrDefault(s => s.Id == entity.Id);
        if (existing != null)
        {
            _steps.Remove(existing);
            _steps.Add(entity);
            entity.UpdatedAt = DateTime.UtcNow;
        }
        return Task.FromResult(existing);
    }

    public Task<List<SagaStep>> GetBySagaIdAsync(string sagaId)
    {
        var steps = _steps.Where(s => s.SagaId == sagaId).ToList();
        return Task.FromResult(steps);
    }
}

public class CompensationService
{
    public Task BeginCompensationAsync(Saga saga)
    {
        return Task.CompletedTask;
    }
}