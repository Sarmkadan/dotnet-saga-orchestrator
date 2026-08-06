#nullable enable

using System;
using System.Collections.Generic;
using SagaOrchestrator.Configuration;
using Xunit;

namespace SagaOrchestrator.Tests;

public class SagaOptionsValidationTests
{
    private static SagaOptions CreateValidOptions()
    {
        return new SagaOptions
        {
            TimeoutPolicies = new TimeoutPolicies
            {
                DefaultStepTimeoutSeconds = 10,
                DefaultSagaTimeoutSeconds = 30,
                MaxStepTimeoutSeconds = 20,
                MaxSagaTimeoutSeconds = 60,
                CompensationTimeoutSeconds = 5
            },
            RetryPolicies = new RetryPolicies
            {
                DefaultMaxRetries = 3,
                DefaultRetryDelayMs = 100,
                MaxRetries = 5,
                MaxBackoffDelayMs = 1000,
                BackoffMultiplier = 2.0,
                UseExponentialBackoff = true
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
                WebhookRetryDelayMs = 200,
                MaxWebhookPayloadBytes = 1024
            }
        };
    }

    [Fact]
    public void Validate_ReturnsEmpty_WhenAllPoliciesAreValid()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        IReadOnlyList<string> errors = options.Validate();

        // Assert
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenAllPoliciesAreValid()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act
        bool isValid = options.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void EnsureValid_DoesNotThrow_WhenAllPoliciesAreValid()
    {
        // Arrange
        var options = CreateValidOptions();

        // Act & Assert
        var exception = Record.Exception(() => options.EnsureValid());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_ReturnsErrors_ForInvalidTimeoutPolicies()
    {
        // Arrange
        var options = CreateValidOptions();
        options.TimeoutPolicies.DefaultStepTimeoutSeconds = 0; // invalid

        // Act
        IReadOnlyList<string> errors = options.Validate();

        // Assert
        Assert.Contains(
            "TimeoutPolicies.DefaultStepTimeoutSeconds must be greater than 0, but was 0.",
            errors);
    }

    [Fact]
    public void EnsureValid_ThrowsArgumentException_WithErrorMessages()
    {
        // Arrange
        var options = CreateValidOptions();
        options.RetryPolicies.DefaultMaxRetries = -1; // invalid

        // Act
        var ex = Assert.Throws<ArgumentException>(() => options.EnsureValid());

        // Assert
        Assert.Contains(
            "RetryPolicies.DefaultMaxRetries must be non-negative, but was -1.",
            ex.Message);
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenSagaOptionsIsNull()
    {
        // Arrange
        SagaOptions? options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options!.Validate());
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenTimeoutPoliciesIsNull()
    {
        // Arrange
        TimeoutPolicies? policies = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => policies!.Validate());
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenRetryPoliciesIsNull()
    {
        // Arrange
        RetryPolicies? policies = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => policies!.Validate());
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenCachePoliciesIsNull()
    {
        // Arrange
        CachePolicies? policies = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => policies!.Validate());
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenWorkerPoliciesIsNull()
    {
        // Arrange
        WorkerPolicies? policies = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => policies!.Validate());
    }

    [Fact]
    public void Validate_ThrowsArgumentNullException_WhenWebhookPoliciesIsNull()
    {
        // Arrange
        WebhookPolicies? policies = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => policies!.Validate());
    }
}
