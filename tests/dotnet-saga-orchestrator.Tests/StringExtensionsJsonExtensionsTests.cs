using System;
using SagaOrchestrator.Core.Extensions;
using Xunit;
using System.Text.Json;

namespace SagaOrchestrator.Tests
{
    public sealed class StringExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidString_ReturnsJsonString()
        {
            // Arrange
            const string input = "myString";

            // Act
            string json = input.ToJson();

            // Assert
            Assert.Equal("\"myString\"", json);
        }

        [Fact]
        public void ToJson_WithNullString_ThrowsArgumentNullException()
        {
            // Arrange
            string input = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => input.ToJson());
        }

        [Fact]
        public void FromJson_WithValidJsonString_ReturnsString()
        {
            // Arrange
            const string json = "\"myString\"";

            // Act
            string result = StringExtensionsJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal("myString", result);
        }

        [Fact]
        public void FromJson_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            const string json = "{ invalid json }";

            // Act & Assert
            Assert.Throws<JsonException>(() => StringExtensionsJsonExtensions.FromJson(json));
        }

        [Fact]
        public void FromJson_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            string json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => StringExtensionsJsonExtensions.FromJson(json!));
        }

        [Fact]
        public void FromJson_WithEmptyInput_ThrowsArgumentException()
        {
            // Arrange
            const string json = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => StringExtensionsJsonExtensions.FromJson(json));
        }

        [Fact]
        public void TryFromJson_WithValidJsonString_ReturnsTrueAndString()
        {
            // Arrange
            const string json = "\"myString\"";

            // Act
            bool success = StringExtensionsJsonExtensions.TryFromJson(json, out string? result);

            // Assert
            Assert.True(success);
            Assert.Equal("myString", result);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            const string json = "{ invalid json }";

            // Act
            bool success = StringExtensionsJsonExtensions.TryFromJson(json, out string? result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_WithNullInput_ThrowsArgumentNullException()
        {
            // Arrange
            string json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => StringExtensionsJsonExtensions.TryFromJson(json!, out _));
        }

        [Fact]
        public void TryFromJson_WithEmptyInput_ThrowsArgumentException()
        {
            // Arrange
            const string json = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => StringExtensionsJsonExtensions.TryFromJson(json, out _));
        }
    }
}