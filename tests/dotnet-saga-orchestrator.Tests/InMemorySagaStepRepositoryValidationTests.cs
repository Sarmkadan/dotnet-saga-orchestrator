using System;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests
{
    public class InMemorySagaStepRepositoryValidationTests
    {
        private static SagaStep CreateValidStep()
        {
            return new SagaStep
            {
                Id = Guid.NewGuid().ToString(),
                SagaId = Guid.NewGuid().ToString(),
                Name = "SampleStep",
                Order = 1,
                Status = SagaStepStatus.Pending,
                ServiceUrl = "https://example.com/service",
                CompensationUrl = "https://example.com/compensate",
                MaxRetries = 3,
                TimeoutSeconds = 30
            };
        }

        [Fact]
        public void Validate_Repository_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var repo = new InMemorySagaStepRepository();

            // Act
            var errors = repo.Validate();

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_Repository_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            InMemorySagaStepRepository repo = null!;

            // Act
            Action act = () => repo.Validate();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Validate_SagaStep_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var step = CreateValidStep();

            // Act
            var errors = InMemorySagaStepRepositoryValidation.Validate(step);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_SagaStep_NullStep_ThrowsArgumentNullException()
        {
            // Arrange
            SagaStep step = null!;

            // Act
            Action act = () => InMemorySagaStepRepositoryValidation.Validate(step);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Validate_SagaStep_InvalidBoundaryValues_ReturnsExpectedProblems()
        {
            // Arrange
            var step = CreateValidStep();
            step.Id = "";
            step.SagaId = "  ";
            step.Name = null!;
            step.Order = 0;
            step.ServiceUrl = "";
            step.MaxRetries = -1;
            step.TimeoutSeconds = 0;

            // Act
            var errors = InMemorySagaStepRepositoryValidation.Validate(step);

            // Assert
            errors.Should().Contain("SagaStep.Id must be a non-empty string.");
            errors.Should().Contain("SagaStep.SagaId must be a non-empty string.");
            errors.Should().Contain("SagaStep.Name must be a non-empty string.");
            errors.Should().Contain("SagaStep.Order must be a positive integer (1-based).");
            errors.Should().Contain("SagaStep.ServiceUrl must be a non-empty string.");
            errors.Should().Contain("SagaStep.MaxRetries must be a non-negative integer.");
            errors.Should().Contain("SagaStep.TimeoutSeconds must be a positive integer.");
        }

        [Fact]
        public void Validate_SagaStep_CompletedWithoutCompletedAt_ReturnsProblem()
        {
            // Arrange
            var step = CreateValidStep();
            step.Status = SagaStepStatus.Completed;
            step.CompletedAt = null;

            // Act
            var errors = InMemorySagaStepRepositoryValidation.Validate(step);

            // Assert
            errors.Should().Contain("SagaStep.CompletedAt must be set when Status is Completed.");
        }

        [Fact]
        public void IsValid_Repository_HappyPath_ReturnsTrue()
        {
            // Arrange
            var repo = new InMemorySagaStepRepository();

            // Act
            var result = repo.IsValid();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValid_SagaStep_InvalidStep_ReturnsFalse()
        {
            // Arrange
            var step = CreateValidStep();
            step.Name = "";

            // Act
            var result = InMemorySagaStepRepositoryValidation.IsValid(step);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void EnsureValid_Repository_HappyPath_DoesNotThrow()
        {
            // Arrange
            var repo = new InMemorySagaStepRepository();

            // Act
            Action act = () => repo.EnsureValid();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void EnsureValid_SagaStep_InvalidStep_ThrowsArgumentException()
        {
            // Arrange
            var step = CreateValidStep();
            step.Order = -1;

            // Act
            Action act = () => InMemorySagaStepRepositoryValidation.EnsureValid(step);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*SagaStep is not valid*");
        }

        [Fact]
        public void EnsureValid_SagaStep_NullStep_ThrowsArgumentNullException()
        {
            // Arrange
            SagaStep step = null!;

            // Act
            Action act = () => InMemorySagaStepRepositoryValidation.EnsureValid(step);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
