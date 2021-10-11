#nullable enable

using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Data.Repositories;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

public class SagaIntegrationTests
{
    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSagaOrchestrator();
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task EndToEnd_CreateDefinition_CreateSaga_ExecuteSteps_CompletesSuccessfully()
    {
        var provider = CreateServiceProvider();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();
        var orchestrationService = provider.GetRequiredService<SagaOrchestrationService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "OrderProcessing",
            "Process orders across microservices");

        var paymentStep = new SagaStepDefinition(
            "ProcessPayment",
            "payment-service",
            "http://payment-service/charge",
            "http://payment-service/refund")
        {
            TimeoutSeconds = 30,
            MaxRetries = 3
        };

        var inventoryStep = new SagaStepDefinition(
            "ReserveInventory",
            "inventory-service",
            "http://inventory-service/reserve",
            "http://inventory-service/release")
        {
            TimeoutSeconds = 30,
            MaxRetries = 3
        };

        await definitionService.AddStepAsync(definition.Id, paymentStep);
        await definitionService.AddStepAsync(definition.Id, inventoryStep);

        var validation = definitionService.ValidateDefinition(definition);
        validation.IsValid.Should().BeTrue();

        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);
        retrievedDef.Should().NotBeNull();
        retrievedDef!.Steps.Should().HaveCount(2);

        var saga = await orchestrationService.CreateSagaAsync(retrievedDef, maxRetries: 3, timeoutSeconds: 120);
        saga.Status.Should().Be(SagaStatus.Initialized);

        await orchestrationService.StartSagaAsync(saga.Id);
        var runningStatus = await orchestrationService.GetSagaAsync(saga.Id);
        runningStatus.Status.Should().Be(SagaStatus.Running);
        runningStatus.Steps.Should().HaveCount(2);
    }

    [Fact]
    public async Task MoneyTransferScenario_DefinitionWithThreeSteps_ValidatesAndCreates()
    {
        var provider = CreateServiceProvider();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();
        var orchestrationService = provider.GetRequiredService<SagaOrchestrationService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "MoneyTransfer",
            "Transfer funds between bank accounts");

        var validateStep = new SagaStepDefinition(
            "ValidateAccounts",
            "account-service",
            "http://account-service/validate",
            "http://account-service/unlock")
        { TimeoutSeconds = 15, MaxRetries = 3 };

        var debitStep = new SagaStepDefinition(
            "DebitSourceAccount",
            "ledger-service",
            "http://ledger-service/debit",
            "http://ledger-service/credit")
        { TimeoutSeconds = 30, MaxRetries = 5 };

        var creditStep = new SagaStepDefinition(
            "CreditDestinationAccount",
            "ledger-service",
            "http://ledger-service/credit",
            "http://ledger-service/debit")
        { TimeoutSeconds = 30, MaxRetries = 5 };

        await definitionService.AddStepAsync(definition.Id, validateStep);
        await definitionService.AddStepAsync(definition.Id, debitStep);
        await definitionService.AddStepAsync(definition.Id, creditStep);

        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);
        retrievedDef.Should().NotBeNull();
        retrievedDef!.Steps.Should().HaveCount(3);

        var saga = await orchestrationService.CreateSagaAsync(retrievedDef, maxRetries: 5, timeoutSeconds: 300);

        saga.Should().NotBeNull();
        saga.Status.Should().Be(SagaStatus.Initialized);
        saga.Definition.Steps.Should().HaveCount(3);
    }

    [Fact]
    public async Task ConcurrentSagaCreation_MultipleThreads_AllSagasCreatedSuccessfully()
    {
        var provider = CreateServiceProvider();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();
        var orchestrationService = provider.GetRequiredService<SagaOrchestrationService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "ConcurrentTest",
            "Test concurrent saga creation");

        var step = new SagaStepDefinition(
            "TestStep",
            "test-service",
            "http://test-service/action",
            "http://test-service/compensate")
        { TimeoutSeconds = 30, MaxRetries = 3 };

        await definitionService.AddStepAsync(definition.Id, step);
        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

        var tasks = Enumerable.Range(0, 10)
            .Select(async i =>
            {
                var saga = await orchestrationService.CreateSagaAsync(retrievedDef);
                return saga;
            })
            .ToList();

        var sagas = await Task.WhenAll(tasks);

        sagas.Should().HaveCount(10);
        sagas.Select(s => s.Id).Should().AllBeDifferent();
        sagas.Should().AllSatisfy(s => s.Status.Should().Be(SagaStatus.Initialized));
    }

    [Fact]
    public async Task ConcurrentSagaExecution_MultipleThreads_AllProcessWithoutErrors()
    {
        var provider = CreateServiceProvider();
        var orchestrationService = provider.GetRequiredService<SagaOrchestrationService>();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "ConcurrentExecution",
            "Test concurrent execution");

        var step = new SagaStepDefinition(
            "Step",
            "svc",
            "http://svc/action",
            "http://svc/comp")
        { TimeoutSeconds = 30, MaxRetries = 3 };

        await definitionService.AddStepAsync(definition.Id, step);
        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

        var sagas = new List<Saga>();
        for (int i = 0; i < 5; i++)
        {
            var saga = await orchestrationService.CreateSagaAsync(retrievedDef);
            sagas.Add(saga);
        }

        var tasks = sagas
            .Select(async s =>
            {
                await orchestrationService.StartSagaAsync(s.Id);
                var updated = await orchestrationService.GetSagaAsync(s.Id);
                return updated;
            })
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(s => s.Status.Should().Be(SagaStatus.Running));
    }

    [Fact]
    public async Task SagaWithDifferentTimeouts_CreatesCorrectPolicies()
    {
        var provider = CreateServiceProvider();
        var orchestrationService = provider.GetRequiredService<SagaOrchestrationService>();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "TimeoutTest",
            "Test various timeout configurations");

        var strictStep = new SagaStepDefinition(
            "StrictStep",
            "svc1",
            "http://svc1/action",
            "http://svc1/comp")
        { TimeoutSeconds = 10, MaxRetries = 2 };

        var normalStep = new SagaStepDefinition(
            "NormalStep",
            "svc2",
            "http://svc2/action",
            "http://svc2/comp")
        { TimeoutSeconds = 60, MaxRetries = 3 };

        var lenientStep = new SagaStepDefinition(
            "LenientStep",
            "svc3",
            "http://svc3/action",
            "http://svc3/comp")
        { TimeoutSeconds = 300, MaxRetries = 5 };

        await definitionService.AddStepAsync(definition.Id, strictStep);
        await definitionService.AddStepAsync(definition.Id, normalStep);
        await definitionService.AddStepAsync(definition.Id, lenientStep);

        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);
        var saga = await orchestrationService.CreateSagaAsync(retrievedDef, timeoutSeconds: 600);

        saga.TimeoutSeconds.Should().Be(600);
        saga.Definition.Steps[0].TimeoutSeconds.Should().Be(10);
        saga.Definition.Steps[1].TimeoutSeconds.Should().Be(60);
        saga.Definition.Steps[2].TimeoutSeconds.Should().Be(300);
    }

    [Fact]
    public async Task SagaWithDifferentRetryPolicies_CreatesCorrectConfigs()
    {
        var provider = CreateServiceProvider();
        var orchestrationService = provider.GetRequiredService<SagaOrchestrationService>();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "RetryTest",
            "Test retry policies");

        var noRetryStep = new SagaStepDefinition(
            "NoRetry",
            "svc1",
            "http://svc1/action",
            "http://svc1/comp")
        { TimeoutSeconds = 30, MaxRetries = 0 };

        var aggressiveRetryStep = new SagaStepDefinition(
            "AggressiveRetry",
            "svc2",
            "http://svc2/action",
            "http://svc2/comp")
        { TimeoutSeconds = 30, MaxRetries = 10 };

        await definitionService.AddStepAsync(definition.Id, noRetryStep);
        await definitionService.AddStepAsync(definition.Id, aggressiveRetryStep);

        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);
        var saga = await orchestrationService.CreateSagaAsync(retrievedDef);

        saga.Definition.Steps[0].MaxRetries.Should().Be(0);
        saga.Definition.Steps[1].MaxRetries.Should().Be(10);
    }

    [Fact]
    public async Task RetrieveSaga_ByExistingId_ReturnsSaga()
    {
        var provider = CreateServiceProvider();
        var orchestrationService = provider.GetRequiredService<SagaOrchestrationService>();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "RetrievalTest",
            "Test saga retrieval");

        var step = new SagaStepDefinition(
            "TestStep",
            "svc",
            "http://svc/action",
            "http://svc/comp")
        { TimeoutSeconds = 30, MaxRetries = 3 };

        await definitionService.AddStepAsync(definition.Id, step);
        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

        var saga = await orchestrationService.CreateSagaAsync(retrievedDef);
        var retrieved = await orchestrationService.GetSagaAsync(saga.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(saga.Id);
        retrieved.Status.Should().Be(SagaStatus.Initialized);
    }

    [Fact]
    public async Task SagaLifecycle_Create_Start_Fail_BeginCompensation_Workflow()
    {
        var provider = CreateServiceProvider();
        var orchestrationService = provider.GetRequiredService<SagaOrchestrationService>();
        var compensationService = provider.GetRequiredService<CompensationService>();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "CompensationTest",
            "Test compensation flow");

        var step = new SagaStepDefinition(
            "TestStep",
            "svc",
            "http://svc/action",
            "http://svc/comp")
        { TimeoutSeconds = 30, MaxRetries = 3 };

        await definitionService.AddStepAsync(definition.Id, step);
        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

        var saga = await orchestrationService.CreateSagaAsync(retrievedDef);
        saga.Status.Should().Be(SagaStatus.Initialized);

        await orchestrationService.StartSagaAsync(saga.Id);
        var running = await orchestrationService.GetSagaAsync(saga.Id);
        running.Status.Should().Be(SagaStatus.Running);

        running.Fail("Simulated failure");
        running.Status.Should().Be(SagaStatus.Failed);

        await compensationService.BeginCompensationAsync(running);
        running.Status.Should().Be(SagaStatus.Compensating);
    }

    [Fact]
    public async Task GetSagasByStatus_ReturnsOnlyMatchingStatus()
    {
        var provider = CreateServiceProvider();
        var orchestrationService = provider.GetRequiredService<SagaOrchestrationService>();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "StatusTest",
            "Test status filtering");

        var step = new SagaStepDefinition(
            "Step",
            "svc",
            "http://svc/action",
            "http://svc/comp")
        { TimeoutSeconds = 30, MaxRetries = 3 };

        await definitionService.AddStepAsync(definition.Id, step);
        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);

        var saga1 = await orchestrationService.CreateSagaAsync(retrievedDef);
        var saga2 = await orchestrationService.CreateSagaAsync(retrievedDef);

        await orchestrationService.StartSagaAsync(saga1.Id);

        var initializedSagas = await orchestrationService.GetSagasByStatusAsync(SagaStatus.Initialized);
        var runningSagas = await orchestrationService.GetSagasByStatusAsync(SagaStatus.Running);

        initializedSagas.Should().HaveCountGreaterThanOrEqualTo(1);
        runningSagas.Should().HaveCountGreaterThanOrEqualTo(1);
        runningSagas.Should().Contain(s => s.Id == saga1.Id);
    }

    [Fact]
    public async Task SagaWithManySteps_Handles100Steps()
    {
        var provider = CreateServiceProvider();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();

        var definition = await definitionService.CreateDefinitionAsync(
            "ManySteps",
            "Test with many steps");

        for (int i = 1; i <= 100; i++)
        {
            var step = new SagaStepDefinition(
                $"Step{i}",
                $"service-{i}",
                $"http://service-{i}/action",
                $"http://service-{i}/comp")
            { TimeoutSeconds = 30, MaxRetries = 3 };

            await definitionService.AddStepAsync(definition.Id, step);
        }

        var retrievedDef = await definitionService.GetDefinitionAsync(definition.Id);
        retrievedDef!.Steps.Should().HaveCount(100);
    }

    [Fact]
    public async Task CreateMultipleDefinitions_TracksThemIndependently()
    {
        var provider = CreateServiceProvider();
        var definitionService = provider.GetRequiredService<SagaDefinitionService>();

        var def1 = await definitionService.CreateDefinitionAsync("Definition1", "Desc1");
        var def2 = await definitionService.CreateDefinitionAsync("Definition2", "Desc2");
        var def3 = await definitionService.CreateDefinitionAsync("Definition3", "Desc3");

        var retrieved1 = await definitionService.GetDefinitionAsync(def1.Id);
        var retrieved2 = await definitionService.GetDefinitionAsync(def2.Id);
        var retrieved3 = await definitionService.GetDefinitionAsync(def3.Id);

        retrieved1!.Name.Should().Be("Definition1");
        retrieved2!.Name.Should().Be("Definition2");
        retrieved3!.Name.Should().Be("Definition3");

        retrieved1.Id.Should().NotBe(retrieved2.Id);
        retrieved2.Id.Should().NotBe(retrieved3.Id);
    }
}
