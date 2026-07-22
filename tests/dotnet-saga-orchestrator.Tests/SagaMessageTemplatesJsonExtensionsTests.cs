using Xunit;
using FluentAssertions;
using SagaOrchestrator.Infrastructure.Messaging;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Tests for SagaMessageTemplatesJsonExtensions class.
/// </summary>
public class SagaMessageTemplatesJsonExtensionsTests
{
    #region ToJson Tests

    [Fact]
    public void ToJson_ShouldSerializeSimpleString()
    {
        // Arrange
        var message = "Hello World";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.ToJson(message);

        // Assert
        result.Should().Be("\"Hello World\"");
    }

    [Fact]
    public void ToJson_ShouldSerializeEmptyString()
    {
        // Arrange
        var message = "";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.ToJson(message);

        // Assert
        result.Should().Be("\"\"");
    }

    [Fact]
    public void ToJson_ShouldSerializeStringWithSpecialCharacters()
    {
        // Arrange
        var message = "Line 1\nLine 2\tTabbed";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.ToJson(message);

        // Assert
        result.Should().Be("\"Line 1\\nLine 2\\tTabbed\"");
    }

    [Fact]
    public void ToJson_ShouldSerializeStringWithQuotes()
    {
        // Arrange
        var message = "Message with \"quotes\"";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.ToJson(message);

        // Assert - JSON serializes quotes as Unicode escape sequence
        result.Should().Be("\"Message with \\u0022quotes\\u0022\"");
    }

    [Fact]
    public void ToJson_WithIndentedTrue_ShouldFormatWithIndentation()
    {
        // Arrange
        var message = "Hello World";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.ToJson(message, indented: true);

        // Assert - Even with indentation, single string values don't add newlines
        result.Should().Be("\"Hello World\"");
    }

