using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using SagaOrchestrator.Application.DTOs;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Infrastructure.Logging;

namespace SagaOrchestrator.Tests;

public class SagaOrchestrationServiceTests
{
    private readonly Mock<ISagaRepository> _sagaRepositoryMock;
    private readonly Mock<ISagaStepRepository> _stepRepositoryMock;
    private readonly Mock<CompensationService> _compensationServiceMock;
    private readonly Mock<ISagaLogger> _loggerMock;
    private readonly SagaOrchestrationService _service;

    public SagaOrchestrationServiceTests()
    {
        _sagaRepositoryMock = new Mock<ISagaRepository>();
        _stepRepositoryMock = new Mock<ISagaStepRepository>();
        _compensationServiceMock = new Mock<CompensationService>();
        _loggerMock = new Mock<ISagaLogger>();

        _service = new SagaOrchestrationService(
            _sagaRepositoryMock.Object,
            _stepRepositoryMock.Object,
            _compensationServiceMock.Object,
            _loggerMock.Object);
    }

    #region CreateSagaAsync

    [Fact]
    public async Task CreateSagaAsync_ValidDefinition_ReturnsSaga()
    {
        // Arrange
        // SagaDefinition members are not virtual, so Moq cannot stub them; use a real, valid definition instead.
        var definition = new SagaDefinition("Valid", "Valid definition") { Id = "def-1" };
        definition.AddStep(new SagaStepDefinition("Step", "svc", "https://svc.example.com/action", "https://svc.example.com/compensate"));

        var expectedSaga = new Saga();
        _sagaRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Saga>()))
            .ReturnsAsync(expectedSaga);

        // Act
        var result = await _service.CreateSagaAsync(definition);

        // Assert
        result.Should().BeSameAs(expectedSaga);
        _sagaRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Saga>()), Times.Once);
    }

    [Fact]
    public async Task CreateSagaAsync_NullDefinition_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _service.CreateSagaAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateSagaAsync_InvalidDefinition_ThrowsInvalidSagaDefinitionException()
    {
        // Arrange
        // No steps makes the definition invalid; SagaDefinition cannot be mocked (non-virtual members).
        var definition = new SagaDefinition("Invalid", "No steps") { Id = "def-2" };

        // Act
        Func<Task> act = async () => await _service.CreateSagaAsync(definition);

        // Assert
        await act.Should().ThrowAsync<InvalidSagaDefinitionException>()
            .Where(e => e.SagaId == "def-2");
    }

    #endregion

    #region StartSagaAsync

    [Fact]
    public async Task StartSagaAsync_NullOrWhiteSpaceId_ThrowsArgumentException()
    {
        // Act
        Func<Task> act = async () => await _service.StartSagaAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Saga ID must be provided*");
    }

    [Fact]
    public async Task StartSagaAsync_SagaNotFound_ThrowsSagaNotFoundException()
    {
        // Arrange
        _sagaRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((Saga?)null);

        // Act
        Func<Task> act = async () => await _service.StartSagaAsync("unknown-id");

        // Assert
        await act.Should().ThrowAsync<SagaNotFoundException>()
            .WithMessage("*unknown-id*");
    }

    [Fact]
    public async Task StartSagaAsync_SagaNotInitialized_ThrowsSagaException()
    {
        // Arrange
        var saga = new Saga { Id = "s1", Status = SagaStatus.Running };
        _sagaRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(saga);

        // Act
        Func<Task> act = async () => await _service.StartSagaAsync("s1");

        // Assert
        await act.Should().ThrowAsync<SagaException>()
            .WithMessage("*Cannot start saga in*");
    }

    #endregion

    #region ExecuteNextStepAsync

    [Fact]
    public async Task ExecuteNextStepAsync_NullOrWhiteSpaceId_ThrowsArgumentException()
    {
        // Act
        Func<Task> act = async () => await _service.ExecuteNextStepAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Saga ID must be provided*");
    }

    [Fact]
    public async Task ExecuteNextStepAsync_SagaNotRunning_ThrowsSagaException()
    {
        // Arrange
        var saga = new Saga { Id = "s2", Status = SagaStatus.Initialized };
        _sagaRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(saga);

        // Act
        Func<Task> act = async () => await _service.ExecuteNextStepAsync("s2");

        // Assert
        await act.Should().ThrowAsync<SagaException>()
            .WithMessage("*Cannot execute step for saga in*");
    }

    #endregion

    #region HandleTimeoutAsync

    [Fact]
    public async Task HandleTimeoutAsync_NullOrWhiteSpaceSagaId_ThrowsArgumentException()
    {
        // Act
        Func<Task> act = async () => await _service.HandleTimeoutAsync(" ", "step-1");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Saga ID must be provided*");
    }

    [Fact]
    public async Task HandleTimeoutAsync_NullOrWhiteSpaceStepId_ThrowsArgumentException()
    {
        // Act
        Func<Task> act = async () => await _service.HandleTimeoutAsync("saga-1", " ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Step ID must be provided*");
    }

    #endregion

    #region AbortSagaAsync

    [Fact]
    public async Task AbortSagaAsync_NullOrWhiteSpaceId_ThrowsArgumentException()
    {
        // Act
        Func<Task> act = async () => await _service.AbortSagaAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Saga ID must be provided*");
    }

    #endregion

    #region CancelSagaAsync

    [Fact]
    public async Task CancelSagaAsync_NullOrWhiteSpaceId_ThrowsArgumentException()
    {
        // Act
        Func<Task> act = async () => await _service.CancelSagaAsync("   ");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Saga ID must be provided*");
    }

    [Fact]
    public async Task CancelSagaAsync_SagaNotRunning_ThrowsSagaException()
    {
        // Arrange
        var saga = new Saga { Id = "saga-1", Status = SagaStatus.Completed };
        _sagaRepositoryMock.Setup(r => r.GetByIdAsync("saga-1")).ReturnsAsync(saga);

        // Act
        Func<Task> act = async () => await _service.CancelSagaAsync("saga-1");

        // Assert
        await act.Should().ThrowAsync<SagaException>()
            .WithMessage("*Cannot cancel saga in Completed status*");
        _compensationServiceMock.Verify(c => c.BeginCompensationAsync(It.IsAny<Saga>()), Times.Never);
    }

    [Fact]
    public async Task CancelSagaAsync_RunningSaga_SetsCancelledAndStartsCompensation()
    {
        // Arrange
        var saga = new Saga { Id = "saga-2", Status = SagaStatus.Running };
        _sagaRepositoryMock.Setup(r => r.GetByIdAsync("saga-2")).ReturnsAsync(saga);
        _compensationServiceMock
            .Setup(c => c.BeginCompensationAsync(It.IsAny<Saga>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CancelSagaAsync("saga-2");

        // Assert
        result.Should().BeSameAs(saga);
        result.Status.Should().Be(SagaStatus.Cancelled);
        _compensationServiceMock.Verify(c => c.BeginCompensationAsync(saga), Times.Once);
    }

    [Fact]
    public async Task CancelSagaAsync_WithEventPublisher_PublishesSagaCancelledEvent()
    {
        // Arrange
        var publisher = new SagaEventPublisher();
        var service = new SagaOrchestrationService(
            _sagaRepositoryMock.Object,
            _stepRepositoryMock.Object,
            _compensationServiceMock.Object,
            _loggerMock.Object,
            publisher);
        var saga = new Saga { Id = "saga-3", Status = SagaStatus.Running };
        _sagaRepositoryMock.Setup(r => r.GetByIdAsync("saga-3")).ReturnsAsync(saga);
        _compensationServiceMock
            .Setup(c => c.BeginCompensationAsync(It.IsAny<Saga>()))
            .Returns(Task.CompletedTask);

        // Act
        await service.CancelSagaAsync("saga-3");

        // Assert
        var events = publisher.GetSagaEvents("saga-3");
        events.Should().ContainSingle(e => e.EventName == "SagaCancelled" && e.Severity == EventSeverity.Warning);
    }

    #endregion

    #region GetSagaAsync

    [Fact]
    public async Task GetSagaAsync_NullOrWhiteSpaceId_ThrowsArgumentException()
    {
        // Act
        Func<Task> act = async () => await _service.GetSagaAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Saga ID must be provided*");
    }

    #endregion

    #region ListSagasAsync

    [Fact]
    public async Task ListSagasAsync_ReturnsOrderedSagas()
    {
        // Arrange
        var sagas = new List<Saga>
        {
            new Saga { Id = "old", StartedAt = DateTime.UtcNow.AddHours(-2) },
            new Saga { Id = "new", StartedAt = DateTime.UtcNow.AddHours(-1) }
        };
        _sagaRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(sagas);

        // Act
        var result = await _service.ListSagasAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().Id.Should().Be("new"); // newest first
    }

    #endregion
}
