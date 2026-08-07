#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Infrastructure.Serialization;

/// <summary>
/// JSON serialization service for saga entities with custom converters.
/// Handles polymorphic serialization and enum conversions.
/// </summary>
public interface ISagaSerializer
{
    string Serialize<T>(T obj);
    T? Deserialize<T>(string json);
    string SerializeIndented<T>(T obj);
}

public class SagaJsonSerializer : ISagaSerializer
{
    private readonly JsonSerializerOptions _options;
    private readonly JsonSerializerOptions _indentedOptions;

    public SagaJsonSerializer()
    {
        _options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new SagaStatusConverter(),
                new SagaStepStatusConverter(),
                new CompensationStatusConverter(),
                new CompensationStrategyConverter(),
                new DateTimeConverter()
            }
        };

        _indentedOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        foreach (var converter in _options.Converters)
        {
            _indentedOptions.Converters.Add(converter);
        }
    }

    public string Serialize<T>(T? obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            return JsonSerializer.Serialize(obj, _options);
        }

    public T? Deserialize<T>(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json);
            return JsonSerializer.Deserialize<T>(json, _options);
        }

    public string SerializeIndented<T>(T obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            return JsonSerializer.Serialize(obj, _indentedOptions);
        }
}

public class SagaStatusConverter : JsonConverter<SagaStatus>
{
    public override SagaStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "pending" => SagaStatus.Pending,
            "initialized" => SagaStatus.Initialized,
            "running" => SagaStatus.Running,
            "completed" => SagaStatus.Completed,
            "failed" => SagaStatus.Failed,
            "compensating" => SagaStatus.Compensating,
            "compensated" => SagaStatus.Compensated,
            "aborted" => SagaStatus.Aborted,
            "timedOut" => SagaStatus.TimedOut,
            _ => throw new JsonException($"Unknown SagaStatus: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, SagaStatus value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            SagaStatus.Pending => "pending",
            SagaStatus.Initialized => "initialized",
            SagaStatus.Running => "running",
            SagaStatus.Completed => "completed",
            SagaStatus.Failed => "failed",
            SagaStatus.Compensating => "compensating",
            SagaStatus.Compensated => "compensated",
            SagaStatus.Aborted => "aborted",
            SagaStatus.TimedOut => "timedOut",
            _ => throw new JsonException($"Unknown SagaStatus: {value}")
        };
        writer.WriteStringValue(stringValue);
    }
}

public class SagaStepStatusConverter : JsonConverter<SagaStepStatus>
{
    public override SagaStepStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "pending" => SagaStepStatus.Pending,
            "executing" => SagaStepStatus.Executing,
            "completed" => SagaStepStatus.Completed,
            "failed" => SagaStepStatus.Failed,
            "waitingForRetry" => SagaStepStatus.WaitingForRetry,
            "compensated" => SagaStepStatus.Compensated,
            "timedOut" => SagaStepStatus.TimedOut,
            "skipped" => SagaStepStatus.Skipped,
            _ => throw new JsonException($"Unknown SagaStepStatus: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, SagaStepStatus value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            SagaStepStatus.Pending => "pending",
            SagaStepStatus.Executing => "executing",
            SagaStepStatus.Completed => "completed",
            SagaStepStatus.Failed => "failed",
            SagaStepStatus.WaitingForRetry => "waitingForRetry",
            SagaStepStatus.Compensated => "compensated",
            SagaStepStatus.TimedOut => "timedOut",
            SagaStepStatus.Skipped => "skipped",
            _ => throw new JsonException($"Unknown SagaStepStatus: {value}")
        };
        writer.WriteStringValue(stringValue);
    }
}

public class CompensationStatusConverter : JsonConverter<CompensationStatus>
{
    public override CompensationStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "pending" => CompensationStatus.Pending,
            "inProgress" => CompensationStatus.InProgress,
            "completed" => CompensationStatus.Completed,
            "failed" => CompensationStatus.Failed,
            "timedOut" => CompensationStatus.TimedOut,
            "skipped" => CompensationStatus.Skipped,
            _ => throw new JsonException($"Unknown CompensationStatus: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, CompensationStatus value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            CompensationStatus.Pending => "pending",
            CompensationStatus.InProgress => "inProgress",
            CompensationStatus.Completed => "completed",
            CompensationStatus.Failed => "failed",
            CompensationStatus.TimedOut => "timedOut",
            CompensationStatus.Skipped => "skipped",
            _ => throw new JsonException($"Unknown CompensationStatus: {value}")
        };
        writer.WriteStringValue(stringValue);
    }
}

public class CompensationStrategyConverter : JsonConverter<CompensationStrategy>
{
    public override CompensationStrategy Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "reverse" => CompensationStrategy.ReverseOrder,
            "forward" => CompensationStrategy.ForwardOrder,
            "parallel" => CompensationStrategy.Parallel,
            "manual" => CompensationStrategy.Manual,
            _ => throw new JsonException($"Unknown CompensationStrategy: {value}")
        };
    }

    public override void Write(Utf8JsonWriter writer, CompensationStrategy value, JsonSerializerOptions options)
    {
        var stringValue = value switch
        {
            CompensationStrategy.ReverseOrder => "reverse",
            CompensationStrategy.ForwardOrder => "forward",
            CompensationStrategy.Parallel => "parallel",
            CompensationStrategy.Manual => "manual",
            _ => throw new JsonException($"Unknown CompensationStrategy: {value}")
        };
        writer.WriteStringValue(stringValue);
    }
}

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.Parse(
            reader.GetString() ?? string.Empty,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("o"));
    }
}
