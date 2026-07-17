using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Infrastructure.Caching
{
    /// <summary>
    /// Provides JSON serialization extensions for <see cref="CacheKeyBuilderValidation"/>.
    /// </summary>
    public static class CacheKeyBuilderValidationJsonExtensions
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
        };

        /// <summary>
        /// Converts the <see cref="CacheKeyBuilderValidation"/> instance to a JSON string.
        /// </summary>
        /// <param name="value">The validation instance to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
        /// <returns>A JSON string representation of the validation instance.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static string ToJson(this CacheKeyBuilderValidation value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            var options = indented
                ? new JsonSerializerOptions(_jsonSerializerOptions)
                {
                    WriteIndented = true,
                }
                : _jsonSerializerOptions;

            return JsonSerializer.Serialize(value, options);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="CacheKeyBuilderValidation"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized <see cref="CacheKeyBuilderValidation"/> instance, or <see langword="null"/> if <paramref name="json"/> is empty or whitespace.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
        /// <exception cref="JsonException">The JSON is invalid.</exception>
        public static CacheKeyBuilderValidation? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<CacheKeyBuilderValidation>(json, _jsonSerializerOptions);
        }

        /// <summary>
        /// Attempts to deserialize a JSON string to a <see cref="CacheKeyBuilderValidation"/> instance.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized <see cref="CacheKeyBuilderValidation"/> instance, or <see langword="null"/> if deserialization fails or <paramref name="json"/> is empty or whitespace.</param>
        /// <returns><see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
        public static bool TryFromJson(string json, out CacheKeyBuilderValidation? value)
        {
            ArgumentNullException.ThrowIfNull(json);

            try
            {
                value = FromJson(json);
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