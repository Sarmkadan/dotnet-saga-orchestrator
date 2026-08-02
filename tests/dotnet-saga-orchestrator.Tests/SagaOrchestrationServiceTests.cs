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

namespace SagaOrchestrator.Tests
{
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
            var definitionMock = new Mock<SagaDefinition>();
            definitionMock.SetupGet(d => d.Id).Returns("def-1");
            definitionMock.Setup(d => d.Validate()).Returns(true);

            var expectedSaga = new Saga();
            _sagaRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<Saga>()))
                .ReturnsAsync(expectedSaga);

            // Act
            var result = await _service.CreateSagaAsync(definitionMock.Object);

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
            var definitionMock = new Mock<SagaDefinition>();
            definitionMock.Setup(d => d.Validate()).Returns(false);
            definitionMock.SetupGet(d => d.Id).Returns("def-2");

            // Act
            Func<Task> act = async () => await _service.CreateSagaAsync(definitionMock.Object);

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
}
