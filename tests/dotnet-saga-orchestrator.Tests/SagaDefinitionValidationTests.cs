namespace SagaOrchestrator.Tests
{
    using FluentAssertions;
    using SagaOrchestrator.Core.Domain.Models;
    using Xunit;

    public class SagaDefinitionValidationTests
    {
        private static SagaDefinition CreateValidDefinition()
        {
            var definition = new SagaDefinition("Valid Saga", "A valid saga definition for testing")
            {
                Version = 1
            };
            definition.AddStep(new SagaStepDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Step 1",
                ServiceName = "ServiceA",
                ServiceUrl = "https://servicea.example.com/action",
                Order = 1,
                TimeoutSeconds = 30,
                MaxRetries = 3,
                RetryDelayMilliseconds = 1000
            });
            return definition;
        }

        [Fact]
        public void Validate_ValidDefinition_ReturnsEmptyList()
        {
            // Arrange
            var definition = CreateValidDefinition();

            // Act
            var errors = SagaDefinitionValidation.Validate(definition);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void IsValid_ValidDefinition_ReturnsTrue()
        {
            // Arrange
            var definition = CreateValidDefinition();

            // Act
            var isValid = SagaDefinitionValidation.IsValid(definition);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void EnsureValid_ValidDefinition_DoesNotThrow()
        {
            // Arrange
            var definition = CreateValidDefinition();

            // Act
            Action act = () => SagaDefinitionValidation.EnsureValid(definition);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_NullDefinition_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaDefinitionValidation.Validate(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("value");
        }

        [Fact]
        public void Validate_EmptyId_ReturnsErrorMessage()
        {
            // Arrange
            var definition = CreateValidDefinition();
            definition.Id = string.Empty;

            // Act
            var errors = SagaDefinitionValidation.Validate(definition);

            // Assert
            errors.Should().ContainSingle()
                .Which.Should().Be("SagaDefinition.Id cannot be null or whitespace.");
        }

        [Fact]
        public void Validate_NameExceedsMaxLength_ReturnsErrorMessage()
        {
            // Arrange
            var definition = CreateValidDefinition();
            definition.Name = new string('A', 256); // 256 > 255

            // Act
            var errors = SagaDefinitionValidation.Validate(definition);

            // Assert
            errors.Should().ContainSingle()
                .Which.Should().Be("SagaDefinition.Name cannot exceed 255 characters.");
        }

        [Fact]
        public void Validate_StepWithEmptyName_ReturnsErrorMessage()
        {
            // Arrange
            var definition = CreateValidDefinition();
            definition.Steps[0].Name = string.Empty;

            // Act
            var errors = SagaDefinitionValidation.Validate(definition);

            // Assert
            errors.Should().ContainSingle()
                .Which.Should().Be("SagaDefinition.Steps[0].Name cannot be null or whitespace.");
        }

        [Fact]
        public void EnsureValid_InvalidDefinition_ThrowsArgumentException()
        {
            // Arrange
            var definition = CreateValidDefinition();
            definition.Id = string.Empty; // Make it invalid

            // Act
            Action act = () => SagaDefinitionValidation.EnsureValid(definition);

            // Assert
            act.Should().Throw<ArgumentException>()
                .Where(ex => ex.Message.Contains("SagaDefinition.Id cannot be null or whitespace"));
        }
    }
}