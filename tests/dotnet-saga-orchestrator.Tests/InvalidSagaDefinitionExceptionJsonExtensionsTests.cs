using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class InvalidSagaDefinitionExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidInstance_ReturnsJson()
        {
            // Arrange
            var exception = new InvalidSagaDefinitionException("Test error");

            // Act
            var json = exception.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_WithIndentation_ReturnsIndentedJson()
        {
            // Arrange
            var exception = new InvalidSagaDefinitionException("Test error");

            // Act
            var json = exception.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json);
        }

        [Fact]
        public void ToJson_NullInstance_ThrowsArgumentNullException()
        {
            // Arrange
            InvalidSagaDefinitionException? exception = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => exception!.ToJson());
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsObject()
        {
            // Arrange
            var original = new InvalidSagaDefinitionException("Test error");
            var json = original.ToJson();

            // Act
            var result = InvalidSagaDefinitionExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original.Message, result.Message);
        }

        [Fact]
        public void FromJson_EmptyString_ReturnsNull()
        {
            // Act
            var result = InvalidSagaDefinitionExceptionJsonExtensions.FromJson(string.Empty);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_NullString_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => InvalidSagaDefinitionExceptionJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndValue()
        {
            // Arrange
            var original = new InvalidSagaDefinitionException("Test error");
            var json = original.ToJson();

            // Act
            var success = InvalidSagaDefinitionExceptionJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal(original.Message, result.Message);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "not json";

            // Act
            var success = InvalidSagaDefinitionExceptionJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }
    }
}
