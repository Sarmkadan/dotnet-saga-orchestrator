using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dotnet_saga_orchestrator.benchmarks
{
    /// <summary>
    /// Provides extension methods for serializing and deserializing <see cref="SagaOrchestratorBenchmarks"/> instances to and from JSON.
    /// </summary>
    public static class SagaOrchestratorBenchmarksJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        /// <summary>
        /// Serializes the <paramref name="value"/> to a JSON string, optionally with indentation.
        /// </summary>
        /// <param name="value">The <see cref="SagaOrchestratorBenchmarks"/> instance to serialize.</param>
        /// <param name="indented">Whether to include indentation in the JSON output.</param>
        /// <returns>The JSON string representation of the <paramref name="value"/>.</returns>
        public static string ToJson(this SagaOrchestratorBenchmarks value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (indented)
            {
                _jsonSerializerOptions.WriteIndented = true;
            }

            return JsonSerializer.Serialize(value, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="SagaOrchestratorBenchmarks"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="SagaOrchestratorBenchmarks"/> instance, or <c>null</c> if the JSON is invalid.</returns>
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

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="SagaOrchestratorBenchmarks"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="SagaOrchestratorBenchmarks"/> instance, or <c>null</c> if the JSON is invalid.</param>
        /// <returns><c>true</c> if the JSON was successfully deserialized, <c>false</c> otherwise.</returns>
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
