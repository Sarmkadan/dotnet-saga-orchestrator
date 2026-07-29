using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class ConfigurationExceptionTests
    {
        [Fact]
        public void Constructor_NoParameters_Succeeds()
        {
            // Act
            var exception = new ConfigurationException();

            // Assert
            Assert.NotNull(exception);
        }

        [Fact]
        public void Constructor_WithMessage_Succeeds()
        {
            // Arrange
            var message = "Test message";

            // Act
            var exception = new ConfigurationException(message);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(message, exception.Message);
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_Succeeds()
        {
            // Arrange
            var message = "Test message";
            var innerException = new Exception("Inner exception");

            // Act
            var exception = new ConfigurationException(message, innerException);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
        }

        [Fact]
        public void Constructor_NullMessage_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ConfigurationException(null!));
        }
    }
}
