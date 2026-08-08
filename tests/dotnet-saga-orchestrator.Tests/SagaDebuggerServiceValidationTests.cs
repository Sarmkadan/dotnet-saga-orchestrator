namespace SagaOrchestrator.Tests.Infrastructure.Debugging;

using System;
using System.Collections.Generic;
using System.Reflection;
using Moq;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Infrastructure.Debugging;
using Xunit;

public class SagaDebuggerServiceValidationTests
{
    private SagaDebuggerService CreateValidService()
    {
        var sagaRepository = Mock.Of<ISagaRepository>();
        var sagaStepRepository = Mock.Of<ISagaStepRepository>();
        var eventPublisher = new SagaEventPublisher();
        var options = new DebuggerOptions { IsEnabled = true };
        return new SagaDebuggerService(sagaRepository, sagaStepRepository, eventPublisher, options);
    }

    [Fact]
    public void Validate_ValidService_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateValidService();

        // Act
        var problems = SagaDebuggerServiceValidation.Validate(service);

        // Assert
        Assert.Empty(problems);
    }

    [Fact]
    public void IsValid_ValidService_ReturnsTrue()
    {
        // Arrange
        var service = CreateValidService();

        // Act
        var isValid = SagaDebuggerServiceValidation.IsValid(service);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_ValidService_DoesNotThrow()
    {
        // Arrange
        var service = CreateValidService();

        // Act
        var exception = Record.Exception(() => SagaDebuggerServiceValidation.EnsureValid(service));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NullService_ThrowsArgumentNullException()
    {
        // Act
        var exception = Record.Exception(() => SagaDebuggerServiceValidation.Validate(null!));

        // Assert
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void IsValid_NullService_ThrowsArgumentNullException()
    {
        // Act
        var exception = Record.Exception(() => SagaDebuggerServiceValidation.IsValid(null!));

        // Assert
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void EnsureValid_NullService_ThrowsArgumentNullException()
    {
        // Act
        var exception = Record.Exception(() => SagaDebuggerServiceValidation.EnsureValid(null!));

        // Assert
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Validate_SnapshotIndexContainsNull_ReturnsProblem()
    {
        // Arrange
        var service = CreateValidService();
        var snapshotIndexField = typeof(SagaDebuggerService).GetField("_snapshotIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        var snapshotIndex = new Dictionary<string, SagaDebugSnapshot> { { "key", null! } };
        snapshotIndexField.SetValue(service, snapshotIndex);

        // Act
        var problems = SagaDebuggerServiceValidation.Validate(service);

        // Assert
        Assert.Contains(problems, p => p.Contains("Snapshot index contains null entry"));
    }

    [Fact]
    public void Validate_BreakpointsContainsNullList_ReturnsProblem()
    {
        // Arrange
        var service = CreateValidService();
        var breakpointsField = typeof(SagaDebuggerService).GetField("_breakpoints", BindingFlags.NonPublic | BindingFlags.Instance);
        var breakpoints = new Dictionary<string, List<SagaDebugBreakpoint>> { { "saga1", null! } };
        breakpointsField.SetValue(service, breakpoints);

        // Act
        var problems = SagaDebuggerServiceValidation.Validate(service);

        // Assert
        Assert.Contains(problems, p => p.Contains("Null breakpoint list found in _breakpoints dictionary"));
    }
}