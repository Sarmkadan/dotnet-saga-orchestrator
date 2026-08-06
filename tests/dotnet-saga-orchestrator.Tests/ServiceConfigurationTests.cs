using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Data.Repositories;
using Xunit;

namespace SagaOrchestrator.Tests;

public class ServiceConfigurationTests
{
    [Fact]
    public void AddSagaOrchestrator_ServicesNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => ServiceConfiguration.AddSagaOrchestrator(null!));
    }

    [Fact]
    public void AddSagaOrchestrator_RegistersExpectedServices()
    {
        var services = new ServiceCollection();
        services.AddSagaOrchestrator();

        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<ISagaRepository>());
        Assert.NotNull(serviceProvider.GetService<ISagaStepRepository>());
        Assert.NotNull(serviceProvider.GetService<ICompensationTransactionRepository>());
        Assert.NotNull(serviceProvider.GetService<ISagaDefinitionRepository>());
        Assert.NotNull(serviceProvider.GetService<SagaOrchestrationService>());
        Assert.NotNull(serviceProvider.GetService<SagaOptions>());
    }

    [Fact]
    public void AddSagaOrchestrator_WithOptions_RegistersExpectedServicesAndOptions()
    {
        var services = new ServiceCollection();
        services.AddSagaOrchestrator(options =>
        {
            // Just verify it's called
        });

        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<SagaOptions>());
        Assert.NotNull(serviceProvider.GetService<ISagaRepository>());
    }

    [Fact]
    public void AddSagaOrchestrator_WithOptions_ConfigureOptionsNull_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => ServiceConfiguration.AddSagaOrchestrator(services, null!));
    }

    [Fact]
    public void AddSagaRepositories_RegistersExpectedRepositories()
    {
        var services = new ServiceCollection();
        services.AddSagaRepositories();

        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<ISagaRepository>());
        Assert.NotNull(serviceProvider.GetService<ISagaStepRepository>());
        Assert.NotNull(serviceProvider.GetService<ICompensationTransactionRepository>());
        Assert.NotNull(serviceProvider.GetService<ISagaDefinitionRepository>());
    }

    [Fact]
    public void AddSagaServices_RegistersExpectedServices()
    {
        var services = new ServiceCollection();
        services.AddSagaServices();

        Assert.Contains(services, d => d.ServiceType == typeof(SagaOrchestrationService));
        Assert.Contains(services, d => d.ServiceType == typeof(SagaDefinitionService));
        Assert.Contains(services, d => d.ServiceType == typeof(CompensationService));
    }
}
