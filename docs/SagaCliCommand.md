# SagaCliCommand

Represents a parsed command-line invocation for the `dotnet-saga-orchestrator` tool. It encapsulates the command type, its named arguments, and boolean options, along with validation state. Instances are typically created by the static `Parse` method and then inspected for correctness before execution.

## API

### `public string CommandType`

The name of the command (e.g., `"run"`, `"list"`). This value is extracted from the first token of the input string. It is read-only after construction; to change it a new instance must be created.

### `public Dictionary<string, string> Arguments`

A dictionary of key-value pairs parsed from the command line. Keys are argument names (without leading dashes), values are the associated values. For example, `--name Alice` produces an entry with key `"name"` and value `"Alice"`. The dictionary is never `null` but may be empty.

### `public List<string> Options`

A list of boolean flags that were present on the command line. Each entry is the flag name without leading dashes (e.g., `"verbose"`, `"dry-run"`). The list is never `null` but may be empty.

### `public bool IsValid`

Indicates whether the parsed command passed all built-in validation rules. Validation is performed during `Parse` and the result is stored in this property. A value of `false` means one or more errors are present in `ValidationErrors`.

### `public List<string> ValidationErrors`

A list of human-readable error messages describing why the command is invalid. If `IsValid` is `true`, this list is empty. The list is never `null`.

### `public static SagaCliCommand Parse(string input)`

Parses a command-line string into a `SagaCliCommand` instance.

- **Parameters**  
  `input`: The raw command-line string to parse (e.g., `"run --name test --verbose"`).

- **Returns**  
  A new `SagaCliCommand` instance. The `IsValid` property will be `true` only if the input conforms to expected syntax and required rules.

- **Throws**  
  `ArgumentNullException` if `input` is `null`.  
  Does not throw for malformed input; instead, `IsValid` is set to `false` and errors are recorded in `ValidationErrors`.

### `public string GetHelpText()`

Returns a formatted help string describing the usage of the command represented by this instance.

- **Parameters**  
  None.

- **Returns**  
  A multi-line string containing a usage synopsis, description of arguments and options, and any command-specific notes. The exact content depends on the `CommandType` and the current tool configuration.

- **Throws**  
  `InvalidOperationException` if the instance is in an invalid state (i.e., `IsValid` is `false`). Callers should check `IsValid` before calling this method.

## Usage

### Example 1: Parsing a command and checking validity

```csharp
string input = "run --name my-saga --verbose";
SagaCliCommand cmd = SagaCliCommand.Parse(input);

if (cmd.IsValid)
{
    Console.WriteLine($"Command: {cmd.CommandType}");
    foreach (var arg in cmd.Arguments)
        Console.WriteLine($"  {arg.Key} = {arg.Value}");
    foreach (var opt in cmd.Options)
        Console.WriteLine($"  Option: {opt}");
}
else
{
    Console.WriteLine("Invalid command:");
    foreach (var err in cmd.ValidationErrors)
        Console.WriteLine($"  - {err}");
}
```

### Example 2: Displaying help for a valid command

```csharp
string input = "list";
SagaCliCommand cmd = SagaCliCommand.Parse(input);

if (cmd.IsValid)
{
    string help = cmd.GetHelpText();
    Console.WriteLine(help);
}
else
{
    Console.WriteLine("Cannot show help for an invalid command.");
}
```

## Notes

- **Edge cases**  
  - An empty string (`""`) or whitespace-only input produces a `SagaCliCommand` with `CommandType` set to an empty string, `IsValid` set to `false`, and a validation error indicating that a command type is required.  
  - Duplicate argument keys (e.g., `--name A --name B`) are not allowed; the parser records a validation error and sets `IsValid` to `false`.  
  - Options that appear multiple times are treated as a single occurrence; the `Options` list will contain the flag only once.  
  - Values containing spaces must be quoted (e.g., `--label "my label"`). Unquoted spaces cause the parser to treat subsequent tokens as separate arguments, which may lead to validation errors.

- **Thread safety**  
  Instances of `SagaCliCommand` are not thread-safe. Their properties are mutable only during construction (via `Parse`). After construction, the properties are effectively read-only, but the underlying `Dictionary` and `List` objects are not synchronized. Concurrent reads from multiple threads are safe as long as no thread modifies the instance. If modification is required (e.g., adding custom validation errors), callers must provide their own synchronization.
