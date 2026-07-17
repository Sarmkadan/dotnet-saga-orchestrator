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

        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if a SagaEventPublisher instance is valid.
    /// </summary>
    /// <param name="value">The SagaEventPublisher instance to check</param>
    /// <returns>True if valid; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null</exception>
    public static bool IsValid(this SagaEventPublisher value)
    {
        return value is not null && value.Validate().Count == 0;
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
    /// <exception cref="ArgumentNullException">Thrown if sagaEvent is null</exception>
    public static IReadOnlyList<string> Validate(this SagaEvent sagaEvent)
    {
        ArgumentNullException.ThrowIfNull(sagaEvent);

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
    /// <param name="value">The string to validate</param>
    /// <returns>True if valid GUID; false otherwise</returns>
    private static bool IsValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }

    /// <summary>
    /// Checks if a file path is valid.
    /// </summary>
    /// <param name="path">The file path to validate</param>
    /// <returns>True if valid path; false otherwise</returns>
    /// <exception cref="ArgumentNullException">Thrown if path is null</exception>
    private static bool IsValidFilePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (path.Length == 0)
        {
            return false;
        }

        // Check for invalid characters
        var invalidChars = System.IO.Path.GetInvalidPathChars();
        if (path.IndexOfAny(invalidChars) >= 0)
        {
            return false;
        }

        // Check for invalid path sequences
        if (path.Contains("..") || path.EndsWith(".") || path.StartsWith("."))
        {
            return false;
        }

        // Check for Windows reserved names
        var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
        if (IsWindowsReservedName(fileName))
        {
            return false;
        }

        // Check if it's an absolute path (valid) or relative path (also valid if format is correct)
        return true;
    }

    /// <summary>
    /// Checks if a filename is a Windows reserved name.
    /// </summary>
    /// <param name="name">The filename to check</param>
    /// <returns>True if reserved name; false otherwise</returns>
    private static bool IsWindowsReservedName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
        return reservedNames.Contains(name, StringComparer.OrdinalIgnoreCase);
    }
}
