#nullable enable

using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;
using SagaOrchestrator.Data.Repositories;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Contains happy path unit tests for the <see cref="SagaOrchestrationService"/> class.
/// Tests the main execution flow where all saga steps succeed without any failures or compensations.
/// </summary>
public class SagaOrchestrationServiceHappyPathTests
{
    /// <summary>
    /// Creates a valid saga step definition for testing purposes.
    /// </summary>
    /// <param name="name">The name of the step to create. Defaults to "Step1".</param>
    /// <param name="order">The execution order of the step. Defaults to 1.</param>
    /// <returns>A new <see cref="SagaStepDefinition"/> instance with the specified parameters.</returns>
    private static SagaStepDefinition CreateValidStep(string name = "Step1", int order = 1)
    {
        var step = new SagaStepDefinition(name, "TestService", "http://service/action", "http://service/comp");
        step.Order = order;
        step.IsCompensable = false; // Set to false since we're testing happy path with no failures
        return step;
    }

    /// <summary>
    /// Creates a saga definition with multiple steps for testing the happy path.
    /// </summary>
    /// <returns>A saga definition with 3 steps in order.</returns>
    private static SagaDefinition CreateMultiStepSagaDefinition()
    {
        var definition = new SagaDefinition("MultiStepSaga", "TestSaga");
        definition.AddStep(CreateValidStep("Step1", 1));
        definition.AddStep(CreateValidStep("Step2", 2));
        definition.AddStep(CreateValidStep("Step3", 3));
        return definition;
    }

    /// <summary>
    /// Creates and initializes a test saga with a single step for testing happy path scenarios.
    /// </summary>
    /// <returns>A new <see cref="Saga"/> instance in Initialized status with initialized definition and step.</returns>
    private static Saga CreateAndInitializeSaga()
    {
        var definition = new SagaDefinition("TestSaga", "Test");
        definition.AddStep(CreateValidStep());
        var saga = new Saga();
        saga.Initialize(definition);
        return saga;
    }

    /// <summary>
    /// Creates a complete in-memory repository setup for testing.
    /// </summary>
    /// <returns>Tuple containing the repositories and service instance.</returns>
    private static (InMemorySagaRepository sagaRepo, InMemorySagaStepRepository stepRepo, CompensationService compService)
        CreateInMemoryRepositories()
    {
        var sagaRepo = new InMemorySagaRepository();
        var stepRepo = new InMemorySagaStepRepository();
        var compRepo = new InMemoryCompensationTransactionRepository();
        var compService = new CompensationService(compRepo, sagaRepo, stepRepo);
        return (sagaRepo, stepRepo, compService);
    }

    [Fact]
    public void Constructor_WithNullSagaRepository_ThrowsArgumentNullException()
    {
        // Arrange
        var stepRepo = new InMemorySagaStepRepository();
        var compService = new CompensationService(
            new InMemoryCompensationTransactionRepository(),
            new InMemorySagaRepository(),
            stepRepo);

        // Act
        var act = () => new SagaOrchestrationService(null!, stepRepo, compService);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullStepRepository_ThrowsArgumentNullException()
    {
        // Arrange
        var sagaRepo = new InMemorySagaRepository();
        var compService = new CompensationService(
            new InMemoryCompensationTransactionRepository(),
            sagaRepo,
            new InMemorySagaStepRepository());

        // Act
        var act = () => new SagaOrchestrationService(sagaRepo, null!, compService);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullCompensationService_ThrowsArgumentNullException()
    {
        // Arrange
        var sagaRepo = new InMemorySagaRepository();
        var stepRepo = new InMemorySagaStepRepository();

        // Act
        var act = () => new SagaOrchestrationService(sagaRepo, stepRepo, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateSagaAsync_WithValidDefinition_CreatesSagaWithInitializedStatus()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);
        var definition = CreateMultiStepSagaDefinition();

        // Act
        var saga = await service.CreateSagaAsync(definition);

        // Assert
        saga.Should().NotBeNull();
        saga.Id.Should().NotBeNullOrEmpty();
        saga.Status.Should().Be(SagaStatus.Initialized);
        saga.Definition.Should().BeSameAs(definition);
        saga.Steps.Should().BeEmpty(); // Steps are created in StartSagaAsync, not CreateSagaAsync
    }

    [Fact]
    public async Task CreateSagaAsync_WithNullDefinition_ThrowsArgumentNullException()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        // Act
        var act = () => service.CreateSagaAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateSagaAsync_WithInvalidDefinition_ThrowsInvalidSagaDefinitionException()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);
        var invalidDefinition = new SagaDefinition("", ""); // Empty name and description

        // Act
        var act = () => service.CreateSagaAsync(invalidDefinition);

        // Assert
        await act.Should().ThrowAsync<InvalidSagaDefinitionException>();
    }

