using System;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Infrastructure.Caching;

namespace SagaOrchestrator.Tests;

public class CacheKeyBuilderTests
{
    [Fact]
    public void BuildSagaKey_ValidInput_ReturnsExpectedKey()
    {
        // Arrange
        string sagaId = "saga123";
        string expected = "saga:saga123";

        // Act
        string actual = CacheKeyBuilder.BuildSagaKey(sagaId);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void BuildSagaKey_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string sagaId = null!;

        // Act
        Action act = () => CacheKeyBuilder.BuildSagaKey(sagaId!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void BuildDefinitionKey_ValidInput_ReturnsExpectedKey()
    {
        // Arrange
        string definitionId = "def456";
        string expected = "definition:def456";

        // Act
        string actual = CacheKeyBuilder.BuildDefinitionKey(definitionId);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void BuildDefinitionKey_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string definitionId = null!;

        // Act
        Action act = () => CacheKeyBuilder.BuildDefinitionKey(definitionId!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("definitionId");
    }

    [Fact]
    public void BuildAllSagasKey_ReturnsExpectedKey()
    {
        // Act
        string actual = CacheKeyBuilder.BuildAllSagasKey();

        // Assert
        actual.Should().Be("sagas:all");
    }

    [Fact]
    public void BuildSagasByStatusKey_ValidInput_ReturnsExpectedKey()
    {
        // Arrange
        string status = "Running";
        string expected = "sagas:status:Running";

        // Act
        string actual = CacheKeyBuilder.BuildSagasByStatusKey(status);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void BuildSagasByStatusKey_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string status = null!;

        // Act
        Action act = () => CacheKeyBuilder.BuildSagasByStatusKey(status!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("status");
    }

    [Fact]
    public void BuildCompensationKey_ValidInput_ReturnsExpectedKey()
    {
        // Arrange
        string sagaId = "saga789";
        string expected = "compensation:saga789";

        // Act
        string actual = CacheKeyBuilder.BuildCompensationKey(sagaId);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void BuildCompensationKey_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string sagaId = null!;

        // Act
        Action act = () => CacheKeyBuilder.BuildCompensationKey(sagaId!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void BuildEventHistoryKey_ValidInput_ReturnsExpectedKey()
    {
        // Arrange
        string sagaId = "sagaXYZ";
        string expected = "events:sagaXYZ";

        // Act
        string actual = CacheKeyBuilder.BuildEventHistoryKey(sagaId);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void BuildEventHistoryKey_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string sagaId = null!;

        // Act
        Action act = () => CacheKeyBuilder.BuildEventHistoryKey(sagaId!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void BuildServiceKey_ValidInput_ReturnsExpectedKey()
    {
        // Arrange
        string serviceName = "PaymentService";
        string expected = "service:PaymentService";

        // Act
        string actual = CacheKeyBuilder.BuildServiceKey(serviceName);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void BuildServiceKey_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        string serviceName = null!;

        // Act
        Action act = () => CacheKeyBuilder.BuildServiceKey(serviceName!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("serviceName");
    }

    [Fact]
    public void BuildHealthCheckKey_ReturnsExpectedKey()
    {
        // Act
        string actual = CacheKeyBuilder.BuildHealthCheckKey();

        // Assert
        actual.Should().Be("health:check");
    }
}