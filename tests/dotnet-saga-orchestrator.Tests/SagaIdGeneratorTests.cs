using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Tests for SagaIdGenerator class.
/// </summary>
public class SagaIdGeneratorTests
{
    /// <summary>
    /// Verifies that the generated saga ID starts with the correct prefix.
    /// </summary>
    [Fact]
    public void GenerateSagaId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateSagaId();
        id.Should().StartWith("saga_");
    }

    /// <summary>
    /// Verifies that the generated correlation ID starts with the correct prefix.
    /// </summary>
    [Fact]
    public void GenerateCorrelationId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateCorrelationId();
        id.Should().StartWith("corr_");
    }

    /// <summary>
    /// Verifies that the generated step ID starts with the correct prefix.
    /// </summary>
    [Fact]
    public void GenerateStepId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateStepId();
        id.Should().StartWith("step_");
    }

    /// <summary>
    /// Verifies that the generated trace ID starts with the correct prefix.
    /// </summary>
    [Fact]
    public void GenerateTraceId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateTraceId();
        id.Should().StartWith("trace_");
    }

    /// <summary>
    /// Verifies that the generated request ID starts with the correct prefix.
    /// </summary>
    [Fact]
    public void GenerateRequestId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateRequestId();
        id.Should().StartWith("req_");
    }

    /// <summary>
    /// Verifies that IsValidSagaId method validates the saga ID correctly.
    /// </summary>
    /// <param name="id">The saga ID to validate.</param>
    /// <param name="expected">The expected result of the validation.</param>
    [Theory]
    [InlineData("saga_1234567890abcdef1234567890abcdef", true)]
    [InlineData("corr_12345", false)]
    [InlineData("", false)]
    public void IsValidSagaId_ShouldValidateCorrectly(string id, bool expected)
    {
        SagaIdGenerator.IsValidSagaId(id).Should().Be(expected);
    }

    /// <summary>
    /// Verifies that IsValidCorrelationId method validates the correlation ID correctly.
    /// </summary>
    /// <param name="id">The correlation ID to validate.</param>
    /// <param name="expected">The expected result of the validation.</param>
    [Theory]
    [InlineData("corr_1234567890abcdef1234567890abcdef", true)]
    [InlineData("12345678-1234-1234-1234-1234567890ab", true)]
    [InlineData("saga_123", false)]
    [InlineData("", false)]
    public void IsValidCorrelationId_ShouldValidateCorrectly(string id, bool expected)
    {
        SagaIdGenerator.IsValidCorrelationId(id).Should().Be(expected);
    }
}
