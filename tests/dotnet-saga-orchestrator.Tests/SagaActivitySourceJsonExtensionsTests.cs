using System;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Infrastructure.Telemetry;

namespace SagaOrchestrator.Tests;

public class SagaActivitySourceJsonExtensionsTests
{
    [Fact]
    public void ToJson_ValidInput_ReturnsJsonString()
    {
        // Arrange
        string name = "test-activity";

        // Act
        string result = SagaActivitySourceJsonExtensions.ToJson(name);

        // Assert
        result.Should().Be("{\"name\":\"test-activity\"}");
    }

    [Fact]
    public void ToJson_EmptyString_ReturnsJsonWithEmptyName()
    {
        // Arrange
        string name = string.Empty;

        // Act
        string result = SagaActivitySourceJsonExtensions.ToJson(name);

        // Assert
        result.Should().Be("{\"name\":\"\"}");
    }

    [Fact]
    public void ToJson_IndentedTrue_ReturnsPrettyJson()
    {
        // Arrange
        string name = "pretty";

        // Act
        string result = SagaActivitySourceJsonExtensions.ToJson(name, indented: true);

        // Assert
        result.Should().Contain("\n");
        result.Should().Be("{\n  \"name\": \"pretty\"\n}");
    }

    [Fact]
    public void ToJson_NullInput_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => SagaActivitySourceJsonExtensions.ToJson(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsTelemetry()
    {
        // Arrange
        string json = "{\"name\":\"test-activity\"}";

        // Act
        var result = SagaActivitySourceJsonExtensions.FromJson(json);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("test-activity");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("null")]
    public void FromJson_EmptyOrWhitespace_ReturnsNull(string json)
    {
        // Act
        var result = SagaActivitySourceJsonExtensions.FromJson(json);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_NullString_ReturnsNull()
    {
        // Act
        var result = SagaActivitySourceJsonExtensions.FromJson(null!);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        string json = "invalid-json";

        // Act
        Action act = () => SagaActivitySourceJsonExtensions.FromJson(json);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndTelemetry()
    {
        // Arrange
        string json = "{\"name\":\"test-activity\"}";

        // Act
        bool success = SagaActivitySourceJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeTrue();
        result.Should().NotBeNull();
        result!.Name.Should().Be("test-activity");
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalse()
    {
        // Arrange
        string json = "invalid-json";

        // Act
        bool success = SagaActivitySourceJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("null")]
    public void TryFromJson_EmptyOrWhitespace_ReturnsFalse(string json)
    {
        // Act
        bool success = SagaActivitySourceJsonExtensions.TryFromJson(json, out var result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_NullString_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => SagaActivitySourceJsonExtensions.TryFromJson(null!, out _);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Telemetry_NamePropertyCanBeSetAndRead()
    {
        // Arrange
        var telemetry = new SagaActivitySourceJsonExtensions.SagaActivitySourceTelemetry();

        // Act
        telemetry.Name = "custom-name";

        // Assert
        telemetry.Name.Should().Be("custom-name");
    }
}
