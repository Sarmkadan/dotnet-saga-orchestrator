using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SagaOrchestrator.Core.Exceptions;

/// <summary>
/// Provides extension methods for serializing and deserializing <see cref="SagaStepExecutionException"/> instances to and from JSON.
/// </summary>
public static class SagaStepExecutionExceptionJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	/// <summary>
	/// Serializes the <paramref name="value"/> to a JSON string, optionally with indentation.
	/// </summary>
	/// <param name="value">The <see cref="SagaStepExecutionException"/> instance to serialize.</param>
	/// <param name="indented">Whether to include indentation in the JSON output.</param>
	/// <returns>The JSON representation of the <paramref name="value"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
	public static string ToJson(this SagaStepExecutionException value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = new JsonSerializerOptions(_jsonSerializerOptions)
		{
			WriteIndented = indented
		};
		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="SagaStepExecutionException"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized <see cref="SagaStepExecutionException"/> instance, or <c>null</c> if deserialization fails.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
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

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="SagaStepExecutionException"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">The deserialized <see cref="SagaStepExecutionException"/> instance, or <c>null</c> if deserialization fails.</param>
	/// <returns><c>true</c> if deserialization is successful, <c>false</c> otherwise.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="json"/> is <c>null</c>.</exception>
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