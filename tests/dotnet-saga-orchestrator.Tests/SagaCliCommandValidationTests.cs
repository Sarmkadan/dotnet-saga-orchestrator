using Xunit;
using System.Collections.Generic;
using SagaOrchestrator.Presentation.Cli.Commands;

namespace SagaOrchestrator.Tests
{
    public class SagaCliCommandValidationTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var command = new SagaCliCommand { CommandType = "create", Arguments = new Dictionary<string, string> { { "definition", "test" } } };

            // Act
            var errors = SagaCliCommandValidation.Validate(command);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var command = new SagaCliCommand { CommandType = "create", Arguments = new Dictionary<string, string> { { "definition", "test" } } };

            // Act
            var isValid = SagaCliCommandValidation.IsValid(command);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var command = new SagaCliCommand { CommandType = "create", Arguments = new Dictionary<string, string> { { "definition", "test" } } };

            // Act and Assert
            SagaCliCommandValidation.EnsureValid(command);
        }

        [Fact]
        public void Validate_NullCommand_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaCliCommandValidation.Validate(null));
        }

        [Fact]
        public void IsValid_NullCommand_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaCliCommandValidation.IsValid(null));
        }

        [Fact]
        public void EnsureValid_NullCommand_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaCliCommandValidation.EnsureValid(null));
        }

        [Fact]
        public void Validate_InvalidCommandType_ReturnsErrorList()
        {
            // Arrange
            var command = new SagaCliCommand { CommandType = "invalid" };

            // Act
            var errors = SagaCliCommandValidation.Validate(command);

            // Assert
            Assert.NotEmpty(errors);
        }
    }
}
