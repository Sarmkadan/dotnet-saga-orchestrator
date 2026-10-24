using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests;

public class SagaExceptionValidationTests
{
    [Fact]
    public void Validate_WithValidException_ReturnsEmptyList()
    {
        // Arrange
        var exception = new SagaException("Test message");

        // Act
        var problems = SagaExceptionValidation.Validate(exception);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_WithEmptyMessage_ReturnsProblem()
    {
        // Arrange
        var exception = new SagaException(string.Empty);

        // Act
        var problems = SagaExceptionValidation.Validate(exception);

        // Assert
        Assert.Single(problems);
        Assert.Equal("Message must be non-null and non-empty.", problems[0]);
    }

    [Fact]
    public void Constructor_WithNullMessage_ThrowsArgumentNullException()
    {
        // The constructor rejects a null message, so an exception with a null message cannot be built
        // through the public API and the validator's null-message branch is not reachable from here.
        var act = () => new SagaException(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void IsValid_WithValidException_ReturnsTrue()
    {
        // Arrange
        var exception = new SagaException("Test message");

        // Act
        var isValid = SagaExceptionValidation.IsValid(exception);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WithInvalidException_ReturnsFalse()
    {
        // Arrange
        var exception = new SagaException(string.Empty);

        // Act
        var isValid = SagaExceptionValidation.IsValid(exception);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_WithValidException_DoesNotThrow()
    {
        // Arrange
        var exception = new SagaException("Test message");

        // Act & Assert
        SagaExceptionValidation.EnsureValid(exception);
    }

    [Fact]
    public void EnsureValid_WithInvalidException_ThrowsArgumentException()
    {
        // Arrange
        var exception = new SagaException(string.Empty);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => SagaExceptionValidation.EnsureValid(exception));
    }
}
