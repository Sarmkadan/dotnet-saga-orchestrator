#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Domain.Models;
using FluentAssertions;
using Xunit;

namespace dotnet_saga_orchestrator.Tests;

/// <summary>
/// Contains unit tests for validating <see cref="SagaEvent"/> instances.
/// </summary>
public class SagaEventValidationTests
{
    /// <summary>
    /// Creates a valid <see cref="SagaEvent"/> instance for testing.
    /// </summary>
    /// <returns>A valid <see cref="SagaEvent"/> instance.</returns>
    private static SagaEvent CreateValidEvent() =>
        new()
        {
            Id = Guid.NewGuid().ToString(),
            SagaId = Guid.NewGuid().ToString(),
            EventType = "TestEvent",
            EventName = "Test Event",
            Description = "This is a test event description."
        };

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an empty list for a valid event.
    /// </summary>
    [Fact]
    public void Validate_ValidEvent_ReturnsEmptyList()
    {
        // Arrange
        var @event = CreateValidEvent();

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> throws <see cref="ArgumentNullException"/> when the event is null.
    /// </summary>
    [Fact]
    public void Validate_NullEvent_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => SagaEventValidation.Validate(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("value");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an error when Id is null or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_IdIsNullOrWhitespace_ReturnsError(string? id)
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.Id = id!;

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("SagaEvent.Id cannot be null or whitespace.");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an error when Id exceeds 100 characters.
    /// </summary>
    [Fact]
    public void Validate_IdExceedsMaxLength_ReturnsError()
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.Id = new string('a', 101);

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("SagaEvent.Id cannot exceed 100 characters.");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an error when SagaId is null or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_SagaIdIsNullOrWhitespace_ReturnsError(string? sagaId)
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.SagaId = sagaId!;

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("SagaEvent.SagaId cannot be null or whitespace.");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an error when SagaId exceeds 100 characters.
    /// </summary>
    [Fact]
    public void Validate_SagaIdExceedsMaxLength_ReturnsError()
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.SagaId = new string('b', 101);

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("SagaEvent.SagaId cannot exceed 100 characters.");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an error when EventType is null or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EventTypeIsNullOrWhitespace_ReturnsError(string? eventType)
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.EventType = eventType!;

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("SagaEvent.EventType cannot be null or whitespace.");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an error when EventType exceeds 50 characters.
    /// </summary>
    [Fact]
    public void Validate_EventTypeExceedsMaxLength_ReturnsError()
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.EventType = new string('c', 51);

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("SagaEvent.EventType cannot exceed 50 characters.");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an error when EventName is null or whitespace.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EventNameIsNullOrWhitespace_ReturnsError(string? eventName)
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.EventName = eventName!;

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("SagaEvent.EventName cannot be null or whitespace.");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an error when EventName exceeds 100 characters.
    /// </summary>
    [Fact]
    public void Validate_EventNameExceedsMaxLength_ReturnsError()
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.EventName = new string('d', 101);

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("SagaEvent.EventName cannot exceed 100 characters.");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.Validate(SagaEvent)"/> returns an error when Description is null.
    /// </summary>
    [Fact]
    public void Validate_DescriptionIsNull_ReturnsError()
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.Description = null!;

        // Act
        var result = SagaEventValidation.Validate(@event);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("SagaEvent.Description cannot be null.");
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.IsValid(SagaEvent)"/> returns true for a valid event.
    /// </summary>
    [Fact]
    public void IsValid_ValidEvent_ReturnsTrue()
    {
        // Arrange
        var @event = CreateValidEvent();

        // Act
        var result = SagaEventValidation.IsValid(@event);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.IsValid(SagaEvent)"/> returns false for an invalid event.
    /// </summary>
    [Fact]
    public void IsValid_InvalidEvent_ReturnsFalse()
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.Id = "";

        // Act
        var result = SagaEventValidation.IsValid(@event);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.EnsureValid(SagaEvent)"/> does not throw for a valid event.
    /// </summary>
    [Fact]
    public void EnsureValid_ValidEvent_DoesNotThrow()
    {
        // Arrange
        var @event = CreateValidEvent();

        // Act
        Action act = () => SagaEventValidation.EnsureValid(@event);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="SagaEventValidation.EnsureValid(SagaEvent)"/> throws <see cref="ArgumentException"/> for an invalid event.
    /// </summary>
    [Fact]
    public void EnsureValid_InvalidEvent_ThrowsArgumentException()
    {
        // Arrange
        var @event = CreateValidEvent();
        @event.Id = "";

        // Act
        Action act = () => SagaEventValidation.EnsureValid(@event);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*SagaEvent.Id cannot be null or whitespace*");
    }
}