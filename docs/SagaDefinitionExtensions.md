# SagaDefinitionExtensions
The `SagaDefinitionExtensions` class provides a set of extension methods for working with `SagaDefinition` objects, allowing for the creation, modification, and inspection of saga definitions in a fluent and intuitive manner. These extensions enable developers to define and manage complex business processes and workflows in a straightforward and efficient way.

## API
* `public static SagaDefinition Create`: Creates a new `SagaDefinition` instance. This method returns a new `SagaDefinition` object and does not throw any exceptions.
* `public static void AddSteps`: Adds one or more steps to an existing `SagaDefinition`. This method takes a `SagaDefinition` instance and one or more `SagaStepDefinition` instances as parameters. It does not return any value and does not throw any exceptions.
* `public static int GetStepCount`: Returns the number of steps in a `SagaDefinition`. This method takes a `SagaDefinition` instance as a parameter and returns the number of steps as an integer. It does not throw any exceptions.
* `public static bool ContainsStep`: Checks if a `SagaDefinition` contains a specific step. This method takes a `SagaDefinition` instance and a `SagaStepDefinition` instance as parameters and returns a boolean value indicating whether the step is present. It does not throw any exceptions.
* `public static SagaStepDefinition? GetFirstStep`: Returns the first step in a `SagaDefinition`, or `null` if the definition is empty. This method takes a `SagaDefinition` instance as a parameter and returns the first step as a `SagaStepDefinition` instance, or `null` if no steps are present. It does not throw any exceptions.
* `public static SagaStepDefinition? GetLastStep`: Returns the last step in a `SagaDefinition`, or `null` if the definition is empty. This method takes a `SagaDefinition` instance as a parameter and returns the last step as a `SagaStepDefinition` instance, or `null` if no steps are present. It does not throw any exceptions.

## Usage
The following examples demonstrate how to use the `SagaDefinitionExtensions` class to create and manage saga definitions:
```csharp
// Create a new saga definition
var definition = SagaDefinitionExtensions.Create();

// Add steps to the definition
SagaDefinitionExtensions.AddSteps(definition, 
    new SagaStepDefinition("Step 1"), 
    new SagaStepDefinition("Step 2"));

// Check the number of steps in the definition
int stepCount = SagaDefinitionExtensions.GetStepCount(definition);
Console.WriteLine($"Step count: {stepCount}");

// Check if a specific step is present in the definition
bool containsStep = SagaDefinitionExtensions.ContainsStep(definition, new SagaStepDefinition("Step 1"));
Console.WriteLine($"Contains step: {containsStep}");
```

```csharp
// Create a new saga definition with multiple steps
var definition = SagaDefinitionExtensions.Create();
SagaDefinitionExtensions.AddSteps(definition, 
    new SagaStepDefinition("Step 1"), 
    new SagaStepDefinition("Step 2"), 
    new SagaStepDefinition("Step 3"));

// Get the first and last steps in the definition
var firstStep = SagaDefinitionExtensions.GetFirstStep(definition);
var lastStep = SagaDefinitionExtensions.GetLastStep(definition);

Console.WriteLine($"First step: {firstStep?.Name}");
Console.WriteLine($"Last step: {lastStep?.Name}");
```

## Notes
When using the `SagaDefinitionExtensions` class, note that the `AddSteps` method does not check for duplicate steps, so it is possible to add the same step multiple times to a definition. Additionally, the `GetFirstStep` and `GetLastStep` methods return `null` if the definition is empty, so it is essential to check for `null` before attempting to access the step's properties. The `SagaDefinitionExtensions` class is designed to be thread-safe, but it is still important to ensure that the underlying `SagaDefinition` instances are properly synchronized if accessed from multiple threads.
