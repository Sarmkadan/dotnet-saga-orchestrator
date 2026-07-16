#nullable enable

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;
using Xunit;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Integration tests that drive a full multi-step saga, force a mid-transaction failure,
/// and assert that compensating transactions are executed in the correct order.
/// These exercise the orchestrator, the compensation service, and the in-memory repositories
/// wired together exactly as <see cref="ServiceConfiguration.AddSagaOrchestrator(IServiceCollection)"/>
/// registers them.
/// </summary>
public class SagaCompensationFlowTests
{
    private static IServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddSagaOrchestrator();
        return services.BuildServiceProvider();
    }

    private static async Task<SagaDefinition> BuildThreeStepDefinitionAsync(
        SagaDefinitionService definitions,
        CompensationStrategy strategy = CompensationStrategy.ReverseOrder)
    {
        var definition = await definitions.CreateDefinitionAsync(
            "OrderProcessing", "Charge, reserve, then ship an order");

        await definitions.AddStepAsync(definition.Id, new SagaStepDefinition(
            "ProcessPayment", "payment-service",
            "http://payment/charge", "http://payment/refund"));

        await definitions.AddStepAsync(definition.Id, new SagaStepDefinition(
            "ReserveInventory", "inventory-service",
            "http://inventory/reserve", "http://inventory/release"));

        await definitions.AddStepAsync(definition.Id, new SagaStepDefinition(
            "ScheduleShipping", "shipping-service",
            "http://shipping/schedule", "http://shipping/cancel"));

        var stored = await definitions.GetDefinitionAsync(definition.Id);
        stored!.CompensationStrategy = strategy;
        return stored;
    }

    /// <summary>
    /// Drives a three-step saga so the first two steps complete, then forces the saga to fail
    /// as if the third step blew up mid-transaction. Asserts that compensations run in strict
    /// reverse order of completion and that the saga ends in the Compensated state.
    /// </summary>
    [Fact]
    public async Task ForcedMidTransactionFailure_CompensationsRunInReverseOrder()
    {
        var provider = CreateProvider();
        var definitions = provider.GetRequiredService<SagaDefinitionService>();
        var orchestrator = provider.GetRequiredService<SagaOrchestrationService>();
        var compensation = provider.GetRequiredService<CompensationService>();

        var definition = await BuildThreeStepDefinitionAsync(definitions);

        var saga = await orchestrator.CreateSagaAsync(definition, maxRetries: 0, timeoutSeconds: 120);
        await orchestrator.StartSagaAsync(saga.Id);

        // Complete the first two steps normally.
        await orchestrator.ExecuteNextStepAsync(saga.Id); // ProcessPayment
        await orchestrator.ExecuteNextStepAsync(saga.Id); // ReserveInventory

        var running = await orchestrator.GetSagaAsync(saga.Id);
        running.Steps.Count(s => s.Status == SagaStepStatus.Completed).Should().Be(2);

        // Force the mid-transaction failure: the third step (shipping) fails and the saga fails.
        running.Fail("shipping-service rejected the shipment mid-transaction");
        await provider.GetRequiredService<ISagaRepository>().UpdateAsync(running);

        // Begin compensation, then pull compensations one at a time recording their execution order.
        await compensation.BeginCompensationAsync(running);

        var executionOrder = new List<int>();
        var executedNames = new List<string>();
        CompensationTransaction? next;
        while ((next = await compensation.ExecuteNextCompensationAsync(saga.Id)) != null)
        {
            executionOrder.Add(next.Order);
            executedNames.Add(next.StepName);
        }

        // Only the two completed steps are compensated, newest first (reverse order).
        executionOrder.Should().Equal(2, 1);
        executedNames.Should().Equal("ReserveInventory", "ProcessPayment");

        var final = await orchestrator.GetSagaAsync(saga.Id);
        final.Status.Should().Be(SagaStatus.Compensated);
        final.Steps.Where(s => s.CompensatedAt != null)
            .Should().OnlyContain(s => s.Status == SagaStepStatus.Compensated);
    }

    /// <summary>
    /// The orchestrator's own <see cref="SagaOrchestrationService.CompensateSagaAsync(string)"/> should
    /// drive every pending compensation to completion in one call and leave the saga Compensated.
    /// </summary>
    [Fact]
    public async Task CompensateSagaAsync_DrivesAllCompensationsToCompletion()
    {
        var provider = CreateProvider();
        var definitions = provider.GetRequiredService<SagaDefinitionService>();
        var orchestrator = provider.GetRequiredService<SagaOrchestrationService>();
        var sagaRepo = provider.GetRequiredService<ISagaRepository>();

        var definition = await BuildThreeStepDefinitionAsync(definitions);

        var saga = await orchestrator.CreateSagaAsync(definition, maxRetries: 0, timeoutSeconds: 120);
        await orchestrator.StartSagaAsync(saga.Id);
        await orchestrator.ExecuteNextStepAsync(saga.Id);
        await orchestrator.ExecuteNextStepAsync(saga.Id);
        await orchestrator.ExecuteNextStepAsync(saga.Id); // all three complete

        var completed = await orchestrator.GetSagaAsync(saga.Id);
        completed.Status.Should().Be(SagaStatus.Completed);

        // Now force a failure post-hoc and let the orchestrator unwind everything.
        completed.Fail("downstream reconciliation detected an inconsistency");
        await sagaRepo.UpdateAsync(completed);

        var result = await orchestrator.CompensateSagaAsync(saga.Id);

        result.Status.Should().Be(SagaStatus.Compensated);
        var comps = await provider.GetRequiredService<CompensationService>().GetCompensationsAsync(saga.Id);
        comps.Should().HaveCount(3);
        comps.Should().OnlyContain(c => c.Status == CompensationStatus.Completed);
    }

    /// <summary>
    /// With the ForwardOrder strategy the same failed saga compensates oldest step first,
    /// which is the mirror image of the reverse-order default.
    /// </summary>
    [Fact]
    public async Task ForwardOrderStrategy_CompensatesOldestStepFirst()
    {
        var provider = CreateProvider();
        var definitions = provider.GetRequiredService<SagaDefinitionService>();
        var orchestrator = provider.GetRequiredService<SagaOrchestrationService>();
        var compensation = provider.GetRequiredService<CompensationService>();

        var definition = await BuildThreeStepDefinitionAsync(definitions, CompensationStrategy.ForwardOrder);

        var saga = await orchestrator.CreateSagaAsync(definition, maxRetries: 0, timeoutSeconds: 120);
        await orchestrator.StartSagaAsync(saga.Id);
        await orchestrator.ExecuteNextStepAsync(saga.Id);
        await orchestrator.ExecuteNextStepAsync(saga.Id);

        var running = await orchestrator.GetSagaAsync(saga.Id);
        running.Fail("forced failure to exercise the forward compensation strategy");
        await provider.GetRequiredService<ISagaRepository>().UpdateAsync(running);

        await compensation.BeginCompensationAsync(running);

        var executionOrder = new List<int>();
        CompensationTransaction? next;
        while ((next = await compensation.ExecuteNextCompensationAsync(saga.Id)) != null)
        {
            executionOrder.Add(next.Order);
        }

        executionOrder.Should().Equal(1, 2);
    }
}
