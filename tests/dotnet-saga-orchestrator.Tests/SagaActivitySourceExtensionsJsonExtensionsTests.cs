using System;
using System.Text.Json;
using FluentAssertions;
using Xunit;
using SagaOrchestrator.Infrastructure.Telemetry;

namespace SagaOrchestrator.Tests
{
    public class SagaActivitySourceExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_NullValue_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => SagaActivitySourceExtensionsJsonExtensions.ToJson(null!);
            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ToJson_SimpleObject_ReturnsJsonString()
        {
            // Arrange
            var obj = new { Name = "Test", Value = 42 };
            // Act
            var json = SagaActivitySourceExtensionsJsonExtensions.ToJson(obj);
            // Assert
            json.Should().Be("{\"name\":\"Test\",\"value\":42}");
        }

        [Fact]
        public void ToJson_WithIndentedTrue_ReturnsIndentedJson()
        {
            // Arrange
            var obj = new { Name = "Test", Value = 42 };
            // Act
            var json = SagaActivitySourceExtensionsJsonExtensions.ToJson(obj, indented: true);
            // Assert
            json.Should().Contain("\n"); // indented JSON should have newline
            json.Should().Be("{\n  \"name\": \"Test\",\n  \"value\": 42\n}");
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            Action act = () => SagaActivitySourceExtensionsJsonExtensions.FromJson(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_EmptyOrWhitespaceJson_ReturnsNull()
        {
            // Act
            var result1 = SagaActivitySourceExtensionsJsonExtensions.FromJson(string.Empty);
            var result2 = SagaActivitySourceExtensionsJsonExtensions.FromJson("   ");
            var result3 = SagaActivitySourceExtensionsJsonExtensions.FromJson("\t\n\r");
            // Assert
            result1.Should().BeNull();
            result2.Should().BeNull();
            result3.Should().BeNull();
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsObject()
        {
            // Arrange
            var json = "{\"name\":\"test\",\"value\":42}";
            // Act
            var result = SagaActivitySourceExtensionsJsonExtensions.FromJson(json);
            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<JsonElement>();
            var element = result as JsonElement?;
            element.Should().NotBeNull();
            element!.Value.GetProperty("name").GetString().Should().Be("test");
            element.Value.GetProperty("value").GetInt32().Should().Be(42);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "{invalid json";
            // Act
            Action act = () => SagaActivitySourceExtensionsJsonExtensions.FromJson(json);
            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            Action act = () => SagaActivitySourceExtensionsJsonExtensions.TryFromJson(null!, out _);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TryFromJson_EmptyOrWhitespaceJson_ReturnsFalse()
        {
            // Act
            var result1 = SagaActivitySourceExtensionsJsonExtensions.TryFromJson(string.Empty, out var value1);
            var result2 = SagaActivitySourceExtensionsJsonExtensions.TryFromJson("   ", out var value2);
            // Assert
            result1.Should().BeFalse();
            value1.Should().BeNull();
            result2.Should().BeFalse();
            value2.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndValue()
        {
            // Arrange
            var json = "{\"name\":\"test\",\"value\":42}";
            // Act
            var success = SagaActivitySourceExtensionsJsonExtensions.TryFromJson(json, out var value);
            // Assert
            success.Should().BeTrue();
            value.Should().NotBeNull();
            value.Should().BeOfType<JsonElement>();
            var element = value as JsonElement?;
            element.Should().NotBeNull();
            element!.Value.GetProperty("name").GetString().Should().Be("test");
            element.Value.GetProperty("value").GetInt32().Should().Be(42);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var json = "{invalid json";
            // Act
            var success = SagaActivitySourceExtensionsJsonExtensions.TryFromJson(json, out var value);
            // Assert
            success.Should().BeFalse();
            value.Should().BeNull();
        }
    }
}