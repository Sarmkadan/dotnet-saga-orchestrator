using System;
using SagaOrchestrator.Configuration;
using Xunit;
using System.Collections.Generic;

namespace SagaOrchestrator.Tests;

public class SagaOptionsTests
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
                SagaCacheExpirationMinutes = 10,
                DefinitionCacheExpirationMinutes = 5,
                HealthCheckCacheExpirationSeconds = 30,
                MaxCacheSize = 1000
            },
            WorkerPolicies = new WorkerPolicies
            {
                TimeoutWorkerIntervalSeconds = 5,
                CompensationWorkerIntervalSeconds = 5,
                EventProcessingWorkerIntervalSeconds = 5,
                MaxEventsToKeep = 100
            },
            WebhookPolicies = new WebhookPolicies
            {
                WebhookTimeoutSeconds = 10,
                MaxWebhookRetries = 3,
                MaxWebhookPayloadBytes = 1024000
            }
        };
    }

    [Fact]
    public void TestDefaultOptions()
    {
        // Arrange
        var options = CreateDefaultOptions();

        // Act
        var timeoutPolicies = options.TimeoutPolicies;
        var retryPolicies = options.RetryPolicies;
        var cachePolicies = options.CachePolicies;
        var workerPolicies = options.WorkerPolicies;
        var webhookPolicies = options.WebhookPolicies;

        // Assert
        Assert.Equal(10, timeoutPolicies.DefaultStepTimeoutSeconds);
        Assert.Equal(20, timeoutPolicies.DefaultSagaTimeoutSeconds);
        Assert.Equal(30, timeoutPolicies.MaxStepTimeoutSeconds);
        Assert.Equal(60, timeoutPolicies.MaxSagaTimeoutSeconds);
        Assert.Equal(5, timeoutPolicies.CompensationTimeoutSeconds);

        Assert.Equal(2, retryPolicies.DefaultMaxRetries);
        Assert.Equal(100, retryPolicies.DefaultRetryDelayMs);
        Assert.Equal(5, retryPolicies.MaxRetries);
        Assert.False(retryPolicies.UseExponentialBackoff);
        Assert.Equal(2.0, retryPolicies.BackoffMultiplier);
        Assert.Equal(1000, retryPolicies.MaxBackoffDelayMs);

        Assert.Equal(10, cachePolicies.SagaCacheExpirationMinutes);
        Assert.Equal(5, cachePolicies.DefinitionCacheExpirationMinutes);
        Assert.Equal(30, cachePolicies.HealthCheckCacheExpirationSeconds);
        Assert.Equal(1000, cachePolicies.MaxCacheSize);

        Assert.Equal(5, workerPolicies.TimeoutWorkerIntervalSeconds);
        Assert.Equal(5, workerPolicies.CompensationWorkerIntervalSeconds);
        Assert.Equal(5, workerPolicies.EventProcessingWorkerIntervalSeconds);
        Assert.Equal(100, workerPolicies.MaxEventsToKeep);

        Assert.Equal(10, webhookPolicies.WebhookTimeoutSeconds);
        Assert.Equal(3, webhookPolicies.MaxWebhookRetries);
        Assert.Equal(1024000, webhookPolicies.MaxWebhookPayloadBytes);
    }
}
