using System;
using Xunit;
using SagaOrchestrator.Infrastructure.Caching;

namespace SagaOrchestrator.Tests
{
    public class CacheKeyBuilderValidationJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidInstance_ReturnsJson()
        {
            // Arrange
            var validation = new CacheKeyBuilderValidation();

            // Act
            var json = validation.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json);
            Assert.EndsWith("}", json);
        }

        [Fact]
        public void ToJson_WithIndentation_ReturnsIndentedJson()
        {
            // Arrange
            var validation = new CacheKeyBuilderValidation();

            // Act
            var json = validation.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json); // indented JSON contains line breaks
        }

        [Fact]
        public void ToJson_NullInstance_ThrowsArgumentNullException()
        {
            // Arrange
            CacheKeyBuilderValidation? validation = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => validation!.ToJson());
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsObject()
        {
            // Arrange
            var original = new CacheKeyBuilderValidation();
            var json = original.ToJson();

            // Act
            var result = CacheKeyBuilderValidationJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(original, result);
        }

        [Fact]
        public void FromJson_EmptyString_ReturnsNull()
        {
            // Act
            var result = CacheKeyBuilderValidationJsonExtensions.FromJson(string.Empty);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_NullString_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CacheKeyBuilderValidationJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndValue()
        {
            // Arrange
            var original = new CacheKeyBuilderValidation();
            var json = original.ToJson();

            // Act
            var success = CacheKeyBuilderValidationJsonExtensions.TryFromJson(json, out var result);

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
            var success = CacheKeyBuilderValidationJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_NullString_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CacheKeyBuilderValidationJsonExtensions.TryFromJson(null!, out _));
        }
    }
}
