# SagaMessageTemplates

`SagaMessageTemplates` provides a centralized repository of string templates utilized throughout the saga orchestration process for logging, error reporting, and status notifications. These templates standardize message structure, ensuring uniform diagnostic output and system-wide consistency when reporting on saga states, step execution, and failure conditions.

## API

### Format
`public static string Format`
A standard template used for general saga status updates. This template is designed to accept positional arguments to populate context such as the saga name and its current state.

### Detailed
`public static string Detailed`
A verbose template intended for comprehensive diagnostic logging. It supports embedding structured information regarding state transitions, compensation actions, and associated error metadata.

### WithRetry
`public static string WithRetry`
A specific template utilized when a saga step is executed using retry logic. It provides formatted placeholders to report the current retry attempt, the total allowed retries, and the specific exception encountered.

### StepTimeout
`public static string StepTimeout`
A template reserved for reporting events where a saga step has exceeded its configured timeout threshold. It captures the step identifier and the timeout duration.

## Usage

```csharp
// Example: Using the Format template for standard logging
string statusMessage = string.Format(
    SagaMessageTemplates.Format, 
    "PaymentProcessingSaga", 
    "ExecutingCompensation"
);
Logger.Info(statusMessage);

// Example: Using the StepTimeout template for error reporting
string timeoutError = string.Format(
    SagaMessageTemplates.StepTimeout, 
    "InventoryReservationStep", 
    "30 seconds"
);
Logger.Error(timeoutError);
```

## Notes

- **String Formatting:** All members are static string templates containing standard .NET `string.Format` placeholders (e.g., `{0}`, `{1}`). Consumers must ensure the correct number and type of arguments are provided to avoid `FormatException`.
- **Thread Safety:** The members are immutable `static string` fields, which are inherently thread-safe for concurrent read operations.
- **Consumption:** These templates are intended to be consumed by the framework's internal logging infrastructure. Overriding or modifying these templates in derived projects is not supported and may lead to inconsistent log parsing.
