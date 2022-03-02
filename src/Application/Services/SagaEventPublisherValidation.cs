#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Validation helpers for SagaEventPublisher to ensure valid state and parameters.
/// </summary>
public static class SagaEventPublisherValidation
{
    /// <summary>
    /// Validates a SagaEventPublisher instance and returns any validation errors.
    /// </summary>
    /// <param name="value">The SagaEventPublisher instance to validate</param>
    /// <returns>List of validation error messages; empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static IReadOnlyList<string> Validate(this SagaEventPublisher value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = new List<string>();

        // SagaEventPublisher itself doesn't have state to validate
        // Validation is for method parameters when called

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a SagaEventPublisher instance is valid.
    /// </summary>
    /// <param name="value">The SagaEventPublisher instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    public static bool IsValid(this SagaEventPublisher value)
    {
        return value?.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures a SagaEventPublisher instance is valid, throwing if not.
    /// </summary>
    /// <param name="value">The SagaEventPublisher instance to validate</param>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    /// <exception cref="ArgumentException">Thrown if value is invalid with error messages</exception>
    public static void EnsureValid(this SagaEventPublisher value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var errors = value.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"SagaEventPublisher is invalid:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    /// <summary>
    /// Validates a SagaEvent instance.
    /// </summary>
    /// <param name="sagaEvent">The saga event to validate</param>
    /// <returns>List of validation error messages; empty if valid</returns>
    public static IReadOnlyList<string> Validate(this SagaEvent sagaEvent)
    {
        if (sagaEvent == null)
        {
            return new[] { "SagaEvent cannot be null." };
        }

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaEvent.Id))
        {
            errors.Add("Id cannot be null or whitespace.");
        }
        else if (!IsValidGuid(sagaEvent.Id))
        {
            errors.Add("Id must be a valid GUID.");
        }

        if (string.IsNullOrWhiteSpace(sagaEvent.SagaId))
        {
            errors.Add("SagaId cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(sagaEvent.EventType))
        {
            errors.Add("EventType cannot be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(sagaEvent.EventName))
        {
            errors.Add("EventName cannot be null or whitespace.");
        }

        if (sagaEvent.Timestamp == default)
        {
            errors.Add("Timestamp cannot be default (DateTime.MinValue).");
        }
        else if (sagaEvent.Timestamp.Kind != DateTimeKind.Utc)
        {
            errors.Add("Timestamp must be in UTC format.");
        }

        if (sagaEvent.Severity < EventSeverity.Information || sagaEvent.Severity > EventSeverity.Critical)
        {
            errors.Add("Severity must be a valid EventSeverity value.");
        }

        if (string.IsNullOrWhiteSpace(sagaEvent.Source))
        {
            errors.Add("Source cannot be null or whitespace.");
        }

        if (sagaEvent.Data == null)
        {
            errors.Add("Data dictionary cannot be null.");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if a string is a valid GUID.
    /// </summary>
    private static bool IsValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Checks if a file path is valid.
    /// </summary>
    private static bool IsValidFilePath(string path)
    {
        try
        {
            // Basic validation - check for invalid characters
            var invalidChars = System.IO.Path.GetInvalidPathChars();
            if (path.IndexOfAny(invalidChars) >= 0)
            {
                return false;
            }

            // Check if it's an absolute path or relative path
            if (System.IO.Path.IsPathRooted(path))
            {
                return true;
            }

            // For relative paths, just ensure they don't contain invalid sequences
            return !path.Contains("..") && !path.EndsWith(".") && !path.StartsWith(".");
        }
        catch
        {
            return false;
        }
    }
}