using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Tests
{
    public class SagaDefinitionTests
    {
        [Fact]
        public void Constructor_Default_ShouldInitializeWithExpectedDefaults()
        {
            // Act
            var saga = new SagaDefinition();

            // Assert
            saga.Id.Should().NotBeNullOrWhiteSpace();
            saga.Name.Should().Be("Undefined Saga");
            saga.Description.Should().Be("No description provided");
            saga.Version.Should().Be(1);
            saga.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            saga.IsActive.Should().BeTrue();
            saga.CompensationStrategy.Should().Be(CompensationStrategy.ReverseOrder);
            saga.Steps.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithNameAndDescription_ShouldSetProperties()
        {
            // Arrange
            var name = "OrderProcessingSaga";
            var description = "Handles order processing";

            // Act
            var saga = new SagaDefinition(name, description);

            // Assert
            saga.Id.Should().NotBeNullOrWhiteSpace();
            saga.Name.Should().Be(name);
            saga.Description.Should().Be(description);
            saga.Version.Should().Be(1);
            saga.IsActive.Should().BeTrue();
            saga.CompensationStrategy.Should().Be(CompensationStrategy.ReverseOrder);
            saga.Steps.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithNullName_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new SagaDefinition(null!, "desc");

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("name");
        }

        [Fact]
        public void Constructor_WithNullDescription_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => new SagaDefinition("name", null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("description");
        }

        [Fact]
        public void AddStep_NullStep_ShouldThrowArgumentNullException()
        {
            // Arrange
            var saga = new SagaDefinition();

            // Act
            Action act = () => saga.AddStep(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("stepDefinition");
        }

        [Fact]
        public void AddStep_ValidStep_ShouldAssignOrderAndAddToCollection()
        {
            // Arrange
            var saga = new SagaDefinition();
            var step = new SagaStepDefinition { Name = "StepOne" };

            // Act
            saga.AddStep(step);

            // Assert
            saga.Steps.Should().ContainSingle()
                .Which.Should().Be(step);
            step.Order.Should().Be(1);
        }

        [Fact]
        public void Validate_HappyPath_ReturnsTrue()
        {
            // Arrange
            var saga = new SagaDefinition("TestSaga", "Test description");
            var step = new SagaStepDefinition { Name = "StepOne" };
            saga.AddStep(step);

            // Act
            var result = saga.Validate();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Validate_MissingName_ReturnsFalse()
        {
            // Arrange
            var saga = new SagaDefinition();
            saga.Name = string.Empty; // invalidate name
            var step = new SagaStepDefinition { Name = "StepOne" };
            saga.AddStep(step);

            // Act
            var result = saga.Validate();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Validate_NoSteps_ReturnsFalse()
        {
            // Arrange
            var saga = new SagaDefinition("ValidName", "Valid description");

            // Act
            var result = saga.Validate();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void GetStepByName_ExistingName_ReturnsStep()
        {
            // Arrange
            var saga = new SagaDefinition();
            var step = new SagaStepDefinition { Name = "LookupStep" };
            saga.AddStep(step);

            // Act
            var found = saga.GetStepByName("LookupStep");

            // Assert
            found.Should().BeSameAs(step);
        }

        [Fact]
        public void GetStepByName_NonExistingOrNull_ReturnsNull()
        {
            // Arrange
            var saga = new SagaDefinition();

            // Act
            var foundExisting = saga.GetStepByName("Missing");
            var foundNull = saga.GetStepByName(null!);

            // Assert
            foundExisting.Should().BeNull();
            foundNull.Should().BeNull();
        }

        [Fact]
        public void GetStepByOrder_ExistingOrder_ReturnsStep()
        {
            // Arrange
            var saga = new SagaDefinition();
            var step1 = new SagaStepDefinition { Name = "First" };
            var step2 = new SagaStepDefinition { Name = "Second" };
            saga.AddStep(step1);
            saga.AddStep(step2);

            // Act
            var result = saga.GetStepByOrder(2);

            // Assert
            result.Should().BeSameAs(step2);
        }

        [Fact]
        public void GetStepByOrder_NonExistingOrder_ReturnsNull()
        {
            // Arrange
            var saga = new SagaDefinition();

            // Act
            var result = saga.GetStepByOrder(99);

            // Assert
            result.Should().BeNull();
        }
    }
}
