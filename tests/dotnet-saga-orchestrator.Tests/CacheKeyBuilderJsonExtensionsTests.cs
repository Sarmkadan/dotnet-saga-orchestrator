// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using SagaOrchestrator.Infrastructure.Caching;
using Xunit;

namespace SagaOrchestrator.Tests;

public sealed class CacheKeyBuilderJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidKey_ReturnsCamelCasedJson()
    {
        // Arrange
        const string key = "myCacheKey";

        // Act
        string json = key.ToJson();

        // Assert
        Assert.Equal("{\"key\":\"myCacheKey\"}", json);
    }

    [Fact]
    public void ToJson_WithIndentation_ReturnsIndentedJson()
    {
        // Arrange
        const string key = "indentedKey";

        // Act
        string json = key.ToJson(indented: true);

        // Assert
        // The indented format contains a newline and spaces.
        Assert.Contains("\n", json);
        Assert.Contains("\"key\": \"indentedKey\"", json);
    }

    [Fact]
    public void ToJson_NullKey_ThrowsArgumentNullException()
    {
        // Arrange
        string? key = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => key!.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsKey()
    {
        // Arrange
        const string json = "{\"key\":\"expectedKey\"}";

        // Act
        string? result = CacheKeyBuilderJsonExtensions.FromJson(json);

        // Assert
        Assert.Equal("expectedKey", result);
    }

    [Fact]
    public void FromJson_WhitespaceJson_ReturnsNull()
    {
        // Arrange
        const string json = "   ";

        // Act
        string? result = CacheKeyBuilderJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_InvalidJson_ReturnsNull()
    {
        // Arrange
        const string json = "{ not a valid json }";

        // Act
        string? result = CacheKeyBuilderJsonExtensions.FromJson(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromJson_NullJson_ThrowsArgumentNullException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CacheKeyBuilderJsonExtensions.FromJson(json!));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndKey()
    {
        // Arrange
        const string json = "{\"key\":\"tryKey\"}";

        // Act
        bool success = CacheKeyBuilderJsonExtensions.TryFromJson(json, out string? key);

        // Assert
        Assert.True(success);
        Assert.Equal("tryKey", key);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        const string json = "invalid";

        // Act
        bool success = CacheKeyBuilderJsonExtensions.TryFromJson(json, out string? key);

        // Assert
        Assert.False(success);
        Assert.Null(key);
    }

    [Fact]
    public void CacheKey_Constructor_NullKey_ThrowsArgumentNullException()
    {
        // Arrange
        string? key = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CacheKey(key!));
    }

    [Fact]
    public void CacheKey_Property_GetSet_Works()
    {
        // Arrange
        var cacheKey = new CacheKey("initial");

        // Act
        cacheKey.Key = "updated";

        // Assert
        Assert.Equal("updated", cacheKey.Key);
    }
}
