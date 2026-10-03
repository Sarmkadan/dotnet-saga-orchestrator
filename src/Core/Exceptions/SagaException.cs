#nullable enable
using System;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>Base exception for all saga orchestration errors.</summary>
public class SagaException : Exception
{
    [JsonPropertyName("message")]
    public new string Message => base.Message;
    [JsonPropertyName("sagaId")]
    public string? SagaId { get; init; }
    [JsonPropertyName("errorCode")]
    public string? ErrorCode { get; init; }

    public SagaException() : base("Saga error") { }

    [JsonConstructor]
    public SagaException(string message) : base(message ?? throw new ArgumentNullException(nameof(message))) { }
    public SagaException(string message, Exception? innerException)
        : base(message ?? throw new ArgumentNullException(nameof(message)), innerException) { }
    public SagaException(string message, string? sagaId)
        : base(message ?? throw new ArgumentNullException(nameof(message))) { SagaId = sagaId; }
    public SagaException(string message, string? sagaId, string? errorCode)
        : base(message ?? throw new ArgumentNullException(nameof(message))) { SagaId = sagaId; ErrorCode = errorCode; }
    public SagaException(string message, string? sagaId, string? errorCode, Exception? innerException)
        : base(message ?? throw new ArgumentNullException(nameof(message)), innerException) { SagaId = sagaId; ErrorCode = errorCode; }
}