    [Fact]
    public void ToJson_ShouldThrowArgumentNullException_WhenMessageIsNull()
    {
        // Arrange
        string? message = null;

        // Act
        Action act = () => SagaMessageTemplatesJsonExtensions.ToJson(message!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToJson_WithUnicodeCharacters_ShouldPreserveCharacters()
    {
        // Arrange
        var message = "Привет мир 你好世界 🌍";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.ToJson(message);

        // Assert - JSON may escape Unicode characters, so we check the deserialized value
        var deserialized = SagaMessageTemplatesJsonExtensions.FromJson(result);
        deserialized.Should().Be(message);
    }

    #endregion

    #region FromJson Tests

    [Fact]
    public void FromJson_ShouldDeserializeValidJsonString()
    {
        // Arrange
        var json = "\"Hello World\"";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.FromJson(json);

        // Assert
        result.Should().Be("Hello World");
    }

    [Fact]
    public void FromJson_ShouldDeserializeEmptyString()
    {
        // Arrange
        var json = "\"\"";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.FromJson(json);

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void FromJson_ShouldDeserializeStringWithSpecialCharacters()
    {
        // Arrange
        var json = "\"Line 1\\nLine 2\\tTabbed\"";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.FromJson(json);

        // Assert
        result.Should().Be("Line 1\nLine 2\tTabbed");
    }

    [Fact]
    public void FromJson_ShouldDeserializeStringWithQuotes()
    {
        // Arrange
        var json = "\"Message with \\\"quotes\\\"\"";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.FromJson(json);

        // Assert
        result.Should().Be("Message with \"quotes\"");
    }

    [Fact]
    public void FromJson_ShouldReturnNull_WhenJsonIsInvalid()
    {
        // Arrange
        var invalidJson = "not a valid json string";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.FromJson(invalidJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_ShouldReturnNull_WhenJsonIsMalformed()
    {
        // Arrange
        var malformedJson = "{\"key\": \"value\"}"; // Not a string value

        // Act
        var result = SagaMessageTemplatesJsonExtensions.FromJson(malformedJson);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void FromJson_ShouldThrowArgumentNullException_WhenJsonIsNull()
    {
        // Arrange
        string? json = null;

        // Act
        Action act = () => SagaMessageTemplatesJsonExtensions.FromJson(json!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromJson_WithUnicodeCharacters_ShouldPreserveCharacters()
    {
        // Arrange
        var json = "\"Привет мир 你好世界 🌍\"";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.FromJson(json);

        // Assert
        result.Should().Be("Привет мир 你好世界 🌍");
    }

    #endregion

    #region TryFromJson Tests

    [Fact]
    public void TryFromJson_ShouldReturnTrueAndValue_WhenJsonIsValid()
    {
        // Arrange
        var json = "\"Hello World\"";
        string? value = null;

        // Act
        var result = SagaMessageTemplatesJsonExtensions.TryFromJson(json, out value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be("Hello World");
    }

    [Fact]
    public void TryFromJson_ShouldReturnTrueAndEmptyString_WhenJsonIsEmptyString()
    {
        // Arrange
        var json = "\"\"";
        string? value = null;

        // Act
        var result = SagaMessageTemplatesJsonExtensions.TryFromJson(json, out value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be("");
    }

    [Fact]
    public void TryFromJson_ShouldReturnFalseAndNull_WhenJsonIsInvalid()
    {
        // Arrange
        var invalidJson = "not a valid json string";
        string? value = "default";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.TryFromJson(invalidJson, out value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_ShouldReturnFalseAndOriginalValue_WhenJsonIsMalformed()
    {
        // Arrange
        var malformedJson = "{\"key\": \"value\"}"; // Not a string value
        string? value = "original";

        // Act
        var result = SagaMessageTemplatesJsonExtensions.TryFromJson(malformedJson, out value);

        // Assert
        result.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void TryFromJson_ShouldThrowArgumentNullException_WhenJsonIsNull()
    {
        // Arrange
        string? json = null;
        string? value = null;

        // Act
        Action act = () => SagaMessageTemplatesJsonExtensions.TryFromJson(json!, out value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void TryFromJson_WithUnicodeCharacters_ShouldPreserveCharacters()
    {
        // Arrange
        var json = "\"Привет мир 你好世界 🌍\"";
        string? value = null;

        // Act
        var result = SagaMessageTemplatesJsonExtensions.TryFromJson(json, out value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be("Привет мир 你好世界 🌍");
    }

    #endregion

    #region Round-trip Tests

    [Fact]
    public void RoundTrip_ToJsonThenFromJson_ShouldPreserveOriginalValue()
    {
        // Arrange
        var originalMessage = "Test message with special chars: \n\t\"quotes\" and unicode: ñ";

        // Act
        var json = SagaMessageTemplatesJsonExtensions.ToJson(originalMessage);
        var deserialized = SagaMessageTemplatesJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().Be(originalMessage);
    }

    [Fact]
    public void RoundTrip_ToJsonThenTryFromJson_ShouldPreserveOriginalValue()
    {
        // Arrange
        var originalMessage = "Another test message";
        string? value = null;

        // Act
        var json = SagaMessageTemplatesJsonExtensions.ToJson(originalMessage);
        var result = SagaMessageTemplatesJsonExtensions.TryFromJson(json, out value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(originalMessage);
    }

    [Fact]
    public void RoundTrip_WithUnicodeCharacters_ShouldPreserveOriginalValue()
    {
        // Arrange
        var originalMessage = "Привет мир 你好世界 🌍";

        // Act
        var json = SagaMessageTemplatesJsonExtensions.ToJson(originalMessage);
        var deserialized = SagaMessageTemplatesJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().Be(originalMessage);
    }

    [Fact]
    public void RoundTrip_WithEmptyString_ShouldPreserveOriginalValue()
    {
        // Arrange
        var originalMessage = "";

        // Act
        var json = SagaMessageTemplatesJsonExtensions.ToJson(originalMessage);
        var deserialized = SagaMessageTemplatesJsonExtensions.FromJson(json);

        // Assert
        deserialized.Should().Be(originalMessage);
    }

    #endregion
}