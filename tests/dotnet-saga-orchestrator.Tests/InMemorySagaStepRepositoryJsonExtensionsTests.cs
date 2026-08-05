// Copyright (c) 2024
// SPDX-License-Identifier: MIT

using System;
using SagaOrchestrator.Data.Repositories;
using Xunit;

namespace SagaOrchestrator.Tests;

public sealed class InMemorySagaStepRepositoryJsonExtensionsTests
{
    [Fact]
    public void ToJson_WithValidRepository_ReturnsJsonObject()
    {
        // Arrange
        var repository = new InMemorySagaStepRepository();

        // Act
        string json = repository.ToJson();

        // Assert
        Assert.NotNull(json);
        Assert.StartsWith("{", json);
    }

    [Fact]
    public void ToJson_WithIndentation_ReturnsIndentedJson()
    {
        // Arrange
        var repository = new InMemorySagaStepRepository();

        // Act
        string json = repository.ToJson(indented: true);

        // Assert
        Assert.Contains("\n", json);
    }

    [Fact]
    public void ToJson_NullRepository_ThrowsArgumentNullException()
    {
        // Arrange
        InMemorySagaStepRepository? repository = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => repository!.ToJson());
    }

    [Fact]
    public void FromJson_ValidJson_ReturnsRepository()
    {
        // Arrange
        var repository = new InMemorySagaStepRepository();
        string json = repository.ToJson();

        // Act
        var result = InMemorySagaStepRepositoryJsonExtensions.FromJson(json);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<InMemorySagaStepRepository>(result);
    }

    [Fact]
    public void FromJson_NullOrEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => InMemorySagaStepRepositoryJsonExtensions.FromJson(json!));
        Assert.Throws<ArgumentException>(() => InMemorySagaStepRepositoryJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        const string json = "{ not a valid json }";

        // Act & Assert
        Assert.Throws<System.Text.Json.JsonException>(() => InMemorySagaStepRepositoryJsonExtensions.FromJson(json));
    }

    [Fact]
    public void TryFromJson_ValidJson_ReturnsTrueAndRepository()
    {
        // Arrange
        var repository = new InMemorySagaStepRepository();
        string json = repository.ToJson();

        // Act
        bool success = InMemorySagaStepRepositoryJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.True(success);
        Assert.NotNull(result);
    }

    [Fact]
    public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
    {
        // Arrange
        const string json = "not valid json at all";

        // Act
        bool success = InMemorySagaStepRepositoryJsonExtensions.TryFromJson(json, out var result);

        // Assert
        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryFromJson_NullOrEmptyJson_ThrowsArgumentException()
    {
        // Arrange
        string? json = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => InMemorySagaStepRepositoryJsonExtensions.TryFromJson(json!, out _));
        Assert.Throws<ArgumentException>(() => InMemorySagaStepRepositoryJsonExtensions.TryFromJson(string.Empty, out _));
    }
}
