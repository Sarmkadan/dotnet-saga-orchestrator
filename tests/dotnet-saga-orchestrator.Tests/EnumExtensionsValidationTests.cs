using System;
using System.Collections.Generic;
using System.ComponentModel;
using SagaOrchestrator.Core.Extensions;
using Xunit;

namespace SagaOrchestrator.Tests;

public enum ValidEnum
{
    [Description("First")]
    First = 1,
    [Description("Second")]
    Second = 2
}

public enum DefaultValueEnum
{
    [Description("Default")]
    Default = 0,
    [Description("First")]
    First = 1
}

public enum MissingDescriptionEnum
{
    NoDescription = 1
}

public sealed class EnumExtensionsValidationTests
{
    [Fact]
    public void Validate_ValidValue_ReturnsEmptyList()
    {
        // Act
        var problems = ValidEnum.First.Validate();

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_DefaultValue_ReturnsProblem()
    {
        // Act
        var problems = DefaultValueEnum.Default.Validate();

        // Assert
        Assert.NotEmpty(problems);
        Assert.Contains(problems, p => p.Contains("default value"));
    }

    [Fact]
    public void Validate_MissingDescription_ReturnsProblem()
    {
        // Act
        var problems = MissingDescriptionEnum.NoDescription.Validate();

        // Assert
        Assert.NotEmpty(problems);
        Assert.Contains(problems, p => p.Contains("does not have a Description attribute"));
    }

    [Fact]
    public void IsValid_ValidValue_ReturnsTrue()
    {
        // Act
        bool isValid = ValidEnum.First.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_InvalidValue_ReturnsFalse()
    {
        // Act
        bool isValid = MissingDescriptionEnum.NoDescription.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void EnsureValid_ValidValue_ReturnsValue()
    {
        // Act
        var result = ValidEnum.First.EnsureValid();

        // Assert
        Assert.Equal(ValidEnum.First, result);
    }

    [Fact]
    public void EnsureValid_InvalidValue_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => MissingDescriptionEnum.NoDescription.EnsureValid());
    }
}
