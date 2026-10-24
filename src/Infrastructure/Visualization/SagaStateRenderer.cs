#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SagaOrchestrator.Application.Services;
using System.Globalization;
using System.Text;

namespace SagaOrchestrator.Infrastructure.Visualization;

/// <summary>
/// Renders saga execution state as human-readable ASCII diagrams for terminal display.
/// Supports progress bars, step-by-step state graphs, and full execution reports.
/// </summary>
public interface ISagaStateRenderer
{
    /// <summary>Renders a compact one-line progress summary for the saga.</summary>
    /// <param name="snapshot">The visualization snapshot to render.</param>
    string RenderProgressBar(SagaVisualizationSnapshot snapshot);

    /// <summary>Renders a vertical ASCII state graph showing each step and its current status.</summary>
    /// <param name="snapshot">The visualization snapshot to render.</param>
    string RenderStateDiagram(SagaVisualizationSnapshot snapshot);

    /// <summary>Renders a full execution report combining header, progress, and state diagram.</summary>
    /// <param name="snapshot">The visualization snapshot to render.</param>
    string RenderFullReport(SagaVisualizationSnapshot snapshot);
}

/// <summary>
/// Produces ASCII terminal output for saga state visualization.
/// All render methods are pure and produce deterministic output for a given snapshot.
/// </summary>
public class SagaStateRenderer : ISagaStateRenderer
{
    private const int ProgressBarWidth = 20;

    private readonly ILogger<SagaStateRenderer> _logger;

    /// <summary>Initializes a new instance of <see cref="SagaStateRenderer"/>.</summary>
    public SagaStateRenderer(ILogger<SagaStateRenderer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string RenderProgressBar(SagaVisualizationSnapshot snapshot)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        var filled = (int)Math.Round(snapshot.ProgressPercent / 100 * ProgressBarWidth);
        var sb = new StringBuilder(60); // Capacity estimate: [20-bar] [5-percent] [12-steps-text] etc.
        sb.Append('[');
        for (var i = 0; i < filled; i++) sb.Append('█');
        for (var i = 0; i < ProgressBarWidth - filled; i++) sb.Append('░');
        sb.Append(']');
        sb.Append(' ');
        sb.Append(snapshot.ProgressPercent.ToString("F1"));
        sb.Append('%');
        sb.Append(' ');
        sb.Append('(');
        sb.Append(snapshot.CompletedSteps);
        sb.Append('/');
        sb.Append(snapshot.TotalSteps);
        sb.Append(')');
        sb.Append(' ');
        sb.Append("steps");
        return sb.ToString();
    }

    /// <inheritdoc />
    public string RenderStateDiagram(SagaVisualizationSnapshot snapshot)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        try
        {
            var sb = new StringBuilder();

            if (snapshot.Nodes.Count == 0)
            {
                sb.AppendLine(" (no steps defined)");
                return sb.ToString();
            }

            var names = snapshot.Nodes.Select(n => Sanitize(n.Name)).ToList();
            var nameWidth = Math.Max(names.Max(n => n.Length), 16);

            for (var i = 0; i < snapshot.Nodes.Count; i++)
            {
                var node = snapshot.Nodes[i];
                var icon = GetStatusIcon(Sanitize(node.Status));
                var detail = BuildNodeDetail(node);
                var paddedName = (names[i].Length == 0 ? "Unknown" : names[i]).PadRight(nameWidth);

                sb.AppendLine($" [{icon}] {node.Index,2}. {paddedName} {detail}");

                if (i < snapshot.Nodes.Count - 1)
                    sb.AppendLine(" |");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render state diagram for saga {SagaId}", snapshot.SagaId);
            return "(diagram unavailable)";
        }
    }

    /// <inheritdoc />
    public string RenderFullReport(SagaVisualizationSnapshot snapshot)
    {
        if (snapshot == null)
            throw new ArgumentNullException(nameof(snapshot));

        try
        {
            var sb = new StringBuilder();
            var separator = new string('=', 60);

            sb.AppendLine(separator);
            sb.AppendLine($" {Sanitize(snapshot.SagaName)}");
            sb.AppendLine(separator);
            sb.AppendLine($" Saga ID : {Sanitize(snapshot.SagaId)}");
            sb.AppendLine($" Correlation : {Sanitize(snapshot.CorrelationId)}");
            sb.AppendLine($" Status : {Sanitize(snapshot.Status)}");
            sb.AppendLine($" Progress : {RenderProgressBar(snapshot)}");
            sb.AppendLine($" Elapsed : {FormatElapsed(snapshot.ElapsedMs)}");
            sb.AppendLine($" Captured At : {snapshot.CapturedAt:O}");

            if (!string.IsNullOrWhiteSpace(snapshot.FailureReason))
                sb.AppendLine($" Failure : {Sanitize(snapshot.FailureReason)}");

            sb.AppendLine();
            sb.AppendLine(" Steps:");
            sb.AppendLine(new string('-', 60));
            sb.Append(RenderStateDiagram(snapshot));
            sb.AppendLine(separator);

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render full report for saga {SagaId}", snapshot.SagaId);
            return "(report unavailable)";
        }
    }

