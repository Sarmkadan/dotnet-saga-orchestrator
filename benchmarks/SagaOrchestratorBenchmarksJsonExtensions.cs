using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dotnet_saga_orchestrator.benchmarks
{
    public static class SagaOrchestratorBenchmarksJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public static string ToJson(this SagaOrchestratorBenchmarks value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (indented)
            {
                _jsonSerializerOptions.WriteIndented = true;
            }

            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        public static SagaOrchestratorBenchmarks? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                return JsonSerializer.Deserialize<SagaOrchestratorBenchmarks>(json, _jsonSerializerOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        public static bool TryFromJson(string json, out SagaOrchestratorBenchmarks? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = JsonSerializer.Deserialize<SagaOrchestratorBenchmarks>(json, _jsonSerializerOptions);
                return true;
            }
            catch (JsonException)
            {
                value = null;
                return false;
            }
        }
    }
}
