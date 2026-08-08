using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaStepExecutionExceptionJsonExtensionsTests
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

        [Fact]
        public void ToJson_SerializesCorrectly()
        {
            // Arrange
            var sagaId = "saga-123";
            var stepName = "process-payment";
            var stepOrder = 1;
            var message = "Payment failed";
            var exception = new SagaStepExecutionException(sagaId, stepName, stepOrder, message);

            // Act
            var json = SagaStepExecutionExceptionJsonExtensions.ToJson(exception);

            // Assert
            Assert.NotNull(json);
            Assert.Contains(sagaId, json);
            Assert.Contains(stepName, json);
            Assert.Contains(message, json);
        }

        [Fact]
        public void FromJson_DeserializesCorrectly()
        {
            // Arrange
            var json = "{\"SagaId\":\"saga-123\",\"StepName\":\"process-payment\",\"StepOrder\":1,\"Message\":\"Payment failed\"}";

            // Act
            var exception = SagaStepExecutionExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(exception);
            Assert.Equal("saga-123", exception.SagaId);
            Assert.Equal("process-payment", exception.StepName);
            Assert.Equal(1, exception.StepOrder);
            Assert.Contains("Payment failed", exception.Message);
        }

        [Fact]
        public void TryFromJson_DeserializesCorrectly()
        {
            // Arrange
            var json = "{\"SagaId\":\"saga-123\",\"StepName\":\"process-payment\",\"StepOrder\":1,\"Message\":\"Payment failed\"}";

            // Act
            var result = SagaStepExecutionExceptionJsonExtensions.TryFromJson(json, out var exception);

            // Assert
            Assert.True(result);
            Assert.NotNull(exception);
            Assert.Equal("saga-123", exception.SagaId);
            Assert.Equal("process-payment", exception.StepName);
            Assert.Equal(1, exception.StepOrder);
            Assert.Contains("Payment failed", exception.Message);
        }
    }
}