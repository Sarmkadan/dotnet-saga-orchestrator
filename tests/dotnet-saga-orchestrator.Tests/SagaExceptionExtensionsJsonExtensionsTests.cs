using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaExceptionExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_NullValue_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaExceptionExtensionsJsonExtensions.ToJson(null!);
            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ToJson_SimpleException_ReturnsJsonString()
        {
            // Arrange
            var sagaException = new SagaException("test message");

            // Act
            var json = SagaExceptionExtensionsJsonExtensions.ToJson(sagaException);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("test message");
        }

        [Fact]
        public void ToJson_WithIndentedTrue_ReturnsIndentedJson()
        {
            // Arrange
            var sagaException = new SagaException("test message");

            // Act
            var json = SagaExceptionExtensionsJsonExtensions.ToJson(sagaException, indented: true);

            // Assert
            json.Should().Contain("\n"); // indented JSON should have newline
            json.Should().Contain("test message");
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentException()
        {
            // Act
            Action act = () => SagaExceptionExtensionsJsonExtensions.FromJson(null!);
            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromJson_EmptyOrWhitespaceJson_ThrowsArgumentException()
        {
            // Act
            Action act1 = () => SagaExceptionExtensionsJsonExtensions.FromJson(string.Empty);
            Action act2 = () => SagaExceptionExtensionsJsonExtensions.FromJson("   ");
            Action act3 = () => SagaExceptionExtensionsJsonExtensions.FromJson("\t\n\r");
            // Assert
            act1.Should().Throw<ArgumentException>();
            act2.Should().Throw<ArgumentException>();
            act3.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsSagaException()
        {
            // Arrange
            var sagaException = new SagaException("test message", "saga123", "ERR001");
            var json = SagaExceptionExtensionsJsonExtensions.ToJson(sagaException);

            // Act
            var result = SagaExceptionExtensionsJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<SagaException>();
            result!.Message.Should().Be("test message");
            result.SagaId.Should().Be("saga123");
            result.ErrorCode.Should().Be("ERR001");
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "{invalid json";

            // Act
            Action act = () => SagaExceptionExtensionsJsonExtensions.FromJson(json);
            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentException()
        {
            // Act
            Action act = () => SagaExceptionExtensionsJsonExtensions.TryFromJson(null!, out _);
            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryFromJson_EmptyOrWhitespaceJson_ThrowsArgumentException()
        {
            // Act
            Action act1 = () => SagaExceptionExtensionsJsonExtensions.TryFromJson(string.Empty, out _);
            Action act2 = () => SagaExceptionExtensionsJsonExtensions.TryFromJson("   ", out _);
            Action act3 = () => SagaExceptionExtensionsJsonExtensions.TryFromJson("\t\n\r", out _);
            // Assert
            act1.Should().Throw<ArgumentException>();
            act2.Should().Throw<ArgumentException>();
            act3.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndValue()
        {
            // Arrange
            var sagaException = new SagaException("test message", "saga123", "ERR001");
            var json = SagaExceptionExtensionsJsonExtensions.ToJson(sagaException);

            // Act
            var success = SagaExceptionExtensionsJsonExtensions.TryFromJson(json, out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result.Should().BeOfType<SagaException>();
            result!.Message.Should().Be("test message");
            result.SagaId.Should().Be("saga123");
            result.ErrorCode.Should().Be("ERR001");
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "{invalid json";

            // Act
            var success = SagaExceptionExtensionsJsonExtensions.TryFromJson(json, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }
    }
}