#nullable enable
using System;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>Represents validation errors.</summary>
public class ValidationException : Exception
{
    [JsonPropertyName("message")]
    public new string Message => base.Message;

    public ValidationException() : base(Validate(null, nameof(ValidationException))) { }

    [JsonConstructor]
    public ValidationException(string? message) : base(Validate(message, nameof(message))) { }
    public ValidationException(string? message, Exception? innerException)
        : base(Validate(message, nameof(message)), innerException) { }

    private static string Validate(string? m, string p)
    { if (string.IsNullOrWhiteSpace(m)) throw new ArgumentNullException(p); return m; }
}
