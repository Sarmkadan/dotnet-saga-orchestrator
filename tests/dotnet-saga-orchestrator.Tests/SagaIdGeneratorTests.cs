
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests;

public class SagaIdGeneratorTests
{
    [Fact]
    public void GenerateSagaId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateSagaId();
        id.Should().StartWith("saga_");
    }

    [Fact]
    public void GenerateCorrelationId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateCorrelationId();
        id.Should().StartWith("corr_");
    }

    [Fact]
    public void GenerateStepId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateStepId();
        id.Should().StartWith("step_");
    }

    [Fact]
    public void GenerateTraceId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateTraceId();
        id.Should().StartWith("trace_");
    }

    [Fact]
    public void GenerateRequestId_ShouldHaveCorrectPrefix()
    {
        var id = SagaIdGenerator.GenerateRequestId();
        id.Should().StartWith("req_");
    }

    [Theory]
    [InlineData("saga_1234567890abcdef1234567890abcdef", true)]
    [InlineData("corr_12345", false)]
    [InlineData("", false)]
    public void IsValidSagaId_ShouldValidateCorrectly(string id, bool expected)
    {
        SagaIdGenerator.IsValidSagaId(id).Should().Be(expected);
    }

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
