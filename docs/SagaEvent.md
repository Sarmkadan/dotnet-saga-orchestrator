# SagaEvent
Represents a single event within a saga workflow, capturing identifiers, timestamps, severity, and optional contextual data for tracking and debugging purposes.

## API
### Id
**Type:** `string`  
**Purpose:** Unique identifier for the event instance.  
**Remarks:** Should be set to a globally unique value (e.g., a GUID) when the event is created.

### SagaId
**Type:** `string`  
**Purpose:** Identifier of the saga to which this event belongs.  
**Remarks:** All events belonging to the same saga share the same SagaId.

### EventType
**Type:** `string`  
**Purpose:** High‑level category of the event (e.g., "Lifecycle", "Step", "Error").  
**Remarks:** Often used by listeners to filter events.

### EventName
**Type:** `string`  
**Purpose:** Specific name of the event within its EventType (e.g., "Started", "Completed", "ItemAdded").  
**Remarks:** Provides finer granularity than EventType.

### Description
**Type:** `string`  
**Purpose:** Human‑readable summary of what the event signifies.  
**Remarks:** Intended for logging and diagnostics; may be empty.

### Timestamp
**Type:** `DateTime`  
**Purpose:** Point in time when the event occurred.  
**Remarks:** Typically set to `DateTime.UtcNow` at creation; should not be modified after instantiation.

### Severity
**Type:** `EventSeverity`  
**Purpose:** Indicates the importance or impact level of the event.  
**Remarks:** Values such as Info, Warning, Error guide downstream handling.

### Data
**Type:** `Dictionary<string, object>`  
**Purpose:** Arbitrary payload associated with the event.  
**Remarks:** Consumers can read or inspect values; the dictionary is initialized by the constructor.

### StepId
**Type:** `string?`  
**Purpose:** Optional identifier of the workflow step that produced the event.  
**Remarks:** May be `null` for events not tied to a specific step.

### StepName
**Type:** `string?`  
**Purpose:** Optional human‑readable name of the workflow step.  
**Remarks:** May be `null` when StepId is absent.

### Source
**Type:** `string`  
**Purpose:** Name of the component or service that raised the event.  
**Remarks:** Helps trace the origin of the event in distributed systems.

### CorrelationId
**Type:** `string?`  
**Purpose:** Optional identifier used to relate events across different sagas or services.  
**Remarks:** Useful for end‑to‑end tracing; may be `null`.

### SagaEvent
**Type:** Constructor  
**Purpose:** Creates a new, empty `SagaEvent` instance.  
**Remarks:** Initializes `Data` as an empty dictionary; other members have default values (`null` for reference types, `DateTime.MinValue` for `Timestamp`).

### CreateLifecycleEvent
**Type:** `static SagaEvent`  
**Purpose:** Factory method that returns a `SagaEvent` pre‑configured for lifecycle notifications.  
**Remarks:** The returned instance has `EventType` set to a lifecycle‑specific value; callers should set `Id`, `SagaId`, `Timestamp`, and any additional properties as needed.

### CreateStepEvent
**Type:** `static SagaEvent`  
**Purpose:** Factory method that returns a `SagaEvent` pre‑configured for step‑level notifications.  
**Remarks:** The returned instance has `EventType` set to a step‑specific value; `StepId` and `StepName` can be supplied by the caller.

### CreateErrorEvent
**Type:** `static SagaEvent`  
**Purpose:** Factory method that returns a `SagaEvent` pre‑configured for error conditions.  
**Remarks:** The returned instance has `Severity` set to `EventSeverity.Error`; callers should populate `Description` and optionally `Data` with failure details.

### AddData
**Type:** `void`  
**Purpose:** Inserts a key‑value pair into the event’s `Data` dictionary.  
**Parameters:**  
- `key` (string) – The identifier for the datum; must not be `null`.  
- `value` (object) – The associated value; may be `null`.  
**Exceptions:**  
- Throws `ArgumentNullException` if `key` is `null`.  
**Remarks:** Does not overwrite existing entries; if the key already exists, the value is replaced.

## Usage
```csharp
// Creating a lifecycle event via the factory
var lifecycleEvent
var lifecycle = SagaEvent.CreateLifecycleEvent();
lifecycle.Id = Guid.NewGuid().ToString();
lifecycle.SagaId = saga.Id;
lifecycle.EventName = "Started";
lifecycle.Timestamp = DateTime.UtcNow;
lifecycle.Source = "OrderProcessor";
lifecycle.AddData("CustomerId", customer.Id);

// Creating a step‑specific error event
var stepError = SagaEvent.CreateErrorEvent();
stepError.Id = Guid.NewGuid().ToString();
stepError.SagaId = saga.Id;
stepError.StepId = step.Id;
stepError.StepName = "ValidatePayment";
stepError.Description = "Payment gateway returned insufficient funds.";
stepError.Source = "PaymentService";
stepError.AddData("Amount", 49.95);
stepError.AddData("Currency", "USD");
```

## Notes
- The `Data` dictionary is instantiated by the constructor; accessing it before any call to `AddData` yields an empty collection.  
- String‑typed members (`Id`, `SagaId`, `EventType`, `EventName`, `Description`, `Source`) and the nullable members (`StepId`, `StepName`, `CorrelationId`) can be set to `null`; however, leaving `Id` or `SagaId` as `null` may break correlation logic in consumers.  
- `Timestamp` is not automatically updated after construction; if a different time is required, assign it explicitly before publishing the event.  
- Instances of `SagaEvent` are **not** thread‑safe. Concurrent calls to `AddData` or property writes from multiple threads should be synchronized externally. Reading the properties after construction is safe provided no other thread is mutating the same instance.  
- The static factory methods return new instances; they do not rely on or modify any shared mutable state, making them safe to invoke concurrently.
