using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Tests
{
    public class CompensationServiceExtensionsTests
    {
        private const string SagaId = "saga-123";

        [Fact]
        public async Task ExecuteAllCompensationsAsync_ReturnsAllExecutedCompensations()
        {
            // Arrange
            var compensation1 = new CompensationTransaction { Id = "c1", Status = CompensationStatus.Pending };
            var compensation2 = new CompensationTransaction { Id = "c2", Status = CompensationStatus.Pending };

            var mock = new Mock<CompensationService>();
            mock.SetupSequence(s => s.ExecuteNextCompensationAsync(SagaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(compensation1)
                .ReturnsAsync(compensation2)
                .ReturnsAsync((CompensationTransaction?)null);

            // Act
            var result = await mock.Object.ExecuteAllCompensationsAsync(SagaId);

            // Assert
            result.Should().HaveCount(2);
            result.Should().ContainInOrder(compensation1, compensation2);
        }

        [Fact]
        public async Task ExecuteAllCompensationsAsync_NullService_ThrowsArgumentNullException()
        {
            // Arrange
            CompensationService? service = null;

            // Act
            Func<Task> act = async () => await service!.ExecuteAllCompensationsAsync(SagaId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task ExecuteAllCompensationsAsync_NullOrWhiteSpaceSagaId_ThrowsArgumentException()
        {
            // Arrange
            var mock = new Mock<CompensationService>();

            // Act
            Func<Task> actNull = async () => await mock.Object.ExecuteAllCompensationsAsync(null!);
            Func<Task> actEmpty = async () => await mock.Object.ExecuteAllCompensationsAsync(string.Empty);
            Func<Task> actWhiteSpace = async () => await mock.Object.ExecuteAllCompensationsAsync("   ");

            // Assert
            await actNull.Should().ThrowAsync<ArgumentException>();
            await actEmpty.Should().ThrowAsync<ArgumentException>();
            await actWhiteSpace.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetCompensationsByStatusAsync_FiltersByProvidedStatus()
        {
            // Arrange
            var pending = new CompensationTransaction { Id = "p1", Status = CompensationStatus.Pending };
            var completed = new CompensationTransaction { Id = "c1", Status = CompensationStatus.Completed };
            var all = new List<CompensationTransaction> { pending, completed };

            var mock = new Mock<CompensationService>();
            mock.Setup(s => s.GetCompensationsAsync(SagaId)).ReturnsAsync(all);

            // Act
            var result = await mock.Object.GetCompensationsByStatusAsync(SagaId, CompensationStatus.Pending);

            // Assert
            result.Should().ContainSingle()
                  .Which.Should().Be(pending);
        }

        [Fact]
        public async Task GetCompensationsByStatusAsync_NullService_ThrowsArgumentNullException()
        {
            // Arrange
            CompensationService? service = null;

            // Act
            Func<Task> act = async () => await service!.GetCompensationsByStatusAsync(SagaId, CompensationStatus.Pending);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task HasPendingCompensationsAsync_ReturnsTrueWhenPendingExists()
        {
            // Arrange
            var pending = new CompensationTransaction { Id = "p1", Status = CompensationStatus.Pending };
            var mock = new Mock<CompensationService>();
            mock.Setup(s => s.GetCompensationsAsync(SagaId)).ReturnsAsync(new List<CompensationTransaction> { pending });

            // Act
            var hasPending = await mock.Object.HasPendingCompensationsAsync(SagaId);

            // Assert
            hasPending.Should().BeTrue();
        }

        [Fact]
        public async Task HasPendingCompensationsAsync_ReturnsFalseWhenNoPending()
        {
            // Arrange
            var completed = new CompensationTransaction { Id = "c1", Status = CompensationStatus.Completed };
            var mock = new Mock<CompensationService>();
            mock.Setup(s => s.GetCompensationsAsync(SagaId)).ReturnsAsync(new List<CompensationTransaction> { completed });

            // Act
            var hasPending = await mock.Object.HasPendingCompensationsAsync(SagaId);

            // Assert
            hasPending.Should().BeFalse();
        }

        [Fact]
        public async Task GetCompensationCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var list = new List<CompensationTransaction>
            {
                new CompensationTransaction { Id = "c1", Status = CompensationStatus.Completed },
                new CompensationTransaction { Id = "c2", Status = CompensationStatus.Pending }
            };

            var mock = new Mock<CompensationService>();
            mock.Setup(s => s.GetCompensationsAsync(SagaId)).ReturnsAsync(list);

            // Act
            var count = await mock.Object.GetCompensationCountAsync(SagaId);

            // Assert
            count.Should().Be(2);
        }

        [Fact]
        public async Task GetCompensationCountAsync_NullService_ThrowsArgumentNullException()
        {
            // Arrange
            CompensationService? service = null;

            // Act
            Func<Task> act = async () => await service!.GetCompensationCountAsync(SagaId);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task GetCompensationCountAsync_NullOrWhiteSpaceSagaId_ThrowsArgumentException()
        {
            // Arrange
            var mock = new Mock<CompensationService>();

            // Act
            Func<Task> actNull = async () => await mock.Object.GetCompensationCountAsync(null!);
            Func<Task> actEmpty = async () => await mock.Object.GetCompensationCountAsync(string.Empty);
            Func<Task> actWhiteSpace = async () => await mock.Object.GetCompensationCountAsync("   ");

            // Assert
            await actNull.Should().ThrowAsync<ArgumentException>();
            await actEmpty.Should().ThrowAsync<ArgumentException>();
            await actWhiteSpace.Should().ThrowAsync<ArgumentException>();
        }
    }
}
