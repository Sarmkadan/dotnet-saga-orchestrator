#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;

namespace SagaOrchestrator.Application.DTOs;

/// <summary>
/// Request model for creating a new saga instance.
/// </summary>
public class CreateSagaRequest
{
    [JsonPropertyName("definitionId")]
    public string DefinitionId { get; set; } = string.Empty;

    [JsonPropertyName("definitionName")]
    public string? DefinitionName { get; set; }

    [JsonPropertyName("maxRetries")]
    public int? MaxRetries { get; set; }

    [JsonPropertyName("timeoutSeconds")]
    public int? TimeoutSeconds { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// Optional initial payload data for the saga, serialized as a string.
    /// </summary>
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    /// <summary>
    /// Validates the request
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(DefinitionId) && string.IsNullOrWhiteSpace(DefinitionName))
            return false;

        if (MaxRetries.HasValue && MaxRetries.Value < 0)
            return false;

        if (TimeoutSeconds.HasValue && TimeoutSeconds.Value <= 0)
            return false;

        return true;
    }

    public override string ToString() =>
        $"CreateSagaRequest {{ DefinitionId = {DefinitionId}, DefinitionName = {DefinitionName}, MaxRetries = {MaxRetries}, TimeoutSeconds = {TimeoutSeconds}, Metadata = {Metadata}, Data = {Data} }}";
}
