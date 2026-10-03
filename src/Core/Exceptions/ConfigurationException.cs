#nullable enable
using System;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>Represents configuration errors.</summary>
public class ConfigurationException : Exception
{
    [JsonPropertyName("message")]
    public new string Message => base.Message;

    public ConfigurationException() { }

    [JsonConstructor]
    public ConfigurationException(string message)
        : base(message ?? throw new ArgumentNullException(nameof(message))) { }
    public ConfigurationException(string message, Exception innerException)
        : base(message ?? throw new ArgumentNullException(nameof(message)), innerException)
    { ArgumentNullException.ThrowIfNull(innerException); }
}
