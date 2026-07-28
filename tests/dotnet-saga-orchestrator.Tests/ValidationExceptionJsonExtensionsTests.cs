using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class ValidationExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidInstance_ReturnsJson()
        {
            // Arrange
            var validationException = new ValidationException("Test message");

            // Act
            var json = validationException.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_WithIndentation_ReturnsIndentedJson()
        {
            // Arrange
            var validationException = new ValidationException("Test message");

            // Act
            var json = validationException.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json); // indented JSON contains line breaks
        }

        [Fact]
        public void ToJson_NullInstance_ThrowsArgumentNullException()
        {
            // Arrange
            ValidationException? validationException = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => validationException!.ToJson());
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsObject()
        {
            // Arrange
            var original = new ValidationException("Test message");
            var json = original.ToJson();

            // Act
            var result = ValidationExceptionJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original, result);
        }

        [Fact]
        public void FromJson_EmptyString_ReturnsNull()
        {
            // Act
            var result = ValidationExceptionJsonExtensions.FromJson(string.Empty);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_NullString_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ValidationExceptionJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndValue()
        {
            // Arrange
            var original = new ValidationException("Test message");
            var json = original.ToJson();

            // Act
            var success = ValidationExceptionJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
            Assert.Equal(original, result);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "this is not json";

            // Act
            var success = ValidationExceptionJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_NullString_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ValidationExceptionJsonExtensions.TryFromJson(null!, out _));
        }
    }
}
