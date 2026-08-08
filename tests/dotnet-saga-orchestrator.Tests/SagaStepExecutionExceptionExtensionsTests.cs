using System;
using System.Collections.Generic;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaStepExecutionExceptionExtensionsTests
    {
        [Fact]
        public void ToErrorMessage_ReturnsFormattedMessage()
        {
            // Arrange
            var exception = new SagaStepExecutionException(
                sagaId: "saga-123",
                stepName: "ProcessPayment",
                stepOrder: 2,
                message: "Payment gateway timeout");

            // Act
            var result = exception.ToErrorMessage();

            // Assert
            Assert.Contains("Saga step execution failed: Step 'ProcessPayment' (order 2) in saga 'saga-123'. Payment gateway timeout", result);
        }

        [Fact]
        public void ToErrorMessage_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            SagaStepExecutionException exception = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.ToErrorMessage());
        }

        [Fact]
        public void IsRetryable_ReturnsTrue_ForTimeoutExceptionInnerException()
        {
            // Arrange
            var innerException = new TimeoutException("Operation timed out");
            var exception = new SagaStepExecutionException(
                sagaId: "saga-123",
                stepName: "ProcessPayment",
                stepOrder: 1,
                message: "Payment failed",
                innerException);

            // Act
            var result = exception.IsRetryable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsRetryable_ReturnsTrue_ForMessageContainingRetryKeyword()
        {
            // Arrange
            var exception = new SagaStepExecutionException(
                sagaId: "saga-123",
                stepName: "ProcessPayment",
                stepOrder: 1,
                message: "Please retry the operation later");

            // Act
            var result = exception.IsRetryable();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsRetryable_ReturnsFalse_ForNonRetryableException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Invalid operation");
            var exception = new SagaStepExecutionException(
                sagaId: "saga-123",
                stepName: "ProcessPayment",
                stepOrder: 1,
                message: "Invalid operation",
                innerException);

            // Act
            var result = exception.IsRetryable();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsRetryable_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            SagaStepExecutionException exception = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.IsRetryable());
        }

        [Fact]
        public void ToErrorContext_ReturnsDictionaryWithExpectedKeysAndValues()
        {
            // Arrange
            var exception = new SagaStepExecutionException(
                sagaId: "saga-123",
                stepName: "ProcessPayment",
                stepOrder: 3,
                message: "Payment failed");

            // Act
            var result = exception.ToErrorContext();

            // Assert
            Assert.Equal("ProcessPayment", result["stepName"]);
            Assert.Equal(3, result["stepOrder"]);
            Assert.Equal("saga-123", result["sagaId"]);
            Assert.Equal("SagaStepExecutionFailed", result["errorType"]);
            Assert.Equal("STEP_EXECUTION_FAILED", result["errorCode"]);
            Assert.Equal("Payment failed", result["message"]);
            Assert.Contains("timestamp", result.Keys);
            Assert.IsType<string>(result["timestamp"]);
        }

        [Fact]
        public void ToErrorContext_IncludesInnerExceptionDetails_WhenPresent()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner error");
            var exception = new SagaStepExecutionException(
                sagaId: "saga-123",
                stepName: "ProcessPayment",
                stepOrder: 1,
                message: "Payment failed",
                innerException);

            // Act
            var result = exception.ToErrorContext();

            // Assert
            Assert.Equal("System.InvalidOperationException", result["innerExceptionType"]);
            Assert.Equal("Inner error", result["innerExceptionMessage"]);
        }

        [Fact]
        public void ToErrorContext_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            SagaStepExecutionException exception = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.ToErrorContext());
        }

        [Fact]
        public void ToTelemetryKey_ReturnsTupleWithStepNameStepOrderAndErrorCode()
        {
            // Arrange
            var exception = new SagaStepExecutionException(
                sagaId: "saga-123",
                stepName: "ProcessPayment",
                stepOrder: 5,
                message: "Payment failed");

            // Act
            var result = exception.ToTelemetryKey();

            // Assert
            Assert.Equal("ProcessPayment", result.StepName);
            Assert.Equal(5, result.StepOrder);
            Assert.Equal("STEP_EXECUTION_FAILED", result.ErrorCode);
        }

        [Fact]
        public void ToTelemetryKey_ReturnsEmptyStringForStepName_WhenStepNameIsNull()
        {
            // Arrange
            var exception = new SagaStepExecutionException(
                sagaId: "saga-123",
                stepName: null,
                stepOrder: 2,
                message: "Payment failed");

            // Act
            var result = exception.ToTelemetryKey();

            // Assert
            Assert.Equal(string.Empty, result.StepName);
            Assert.Equal(2, result.StepOrder);
            Assert.Equal("STEP_EXECUTION_FAILED", result.ErrorCode);
        }

        [Fact]
        public void ToTelemetryKey_ThrowsArgumentNullException_WhenExceptionIsNull()
        {
            // Arrange
            SagaStepExecutionException exception = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.ToTelemetryKey());
        }
    }
}