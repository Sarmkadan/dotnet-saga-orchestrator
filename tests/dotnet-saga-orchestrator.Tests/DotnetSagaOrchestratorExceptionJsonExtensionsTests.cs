using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class DotnetSagaOrchestratorExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ValidException_ReturnsJsonString()
        {
            // Arrange
            var exception = new DotnetSagaOrchestratorException("Test error message");

            // Act
            var json = exception.ToJson();

            // Assert
            Assert.NotNull(json);
            Assert.Contains("test error message", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ToJson_WithIndentation_ReturnsFormattedJson()
        {
            // Arrange
            var exception = new DotnetSagaOrchestratorException("Test");

            // Act
            var json = exception.ToJson(indented: true);

            // Assert
            Assert.Contains(Environment.NewLine, json);
            Assert.Contains("  ", json); // Check for indentation
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Arrange
            DotnetSagaOrchestratorException? exception = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception.ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsException()
        {
            // Arrange
            var originalException = new DotnetSagaOrchestratorException("Deserialization test");
            var json = originalException.ToJson();

            // Act
            var result = DotnetSagaOrchestratorExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Deserialization test", result.Message);
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            // Arrange
            string invalidJson = "{ this is not valid json }";

            // Act
            var result = DotnetSagaOrchestratorExceptionJsonExtensions.FromJson(invalidJson);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_NullOrEmptyInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => DotnetSagaOrchestratorExceptionJsonExtensions.FromJson(null!));
            Assert.Throws<ArgumentException>(() => DotnetSagaOrchestratorExceptionJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndException()
        {
            // Arrange
            var originalException = new DotnetSagaOrchestratorException("TryParse test");
            var json = originalException.ToJson();

            // Act
            var success = DotnetSagaOrchestratorExceptionJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal("TryParse test", result.Message);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            string invalidJson = "invalid";

            // Act
            var success = DotnetSagaOrchestratorExceptionJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }
    }
}
