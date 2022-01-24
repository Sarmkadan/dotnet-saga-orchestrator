using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

public static class SagaStepExecutionExceptionJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string ToJson(this SagaStepExecutionException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (indented)
        {
            _jsonSerializerOptions.WriteIndented = true;
        }

        return JsonSerializer.Serialize(value, _jsonSerializerOptions);
    }

    public static SagaStepExecutionException? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            return JsonSerializer.Deserialize<SagaStepExecutionException>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static bool TryFromJson(string json, out SagaStepExecutionException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<SagaStepExecutionException>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
