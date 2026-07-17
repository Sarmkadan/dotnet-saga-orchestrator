#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Provides validation helpers for <see cref="SagaStepDefinition"/> instances.
/// </summary>
public static class SagaStepDefinitionValidation
{
    /// <summary>
    /// Validates a <see cref="SagaStepDefinition"/> instance and returns a list of validation errors.
    /// </summary>
    /// <param name="value">The <see cref="SagaStepDefinition"/> to validate</param>
    /// <returns>An empty list if valid; otherwise, a list of human-readable error messages</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static IReadOnlyList<string> Validate(this SagaStepDefinition value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // Validate Id
        if (string.IsNullOrWhiteSpace(value.Id))
        {
            errors.Add("Id cannot be null or whitespace.");
        }
        else if (!IsValidGuid(value.Id))
        {
            errors.Add("Id must be a valid GUID.");
        }

        // Validate Name
        if (string.IsNullOrWhiteSpace(value.Name))
        {
            errors.Add("Name cannot be null or whitespace.");
        }
        else if (value.Name.Length > 256)
        {
            errors.Add("Name cannot exceed 256 characters.");
        }

        // Validate Description
        if (string.IsNullOrWhiteSpace(value.Description))
        {
            errors.Add("Description cannot be null or whitespace.");
        }
        else if (value.Description.Length > 1024)
        {
            errors.Add("Description cannot exceed 1024 characters.");
        }

        // Validate Order
        if (value.Order < 0)
        {
            errors.Add("Order cannot be negative.");
        }

        // Validate ServiceName
        if (string.IsNullOrWhiteSpace(value.ServiceName))
        {
            errors.Add("ServiceName cannot be null or whitespace.");
        }
        else if (value.ServiceName.Length > 256)
        {
            errors.Add("ServiceName cannot exceed 256 characters.");
        }

        // Validate ServiceUrl
        if (string.IsNullOrWhiteSpace(value.ServiceUrl))
        {
            errors.Add("ServiceUrl cannot be null or whitespace.");
        }
        else if (!Uri.IsWellFormedUriString(value.ServiceUrl, UriKind.Absolute) && !Uri.IsWellFormedUriString(value.ServiceUrl, UriKind.Relative))
        {
            errors.Add("ServiceUrl must be a valid URI.");
        }
        else if (value.ServiceUrl.Length > 2048)
        {
            errors.Add("ServiceUrl cannot exceed 2048 characters.");
        }

        // Validate CompensationUrl
        if (value.IsCompensable && string.IsNullOrWhiteSpace(value.CompensationUrl))
        {
            errors.Add("CompensationUrl is required when IsCompensable is true.");
        }
        else if (!string.IsNullOrWhiteSpace(value.CompensationUrl))
        {
            if (!Uri.IsWellFormedUriString(value.CompensationUrl, UriKind.Absolute) && !Uri.IsWellFormedUriString(value.CompensationUrl, UriKind.Relative))
            {
                errors.Add("CompensationUrl must be a valid URI when provided.");
            }
            else if (value.CompensationUrl.Length > 2048)
            {
                errors.Add("CompensationUrl cannot exceed 2048 characters.");
            }
        }

        // Validate TimeoutSeconds
        if (value.TimeoutSeconds <= 0)
        {
            errors.Add("TimeoutSeconds must be a positive number.");
        }
        else if (value.TimeoutSeconds > 86400)
        {
            errors.Add("TimeoutSeconds cannot exceed 86400 seconds (24 hours).");
        }

        // Validate MaxRetries
        if (value.MaxRetries < 0)
        {
            errors.Add("MaxRetries cannot be negative.");
        }
        else if (value.MaxRetries > 100)
        {
            errors.Add("MaxRetries cannot exceed 100.");
        }

        // Validate RetryDelayMilliseconds
        if (value.RetryDelayMilliseconds < 0)
        {
            errors.Add("RetryDelayMilliseconds cannot be negative.");
        }
        else if (value.RetryDelayMilliseconds > 3600000) // 1 hour
        {
            errors.Add("RetryDelayMilliseconds cannot exceed 3600000 milliseconds (1 hour).");
        }

        // Validate HttpMethod
        if (string.IsNullOrWhiteSpace(value.HttpMethod))
        {
            errors.Add("HttpMethod cannot be null or whitespace.");
        }
        else if (!IsValidHttpMethod(value.HttpMethod))
        {
            errors.Add("HttpMethod must be a valid HTTP method (e.g., GET, POST, PUT, DELETE, PATCH).");
        }
        else if (value.HttpMethod.Length > 16)
        {
            errors.Add("HttpMethod cannot exceed 16 characters.");
        }

        // Validate RetryPolicy if set
        if (value.RetryPolicy is not null)
        {
            var retryPolicyErrors = ValidateRetryPolicy(value.RetryPolicy);
            if (retryPolicyErrors.Count > 0)
            {
                errors.AddRange(retryPolicyErrors);
            }
        }

        // Validate Metadata
        if (value.Metadata is null)
        {
            errors.Add("Metadata dictionary cannot be null.");
        }
        else
        {
            if (value.Metadata.Count > 1000)
            {
                errors.Add("Metadata dictionary cannot contain more than 1000 entries.");
            }

            foreach (var kvp in value.Metadata)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("Metadata keys cannot be null or whitespace.");
                    break;
                }

                if (kvp.Key.Length > 256)
                {
                    errors.Add("Metadata keys cannot exceed 256 characters.");
                    break;
                }

                if (kvp.Value is not null && kvp.Value.Length > 4096)
                {
                    errors.Add("Metadata values cannot exceed 4096 characters.");
                    break;
                }
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a <see cref="SagaStepDefinition"/> instance is valid.
    /// </summary>
    /// <param name="value">The <see cref="SagaStepDefinition"/> to check</param>
    /// <returns>True if valid; otherwise, false</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static bool IsValid(this SagaStepDefinition value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="SagaStepDefinition"/> instance is valid, throwing an exception if it is not.
    /// </summary>
    /// <param name="value">The <see cref="SagaStepDefinition"/> to validate</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not valid, containing all validation errors</exception>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
    public static void EnsureValid(this SagaStepDefinition value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = Validate(value);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SagaStepDefinition is invalid. Validation failed with {errors.Count} error(s):{Environment.NewLine}- ".Replace("- ", "") +
                string.Join($"{Environment.NewLine}- ", errors)
            );
        }
    }

    private static bool IsValidGuid(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            _ = Guid.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidHttpMethod(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
        {
            return false;
        }

        var validMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };
        return Array.Exists(validMethods, m => string.Equals(m, method, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> ValidateRetryPolicy(RetryPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var errors = new List<string>();

        if (policy.MaxRetries < 0)
        {
            errors.Add("RetryPolicy.MaxRetries cannot be negative.");
        }

        if (policy.InitialDelayMs < 0)
        {
            errors.Add("RetryPolicy.InitialDelayMs cannot be negative.");
        }

        if (policy.BackoffMultiplier < 1.0)
        {
            errors.Add("RetryPolicy.BackoffMultiplier must be >= 1.0.");
        }

        if (policy.MaxDelayMs < policy.InitialDelayMs)
        {
            errors.Add("RetryPolicy.MaxDelayMs must be >= InitialDelayMs.");
        }

        return errors.AsReadOnly();
    }
}