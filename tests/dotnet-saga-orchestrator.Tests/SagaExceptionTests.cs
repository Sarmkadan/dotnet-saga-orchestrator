using System;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_SetsMessageOnly()
        {
            // Arrange
            var message = "Something went wrong";

            // Act
            var ex = new SagaException(message);

            // Assert
            ex.Message.Should().Be(message);
            ex.SagaId.Should().BeNull();
            ex.ErrorCode.Should().BeNull();
            ex.InnerException.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_SetsBoth()
        {
            // Arrange
            var message = "Outer error";
            var inner = new InvalidOperationException("Inner error");

            // Act
            var ex = new SagaException(message, inner);

            // Assert
            ex.Message.Should().Be(message);
            ex.InnerException.Should().BeSameAs(inner);
            ex.SagaId.Should().BeNull();
            ex.ErrorCode.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithMessageAndSagaId_SetsSagaId()
        {
            // Arrange
            var message = "Saga failed";
            var sagaId = "saga-123";

            // Act
            var ex = new SagaException(message, sagaId);

            // Assert
            ex.Message.Should().Be(message);
            ex.SagaId.Should().Be(sagaId);
            ex.ErrorCode.Should().BeNull();
            ex.InnerException.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithMessageSagaIdAndErrorCode_SetsAllProperties()
        {
            // Arrange
            var message = "Detailed failure";
            var sagaId = "saga-456";
            var errorCode = "ERR001";

            // Act
            var ex = new SagaException(message, sagaId, errorCode);

            // Assert
            ex.Message.Should().Be(message);
            ex.SagaId.Should().Be(sagaId);
            ex.ErrorCode.Should().Be(errorCode);
            ex.InnerException.Should().BeNull();
        }

        [Fact]
        public void Constructor_WithAllParameters_SetsEverything()
        {
            // Arrange
            var message = "Full info";
            var sagaId = "saga-789";
            var errorCode = "ERR999";
            var inner = new ArgumentException("Bad argument");

            // Act
            var ex = new SagaException(message, sagaId, errorCode, inner);

            // Assert
            ex.Message.Should().Be(message);
            ex.SagaId.Should().Be(sagaId);
            ex.ErrorCode.Should().Be(errorCode);
            ex.InnerException.Should().BeSameAs(inner);
        }

        [Fact]
        public void Constructor_WithNullMessage_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new SagaException(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Constructor_WithNullSagaIdAndErrorCode_AllowsNullProperties()
        {
            // Arrange
            var message = "Null saga and code";

            // Act
            var ex = new SagaException(message, null, null);

            // Assert
            ex.Message.Should().Be(message);
            ex.SagaId.Should().BeNull();
            ex.ErrorCode.Should().BeNull();
        }
    }
}
