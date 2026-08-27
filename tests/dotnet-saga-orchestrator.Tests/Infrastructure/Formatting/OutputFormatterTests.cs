using Moq;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Infrastructure.Formatting;
using SagaOrchestrator.Infrastructure.Serialization;
using Xunit;

namespace SagaOrchestrator.Tests.Infrastructure.Formatting;

/// <summary>
/// Tests for the <see cref="OutputFormatter"/> class.
/// </summary>
public class OutputFormatterTests
{
    private readonly Mock<ISagaSerializer> _mockSerializer;
    private readonly OutputFormatter _formatter;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputFormatterTests"/> class with mocked dependencies.
    /// </summary>
    public OutputFormatterTests()
    {
        _mockSerializer = new Mock<ISagaSerializer>();
        _formatter = new OutputFormatter(_mockSerializer.Object);
    }

    /// <summary>
    /// Verifies that <see cref="OutputFormatter.FormatAsJson{T}"/> returns a serialized string for a generic object.
    /// </summary>
    [Fact]
    public void FormatAsJson_Generic_ReturnsSerializedString()
    {
        var data = new { Message = "Hello, World!" };
        _mockSerializer.Setup(s => s.SerializeIndented(data)).Returns("{\"Message\": \"Hello, World!\"}");

        var result = _formatter.FormatAsJson(data);

        result.Should().Be("{\"Message\": \"Hello, World!\"}");
    }

    /// <summary>
    /// Verifies that <see cref="OutputFormatter.FormatAsJson{T}"/> returns a serialized string for a <see cref="Saga"/> object.
    /// </summary>
    [Fact]
    public void FormatAsJson_Saga_ReturnsSerializedString()
    {
        var saga = new Saga { Id = "1" };
        _mockSerializer.Setup(s => s.SerializeIndented(saga)).Returns("{\"Id\": \"1\"}");

        var result = _formatter.FormatAsJson(saga);

        result.Should().Be("{\"Id\": \"1\"}");
    }

    /// <summary>
    /// Verifies that <see cref="OutputFormatter.FormatAsTable{T}"/> returns a "No sagas found." message when the input list is empty.
    /// </summary>
    [Fact]
    public void FormatAsTable_EmptyList_ReturnsNoSagasFoundMessage()
    {
        var result = _formatter.FormatAsTable(new List<Saga>());
        result.Should().Be("No sagas found.");
    }

    /// <summary>
    /// Verifies that <see cref="OutputFormatter.FormatAsTable{T}"/> returns a formatted table containing the saga ID and status for a non-empty list.
    /// </summary>
    [Fact]
    public void FormatAsTable_NormalList_ReturnsFormattedTable()
    {
        var sagas = new List<Saga>
        {
            new Saga { Id = "s1", Status = SagaStatus.Running, Steps = new List<SagaStep>() }
        };

        var result = _formatter.FormatAsTable(sagas);

        result.Should().Contain("s1");
        result.Should().Contain("Running");
    }

    /// <summary>
    /// Verifies that <see cref="OutputFormatter.FormatAsTable{T}"/> correctly handles special characters (like newline) in saga fields.
    /// </summary>
    [Fact]
    public void FormatAsTable_SpecialCharactersInFields_HandlesCorrectly()
    {
        var sagas = new List<Saga>
        {
            new Saga { Id = "s\n1", Status = SagaStatus.Running, Steps = new List<SagaStep>() }
        };

        var result = _formatter.FormatAsTable(sagas);

        result.Should().Contain("s");
        result.Should().Contain("1");
    }

    /// <summary>
    /// Verifies that <see cref="OutputFormatter.FormatAsCsv{T}"/> returns a formatted CSV string with the expected headers for a non-empty list.
    /// </summary>
    [Fact]
    public void FormatAsCsv_NormalList_ReturnsFormattedCsv()
    {
        var sagas = new List<Saga>
        {
            new Saga { Id = "s1", Status = SagaStatus.Running, Steps = new List<SagaStep>() }
        };

        var result = _formatter.FormatAsCsv(sagas);

        result.Should().Contain("Id,Name,Status,DefinitionId,CreatedAt,CompletedSteps,TotalSteps");
    }

    /// <summary>
    /// Verifies that <see cref="OutputFormatter.FormatAsCsv{T}"/> correctly escapes special characters (like newline) in CSV output.
    /// </summary>
    [Fact]
    public void FormatAsCsv_SpecialCharacters_EscapesCorrectly()
    {
        var sagas = new List<Saga>
        {
            new Saga { Id = "s\n1", Status = SagaStatus.Running, Steps = new List<SagaStep>() }
        };

        var result = _formatter.FormatAsCsv(sagas);

        result.Should().Contain("\"s\n1\"");
    }
}