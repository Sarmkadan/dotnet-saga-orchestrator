# TimelineEntry

`TimelineEntry` functions as a structured record within the saga orchestration system, capturing significant events, state snapshots, or breakpoint interactions tied to a specific saga instance's lifecycle. It facilitates auditability and observability by persisting crucial contextual information, including timestamps, step identifiers, and associated metadata for saga execution analysis.

## API

- **EntryId** (`required string`): A unique identifier for the specific timeline entry.
- **Kind** (`required TimelineEntryKind`): Defines the categorization or type of the event recorded.
- **Timestamp** (`required DateTime`): The exact time the event occurred within the system.
- **Title** (`required string`): A concise summary of the event.
- **Description** (`required string`): A detailed explanation providing further context for the entry.
- **SnapshotId** (`string?`): The identifier of an associated state snapshot, if relevant to the entry.
- **StepName** (`string?`): The name of the saga step associated with this timeline entry.
- **Metadata** (`IReadOnlyDictionary<string, object>`): A collection of arbitrary key-value pairs containing additional diagnostic or context data.
- **BreakpointId** (`required string`): The identifier for the debugging breakpoint associated with this entry.
- **SagaId** (`required string`): The unique identifier for the saga instance this entry belongs to.
- **IsEnabled** (`bool`): Indicates the active status of the associated breakpoint.
- **CreatedAt** (`required DateTime`): The timestamp indicating when this record was generated.
- **HitCount** (`required int`): Tracks how many times the associated breakpoint has been triggered.
- **Note** (`string?`): An optional user-provided comment or description attached to the entry.

### Methods

- **static TimelineEntry FromSnapshot(...)**: Factory method to initialize a `TimelineEntry` based on a system state snapshot.
- **static TimelineEntry FromSagaEvent(...)**: Factory method to initialize a `TimelineEntry` derived from a specific saga event.
- **SagaDebugBreakpoint WithIncrementedHitCount()**: Returns a new `SagaDebugBreakpoint` instance with the `HitCount` property incremented by one.
- **SagaDebugBreakpoint WithEnabled(bool enabled)**: Returns a new `SagaDebugBreakpoint` instance with the `IsEnabled` property set to the specified value.

## Usage

### Creating a Timeline Entry from a Saga Event
```csharp
var entry = TimelineEntry.FromSagaEvent(
    sagaId: "order-123",
    stepName: "ProcessPayment",
    eventName: "PaymentReceived",
    metadata: new Dictionary<string, object> { { "Amount", 50.00 } }
);
```

### Modifying a Breakpoint Hit Count
```csharp
// Assuming an existing SagaDebugBreakpoint breakpoint
var updatedBreakpoint = breakpoint.WithIncrementedHitCount();
Console.WriteLine($"New hit count: {updatedBreakpoint.HitCount}");
```

## Notes

- **Immutability**: While the `SagaDebugBreakpoint` methods return new instances, the underlying properties of `TimelineEntry` are intended to represent a point-in-time record and should generally be treated as immutable once created.
- **Thread Safety**: This type is primarily designed for data transfer and representation. While instances themselves are thread-safe if treated as immutable, applications should ensure proper synchronization if these objects are shared and mutated across multiple threads during their construction or processing.
- **Validation**: Ensure all `required` properties are populated during initialization, as failure to do so may result in validation errors depending on the serialization or persistence layer used by the orchestrator.
