#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Core.Builders;

/// <summary>
/// Fluent builder for constructing saga steps with validation.
/// Provides a convenient API for creating and configuring saga step definitions.
/// </summary>
public class SagaStepBuilder
{
    private readonly SagaStepDefinition _step;

    private SagaStepBuilder(string name, string serviceName, string action)
    {
        _step = new SagaStepDefinition(name, serviceName, action, null)
        {
            Order = 1,
            TimeoutSeconds = 30,
            MaxRetries = 3
        };
    }

    public static SagaStepBuilder Create(string name, string serviceName, string action)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Step name cannot be null or empty", nameof(name));
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action URL cannot be null or empty", nameof(action));

        return new SagaStepBuilder(name, serviceName, action);
    }

    public SagaStepBuilder WithOrder(int order)
    {
        if (order <= 0)
            throw new ArgumentException("Order must be greater than 0", nameof(order));
        _step.Order = order;
        return this;
    }

    public SagaStepBuilder WithCompensation(string compensationUrl)
    {
        if (!string.IsNullOrWhiteSpace(compensationUrl))
        {
            if (!Uri.IsWellFormedUriString(compensationUrl, UriKind.Absolute))
                throw new ArgumentException("Compensation URL is not valid", nameof(compensationUrl));
            _step.Compensation = compensationUrl;
        }
        return this;
    }

    public SagaStepBuilder WithTimeout(int seconds)
    {
        if (seconds <= 0 || seconds > 3600)
            throw new ArgumentException("Timeout must be between 1 and 3600 seconds", nameof(seconds));
        _step.TimeoutSeconds = seconds;
        return this;
    }

    public SagaStepBuilder WithRetryPolicy(int maxRetries, int delayMs)
    {
        if (maxRetries < 0 || maxRetries > 10)
            throw new ArgumentException("Max retries must be between 0 and 10", nameof(maxRetries));
        if (delayMs < 0)
            throw new ArgumentException("Delay must be non-negative", nameof(delayMs));

        _step.MaxRetries = maxRetries;
        _step.RetryDelayMs = delayMs;
        return this;
    }

    /// <summary>
    /// Configures per-step retry policy with exponential backoff and optional jitter.
    /// </summary>
    public SagaStepBuilder WithRetryPolicy(RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy, nameof(policy));
        _step.RetryPolicy = policy;
        _step.MaxRetries = policy.MaxRetries;
        _step.RetryDelayMs = policy.InitialDelayMs;
        return this;
    }

    public SagaStepBuilder WithMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Metadata key cannot be null or empty", nameof(key));

        _step.Metadata[key] = value ?? string.Empty;
        return this;
    }

    public SagaStepBuilder WithMetadata(Dictionary<string, string> metadata)
    {
        if (metadata != null)
        {
            foreach (var kvp in metadata)
            {
                _step.Metadata[kvp.Key] = kvp.Value;
            }
        }
        return this;
    }

    public SagaStepBuilder WithCircuitBreakerThreshold(int failureThreshold)
    {
        if (failureThreshold <= 0)
            throw new ArgumentException("Failure threshold must be greater than 0", nameof(failureThreshold));
        _step.Metadata["circuitBreakerThreshold"] = failureThreshold.ToString();
        return this;
    }

    public SagaStepBuilder Async()
    {
        _step.Metadata["async"] = "true";
        return this;
    }

    public SagaStepBuilder Synchronous()
    {
        _step.Metadata["async"] = "false";
        return this;
    }

    public SagaStepDefinition Build()
    {
        ValidateStep();
        return _step;
    }

    private void ValidateStep()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_step.Name))
            errors.Add("Step name is required");

        if (string.IsNullOrWhiteSpace(_step.ServiceName))
            errors.Add("Service name is required");

        if (string.IsNullOrWhiteSpace(_step.Action))
            errors.Add("Action URL is required");

        if (!Uri.IsWellFormedUriString(_step.Action, UriKind.Absolute))
            errors.Add("Action URL is not valid");

        if (_step.TimeoutSeconds <= 0)
            errors.Add("Timeout must be greater than 0");

        if (_step.MaxRetries < 0)
            errors.Add("Max retries cannot be negative");

        if (errors.Count > 0)
            throw new InvalidOperationException($"Invalid step configuration: {string.Join(", ", errors)}");
    }
}

/// <summary>
/// Fluent builder for saga definitions.
/// </summary>
public class SagaDefinitionBuilder
{
    private readonly SagaDefinition _definition;
    private int _stepOrder = 1;

    private SagaDefinitionBuilder(string name, string description)
    {
        _definition = new SagaDefinition(Guid.NewGuid().ToString(), name, description);
    }

    public static SagaDefinitionBuilder Create(string name, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Definition name cannot be null or empty", nameof(name));

        return new SagaDefinitionBuilder(name, description);
    }

    public SagaDefinitionBuilder AddStep(SagaStepDefinition step)
    {
        step.NotNull(nameof(step));
        step.Order = _stepOrder++;
        _definition.Steps.Add(step);
        return this;
    }

    public SagaDefinitionBuilder AddStep(string name, string serviceName, string action)
    {
        var step = SagaStepBuilder.Create(name, serviceName, action)
            .WithOrder(_stepOrder++)
            .Build();
        _definition.Steps.Add(step);
        return this;
    }

    public SagaDefinitionBuilder WithDescription(string description)
    {
        _definition.Description = description ?? string.Empty;
        return this;
    }

    public SagaDefinition Build()
    {
        if (_definition.Steps.Count == 0)
            throw new InvalidOperationException("Definition must contain at least one step");

        return _definition;
    }
}
