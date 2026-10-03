#nullable enable
using System;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>Base exception for saga orchestrator errors.</summary>
public class DotnetSagaOrchestratorException : Exception
{
    [JsonPropertyName("message")]
    public new string Message => base.Message;

    public DotnetSagaOrchestratorException() { }

    [JsonConstructor]
    public DotnetSagaOrchestratorException(string message) : base(message) { }
    public DotnetSagaOrchestratorException(string message, Exception innerException) : base(message, innerException) { }
}