    /// <summary>Renders a Graphviz DOT digraph of the saga steps.</summary>
    /// <param name="saga">The saga to render.</param>
    /// <returns>Graphviz DOT text representation of the saga.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="saga"/> is <see langword="null"/>.</exception>
    public string RenderDot(SagaOrchestrator.Core.Domain.Models.Saga saga)
    {
        if (saga == null)
            throw new ArgumentNullException(nameof(saga));

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph Saga {");
            sb.AppendLine("    rankdir=LR;");
            sb.AppendLine("    node [shape=box, style=filled, fontname=Arial, fontsize=10];");
            sb.AppendLine("    edge [fontname=Arial, fontsize=8];");
            sb.AppendLine();

            // Add nodes
            foreach (var step in saga.Steps)
            {
                // Convert SagaStepStatus to string status
                var status = step.Status.ToString();
                var color = status switch
                {
                    "Failed" => "red",
                    "Compensated" => "orange",
                    _ => "#e5e5e5"
                };

                var shape = status switch
                {
                    "Compensated" => "ellipse",
                    _ => "box"
                };

                sb.AppendLine($"    {DotNodeId(step)} [label=\"{EscapeDot(step.Name)}\",");
                sb.AppendLine($"        shape={shape},");
                sb.AppendLine($"        style=\"filled\",");
                sb.AppendLine($"        fillcolor=\"{color}\",");
                sb.AppendLine($"        tooltip=\"Step {step.Order}: {EscapeDot(step.Name)}\\nStatus: {status}\"");
                sb.AppendLine("    ]");
            }

            sb.AppendLine();

            // Add edges in execution order
            for (int i = 0; i < saga.Steps.Count - 1; i++)
            {
                var fromStep = saga.Steps[i];
                var toStep = saga.Steps[i + 1];
                sb.AppendLine($"    {DotNodeId(fromStep)} -> {DotNodeId(toStep)} [label=\"exec\", arrowhead=normal];");
            }

            sb.AppendLine("}");
            return sb.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to render DOT for saga {SagaId}", saga.Id);
            return "// DOT rendering failed";
        }
    }

    /// <summary>Builds a quoted, escaped DOT node identifier so step IDs cannot break the graph syntax.</summary>
    private static string DotNodeId(SagaOrchestrator.Core.Domain.Models.SagaStep step) =>
        $"\"node_{EscapeDot(step.Id)}\"";

    /// <summary>Escapes text for use inside a double-quoted DOT string, after stripping control characters.</summary>
    private static string EscapeDot(string? text) =>
        Sanitize(text).Replace("\\", "\\\\").Replace("\"", "\\\"");

    /// <summary>
    /// Replaces control characters and line/paragraph separators with a space so that untrusted
    /// saga or step values cannot break the diagram layout or inject terminal escape sequences.
    /// </summary>
    private static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            var isUnsafe = char.IsControl(ch)
                           || char.GetUnicodeCategory(ch) is UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator;
            sb.Append(isUnsafe ? ' ' : ch);
        }
        return sb.ToString();
    }

    private static string GetStatusIcon(string status) => status switch
    {
        "Completed" => "✓",
        "Executing" => "►",
        "Failed" => "✗",
        "Compensated" => "↩",
        "WaitingForRetry" => "⟳",
        "TimedOut" => "⏱",
        "Skipped" => "–",
        _ => "○"
    };

    private static string BuildNodeDetail(VisualizationNode node)
    {
        var parts = new List<string> { Sanitize(node.Status) };

        if (node.DurationMs.HasValue)
            parts.Add(FormatDurationMs(node.DurationMs.Value));

        if (node.RetryCount > 0)
            parts.Add($"retried {node.RetryCount}x");

        if (!string.IsNullOrWhiteSpace(node.ErrorMessage))
        {
            var error = Sanitize(node.ErrorMessage);
            var truncated = error.Length > 40
                ? error[..40] + "..."
                : error;
            parts.Add(truncated);
        }

        return string.Join(" | ", parts);
    }

    private static string FormatDurationMs(double ms) =>
        ms >= 1000 ? $"{ms / 1000:F2}s" : $"{ms:F0}ms";

    private static string FormatElapsed(double ms) =>
        ms >= 60_000 ? $"{ms / 60_000:F1}m"
        : ms >= 1000 ? $"{ms / 1000:F2}s"
        : $"{ms:F0}ms";
}
