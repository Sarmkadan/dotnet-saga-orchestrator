using System;
using System.Collections.Generic;
using Xunit;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Tests;

public class SagaCommandResultValidationTests
{
    [Fact]
    public void Validate_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var result = new SagaCommandResult
        {
            Message = "Operation successful",
            Timestamp = DateTime.UtcNow,
            RequestId = "req-123",
            Errors = new List<string>(),
            Success = true
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_Generic_HappyPath_ReturnsEmptyList()
    {
        // Arrange
        var result = new SagaCommandResult<int>
        {
            Message = "Data retrieved",
            Timestamp = DateTime.UtcNow,
            RequestId = "req-456",
            Errors = new List<string>(),
            Success = true,
            Data = 42
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_MultipleErrors_ReturnsAllProblems()
    {
        // Arrange
        var result = new SagaCommandResult
        {
            Message = "", // Invalid
            Timestamp = DateTime.Now, // Invalid (Local time)
            RequestId = null, // Invalid
            Errors = null, // Invalid
            Success = true
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.Contains("Message is required.", problems);
        Assert.Contains("Timestamp must be a valid UTC date.", problems);
        Assert.Contains("RequestId is required.", problems);
        Assert.Contains("Errors collection must not be null.", problems);
    }

    [Fact]
    public void Validate_SuccessTrueWithErrors_ReturnsProblem()
    {
        // Arrange
        var result = new SagaCommandResult
        {
            Message = "Partial failure",
            Timestamp = DateTime.UtcNow,
            RequestId = "req-789",
            Errors = new List<string> { "Error 1" },
            Success = true // Invalid state
        };

        // Act
        var problems = result.Validate();

        // Assert
        Assert.Contains("Success cannot be true when there are errors.", problems);
    }

    [Fact]
    public void IsValid_Valid_ReturnsTrue()
    {
        // Arrange
        var result = new SagaCommandResult
        {
            Message = "Ok",
            Timestamp = DateTime.UtcNow,
            RequestId = "req-1",
            Errors = new List<string>(),
            Success = true
        };

        // Act
        var isValid = result.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_Generic_Invalid_ReturnsFalse()
    {
        // Arrange
        var result = new SagaCommandResult<string>
        {
            Message = "Ok",
            Timestamp = DateTime.MinValue, // Invalid
            RequestId = "req-1",
            Errors = new List<string>(),
            Success = true,
            Data = "value"
        };

        // Act
        var isValid = result.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_Invalid_ThrowsArgumentException()
    {
        // Arrange
        var result = new SagaCommandResult
        {
            Message = "Ok",
            Timestamp = DateTime.UtcNow,
            RequestId = "", // Invalid
            Errors = new List<string>(),
            Success = true
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => result.EnsureValid());
    }

    [Fact]
    public void EnsureValid_Generic_Valid_DoesNotThrow()
    {
        // Arrange
        var result = new SagaCommandResult<object>
        {
            Message = "Ok",
            Timestamp = DateTime.UtcNow,
            RequestId = "req-1",
            Errors = new List<string>(),
            Success = true,
            Data = new object()
        };

        // Act & Assert
        result.EnsureValid(); // Should not throw
    }

    [Fact]
    public void Validate_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        SagaCommandResult? result = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => result.Validate());
    }
}
