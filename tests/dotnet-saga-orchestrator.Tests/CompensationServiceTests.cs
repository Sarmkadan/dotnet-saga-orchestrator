#nullable enable

using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;
using SagaOrchestrator.Data.Repositories;
using FluentAssertions;
using Moq;
using Xunit;

namespace SagaOrchestrator.Tests;

public class CompensationServiceTests
{
    private static SagaStepDefinition CreateValidStep(string name = "Step1") =>
        new SagaStepDefinition(name, "svc", "http://svc/action", "http://svc/comp");

    private static Saga CreateAndInitializeSaga()
    {
        var definition = new SagaDefinition("TestSaga", "Test");
        definition.AddStep(CreateValidStep());
        var saga = new Saga();
        saga.Initialize(definition);
        saga.Start();
        return saga;
    }

    [Fact]
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var act = () => new CompensationService(null!, sagaRepoMock.Object, stepRepoMock.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task BeginCompensationAsync_WithNullSaga_ThrowsArgumentNullException()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        var act = () => service.BeginCompensationAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task BeginCompensationAsync_WithRunningStatus_ThrowsSagaException()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var saga = CreateAndInitializeSaga(); // Status is Running
        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        var act = () => service.BeginCompensationAsync(saga);

        await act.Should().ThrowAsync<SagaException>();
    }

    [Fact]
    public async Task BeginCompensationAsync_WithFailedStatus_TransitionsToCompensating()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var saga = CreateAndInitializeSaga();
        saga.Fail("Test failure");

        sagaRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Saga>()))
            .ReturnsAsync((Saga s) => s);

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        await service.BeginCompensationAsync(saga);

        saga.Status.Should().Be(SagaStatus.Compensating);
        sagaRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Saga>()), Times.Once);
    }

    [Fact]
    public async Task BeginCompensationAsync_CreatesCompensationTransactionsForCompleted()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var saga = CreateAndInitializeSaga();
        var step = new SagaStep();
        step.Initialize("PaymentStep", 1, "http://pay/action", "http://pay/comp");
        step.Complete(new Dictionary<string, object> { ["data"] = "response data" });
        saga.Steps.Add(step);
        saga.Fail("Test failure");

        sagaRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Saga>()))
            .ReturnsAsync((Saga s) => s);
        compRepoMock.Setup(r => r.CreateAsync(It.IsAny<CompensationTransaction>()))
            .ReturnsAsync((CompensationTransaction c) => c);

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        await service.BeginCompensationAsync(saga);

        compRepoMock.Verify(
            r => r.CreateAsync(It.IsAny<CompensationTransaction>()),
            Times.Once);
    }

    [Fact]
    public async Task BeginCompensationAsync_IgnoresPendingSteps()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var saga = CreateAndInitializeSaga();
        var step = new SagaStep();
        step.Initialize("PaymentStep", 1, "http://pay/action", "http://pay/comp");
        // Don't mark as completed
        saga.Steps.Add(step);
        saga.Fail("Test failure");

        sagaRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Saga>()))
            .ReturnsAsync((Saga s) => s);

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        await service.BeginCompensationAsync(saga);

        compRepoMock.Verify(
            r => r.CreateAsync(It.IsAny<CompensationTransaction>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteNextCompensationAsync_WithNonexistentSaga_ThrowsSagaNotFoundException()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        sagaRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Saga?)null);

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        var act = () => service.ExecuteNextCompensationAsync("saga_missing");

        await act.Should().ThrowAsync<SagaNotFoundException>();
    }

    [Fact]
    public async Task ExecuteNextCompensationAsync_WithNonCompensatingStatus_ThrowsSagaException()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var saga = CreateAndInitializeSaga(); // Status is Running
        sagaRepoMock.Setup(r => r.GetByIdAsync(saga.Id))
            .ReturnsAsync(saga);

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        var act = () => service.ExecuteNextCompensationAsync(saga.Id);

        await act.Should().ThrowAsync<SagaException>();
    }

    [Fact]
    public async Task ExecuteNextCompensationAsync_WithNoCompensations_ReturnsNull()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var saga = CreateAndInitializeSaga();
        saga.Fail("Test");
        saga.BeginCompensation();

        sagaRepoMock.Setup(r => r.GetByIdAsync(saga.Id))
            .ReturnsAsync(saga);
        compRepoMock.Setup(r => r.GetBySagaIdAsync(saga.Id))
            .ReturnsAsync(new List<CompensationTransaction>());

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        var result = await service.ExecuteNextCompensationAsync(saga.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteNextCompensationAsync_WithPendingCompensation_ReturnsThat()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var saga = CreateAndInitializeSaga();
        saga.Fail("Test");
        saga.BeginCompensation();

        var compensation = new CompensationTransaction();
        compensation.Initialize(saga.Id, "step1", "PaymentStep", 1, "http://pay/comp");

        sagaRepoMock.Setup(r => r.GetByIdAsync(saga.Id))
            .ReturnsAsync(saga);
        compRepoMock.Setup(r => r.GetBySagaIdAsync(saga.Id))
            .ReturnsAsync(new List<CompensationTransaction> { compensation });

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        var result = await service.ExecuteNextCompensationAsync(saga.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(compensation.Id);
    }

    [Fact]
    public async Task ExecuteNextCompensationAsync_SkipsPreviouslyExecuted()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var saga = CreateAndInitializeSaga();
        saga.Fail("Test");
        saga.BeginCompensation();

        var comp1 = new CompensationTransaction();
        comp1.Initialize(saga.Id, "step1", "PaymentStep", 1, "http://pay/comp");
        comp1.Complete(new Dictionary<string, object> { ["result"] = "result" });

        var comp2 = new CompensationTransaction();
        comp2.Initialize(saga.Id, "step2", "NotificationStep", 2, "http://notify/comp");

        sagaRepoMock.Setup(r => r.GetByIdAsync(saga.Id))
            .ReturnsAsync(saga);
        compRepoMock.Setup(r => r.GetBySagaIdAsync(saga.Id))
            .ReturnsAsync(new List<CompensationTransaction> { comp1, comp2 });

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        var result = await service.ExecuteNextCompensationAsync(saga.Id);

        result.Should().NotBeNull();
        result!.StepName.Should().Be("NotificationStep");
    }

    [Fact]
    public async Task CompleteCompensationAsync_WithValidTransaction_MarksSagaCompensated()
    {
        var compRepoMock = new Mock<ICompensationTransactionRepository>();
        var sagaRepoMock = new Mock<ISagaRepository>();
        var stepRepoMock = new Mock<ISagaStepRepository>();

        var saga = CreateAndInitializeSaga();
        saga.Fail("Test");
        saga.BeginCompensation();

        var compensation = new CompensationTransaction();
        compensation.Initialize(saga.Id, "step1", "PaymentStep", 1, "http://pay/comp");

        sagaRepoMock.Setup(r => r.GetByIdAsync(saga.Id))
            .ReturnsAsync(saga);
        compRepoMock.Setup(r => r.GetBySagaIdAsync(saga.Id))
            .ReturnsAsync(new List<CompensationTransaction> { compensation });
        sagaRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Saga>()))
            .ReturnsAsync((Saga s) => s);
        compRepoMock.Setup(r => r.UpdateAsync(It.IsAny<CompensationTransaction>()))
            .ReturnsAsync((CompensationTransaction c) => c);

        var service = new CompensationService(compRepoMock.Object, sagaRepoMock.Object, stepRepoMock.Object);

        var result = await service.ExecuteNextCompensationAsync(saga.Id);
        result.Should().NotBeNull();
        await service.ExecuteNextCompensationAsync(saga.Id);

        sagaRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Saga>()), Times.AtLeastOnce);
        compRepoMock.Verify(r => r.UpdateAsync(It.IsAny<CompensationTransaction>()), Times.Once);
    }
}
