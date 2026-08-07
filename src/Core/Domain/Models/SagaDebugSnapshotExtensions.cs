// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Text.Json;

namespace SagaOrchestrator.Core.Domain.Models;

/// <summary>
/// Extension methods for <see cref="SagaDebugSnapshot"/>.
/// </summary>
public static class SagaDebugSnapshotExtensions
{
    /// <summary>
    /// Serializes the snapshot to a JSON string.
    /// </summary>
    public static string ToJson(this SagaDebugSnapshot snapshot)
    {
        return JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Compares this snapshot against another and returns a list of changed field names.
    /// </summary>
    public static IEnumerable<string> DiffAgainst(this SagaDebugSnapshot snapshot, SagaDebugSnapshot other)
    {
        var differences = new List<string>();

        if (snapshot.SagaStatus != other.SagaStatus) differences.Add(nameof(snapshot.SagaStatus));
        if (snapshot.Trigger != other.Trigger) differences.Add(nameof(snapshot.Trigger));
        if (snapshot.FailureReason != other.FailureReason) differences.Add(nameof(snapshot.FailureReason));
        if (snapshot.RetryCount != other.RetryCount) differences.Add(nameof(snapshot.RetryCount));
        if (snapshot.Label != other.Label) differences.Add(nameof(snapshot.Label));
        if (snapshot.SequenceNumber != other.SequenceNumber) differences.Add(nameof(snapshot.SequenceNumber));
        // Note: Comparing Steps or Metadata dictionaries would be more complex and might 
        // require deep comparison, which is usually out of scope for a basic "field changed" diff.
        // For now, I am only comparing direct scalar properties.

        return differences;
    }
}
