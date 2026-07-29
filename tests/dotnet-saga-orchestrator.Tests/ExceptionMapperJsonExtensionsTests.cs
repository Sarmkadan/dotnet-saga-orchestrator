using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class ExceptionMapperJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithValidErrorResponse_ReturnsJsonString()
        {
            // Arrange
            var error = new ErrorResponse();

            // Act
            var json = error.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.StartsWith("{", json.TrimStart());
        }

        [Fact]
        public void ToJson_WithIndentation_ReturnsFormattedJson()
        {
            // Arrange
            var error = new ErrorResponse();

            // Act
            var json = error.ToJson(indented: true);

            // Assert
            Assert.Contains(Environment.NewLine, json);
        }

        [Fact]
        public void ToJson_NullErrorResponse_ThrowsArgumentNullException()
        {
            // Arrange
            ErrorResponse? error = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => error!.ToJson());
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsErrorResponse()
        {
            // Arrange
            var original = new ErrorResponse();
            var json = original.ToJson();

            // Act
            var result = ExceptionMapperJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void FromJson_NullOrEmptyInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ExceptionMapperJsonExtensions.FromJson(null!));
            Assert.Throws<ArgumentException>(() => ExceptionMapperJsonExtensions.FromJson(string.Empty));
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var invalidJson = "this is not json";

            // Act & Assert
            Assert.Throws<System.Text.Json.JsonException>(() => ExceptionMapperJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndValue()
        {
            // Arrange
            var original = new ErrorResponse();
            var json = original.ToJson();

            // Act
            var success = ExceptionMapperJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var invalidJson = "invalid";

            // Act
            var success = ExceptionMapperJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_NullOrEmptyInput_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ExceptionMapperJsonExtensions.TryFromJson(null!, out _));
            Assert.Throws<ArgumentException>(() => ExceptionMapperJsonExtensions.TryFromJson(string.Empty, out _));
        }
    }
}
