using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaDefinitionServiceExtensionsTests
    {
        [Fact]
        public async Task CreateAndActivateDefinitionAsync_ReturnsActivatedDefinition()
        {
            // Arrange
            var mock = new Mock<SagaDefinitionService>();
            var created = new SagaDefinition { Id = "def-1", IsActive = false };
            var activated = new SagaDefinition { Id = "def-1", IsActive = true };

            mock.Setup(s => s.CreateDefinitionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(created);
            mock.Setup(s => s.ActivateDefinitionAsync(It.IsAny<string>()))
                .ReturnsAsync(activated);

            // Act
            var result = await mock.Object.CreateAndActivateDefinitionAsync("name", "desc");

            // Assert
            result.Should().BeSameAs(activated);
            result.IsActive.Should().BeTrue();
            mock.Verify(s => s.CreateDefinitionAsync("name", "desc"), Times.Once);
            mock.Verify(s => s.ActivateDefinitionAsync("def-1"), Times.Once);
        }

        [Fact]
        public async Task AddStepsAsync_AddsAllSteps()
        {
            // Arrange
            var mock = new Mock<SagaDefinitionService>();
            var definition = new SagaDefinition { Id = "def-2", IsActive = false };
            var steps = new[]
            {
                new SagaStepDefinition { Name = "step1" },
                new SagaStepDefinition { Name = "step2" }
            };

            mock.Setup(s => s.GetDefinitionAsync(It.IsAny<string>()))
                .ReturnsAsync(definition);
            mock.Setup(s => s.AddStepAsync(It.IsAny<string>(), It.IsAny<SagaStepDefinition>()))
                .ReturnsAsync((string id, SagaStepDefinition step) => definition);

            // Act
            var result = await mock.Object.AddStepsAsync("def-2", steps);

            // Assert
            result.Should().BeSameAs(definition);
            mock.Verify(s => s.GetDefinitionAsync("def-2"), Times.Once);
            mock.Verify(s => s.AddStepAsync("def-2", steps[0]), Times.Once);
            mock.Verify(s => s.AddStepAsync("def-2", steps[1]), Times.Once);
        }

        [Fact]
        public async Task DefinitionExistsAsync_ReturnsTrueWhenExists()
        {
            // Arrange
            var mock = new Mock<SagaDefinitionService>();
            var existing = new SagaDefinition { Id = "def-3", IsActive = true };

            mock.Setup(s => s.GetDefinitionByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(existing);

            // Act
            var exists = await mock.Object.DefinitionExistsAsync("some-name");

            // Assert
            exists.Should().BeTrue();
            mock.Verify(s => s.GetDefinitionByNameAsync("some-name"), Times.Once);
        }

        [Fact]
        public async Task GetOrCreateDefinitionAsync_WhenExists_ReturnsExisting()
        {
            // Arrange
            var mock = new Mock<SagaDefinitionService>();
            var existing = new SagaDefinition { Id = "def-4", IsActive = false };

            mock.Setup(s => s.GetDefinitionByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(existing);

            // Act
            var result = await mock.Object.GetOrCreateDefinitionAsync("name", "desc");

            // Assert
            result.Should().BeSameAs(existing);
            mock.Verify(s => s.GetDefinitionByNameAsync("name"), Times.Once);
            mock.Verify(s => s.CreateDefinitionAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetOrCreateDefinitionAsync_WhenNotExists_CreatesAndActivates()
        {
            // Arrange
            var mock = new Mock<SagaDefinitionService>();
            var created = new SagaDefinition { Id = "def-5", IsActive = false };
            var activated = new SagaDefinition { Id = "def-5", IsActive = true };

            mock.Setup(s => s.GetDefinitionByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((SagaDefinition?)null);
            mock.Setup(s => s.CreateDefinitionAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(created);
            mock.Setup(s => s.ActivateDefinitionAsync(It.IsAny<string>()))
                .ReturnsAsync(activated);

            // Act
            var result = await mock.Object.GetOrCreateDefinitionAsync("new-name", "new-desc", activateIfCreated: true);

            // Assert
            result.Should().BeSameAs(activated);
            result.IsActive.Should().BeTrue();
            mock.Verify(s => s.GetDefinitionByNameAsync("new-name"), Times.Once);
            mock.Verify(s => s.CreateDefinitionAsync("new-name", "new-desc"), Times.Once);
            mock.Verify(s => s.ActivateDefinitionAsync("def-5"), Times.Once);
        }

        [Fact]
        public async Task GetAndValidateDefinitionAsync_NotFound_ThrowsSagaException()
        {
            // Arrange
            var mock = new Mock<SagaDefinitionService>();
            mock.Setup(s => s.GetDefinitionByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((SagaDefinition?)null);

            // Act
            Func<Task> act = async () => await mock.Object.GetAndValidateDefinitionAsync("missing-name");

            // Assert
            await act.Should().ThrowAsync<SagaException>()
                .WithMessage("Definition 'missing-name' not found");
            mock.Verify(s => s.GetDefinitionByNameAsync("missing-name"), Times.Once);
        }
    }
}
