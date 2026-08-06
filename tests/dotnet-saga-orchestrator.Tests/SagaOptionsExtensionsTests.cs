#nullable enable

using System;
using SagaOrchestrator.Configuration;
using Xunit;

namespace SagaOrchestrator.Tests;

public class SagaOptionsExtensionsTests
{
    private static SagaOptions CreateDefaultOptions()
    {
        return new SagaOptions
        {
            TimeoutPolicies = new TimeoutPolicies
            {
                DefaultStepTimeoutSeconds = 10,
                DefaultSagaTimeoutSeconds = 20,
                MaxStepTimeoutSeconds = 30,
                MaxSagaTimeoutSeconds = 60,
                CompensationTimeoutSeconds = 5
            },
            RetryPolicies = new RetryPolicies
            {
                DefaultMaxRetries = 2,
                DefaultRetryDelayMs = 100,
                MaxRetries = 5,
                UseExponentialBackoff = false,
                BackoffMultiplier = 2.0,
                MaxBackoffDelayMs = 1000
            },
            CachePolicies = new CachePolicies
            {
                EnableCaching = false,
                SagaCacheExpirationMinutes = 15,
                DefinitionCacheExpirationMinutes = 10,
                HealthCheckCacheExpirationSeconds = 30,
                MaxCacheSize = 1024
            },
            WorkerPolicies = new WorkerPolicies
            {
                EnableTimeoutWorker = true,
                TimeoutWorkerIntervalSeconds = 60,
                EnableCompensationWorker = true,
                CompensationWorkerIntervalSeconds = 120,
                EnableEventProcessingWorker = true,
                EventProcessingWorkerIntervalSeconds = 30,
                MaxEventsToKeep = 1000
            },
            WebhookPolicies = new WebhookPolicies
            {
                EnableWebhooks = true,
                WebhookTimeoutSeconds = 10,
                MaxWebhookRetries = 3,
                WebhookRetryDelayMs = 200,
                MaxWebhookPayloadBytes = 1024 * 1024
            }
        };
    }

    [Fact]
    public void IsCachingEnabled_ReturnsTrue_WhenEnabled()
    {
        var options = CreateDefaultOptions();
        options.CachePolicies.EnableCaching = true;

        bool result = options.IsCachingEnabled();

        Assert.True(result);
    }

    [Fact]
    public void IsCachingEnabled_ReturnsFalse_WhenDisabled()
    {
        var options = CreateDefaultOptions();
        options.CachePolicies.EnableCaching = false;

        bool result = options.IsCachingEnabled();

        Assert.False(result);
    }

    [Fact]
    public void IsCachingEnabled_Throws_WhenOptionsNull()
    {
        SagaOptions? options = null;
        Assert.Throws<ArgumentNullException>(() => options!.IsCachingEnabled());
    }

    [Theory]
    [InlineData(5, 10)]   // below default -> default used
    [InlineData(15, 15)]  // between default and max -> requested used
    [InlineData(40, 30)]  // above max -> max used
    public void GetEffectiveStepTimeout_ReturnsExpected(int requested, int expected)
    {
        var options = CreateDefaultOptions();

        int result = options.GetEffectiveStepTimeout(requested);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetEffectiveStepTimeout_Throws_WhenOptionsNull()
    {
        SagaOptions? options = null;
        Assert.Throws<ArgumentNullException>(() => options!.GetEffectiveStepTimeout(10));
    }

    [Fact]
    public void GetEffectiveStepTimeout_Throws_WhenRequestedNotPositive()
    {
        var options = CreateDefaultOptions();
        Assert.Throws<ArgumentOutOfRangeException>(() => options.GetEffectiveStepTimeout(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => options.GetEffectiveStepTimeout(-5));
    }

    [Theory]
    [InlineData(10, 20)]  // below default -> default used
    [InlineData(40, 40)]  // between default and max -> requested used
    [InlineData(80, 60)]  // above max -> max used
    public void GetEffectiveSagaTimeout_ReturnsExpected(int requested, int expected)
    {
        var options = CreateDefaultOptions();

        int result = options.GetEffectiveSagaTimeout(requested);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetEffectiveSagaTimeout_Throws_WhenOptionsNull()
    {
        SagaOptions? options = null;
        Assert.Throws<ArgumentNullException>(() => options!.GetEffectiveSagaTimeout(10));
    }

    [Fact]
    public void GetEffectiveSagaTimeout_Throws_WhenRequestedNotPositive()
    {
        var options = CreateDefaultOptions();
        Assert.Throws<ArgumentOutOfRangeException>(() => options.GetEffectiveSagaTimeout(0));
    }

    [Theory]
    [InlineData(-1, 2)]   // negative -> default used (method throws before, so we test only non‑negative)
    [InlineData(0, 2)]    // zero -> default used
    [InlineData(3, 3)]    // between default and max -> requested used
    [InlineData(10, 5)]   // above max -> max used
    public void GetEffectiveMaxRetries_ReturnsExpected(int requested, int expected)
    {
        var options = CreateDefaultOptions();

        int result = options.GetEffectiveMaxRetries(requested);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetEffectiveMaxRetries_Throws_WhenOptionsNull()
    {
        SagaOptions? options = null;
        Assert.Throws<ArgumentNullException>(() => options!.GetEffectiveMaxRetries(1));
    }

    [Fact]
    public void GetEffectiveMaxRetries_Throws_WhenRequestedNegative()
    {
        var options = CreateDefaultOptions();
        Assert.Throws<ArgumentOutOfRangeException>(() => options.GetEffectiveMaxRetries(-1));
    }

    [Fact]
    public void WithOverrides_ReturnsNewInstance_WithAppliedChanges()
    {
        var original = CreateDefaultOptions();

        var overridden = original.WithOverrides(o =>
        {
            o.TimeoutPolicies.DefaultStepTimeoutSeconds = 99;
            o.CachePolicies.EnableCaching = true;
        });

        // original must stay unchanged
        Assert.Equal(10, original.TimeoutPolicies.DefaultStepTimeoutSeconds);
        Assert.False(original.CachePolicies.EnableCaching);

        // overridden must reflect changes
        Assert.Equal(99, overridden.TimeoutPolicies.DefaultStepTimeoutSeconds);
        Assert.True(overridden.CachePolicies.EnableCaching);
    }

    [Fact]
    public void WithOverrides_Throws_WhenOptionsNull()
    {
        SagaOptions? options = null;
        Assert.Throws<ArgumentNullException>(() => options!.WithOverrides(_ => { }));
    }

    [Fact]
    public void WithOverrides_Throws_WhenConfigureNull()
    {
        var options = CreateDefaultOptions();
        Assert.Throws<ArgumentNullException>(() => options.WithOverrides(null!));
    }
}