    [Fact]
    public async Task StartSagaAsync_WithInitializedSaga_StartsSagaAndCreatesSteps()
    {
        // Arrange
        var (sagaRepo, stepRepo, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, stepRepo, compService);
        var definition = CreateMultiStepSagaDefinition();
        var saga = await service.CreateSagaAsync(definition);
        var sagaId = saga.Id;

        // Act
        var startedSaga = await service.StartSagaAsync(sagaId);

        // Assert
        startedSaga.Should().NotBeNull();
        startedSaga.Status.Should().Be(SagaStatus.Running);
        startedSaga.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        // Verify steps were created and initialized
        var steps = await stepRepo.GetBySagaIdAsync(sagaId);
        steps.Should().HaveCount(3);
        steps[0].Status.Should().Be(SagaStepStatus.Pending);
        steps[0].Order.Should().Be(1);
        steps[0].Name.Should().Be("Step1");
        steps[1].Status.Should().Be(SagaStepStatus.Pending);
        steps[1].Order.Should().Be(2);
        steps[1].Name.Should().Be("Step2");
        steps[2].Status.Should().Be(SagaStepStatus.Pending);
        steps[2].Order.Should().Be(3);
        steps[2].Name.Should().Be("Step3");
    }

    [Fact]
    public async Task StartSagaAsync_WithNullSagaId_ThrowsArgumentException()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        // Act
        var act = () => service.StartSagaAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task StartSagaAsync_WithEmptySagaId_ThrowsArgumentException()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        // Act
        var act = () => service.StartSagaAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task StartSagaAsync_WithNonExistentSagaId_ThrowsSagaNotFoundException()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        // Act
        var act = () => service.StartSagaAsync("non-existent-id");

