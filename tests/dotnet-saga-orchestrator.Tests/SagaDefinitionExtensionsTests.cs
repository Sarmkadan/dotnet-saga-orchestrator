using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Tests
{
    public class SagaDefinitionExtensionsTests
    {
        [Fact]
        public void Create_WithValidParameters_ReturnsSagaDefinition()
        {
            // Arrange
            var name = "TestSaga";
            var description = "Test description";
            var compensationStrategy = CompensationStrategy.Parallel;

            // Act
            var saga = SagaDefinitionExtensions.Create(name, description, compensationStrategy);

            // Assert
            saga.Should().NotBeNull();
            saga.Name.Should().Be(name);
            saga.Description.Should().Be(description);
            saga.CompensationStrategy.Should().Be(compensationStrategy);
            saga.Version.Should().Be(1);
            saga.IsActive.Should().BeTrue();
            saga.Steps.Should().BeEmpty();
        }

        [Fact]
        public void Create_WithDefaultCompensationStrategy_UsesReverseOrder()
        {
            // Arrange
            var name = "TestSaga";
            var description = "Test description";

            // Act
            var saga = SagaDefinitionExtensions.Create(name, description);

            // Assert
            saga.CompensationStrategy.Should().Be(CompensationStrategy.ReverseOrder);
        }

        [Fact]
        public void Create_WithNullName_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaDefinitionExtensions.Create(null, "description");

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("name");
        }

        [Fact]
        public void Create_WithNullDescription_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaDefinitionExtensions.Create("name", null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("description");
        }

        [Fact]
        public void AddSteps_WithValidParameters_AddsStepsToSaga()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");
            var steps = new List<SagaStepDefinition>
            {
                new("Step1", "Service1", "http://step1.com", "http://step1.com/compensate"),
                new("Step2", "Service2", "http://step2.com", "http://step2.com/compensate")
            };

            // Act
            saga.AddSteps(steps);

            // Assert
            saga.Steps.Should().HaveCount(2);
            saga.Steps.First().Name.Should().Be("Step1");
            saga.Steps.Last().Name.Should().Be("Step2");
        }

        [Fact]
        public void AddSteps_WithNullSaga_ThrowsArgumentNullException()
        {
            // Arrange
            var steps = new List<SagaStepDefinition> { new("Step1", "Service1", "http://step1.com", "http://step1.com/compensate") };

            // Act
            Action act = () => SagaDefinitionExtensions.AddSteps(null!, steps);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sagaDefinition");
        }

        [Fact]
        public void AddSteps_WithNullSteps_ThrowsArgumentNullException()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");

            // Act
            Action act = () => saga.AddSteps(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("stepDefinitions");
        }

        [Fact]
        public void AddSteps_WithEmptyCollection_DoesNotThrowAndDoesNotAddSteps()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");
            var steps = Enumerable.Empty<SagaStepDefinition>();

            // Act
            Action act = () => saga.AddSteps(steps);

            // Assert
            act.Should().NotThrow();
            saga.Steps.Should().BeEmpty();
        }

        [Fact]
        public void GetStepCount_WithSagaHavingSteps_ReturnsCorrectCount()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");
            var steps = new List<SagaStepDefinition>
            {
                new("Step1", "Service1", "http://step1.com", "http://step1.com/compensate"),
                new("Step2", "Service2", "http://step2.com", "http://step2.com/compensate"),
                new("Step3", "Service3", "http://step3.com", "http://step3.com/compensate")
            };
            saga.AddSteps(steps);

            // Act
            var count = saga.GetStepCount();

            // Assert
            count.Should().Be(3);
        }

        [Fact]
        public void GetStepCount_WithSagaHavingNoSteps_ReturnsZero()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");

            // Act
            var count = saga.GetStepCount();

            // Assert
            count.Should().Be(0);
        }

        [Fact]
        public void GetStepCount_WithNullSaga_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaDefinitionExtensions.GetStepCount(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sagaDefinition");
        }

        [Fact]
        public void ContainsStep_WithExistingStepName_ReturnsTrue()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");
            var step = new SagaStepDefinition("ExistingStep", "Service", "http://step.com", "http://step.com/compensate");
            saga.AddStep(step);

            // Act
            var result = saga.ContainsStep("ExistingStep");

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ContainsStep_WithNonExistingStepName_ReturnsFalse()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");
            var step = new SagaStepDefinition("ExistingStep", "Service", "http://step.com", "http://step.com/compensate");
            saga.AddStep(step);

            // Act
            var result = saga.ContainsStep("NonExistingStep");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ContainsStep_WithNullSaga_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaDefinitionExtensions.ContainsStep(null!, "stepName");

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sagaDefinition");
        }

        [Fact]
        public void ContainsStep_WithNullOrEmptyStepName_ThrowsArgumentException()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");

            // Act & Assert
            Action actNull = () => saga.ContainsStep(null!);
            actNull.Should().Throw<ArgumentException>().WithParameterName("stepName");

            Action actEmpty = () => saga.ContainsStep(string.Empty);
            actEmpty.Should().Throw<ArgumentException>().WithParameterName("stepName");

            Action actWhitespace = () => saga.ContainsStep("   ");
            actWhitespace.Should().Throw<ArgumentException>().WithParameterName("stepName");
        }

        [Fact]
        public void GetFirstStep_WithSagaHavingSteps_ReturnsFirstStep()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");
            var firstStep = new SagaStepDefinition("FirstStep", "Service1", "http://first.com", "http://first.com/compensate");
            var secondStep = new SagaStepDefinition("SecondStep", "Service2", "http://second.com", "http://second.com/compensate");
            saga.AddSteps(new List<SagaStepDefinition> { firstStep, secondStep });

            // Act
            var result = saga.GetFirstStep();

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("FirstStep");
        }

        [Fact]
        public void GetFirstStep_WithSagaHavingNoSteps_ReturnsNull()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");

            // Act
            var result = saga.GetFirstStep();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetFirstStep_WithNullSaga_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaDefinitionExtensions.GetFirstStep(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sagaDefinition");
        }

        [Fact]
        public void GetLastStep_WithSagaHavingSteps_ReturnsLastStep()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");
            var firstStep = new SagaStepDefinition("FirstStep", "Service1", "http://first.com", "http://first.com/compensate");
            var lastStep = new SagaStepDefinition("LastStep", "Service2", "http://last.com", "http://last.com/compensate");
            saga.AddSteps(new List<SagaStepDefinition> { firstStep, lastStep });

            // Act
            var result = saga.GetLastStep();

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be("LastStep");
        }

        [Fact]
        public void GetLastStep_WithSagaHavingNoSteps_ReturnsNull()
        {
            // Arrange
            var saga = SagaDefinitionExtensions.Create("TestSaga", "Test description");

            // Act
            var result = saga.GetLastStep();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetLastStep_WithNullSaga_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaDefinitionExtensions.GetLastStep(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithParameterName("sagaDefinition");
        }
    }
}