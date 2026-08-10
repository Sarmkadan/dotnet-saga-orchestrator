// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using SagaOrchestrator.Core.Builders;
using SagaOrchestrator.Core.Domain.Models;
using Xunit;

namespace SagaOrchestrator.Tests;

public sealed class SagaStepBuilderJsonExtensionsTests
{
    private static SagaStepBuilder CreateValidBuilder()
    {
        return SagaStepBuilder.Create("TestStep", "TestService", "https://example.com/action")
            .WithOrder(1)
            .WithTimeout(30)
            .WithRetryPolicy(3, 100)
            .WithMetadata("test-key", "test-value");
    }

    [Fact]
    public void ToJson_WithValidBuilder_ReturnsJsonString()
    {
        // Arrange
        var builder = CreateValidBuilder();

        // Act
        string json = builder.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\"name\":\"TestStep\"", json);
        Assert.Contains("\"serviceName\":\"TestService\"", json);
        Assert.Contains("\"serviceUrl\":\"https://example.com/action\"", json);
        Assert.Contains("\"order\":1", json);
        Assert.Contains("\"timeoutSeconds\":30", json);
        Assert.Contains("\"maxRetries\":3", json);
        Assert.Contains("\"retryDelayMilliseconds\":100", json);
        Assert.Contains("\"test-key\":\"test-value\"", json);
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ReturnsIndentedJson()
    {
        // Arrange
        var builder = CreateValidBuilder();

        // Act
        string json = builder.ToJson(indented: true);

        // Assert
        Assert.NotNull(json);
        Assert.Contains("\n", json);
        Assert.Contains("\"name\": \"TestStep\"", json);
    }

    [Fact]
    public void ToJson_NullBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        SagaStepBuilder builder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => builder.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsBuilder()
    {
        // Arrange
        var originalBuilder = CreateValidBuilder();
        string json = originalBuilder.ToJson();

        // Act
        SagaStepBuilder? result = SagaStepBuilderJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalBuilder.Build().Name, result?.Build().Name);
        Assert.Equal(originalBuilder.Build().ServiceName, result?.Build().ServiceName);
        Assert.Equal(originalBuilder.Build().ServiceUrl, result?.Build().ServiceUrl);
        Assert.Equal(originalBuilder.Build().Order, result?.Build().Order);
        Assert.Equal(originalBuilder.Build().TimeoutSeconds, result?.Build().TimeoutSeconds);
        Assert.Equal(originalBuilder.Build().MaxRetries, result?.Build().MaxRetries);
        Assert.Equal(originalBuilder.Build().RetryDelayMilliseconds, result?.Build().RetryDelayMilliseconds);
        Assert.Equal(originalBuilder.Build().Metadata["test-key"], result?.Build().Metadata["test-key"]);
    }

    [Fact]
    public void FromJson_EmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string json = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SagaStepBuilderJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SagaStepBuilderJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Arrange
        string json = "invalid json";

        // Act
        SagaStepBuilder? result = SagaStepBuilderJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_WhitespaceJson_ReturnsNull()
    {
        // Arrange
        string json = "   ";

        // Act
        SagaStepBuilder? result = SagaStepBuilderJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndBuilder()
    {
        // Arrange
        var originalBuilder = CreateValidBuilder();
        string json = originalBuilder.ToJson();

        // Act
        bool success = SagaStepBuilderJsonExtensions.TryFromJson(json, out SagaStepBuilder? result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(originalBuilder.Build().Name, result?.Build().Name);
        Assert.Equal(originalBuilder.Build().ServiceName, result?.Build().ServiceName);
        Assert.Equal(originalBuilder.Build().ServiceUrl, result?.Build().ServiceUrl);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        string json = "invalid json";

        // Act
        bool success = SagaStepBuilderJsonExtensions.TryFromJson(json, out SagaStepBuilder? result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullJson_ThrowsArgumentException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SagaStepBuilderJsonExtensions.TryFromJson(json!, out _));
    }

    [Fact]
    public void TryFromJson_EmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string json = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SagaStepBuilderJsonExtensions.TryFromJson(json, out _));
    }
}