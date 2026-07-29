using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaStepExecutionExceptionTests
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var sagaId = "saga-123";
            var stepName = "process-payment";
            var stepOrder = 1;
            var message = "Payment failed";

            // Act
            var exception = new SagaStepExecutionException(sagaId, stepName, stepOrder, message);

            // Assert
            Assert.Equal(stepName, exception.StepName);
            Assert.Equal(stepOrder, exception.StepOrder);
            Assert.Contains(sagaId, exception.Message);
            Assert.Contains(stepName, exception.Message);
            Assert.Contains(message, exception.Message);
        }

        [Fact]
        public void Constructor_WithInnerException_SetsPropertiesCorrectly()
        {
            // Arrange
            var sagaId = "saga-123";
            var stepName = "process-payment";
            var stepOrder = 1;
            var message = "Payment failed";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new SagaStepExecutionException(sagaId, stepName, stepOrder, message, innerException);

            // Assert
            Assert.Equal(stepName, exception.StepName);
            Assert.Equal(stepOrder, exception.StepOrder);
            Assert.Equal(innerException, exception.InnerException);
            Assert.Contains(sagaId, exception.Message);
            Assert.Contains(message, exception.Message);
        }

        [Fact]
        public void Constructor_WithNegativeStepOrder_SetsProperty()
        {
            // Arrange
            var sagaId = "saga-123";
            var stepName = "test-step";
            var stepOrder = -1;
            var message = "Some error";

            // Act
            var exception = new SagaStepExecutionException(sagaId, stepName, stepOrder, message);

            // Assert
            Assert.Equal(stepOrder, exception.StepOrder);
        }

        [Fact]
        public void Constructor_WithEmptyStepName_SetsProperty()
        {
            // Arrange
            var sagaId = "saga-123";
            var stepName = string.Empty;
            var stepOrder = 1;
            var message = "Some error";

            // Act
            var exception = new SagaStepExecutionException(sagaId, stepName, stepOrder, message);

            // Assert
            Assert.Equal(stepName, exception.StepName);
        }
    }
}
