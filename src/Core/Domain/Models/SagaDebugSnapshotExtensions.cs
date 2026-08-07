#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// Extension methods for SagaDebugSnapshot
// =============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// <param name="snapshot">The snapshot to serialize.</param>
    /// <returns>Indented JSON representation of the snapshot.</returns>
    public static string ToJson(this SagaDebugSnapshot snapshot)
    {
        if (snapshot is null) throw new ArgumentNullException(nameof(snapshot));

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        return JsonSerializer.Serialize(snapshot, options);
    }

    /// <summary>
    /// Compares two snapshots and returns the names of the fields that differ.
    /// The comparison is shallow for simple properties and sequence‑based for collections.
    /// </summary>
    /// <param name="snapshot">The snapshot to compare (the "left" side).</param>
    /// <param name="other">The snapshot to compare against (the "right" side).</param>
    /// <returns>A collection of property names whose values differ.</returns>
    public static IEnumerable<string> DiffAgainst(this SagaDebugSnapshot snapshot, SagaDebugSnapshot other)
    {
        if (snapshot is null) throw new ArgumentNullException(nameof(snapshot));
        if (other is null) throw new ArgumentNullException(nameof(other));

        var changed = new List<string>();

        // Get all public instance readable properties of the record
        var properties = typeof(SagaDebugSnapshot)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        foreach (var prop in properties)
        {
            var leftValue = prop.GetValue(snapshot);
            var rightValue = prop.GetValue(other);

            if (!ValuesEqual(leftValue, rightValue))
            {
                changed.Add(prop.Name);
            }
        }

        return changed;
    }

    private static bool ValuesEqual(object? left, object? right)
    {
        // Both null -> equal
        if (left is null && right is null) return true;
        // One null -> not equal
        if (left is null || right is null) return false;

        // Strings: compare by value
        if (left is string leftStr && right is string rightStr)
            return leftStr.Equals(rightStr, StringComparison.Ordinal);

        // Collections (except string)
        if (left is IEnumerable leftEnum && !(left is string))
        {
            if (right is IEnumerable rightEnum && !(right is string))
            {
                var leftList = leftEnum.Cast<object>().ToList();
                var rightList = rightEnum.Cast<object>().ToList();
                return leftList.SequenceEqual(rightList);
            }

            // left is collection, right is not
            return false;
        }

        // Fallback to default equality
        return left.Equals(right);
    }
}
