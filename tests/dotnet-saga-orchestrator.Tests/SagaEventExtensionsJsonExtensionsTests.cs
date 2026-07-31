using System;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Tests
{
    public class SagaEventExtensionsJsonExtensionsTests
    {
        private static SagaEventExtensions CreateSample()
            => SagaEventExtensions.CreateDefault();

        [Fact]
        public void ToJson_WithValidObject_ReturnsJson()
        {
            // Arrange
            var evt = CreateSample();

            // Act
            var json = evt.ToJson();

            // Assert
            json.Should().NotBeNullOrWhiteSpace();
            // round‑trip check
            var deserialized = SagaEventExtensionsJsonExtensions.FromJson(json);
            deserialized.Should().Be(evt);
        }

        [Fact]
        public void ToJson_WithIndentation_ProducesMultilineJson()
        {
            // Arrange
            var evt = CreateSample();

            // Act
            var json = evt.ToJson(indented: true);

            // Assert
            json.Should().Contain(Environment.NewLine);
        }

        [Fact]
        public void ToJson_NullArgument_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => ((SagaEventExtensions?)null)!.ToJson();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            var original = CreateSample();
            var json = original.ToJson();

            // Act
            var result = SagaEventExtensionsJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.Should().Be(original);
        }

        [Fact]
        public void FromJson_NullOrEmptyJson_ThrowsArgumentException()
        {
            // Act
            Action actNull = () => SagaEventExtensionsJsonExtensions.FromJson(null!);
            Action actEmpty = () => SagaEventExtensionsJsonExtensions.FromJson(string.Empty);

            // Assert
            actNull.Should().Throw<ArgumentException>();
            actEmpty.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndOutValue()
        {
            // Arrange
            var original = CreateSample();
            var json = original.ToJson();

            // Act
            var success = SagaEventExtensionsJsonExtensions.TryFromJson(json, out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result!.Should().Be(original);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNullOut()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";

            // Act
            var success = SagaEventExtensionsJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_NullOrEmptyJson_ThrowsArgumentException()
        {
            // Act
            Action actNull = () => SagaEventExtensionsJsonExtensions.TryFromJson(null!, out _);
            Action actEmpty = () => SagaEventExtensionsJsonExtensions.TryFromJson(string.Empty, out _);

            // Assert
            actNull.Should().Throw<ArgumentException>();
            actEmpty.Should().Throw<ArgumentException>();
        }
    }
}
