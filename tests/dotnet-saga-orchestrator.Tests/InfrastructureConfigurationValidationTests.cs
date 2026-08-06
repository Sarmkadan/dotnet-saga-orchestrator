using System;
using SagaOrchestrator.Configuration;
using Xunit;

namespace SagaOrchestrator.Tests;

public class InfrastructureConfigurationValidationTests
{
    [Fact]
    public void Validate_Null_ThrowsArgumentNullException()
    {
        // Arrange
        InfrastructureConfiguration? config = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => InfrastructureConfigurationValidation.Validate(config));
    }

    [Fact]
    public void Validate_ValidConfiguration_ReturnsEmptyList()
    {
        // Arrange
        var config = new InfrastructureConfiguration();

        // Act
        var result = InfrastructureConfigurationValidation.Validate(config);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void IsValid_Null_ReturnsFalse()
    {
        // Arrange
        InfrastructureConfiguration? config = null;

        // Act
        var result = InfrastructureConfigurationValidation.IsValid(config);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var config = new InfrastructureConfiguration();

        // Act
        var result = InfrastructureConfigurationValidation.IsValid(config);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void EnsureValid_Null_ThrowsArgumentNullException()
    {
        // Arrange
        InfrastructureConfiguration? config = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => InfrastructureConfigurationValidation.EnsureValid(config));
    }

    [Fact]
    public void EnsureValid_ValidConfiguration_DoesNotThrow()
    {
        // Arrange
        var config = new InfrastructureConfiguration();

        // Act
        var exception = Record.Exception(() => InfrastructureConfigurationValidation.EnsureValid(config));

        // Assert
        Assert.Null(exception);
    }
}