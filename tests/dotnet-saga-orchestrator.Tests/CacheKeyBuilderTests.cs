// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using SagaOrchestrator.Infrastructure.Caching;

namespace SagaOrchestrator.Tests;

public class CacheKeyBuilderTests
{
    [Fact]
    public void BuildSagaKey_IncludesSagaId()
    {
        CacheKeyBuilder.BuildSagaKey("saga-123").Should().Be("saga:saga-123");
    }

    [Fact]
    public void BuildDefinitionKey_IncludesDefinitionId()
    {
        CacheKeyBuilder.BuildDefinitionKey("def-001").Should().Be("definition:def-001");
    }

    [Fact]
    public void BuildAllSagasKey_ReturnsStaticKey()
    {
        CacheKeyBuilder.BuildAllSagasKey().Should().Be("sagas:all");
    }

    [Fact]
    public void BuildAllDefinitionsKey_ReturnsStaticKey()
    {
        CacheKeyBuilder.BuildAllDefinitionsKey().Should().Be("definitions:all");
    }

    [Fact]
    public void BuildSagasByStatusKey_IncludesStatus()
    {
        CacheKeyBuilder.BuildSagasByStatusKey("running").Should().Be("sagas:status:running");
    }

    [Fact]
    public void BuildCompensationKey_IncludesSagaId()
    {
        CacheKeyBuilder.BuildCompensationKey("saga-456").Should().Be("compensation:saga-456");
    }

    [Fact]
    public void BuildEventHistoryKey_IncludesSagaId()
    {
        CacheKeyBuilder.BuildEventHistoryKey("saga-789").Should().Be("events:saga-789");
    }

    [Fact]
    public void BuildRateLimitKey_IncludesIdentifierAndResource()
    {
        CacheKeyBuilder.BuildRateLimitKey("user-1", "api")
            .Should().Be("ratelimit:user-1:api");
    }

    [Fact]
    public void BuildMetricsKey_ReturnsStaticKey()
    {
        CacheKeyBuilder.BuildMetricsKey().Should().Be("metrics");
    }

    [Fact]
    public void BuildSessionKey_IncludesSessionId()
    {
        CacheKeyBuilder.BuildSessionKey("sess-abc").Should().Be("session:sess-abc");
    }

    [Fact]
    public void BuildWebhookKey_IncludesWebhookId()
    {
        CacheKeyBuilder.BuildWebhookKey("wh-001").Should().Be("webhook:wh-001");
    }

    [Fact]
    public void DifferentKeys_ProduceDifferentValues()
    {
        var sagaKey = CacheKeyBuilder.BuildSagaKey("123");
        var defKey = CacheKeyBuilder.BuildDefinitionKey("123");
        sagaKey.Should().NotBe(defKey);
    }
}
