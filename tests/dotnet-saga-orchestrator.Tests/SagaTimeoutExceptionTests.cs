using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaTimeoutExceptionTests
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var sagaId = "saga-123";
            var timeoutSeconds = 30;

            // Act
            var exception = new SagaTimeoutException(sagaId, timeoutSeconds);

            // Assert
            Assert.Equal(timeoutSeconds, exception.TimeoutSeconds);
            Assert.Contains(sagaId, exception.Message);
            Assert.Contains("exceeded timeout", exception.Message);
        }

        [Fact]
        public void Constructor_WithStepName_SetsPropertiesCorrectly()
        {
            // Arrange
            var sagaId = "saga-123";
            var stepName = "process-payment";
            var timeoutSeconds = 30;

            // Act
            var exception = new SagaTimeoutException(sagaId, stepName, timeoutSeconds);

            // Assert
            Assert.Equal(timeoutSeconds, exception.TimeoutSeconds);
            Assert.Contains(sagaId, exception.Message);
            Assert.Contains(stepName, exception.Message);
            Assert.Contains("exceeded timeout", exception.Message);
        }

        [Fact]
        public void NullSagaId_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>("sagaId", () => new SagaTimeoutException(null, 30));
        }

        [Fact]
        public void NegativeTimeoutSeconds_ThrowsArgumentException()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>("timeoutSeconds", () => new SagaTimeoutException("saga-123", -1));
        }
    }
}