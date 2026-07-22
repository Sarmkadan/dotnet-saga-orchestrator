using System;
using Xunit;
using FluentAssertions;
using Moq;
using SagaOrchestrator.Infrastructure.Debugging;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Tests for SagaDebuggerServiceJsonExtensionsJsonExtensions class.
/// Tests JSON serialization and deserialization behavior of SagaDebuggerService instances.
/// </summary>
public class SagaDebuggerServiceJsonExtensionsJsonExtensionsTests
{
    /// <summary>
    /// Verifies that ToJson serializes a SagaDebuggerService instance to JSON.
    /// </summary>
    [Fact]
    public void ToJson_ShouldSerializeValidJson()
    {
        // Arrange
        var service = new SagaDebuggerService(
            Mock.Of<ISagaRepository>(),
            Mock.Of<ISagaStepRepository>(),
            new SagaEventPublisher(),
            new DebuggerOptions { IsEnabled = true }
        );

        // Act
        var json = SagaDebuggerServiceJsonExtensionsJsonExtensions.ToJson(service);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("SagaDebuggerService");
    }

    /// <summary>
    /// Verifies that ToJson with indented parameter produces formatted JSON.
    /// </summary>
    [Fact]
    public void ToJson_WithIndentedTrue_ShouldProduceFormattedJson()
    {
        // Arrange
        var service = new SagaDebuggerService(
            Mock.Of<ISagaRepository>(),
            Mock.Of<ISagaStepRepository>(),
            new SagaEventPublisher(),
            new DebuggerOptions { IsEnabled = true }
        );

        // Act
        var json = SagaDebuggerServiceJsonExtensionsJsonExtensions.ToJson(service, indented: true);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("{\n"); // Should contain newlines and indentation
    }

    /// <summary>
    /// Verifies that ToJson with indented parameter produces compact JSON.
    /// </summary>
    [Fact]
    public void ToJson_WithIndentedFalse_ShouldProduceCompactJson()
    {
        // Arrange
        var service = new SagaDebuggerService(
            Mock.Of<ISagaRepository>(),
            Mock.Of<ISagaStepRepository>(),
            new SagaEventPublisher(),
            new DebuggerOptions { IsEnabled = true }
        );

        // Act
        var json = SagaDebuggerServiceJsonExtensionsJsonExtensions.ToJson(service, indented: false);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().NotContain("{\n"); // Should not contain newlines
    }

    /// <summary>
    /// Verifies that ToJson throws ArgumentNullException when value is null.
    /// </summary>
    [Fact]
    public void ToJson_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Arrange
        SagaDebuggerService? service = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => SagaDebuggerServiceJsonExtensionsJsonExtensions.ToJson(service!));
    }

    /// <summary>
    /// Verifies that FromJson throws ArgumentException when JSON is null or whitespace.
    /// </summary>
    [Fact]
    public void FromJson_WithNullOrWhitespaceJson_ShouldThrowArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => SagaDebuggerServiceJsonExtensionsJsonExtensions.FromJson(null!));
        Assert.Throws<ArgumentException>(() => SagaDebuggerServiceJsonExtensionsJsonExtensions.FromJson(""));
        Assert.Throws<ArgumentException>(() => SagaDebuggerServiceJsonExtensionsJsonExtensions.FromJson("   "));
    }

    /// <summary>
    /// Verifies that TryFromJson throws ArgumentException when JSON is null or whitespace.
    /// </summary>
    [Fact]
    public void TryFromJson_WithNullOrWhitespaceJson_ShouldThrowArgumentException()
    {
        // Arrange
        SagaDebuggerService? result = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SagaDebuggerServiceJsonExtensionsJsonExtensions.TryFromJson(null!, out result));
        Assert.Throws<ArgumentException>(() => SagaDebuggerServiceJsonExtensionsJsonExtensions.TryFromJson("", out result));
        Assert.Throws<ArgumentException>(() => SagaDebuggerServiceJsonExtensionsJsonExtensions.TryFromJson("   ", out result));
    }

    /// <summary>
    /// Verifies that TryFromJson returns false when JSON is invalid.
    /// </summary>
    [Fact]
    public void TryFromJson_WithInvalidJson_ShouldReturnFalseAndSetNull()
    {
        // Arrange
        var invalidJson = "{ invalid json {";
        SagaDebuggerService? result = null;

        // Act
        var success = SagaDebuggerServiceJsonExtensionsJsonExtensions.TryFromJson(invalidJson, out result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that TryFromJson returns false for JSON that can't be deserialized.
    /// </summary>
    [Fact]
    public void TryFromJson_WithUnDeserializableJson_ShouldReturnFalseAndSetNull()
    {
        // Arrange
        var json = "{\"some\": \"data\"}"; // Valid JSON but can't deserialize to SagaDebuggerService
        SagaDebuggerService? result = null;

        // Act
        var success = SagaDebuggerServiceJsonExtensionsJsonExtensions.TryFromJson(json, out result);

        // Assert
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies that JSON uses camelCase property naming policy.
    /// </summary>
    [Fact]
    public void ToJson_ShouldUseCamelCaseNamingPolicy()
    {
        // Arrange
        var service = new SagaDebuggerService(
            Mock.Of<ISagaRepository>(),
            Mock.Of<ISagaStepRepository>(),
            new SagaEventPublisher(),
            new DebuggerOptions { IsEnabled = true }
        );

        // Act
        var json = SagaDebuggerServiceJsonExtensionsJsonExtensions.ToJson(service);

        // Assert
        json.Should().Contain("snapshots"); // camelCase property name
        json.Should().NotContain("Snapshots"); // PascalCase should not be present
    }

    /// <summary>
    /// Verifies that ToJson produces non-empty output.
    /// </summary>
    [Fact]
    public void ToJson_ShouldProduceNonEmptyOutput()
    {
        // Arrange
        var service = new SagaDebuggerService(
            Mock.Of<ISagaRepository>(),
            Mock.Of<ISagaStepRepository>(),
            new SagaEventPublisher(),
            new DebuggerOptions { IsEnabled = true }
        );

        // Act
        var json = SagaDebuggerServiceJsonExtensionsJsonExtensions.ToJson(service);

        // Assert
        json.Should().NotBeEmpty();
        json.Length.Should().BeGreaterThan(10);
    }
}