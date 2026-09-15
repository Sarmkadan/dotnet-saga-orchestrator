# SagaStateRenderer

Renders saga execution state as human-readable ASCII diagrams for terminal display. Supports progress bars, step-by-step state graphs, and full execution reports.

## Purpose

The `SagaStateRenderer` class provides visualization capabilities for saga executions, producing ASCII terminal output that shows:
- Progress bars with completion percentages
- Vertical state graphs showing each step and its current status
- Full execution reports combining header, progress, and state diagram information
- Graphviz DOT representation for graphical visualization

## Public API

### Constructor

```csharp
public SagaStateRenderer(ILogger<SagaStateRenderer> logger)
```
Initializes a new instance of the renderer with the specified logger.

### Methods

#### `RenderProgressBar(SagaVisualizationSnapshot snapshot)`
Renders a compact one-line progress summary for the saga.

**Parameters:**
- `snapshot`: The visualization snapshot to render

**Returns:**
A string representing the progress bar in format: `[███░░░░░░░░░░] 45.0% (3/5) steps`

#### `RenderStateDiagram(SagaVisualizationSnapshot snapshot)`
Renders a vertical ASCII state graph showing each step and its current status.

**Parameters:**
- `snapshot`: The visualization snapshot to render

**Returns:**
A string representing the state diagram with icons indicating step status:
```
 [✓]  1. StepName           Completed | 250ms
 [►]  2. AnotherStep        Executing | retried 2x
 [✗]  3. FailedStep         Failed | 1.2s | Error message...
```

#### `RenderFullReport(SagaVisualizationSnapshot snapshot)`
Renders a full execution report combining header, progress, and state diagram.

**Parameters:**
- `snapshot`: The visualization snapshot to render

**Returns:**
A comprehensive report string containing:
- Saga name and ID
- Correlation ID and current status
- Progress bar and elapsed time
- Failure reason (if applicable)
- Detailed step-by-step state diagram

#### `RenderDot(SagaOrchestrator.Core.Domain.Models.Saga saga)`
Renders a Graphviz DOT digraph of the saga steps.

**Parameters:**
- `saga`: The saga to render

**Returns:**
A string containing Graphviz DOT syntax representing the saga as a directed graph.

## Usage Example

```csharp
// Assuming you have a saga visualization snapshot
var snapshot = GetSagaVisualizationSnapshot();

// Create renderer instance (typically via dependency injection)
var renderer = new SagaStateRenderer(logger);

// Render different views
string progressBar = renderer.RenderProgressBar(snapshot);
// Output: [████████░░░░░░░░] 60.0% (3/5) steps

string stateDiagram = renderer.RenderStateDiagram(snapshot);
// Output: 
//  [✓]  1. ValidateOrder      Completed | 120ms
//  [►]  2. ProcessPayment    Executing | retried 1x
//  [○]  3. ReserveInventory  WaitingForRetry

string fullReport = renderer.RenderFullReport(snapshot);
// Output: Full formatted report with header, progress, and diagram

// For Graphviz visualization
string dotGraph = renderer.RenderDot(sagaModel);
// Can be used with Graphviz tools to generate PNG/SVG visualizations
```

## Status Icons

The renderer uses the following icons to represent step status:
- `✓` - Completed
- `►` - Executing
- `✗` - Failed
- `↩` - Compensated
- `⟳` - WaitingForRetry
- `⏱` - TimedOut
- `–` - Skipped
- `○` - Unknown/Other

## Implementation Notes

- All render methods are pure and produce deterministic output for a given snapshot
- Methods handle null arguments by throwing `ArgumentNullException`
- Internal errors are logged and return fallback strings like "(diagram unavailable)"
- Progress bar uses a fixed width of 20 characters with solid (`█`) and light (`░`) blocks
- State diagrams align step names and show duration, retry counts, and error messages when available