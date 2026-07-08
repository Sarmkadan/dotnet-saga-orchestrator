#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Infrastructure.Serialization;

namespace SagaOrchestrator.Infrastructure.Formatting;

/// <summary>
/// Multi-format output formatter for saga data (JSON, CSV, Table).
/// Provides flexible formatting for CLI and API responses.
/// </summary>
public interface IOutputFormatter
{
    string FormatAsJson<T>(T data);
    string FormatAsTable(List<Saga> sagas);
    string FormatAsJson(Saga saga);
    string FormatAsCsv(List<Saga> sagas);
}

public class OutputFormatter : IOutputFormatter
{
    private readonly ISagaSerializer _serializer;

    public OutputFormatter(ISagaSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public string FormatAsJson<T>(T data)
    {
        return _serializer.SerializeIndented(data);
    }

    public string FormatAsJson(Saga saga)
    {
        return _serializer.SerializeIndented(saga);
    }

    public string FormatAsTable(List<Saga> sagas)
    {
        if (sagas.Count == 0)
            return "No sagas found.";

        var maxIdLength = Math.Max(10, sagas.Max(s => s.Id.Length));
        var maxNameLength = Math.Max(10, sagas.Max(s => s.Name.Length));

        var lines = new List<string>();
        lines.Add(new string('-', maxIdLength + maxNameLength + 30));
        lines.Add($"{"ID".PadRight(maxIdLength)} | {"Name".PadRight(maxNameLength)} | {"Status",-15} | {"Steps",-5}");
        lines.Add(new string('-', maxIdLength + maxNameLength + 30));

        foreach (var saga in sagas)
        {
            var completedSteps = saga.Steps.Count(s => s.Status.ToString() == "Completed");
            var totalSteps = saga.Steps.Count;
            lines.Add($"{saga.Id.PadRight(maxIdLength)} | {saga.Name.PadRight(maxNameLength)} | {saga.Status,-15} | {completedSteps}/{totalSteps,-3}");
        }

        lines.Add(new string('-', maxIdLength + maxNameLength + 30));
        return string.Join("\n", lines);
    }

    public string FormatAsCsv(List<Saga> sagas)
    {
        var lines = new List<string>
        {
            "Id,Name,Status,DefinitionId,CreatedAt,CompletedSteps,TotalSteps"
        };

        foreach (var saga in sagas)
        {
            var completedSteps = saga.Steps.Count(s => s.Status.ToString() == "Completed");
            var totalSteps = saga.Steps.Count;
            var createdAt = saga.CreatedAt.ToString("o");

            lines.Add($"\"{saga.Id}\",\"{saga.Name}\",\"{saga.Status}\",\"{saga.DefinitionId}\",\"{createdAt}\",{completedSteps},{totalSteps}");
        }

        return string.Join("\n", lines);
    }
}
