# CliHandler

Command-line interface handler for saga operations. Parses CLI commands and dispatches to appropriate services.

## Purpose

The `CliHandler` class implements the `ICliHandler` interface to provide command-line interface functionality for managing sagas in the Saga Orchestrator system. It parses command-line arguments, validates them, and routes them to the appropriate service methods for saga creation, execution, status checking, listing, and compensation.

## Public API

### Interface: `ICliHandler`

```csharp
Task<int> HandleCommandAsync(string[] args);
```

Parses and dispatches a single CLI invocation.

**Parameters:**
- `args`: The raw command-line arguments.

**Returns:**
- A process exit code: `0` on success, non-zero on failure.

### Class: `CliHandler`

#### Constructor

```csharp
public CliHandler(
    SagaOrchestrationService orchestrationService,
    SagaDefinitionService definitionService,
    ISagaLogger sagaLogger,
    IOutputFormatter outputFormatter,
    ILogger<CliHandler> logger)
```

Initializes a new `CliHandler` with its required services.

**Parameters:**
- `orchestrationService`: The saga orchestration service.
- `definitionService`: The saga definition service.
- `sagaLogger`: The saga logger used to render timelines.
- `outputFormatter`: The formatter used to render command output.
- `logger`: The diagnostic logger.

#### Methods

All methods are private implementation details except for `HandleCommandAsync` which implements the interface.

## Usage Example

```csharp
// In your application setup
var services = new ServiceCollection();
// Register required services...
var serviceProvider = services.BuildServiceProvider();

var cliHandler = serviceProvider.GetRequiredService<ICliHandler>();

// Process command line arguments
string[] args = Environment.GetCommandLineArgs().Skip(1).ToArray();
int exitCode = await cliHandler.HandleCommandAsync(args);
Environment.Exit(exitCode);
```

### Supported Commands

The CLI handler supports the following commands:

1. **create** - Create a new saga instance
   ```
   saga create --definition <definition-name> [--verbose]
   ```

2. **execute** - Execute the next step of a saga
   ```
   saga execute --saga-id <saga-id> [--async]
   ```

3. **status** - Get the status of a saga
   ```
   saga status --saga-id <saga-id> [--verbose]
   ```

4. **list** - List saga instances
   ```
   saga list [--limit <number>] [--filter <status>] [--json|--csv]
   ```

5. **compensate** - Initiate compensation for a saga
   ```
   saga compensate --saga-id <saga-id>
   ```

6. **help** - Display help information
   ```
   saga help
   ```

### Example Usage

Create a saga:
```bash
saga create --definition OrderProcessing
```

Execute a saga step:
```bash
saga execute --saga-id 123e4567-e89b-12d3-a456-426614174000
```

Check saga status:
```bash
saga status --saga-id 123e4567-e89b-12d3-a456-426614174000 --verbose
```

List sagas:
```bash
saga list --limit 10 --filter Running
```