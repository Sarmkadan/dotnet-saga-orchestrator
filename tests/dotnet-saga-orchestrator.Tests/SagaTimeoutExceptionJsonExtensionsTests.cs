using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaTimeoutExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ValidException_ReturnsJsonString()
        {
            // Arrange
            var exception = new SagaTimeoutException("saga-123", 30);

            // Act
            var json = exception.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("saga-123", json);
            Assert.Contains("30", json);
        }

        [Fact]
        public void ToJson_IndentedValidException_ReturnsIndentedJsonString()
        {
            // Arrange
            var exception = new SagaTimeoutException("saga-123", "step-1", 30);

            // Act
            var json = exception.ToJson(indented: true);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\n", json); // Verify indentation occurred
            Assert.Contains("saga-123", json);
        }

        [Fact]
        public void FromJson_ValidJson_ThrowsNotSupportedException()
        {
            // Arrange
            var exception = new SagaTimeoutException("saga-123", 30);
            var json = exception.ToJson();

            // Act & Assert
            Assert.Throws<System.NotSupportedException>(() => SagaTimeoutExceptionJsonExtensions.FromJson(json));
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsNotSupportedException()
        {
            // Arrange
            var json = "{ \"invalid\": \"json\" }";

            // Act & Assert
            Assert.Throws<System.NotSupportedException>(() => SagaTimeoutExceptionJsonExtensions.FromJson(json));
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var deserialized = SagaTimeoutExceptionJsonExtensions.FromJson("");

            // Assert
            Assert.Null(deserialized);
        }

        [Fact]
        public void TryFromJson_ValidJson_ThrowsNotSupportedException()
        {
            // Arrange
            var exception = new SagaTimeoutException("saga-123", 30);
            var json = exception.ToJson();

            // Act & Assert
            Assert.Throws<System.NotSupportedException>(() => SagaTimeoutExceptionJsonExtensions.TryFromJson(json, out _));
        }

        [Fact]
        public void TryFromJson_InvalidJson_ThrowsNotSupportedException()
        {
            // Arrange
            var json = "{ \"invalid\": \"json\" }";

            // Act & Assert
            Assert.Throws<System.NotSupportedException>(() => SagaTimeoutExceptionJsonExtensions.TryFromJson(json, out _));
        }
    }
}
