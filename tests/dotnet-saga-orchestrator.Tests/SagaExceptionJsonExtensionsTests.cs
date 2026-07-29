using System;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class SagaExceptionJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ValidException_ReturnsJsonString()
        {
            // Arrange
            var exception = new SagaException("Test error", "saga123", "ERR001");

            // Act
            string json = SagaExceptionJsonExtensions.ToJson(exception);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"sagaId\":\"saga123\"");
            json.Should().Contain("\"errorCode\":\"ERR001\"");
        }

        [Fact]
        public void ToJson_Indented_ReturnsFormattedJsonString()
        {
            // Arrange
            var exception = new SagaException("Test error", "saga123", "ERR001");

            // Act
            string json = SagaExceptionJsonExtensions.ToJson(exception, indented: true);

            // Assert
            json.Should().Contain("\n");
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaExceptionJsonExtensions.ToJson(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_ValidJson_ThrowsNotSupportedException()
        {
            // Arrange
            var json = "{\"sagaId\":\"saga123\",\"errorCode\":\"ERR001\"}";

            // Act
            Action act = () => SagaExceptionJsonExtensions.FromJson(json);

            // Assert
            act.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void FromJson_EmptyJson_ReturnsNull()
        {
            // Act
            var exception = SagaExceptionJsonExtensions.FromJson("");

            // Assert
            exception.Should().BeNull();
        }

        [Fact]
        public void FromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaExceptionJsonExtensions.FromJson(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TryFromJson_ValidJson_ThrowsNotSupportedException()
        {
            // Arrange
            var json = "{\"sagaId\":\"saga123\",\"errorCode\":\"ERR001\"}";

            // Act
            Action act = () => SagaExceptionJsonExtensions.TryFromJson(json, out _);

            // Assert
            act.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var json = "invalid-json";

            // Act
            bool success = SagaExceptionJsonExtensions.TryFromJson(json, out var exception);

            // Assert
            success.Should().BeFalse();
            exception.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_NullInput_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaExceptionJsonExtensions.TryFromJson(null!, out _);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
