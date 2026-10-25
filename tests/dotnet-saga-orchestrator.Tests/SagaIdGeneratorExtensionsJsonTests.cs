#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using FluentAssertions;
using SagaOrchestrator.Core.Utilities;
using Xunit;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Tests for the internal <c>SagaIdGeneratorExtensionsJson</c> helpers. The class is internal and the
/// assembly exposes no InternalsVisibleTo, so the methods are invoked through reflection.
/// </summary>
public class SagaIdGeneratorExtensionsJsonTests
{
    private static readonly Type _extensionsType = typeof(SagaIdGenerator).Assembly.GetType(
        "SagaOrchestrator.Core.Utilities.SagaIdGeneratorExtensionsJson", throwOnError: true)!;

    [Fact]
    public void ToJson_WritesCamelCaseSagaIdProperty()
    {
        SerializeId("saga_123").Should().Be("{\"sagaId\":\"saga_123\"}");
    }

    [Fact]
    public void ToJson_Indented_WritesMultiLineJsonThatRoundTrips()
    {
        var json = SerializeId("saga_123", indented: true);

        json.Should().Contain("\n");
        DeserializeId(json).Should().Be("saga_123");
    }

    [Fact]
    public void ToJson_NullValue_ThrowsArgumentNullException()
    {
        Action act = () => Invoke("ToJson", (string?)null, false);

        act.Should().Throw<ArgumentNullException>().WithParameterName("value");
    }

    [Theory]
    [InlineData("")]
    [InlineData("saga_1a2b")]
    [InlineData("quote\"and\\backslash")]
    public void ToJson_ThenFromJson_PreservesValue(string value)
    {
        DeserializeId(SerializeId(value)).Should().Be(value);
    }

    [Fact]
    public void FromJson_AndTryFromJson_NullJson_ThrowArgumentNullException()
    {
        Action fromJson = () => Invoke("FromJson", (string?)null);
        Action tryFromJson = () => TryDeserializeId(null!, out _);

        fromJson.Should().Throw<ArgumentNullException>().WithParameterName("json");
        tryFromJson.Should().Throw<ArgumentNullException>().WithParameterName("json");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("null")]
    [InlineData("{\"other\":\"x\"}")]
    [InlineData("{\"SagaId\":\"saga_1\"}")]
    public void FromJson_BlankMalformedOrMissingProperty_ReturnsNull(string json)
    {
        DeserializeId(json).Should().BeNull();
    }

    [Fact]
    public void TryFromJson_ValidPayload_ReturnsTrueWithId()
    {
        TryDeserializeId("{\"sagaId\":\"saga_abc\"}", out var value).Should().BeTrue();
        value.Should().Be("saga_abc");

        TryDeserializeId("{}", out var empty).Should().BeTrue();
        empty.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t ")]
    [InlineData("{")]
    [InlineData("not json")]
    public void TryFromJson_BlankOrMalformedJson_ReturnsFalseAndNullValue(string json)
    {
        TryDeserializeId(json, out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    private static string SerializeId(string value, bool indented = false) =>
        (string)Invoke("ToJson", value, indented)!;

    private static string? DeserializeId(string json) =>
        (string?)Invoke("FromJson", json);

    private static bool TryDeserializeId(string json, out string? value)
    {
        var args = new object?[] { json, null };
        var succeeded = (bool)Invoke("TryFromJson", args)!;
        value = (string?)args[1];
        return succeeded;
    }

    private static object? Invoke(string methodName, params object?[] args)
    {
        var method = _extensionsType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static)!;

        try
        {
            return method.Invoke(null, args);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Throw(ex.InnerException);
            throw;
        }
    }
}