        // Assert
        await act.Should().ThrowAsync<SagaNotFoundException>();
    }

    [Fact]
    public async Task StartSagaAsync_WithAlreadyRunningSaga_ThrowsSagaException()
    {
        // Arrange
        var (sagaRepo, stepRepo, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, stepRepo, compService);
        var definition = CreateMultiStepSagaDefinition();
        var saga = await service.CreateSagaAsync(definition);
        await service.StartSagaAsync(saga.Id);

        // Act
        var act = () => service.StartSagaAsync(saga.Id);

        // Assert
        await act.Should().ThrowAsync<SagaException>();
    }

    [Fact]
    public async Task ExecuteNextStepAsync_WithRunningSaga_ExecutesFirstStep()
    {
        // Arrange
        var (sagaRepo, stepRepo, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, stepRepo, compService);
        var definition = CreateMultiStepSagaDefinition();
        var saga = await service.CreateSagaAsync(definition);
        await service.StartSagaAsync(saga.Id);

        // Act
        var executedStep = await service.ExecuteNextStepAsync(saga.Id);

        // Assert
        executedStep.Should().NotBeNull();
        executedStep.Status.Should().Be(SagaStepStatus.Completed);
        executedStep.AttemptCount.Should().Be(1);
        executedStep.StartedAt.Should().NotBeNull();
        executedStep.CompletedAt.Should().NotBeNull();
        executedStep.Response.Should().NotBeNull();
        executedStep.Response["status"].Should().Be("success");

        // Verify saga is still running (not all steps completed yet)
        var updatedSaga = await sagaRepo.GetByIdAsync(saga.Id);
        updatedSaga.Should().NotBeNull();
        updatedSaga.Status.Should().Be(SagaStatus.Running);
    }

    [Fact]
    public async Task ExecuteNextStepAsync_ExecutesAllStepsInOrder_CompletesSagaSuccessfully()
    {
        // Arrange
        var (sagaRepo, stepRepo, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, stepRepo, compService);
        var definition = CreateMultiStepSagaDefinition();
        var saga = await service.CreateSagaAsync(definition);
        await service.StartSagaAsync(saga.Id);

        // Act - Execute all 3 steps
        var step1 = await service.ExecuteNextStepAsync(saga.Id);
        var step2 = await service.ExecuteNextStepAsync(saga.Id);
        var step3 = await service.ExecuteNextStepAsync(saga.Id);

        // The fourth call should return null (no more steps)
        var noMoreSteps = await service.ExecuteNextStepAsync(saga.Id);

        // Assert - Step 1
        step1.Should().NotBeNull();
        step1.Status.Should().Be(SagaStepStatus.Completed);
        step1.Order.Should().Be(1);
        step1.Name.Should().Be("Step1");

        // Assert - Step 2
        step2.Should().NotBeNull();
        step2.Status.Should().Be(SagaStepStatus.Completed);
        step2.Order.Should().Be(2);
        step2.Name.Should().Be("Step2");

        // Assert - Step 3
        step3.Should().NotBeNull();
        step3.Status.Should().Be(SagaStepStatus.Completed);
        step3.Order.Should().Be(3);
        step3.Name.Should().Be("Step3");

        // Assert - No more steps
        noMoreSteps.Should().BeNull();

        // Assert - Saga is completed
        var completedSaga = await sagaRepo.GetByIdAsync(saga.Id);
        completedSaga.Should().NotBeNull();
        completedSaga.Status.Should().Be(SagaStatus.Completed);
        completedSaga.CompletedAt.Should().NotBeNull();
        completedSaga.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

        // Assert - All steps are completed
        completedSaga.Steps.Should().HaveCount(3);
        foreach (var step in completedSaga.Steps)
        {
            step.Status.Should().Be(SagaStepStatus.Completed);
        }
    }

    [Fact]
    public async Task ExecuteNextStepAsync_Idempotency_ExecutingCompletedStepReturnsSameStep()
    {
        // Arrange
        var (sagaRepo, stepRepo, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, stepRepo, compService);
        var definition = CreateMultiStepSagaDefinition();
        var saga = await service.CreateSagaAsync(definition);
        await service.StartSagaAsync(saga.Id);

        // Execute step 1
        var step1FirstCall = await service.ExecuteNextStepAsync(saga.Id);

        // Execute step 1 again (should return the same completed step due to idempotency)
        var step1SecondCall = await service.ExecuteNextStepAsync(saga.Id);

        // Assert
        step1FirstCall.Should().NotBeNull();
        step1SecondCall.Should().NotBeNull();
        step1FirstCall.Id.Should().Be(step1SecondCall.Id);
        step1SecondCall.Status.Should().Be(SagaStepStatus.Completed);
    }

    [Fact]
    public async Task ExecuteNextStepAsync_WithNullSagaId_ThrowsArgumentException()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        // Act
        var act = () => service.ExecuteNextStepAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteNextStepAsync_WithEmptySagaId_ThrowsArgumentException()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        // Act
        var act = () => service.ExecuteNextStepAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteNextStepAsync_WithNonExistentSagaId_ThrowsSagaNotFoundException()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        // Act
        var act = () => service.ExecuteNextStepAsync("non-existent-id");

        // Assert
        await act.Should().ThrowAsync<SagaNotFoundException>();
    }

    [Fact]
    public async Task ExecuteNextStepAsync_WithCompletedSaga_ReturnsNull()
    {
        // Arrange
        var (sagaRepo, stepRepo, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, stepRepo, compService);
        var definition = CreateMultiStepSagaDefinition();
        var saga = await service.CreateSagaAsync(definition);
        await service.StartSagaAsync(saga.Id);

        // Execute all steps to complete the saga
        await service.ExecuteNextStepAsync(saga.Id);
        await service.ExecuteNextStepAsync(saga.Id);
        await service.ExecuteNextStepAsync(saga.Id);

        // Verify saga is completed
        var completedSaga = await sagaRepo.GetByIdAsync(saga.Id);
        completedSaga.Status.Should().Be(SagaStatus.Completed);

        // Act - Try to execute another step (should return null)
        var result = await service.ExecuteNextStepAsync(saga.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSagaAsync_WithValidId_ReturnsSaga()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);
        var definition = CreateMultiStepSagaDefinition();
        var createdSaga = await service.CreateSagaAsync(definition);

        // Act
        var retrievedSaga = await service.GetSagaAsync(createdSaga.Id);

        // Assert
        retrievedSaga.Should().NotBeNull();
        retrievedSaga.Id.Should().Be(createdSaga.Id);
    }

    [Fact]
    public async Task GetSagaAsync_WithNullId_ThrowsArgumentException()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        // Act
        var act = () => service.GetSagaAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ListSagasAsync_WithNoFilter_ReturnsAllSagas()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        var definition1 = CreateMultiStepSagaDefinition();
        definition1.Name = "Saga1";
        definition1.Description = "Test";

        var definition2 = CreateMultiStepSagaDefinition();
        definition2.Name = "Saga2";
        definition2.Description = "Test";

        await service.CreateSagaAsync(definition1);
        await service.CreateSagaAsync(definition2);

        // Act
        var sagas = await service.ListSagasAsync();

        // Assert
        sagas.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListSagasAsync_WithStatusFilter_ReturnsOnlyMatchingSagas()
    {
        // Arrange
        var (sagaRepo, _, compService) = CreateInMemoryRepositories();
        var service = new SagaOrchestrationService(sagaRepo, new InMemorySagaStepRepository(), compService);

        var definition1 = CreateMultiStepSagaDefinition();
        definition1.Name = "RunningSaga";
        definition1.Description = "Test";

        var definition2 = CreateMultiStepSagaDefinition();
        definition2.Name = "CompletedSaga";
        definition2.Description = "Test";

        var saga1 = await service.CreateSagaAsync(definition1);
        await service.StartSagaAsync(saga1.Id);

        var saga2 = await service.CreateSagaAsync(definition2);

        // Act - Get only running sagas
        var runningSagas = await service.ListSagasAsync(SagaStatus.Running);

        // Assert
        runningSagas.Should().HaveCount(1);
        runningSagas[0].Id.Should().Be(saga1.Id);
    }
}
