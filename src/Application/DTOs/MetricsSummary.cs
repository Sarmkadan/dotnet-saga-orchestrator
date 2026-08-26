#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;

namespace SagaOrchestrator.Application.DTOs;

/// <summary>
/// Summary metrics for saga orchestration system.
/// Includes total sagas by status, average duration, and compensation rate.
/// </summary>
public class MetricsSummary
{
    [JsonPropertyName("totalSagas")]
    public int TotalSagas { get; set; }

    [JsonPropertyName("byStatus")]
    public Dictionary<string, int> ByStatus { get; set; } = new();

    [JsonPropertyName("averageDurationSeconds")]
    public double AverageDurationSeconds { get; set; }

    [JsonPropertyName("compensationRate")]
    public double CompensationRate { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    public override string ToString()
    {
        return $"MetricsSummary {{ TotalSagas = {TotalSagas}, ByStatus = {ByStatus}, AverageDurationSeconds = {AverageDurationSeconds}, CompensationRate = {CompensationRate}, Timestamp = {Timestamp} }}";
    }
}