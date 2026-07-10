# ISagaStateRenderer

The `ISagaStateRenderer` interface provides a standardized contract for visualizing and exporting the current execution status of a saga orchestration. Implementations of this interface are responsible for transforming complex saga state data into human-readable formats, including progress indicators, state diagrams, and detailed textual reports suitable for logging, monitoring, or UI components.

## API

### SagaStateRenderer
Initializes a new instance of a class that implements `ISagaStateRenderer`, configuring it with the necessary dependencies to access saga orchestration state.

### RenderProgressBar
Generates a string representation of the saga's progress, typically formatted as a graphical progress bar.
- **Returns:** A `string` containing the visual progress bar representation.
- **Throws:** `InvalidOperationException` if the underlying saga state is unavailable or improperly initialized.

### RenderStateDiagram
Generates a string representation of the current saga state machine, illustrating the transitions and current active steps.
- **Returns:** A `string` containing the state diagram in a format such as Mermaid or Graphviz.
- **Throws:** `InvalidOperationException` if the diagram cannot be generated from the current saga definition.

### RenderFullReport
Generates a detailed, comprehensive report covering the current state, history of completed steps, and any failures or compensation activities associated with the saga.
- **Returns:** A `string` containing the full report.
- **Throws:** `ArgumentException` if formatting options provided to the renderer are invalid.

## Usage

### Example 1: Basic Console Output
```csharp
public void DisplaySagaStatus(ISagaStateRenderer renderer)
{
    Console.WriteLine("Progress: " + renderer.RenderProgressBar());
    Console.WriteLine("Summary: " + renderer.RenderFullReport());
}
```

### Example 2: Exporting a State Diagram for Monitoring
```csharp
public async Task SaveSagaVisualization(ISagaStateRenderer renderer, string filePath)
{
    string diagram = renderer.RenderStateDiagram();
    await File.WriteAllTextAsync(filePath, diagram);
}
```

## Notes

- **Thread-Safety:** Implementations of this interface are intended to be thread-safe for reading state. However, if the underlying saga state is being modified concurrently, the output of these methods may reflect a transient snapshot rather than a point-in-time consistent state.
- **Edge Cases:** If a saga is in an uninitialized or faulted state, the renderer should return descriptive placeholder strings rather than throwing exceptions, unless the underlying state is completely inaccessible.
- **Formatting:** The returned strings are intended for display purposes; formatting is implementation-defined and may vary depending on the configured culture or output medium.
