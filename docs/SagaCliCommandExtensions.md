# SagaCliCommandExtensions

The `SagaCliCommandExtensions` class provides a set of static extension methods designed to simplify the parsing, validation, and retrieval of command-line arguments within the context of a Saga orchestrator CLI. These utilities offer type-safe access to positional arguments and named options, handling nullability and conversion logic to reduce boilerplate code in command handlers while ensuring consistent argument processing patterns across the application.

## API

### `GetArgument` (Nullable)
```csharp
public static string? GetArgument(this ICommand command, int index)
```
Retrieves a positional argument at the specified zero-based index. If the index is within the bounds of the provided arguments, the value is returned as a string; otherwise, `null` is returned. This method does not throw exceptions for missing arguments, making it suitable for optional positional parameters.

### `GetArgument` (Non-Nullable)
```csharp
public static string GetArgument(this ICommand command, int index)
```
Retrieves a positional argument at the specified zero-based index, enforcing presence. If the argument exists at the given index, it is returned. If the index is out of range or the argument is missing, this method throws an exception indicating the required argument was not found. Use this when the argument is mandatory for command execution.

### `GetIntArgument`
```csharp
public static int? GetIntArgument(this ICommand command, int index)
```
Attempts to retrieve and parse a positional argument at the specified index as a 32-bit integer. If the argument exists and is a valid integer representation, the parsed value is returned. If the argument is missing or the parsing fails (e.g., non-numeric input), `null` is returned. No exceptions are thrown for format errors.

### `GetBooleanArgument`
```csharp
public static bool? GetBooleanArgument(this ICommand command, int index)
```
Attempts to retrieve and parse a positional argument at the specified index as a boolean value. Recognized true values typically include "true", "1", or "yes" (case-insensitive), while false values include "false", "0", or "no". If the argument is missing or cannot be parsed into a boolean, `null` is returned.

### `HasOption`
```csharp
public static bool HasOption(this ICommand command, string name)
```
Determines whether a specific named option (flag or key) is present in the command input. The `name` parameter should match the option identifier (e.g., "verbose" or "v"). Returns `true` if the option exists, regardless of whether it has an associated value; otherwise, returns `false`.

### `GetArguments`
```csharp
public static IReadOnlyDictionary<string, string> GetArguments(this ICommand command)
```
Extracts all named arguments provided in the command as a read-only dictionary. The keys represent the option names, and the values represent the associated string values. If an option is a flag without a value, the behavior depends on the underlying parser implementation, but typically such entries are either omitted or contain an empty string.

### `GetOptions`
```csharp
public static IReadOnlyList<string> GetOptions(this ICommand command)
```
Returns a read-only list of all option names present in the command. This is useful for iterating over provided flags or validating that no unexpected options were supplied. The list contains only the keys/names, not the values.

### `ValidateRequiredArguments`
```csharp
public static bool ValidateRequiredArguments(this ICommand command, params string[] requiredOptionNames)
```
Validates that all specified required option names are present in the command. The method accepts a variable number of strings representing the names of mandatory options. Returns `true` if all listed options are found; returns `false` if any required option is missing. This method performs a presence check and does not validate the values associated with the options.

### `ToLogString`
```csharp
public static string ToLogString(this ICommand command)
```
Generates a sanitized string representation of the command suitable for logging. This method serializes the command structure, including arguments and options, into a single string. Sensitive data (such as passwords or tokens passed as arguments) may be redacted depending on the underlying implementation to prevent security leaks in log files.

## Usage

### Example 1: Parsing Mixed Arguments with Validation
This example demonstrates retrieving a mandatory ID, an optional port number, and checking for a debug flag.

```csharp
using DotnetSagaOrchestrator.Cli;
using System.CommandLine;

public async Task<int> ExecuteAsync(ICommand command)
{
    // Retrieve mandatory first argument (Saga ID)
    string sagaId = command.GetArgument(0); 
    
    // Attempt to retrieve optional port as integer
    int? port = command.GetIntArgument(1);
    if (port.HasValue && (port < 1024 || port > 65535))
    {
        Console.Error.WriteLine("Port must be between 1024 and 65535.");
        return 1;
    }

    // Check for optional debug flag
    bool isDebug = command.GetBooleanArgument(2) ?? false;
    
    // Validate that the '--environment' option is provided
    if (!command.ValidateRequiredArguments("environment"))
    {
        Console.Error.WriteLine("The '--environment' option is required.");
        return 1;
    }

    string environment = command.GetArguments()["environment"];
    
    Console.WriteLine($"Starting Saga {sagaId} on port {port ?? 8080} in {environment} mode (Debug: {isDebug})");
    return 0;
}
```

### Example 2: Auditing and Logging Command Input
This example shows how to inspect all provided options and generate a safe log entry before processing.

```csharp
using DotnetSagaOrchestrator.Cli;
using System.CommandLine;
using Microsoft.Extensions.Logging;

public void ProcessCommand(ICommand command, ILogger logger)
{
    // Log the command structure safely (sensitive data redacted)
    logger.LogInformation("Received command: {CommandData}", command.ToLogString());

    // Retrieve all specific options provided by the user
    var options = command.GetOptions();
    
    if (options.Contains("dry-run"))
    {
        logger.LogWarning("Dry-run mode detected. No changes will be persisted.");
    }

    // Access the full dictionary for complex processing
    var args = command.GetArguments();
    if (args.TryGetValue("timeout", out string? timeoutValue))
    {
        // Further parsing logic here
        logger.LogDebug("Custom timeout specified: {Timeout}", timeoutValue);
    }
}
```

## Notes

*   **Index Out of Range**: The non-nullable `GetArgument(int index)` method will throw an exception if the requested index does not exist. Always ensure the argument count is sufficient or use the nullable overload `GetArgument?(int index)` when dealing with optional positional parameters.
*   **Parsing Failures**: `GetIntArgument` and `GetBooleanArgument` return `null` rather than throwing exceptions when parsing fails. Callers must explicitly check for `null` before using the returned values to avoid `NullReferenceException`.
*   **Thread Safety**: As this class consists entirely of static methods that operate on the passed `ICommand` instance without maintaining internal mutable state, the methods themselves are thread-safe. However, thread safety regarding the `ICommand` object passed into these methods depends on the implementation of that interface; if the command object is mutated concurrently by other threads, race conditions may occur during retrieval.
*   **Option Name Matching**: The `HasOption`, `ValidateRequiredArguments`, and dictionary lookups rely on exact string matching for option names. Ensure consistency in naming conventions (e.g., handling leading dashes like `--verbose` vs `verbose`) based on how the underlying `ICommand` parser normalizes input.
*   **Logging Security**: While `ToLogString` attempts to sanitize output, developers should verify its behavior against specific sensitive argument names used in their Sagas. Do not assume automatic redaction for custom secret keys without testing.
