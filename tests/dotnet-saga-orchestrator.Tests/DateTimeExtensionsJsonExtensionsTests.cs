using System;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Extensions;

namespace SagaOrchestrator.Tests
{
    public class DateTimeExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HappyPath_ReturnsJsonString()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            string json = now.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            // The JSON representation of a DateTime is a quoted string
            json.Should().StartWith("\"");
            json.Should().EndWith("\"");
        }

        [Fact]
        public void ToJson_Indented_ReturnsFormattedJsonString()
        {
            // Arrange
            var now = DateTime.UtcNow;

            // Act
            string json = now.ToJson(indented: true);

            // Assert
            json.Should().Contain("\n");
        }

        [Fact]
        public void FromJson_HappyPath_ReturnsSameDateTime()
        {
            // Arrange
            var original = new DateTime(2023, 8, 1, 12, 34, 56, DateTimeKind.Utc);
            string json = original.ToJson();

            // Act
            DateTime result = DateTimeExtensionsJsonExtensions.FromJson(json);

            // Assert
            result.Should().Be(original);
        }

        [Fact]
        public void FromJson_NullOrEmpty_ThrowsArgumentException()
        {
            // Null input
            Action actNull = () => DateTimeExtensionsJsonExtensions.FromJson(null!);
            actNull.Should().Throw<ArgumentException>();

            // Empty input
            Action actEmpty = () => DateTimeExtensionsJsonExtensions.FromJson(string.Empty);
            actEmpty.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            string invalidJson = "invalid-json";

            // Act
            Action act = () => DateTimeExtensionsJsonExtensions.FromJson(invalidJson);

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void TryFromJson_HappyPath_ReturnsTrueAndValue()
        {
            // Arrange
            var original = new DateTime(2023, 8, 1, 12, 34, 56, DateTimeKind.Utc);
            string json = original.ToJson();

            // Act
            bool success = DateTimeExtensionsJsonExtensions.TryFromJson(json, out DateTime result);

            // Assert
            success.Should().BeTrue();
            result.Should().Be(original);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndDefault()
        {
            // Arrange
            string invalidJson = "invalid-json";

            // Act
            bool success = DateTimeExtensionsJsonExtensions.TryFromJson(invalidJson, out DateTime result);

            // Assert
            success.Should().BeFalse();
            result.Should().Be(default);
        }

        [Fact]
        public void TryFromJson_NullOrEmpty_ThrowsArgumentException()
        {
            // Null input
            Action actNull = () => DateTimeExtensionsJsonExtensions.TryFromJson(null!, out _);
            actNull.Should().Throw<ArgumentException>();

            // Empty input
            Action actEmpty = () => DateTimeExtensionsJsonExtensions.TryFromJson(string.Empty, out _);
            actEmpty.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void BoundaryValues_ToJsonAndFromJson_Success()
        {
            // Arrange
            var min = DateTime.MinValue;
            var max = DateTime.MaxValue;

            // Act
            string minJson = min.ToJson();
            string maxJson = max.ToJson();

            DateTime minResult = DateTimeExtensionsJsonExtensions.FromJson(minJson);
            DateTime maxResult = DateTimeExtensionsJsonExtensions.FromJson(maxJson);

            // Assert
            minResult.Should().Be(min);
            maxResult.Should().Be(max);
        }
    }
}
