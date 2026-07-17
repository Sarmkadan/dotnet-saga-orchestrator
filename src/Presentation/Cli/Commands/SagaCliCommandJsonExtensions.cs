using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Presentation.Cli.Commands;

/// <summary>
/// Provides JSON serialization and deserialization helpers for the <see cref="SagaCliCommand"/> class.
/// </summary>
public static class SagaCliCommandJsonExtensions
{
	/// <summary>
	/// Configured JSON serializer options with camelCase naming policy.
	/// </summary>
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	/// <summary>
	/// Serializes a <see cref="SagaCliCommand"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The <see cref="SagaCliCommand"/> instance to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation.</param>
	/// <returns>A JSON string representation of the <see cref="SagaCliCommand"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
	public static string ToJson(this SagaCliCommand value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);
		return JsonSerializer.Serialize(value, new JsonSerializerOptions(JsonOptions) { WriteIndented = indented });
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="SagaCliCommand"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized <see cref="SagaCliCommand"/> instance, or <c>null</c> if deserialization fails.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
	/// <exception cref="JsonException">Thrown when JSON deserialization fails.</exception>
	public static SagaCliCommand? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);
		return JsonSerializer.Deserialize<SagaCliCommand>(json, JsonOptions);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="SagaCliCommand"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">When this method returns, contains the deserialized <see cref="SagaCliCommand"/> if the operation succeeded; otherwise, <c>null</c>.</param>
	/// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is <c>null</c> or empty.</exception>
	public static bool TryFromJson(string json, out SagaCliCommand? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<SagaCliCommand>(json, JsonOptions);
			return value is not null;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
