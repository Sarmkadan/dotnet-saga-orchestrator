#nullable enable
using System;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>Thrown when a saga definition is invalid.</summary>
public class InvalidSagaDefinitionException : SagaException
{
    public InvalidSagaDefinitionException() : base() { }

    [JsonConstructor]
    public InvalidSagaDefinitionException(string message) : base(message) { }
    public InvalidSagaDefinitionException(string message, Exception? innerException) : base(message, innerException) { }
    public InvalidSagaDefinitionException(string definitionId, string message)
        : base(FormatMessage(definitionId, message), definitionId, "INVALID_SAGA_DEFINITION") { }

    private static string FormatMessage(string definitionId, string message)
    {
        ArgumentNullException.ThrowIfNull(definitionId);
        return $"Saga definition '{definitionId}' is invalid: {message}";
    }
}
