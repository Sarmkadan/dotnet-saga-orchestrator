namespace SagaOrchestrator.Tests
{
    using FluentAssertions;
    using SagaOrchestrator.Core.Builders;
    using SagaOrchestrator.Core.Domain.Models;
    using Xunit;

    public class SagaStepBuilderValidationTests
    {
        private static SagaStepBuilder CreateValidBuilder()
        {
            return SagaStepBuilder.Create("Test Step", "Test Service", "https://example.com/action")
                .WithOrder(1)
                .WithTimeout(30)
                .WithRetryPolicy(3, 1000);
        }

        [Fact]
        public void Validate_ValidBuilder_ReturnsEmptyList()
        {
            // Arrange
            var builder = CreateValidBuilder();

            // Act
            var errors = SagaStepBuilderValidation.Validate(builder);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void IsValid_ValidBuilder_ReturnsTrue()
        {
            // Arrange
            var builder = CreateValidBuilder();

            // Act
            var isValid = SagaStepBuilderValidation.IsValid(builder);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void EnsureValid_ValidBuilder_DoesNotThrow()
        {
            // Arrange
            var builder = CreateValidBuilder();

            // Act
            Action act = () => SagaStepBuilderValidation.EnsureValid(builder);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_NullBuilder_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaStepBuilderValidation.Validate(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("value");
        }

        [Fact]
        public void IsValid_NullBuilder_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaStepBuilderValidation.IsValid(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("value");
        }

        [Fact]
        public void EnsureValid_NullBuilder_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaStepBuilderValidation.EnsureValid(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("value");
        }

        [Fact]
        public void Validate_EmptyStepName_ReturnsErrorMessage()
        {
            // Arrange
            var builder = SagaStepBuilder.Create("", "Test Service", "https://example.com/action")
                .WithOrder(1)
                .WithTimeout(30)
                .WithRetryPolicy(3, 1000);

            // Act
            var errors = SagaStepBuilderValidation.Validate(builder);

            // Assert
            errors.Should().ContainSingle()
                .Which.Should().Be("Step name is required and cannot be null or whitespace");
        }

        [Fact]
        public void Validate_TimeoutTooLow_ReturnsErrorMessage()
        {
            // Arrange
            var builder = SagaStepBuilder.Create("Test Step", "Test Service", "https://example.com/action")
                .WithOrder(1)
                .WithTimeout(0) // Invalid: must be between 1 and 3600
                .WithRetryPolicy(3, 1000);

            // Act
            var errors = SagaStepBuilderValidation.Validate(builder);

            // Assert
            errors.Should().ContainSingle()
                .Which.Should().Be("Timeout must be at least 1 second");
        }
    }
}