using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaExceptionExtensionsTests
    {
        [Fact]
        public void IsSagaNotFound_HappyPath_ReturnsTrue()
        {
            // Arrange
            var ex = new SagaException("saga-123", "SAGA_NOT_FOUND", "Saga not found");

            // Act
            var result = SagaExceptionExtensions.IsSagaNotFound(ex);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSagaNotFound_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaExceptionExtensions.IsSagaNotFound(null));
        }

        [Fact]
        public void IsSagaTimeout_HappyPath_ReturnsTrue()
        {
            // Arrange
            var ex = new SagaException("saga-123", "SAGA_TIMEOUT", "Saga timed out");

            // Act
            var result = SagaExceptionExtensions.IsSagaTimeout(ex);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsSagaTimeout_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaExceptionExtensions.IsSagaTimeout(null));
        }

        [Fact]
        public void GetDetailedMessage_HappyPath_ReturnsCorrectString()
        {
            // Arrange
            var ex = new SagaException("saga-123", "SAGA_NOT_FOUND", "Saga not found");

            // Act
            var result = SagaExceptionExtensions.GetDetailedMessage(ex);

            // Assert
            Assert.Contains("Saga Id: saga-123", result);
            Assert.Contains("Error Code: SAGA_NOT_FOUND", result);
            Assert.Contains("Message: Saga not found", result);
        }

        [Fact]
        public void GetDetailedMessage_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaExceptionExtensions.GetDetailedMessage(null));
        }
    }
}
