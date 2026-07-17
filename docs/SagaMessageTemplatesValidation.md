# SagaMessageTemplatesValidation
The `SagaMessageTemplatesValidation` class provides a set of predefined validation templates for saga messages. These templates can be used to validate the state of a saga at various points in its lifecycle, such as when it is created, when a step is started or completed, or when the saga times out. By using these templates, developers can ensure that their sagas are properly validated and that any errors or inconsistencies are caught and handled.

## API
The `SagaMessageTemplatesValidation` class contains the following public static members:
* `ValidateSagaCreated`: Returns a list of validation messages for when a saga is created.
* `ValidateStepStarted`: Returns a list of validation messages for when a step is started.
* `ValidateStepCompleted`: Returns a list of validation messages for when a step is completed.
* `ValidateStepFailed`: Returns a list of validation messages for when a step fails.
* `ValidateSagaCompleted`: Returns a list of validation messages for when a saga is completed.
* `ValidateSagaFailed`: Returns a list of validation messages for when a saga fails.
* `ValidateCompensationStarted`: Returns a list of validation messages for when compensation is started.
* `ValidateCompensationCompleted`: Returns a list of validation messages for when compensation is completed.
* `ValidateSagaTimeout`: Returns a list of validation messages for when a saga times out.
* `ValidateDefinitionInvalid`: Returns a list of validation messages for when a saga definition is invalid.
* `ValidateServiceHealth`: Returns a list of validation messages for when a service's health is being validated.
* `ValidateWebhookDelivery`: Returns a list of validation messages for when a webhook delivery is being validated.
All of these members return an `IReadOnlyList<string>`, which contains the validation messages. None of these members take any parameters or throw any exceptions.

## Usage
Here are two examples of how to use the `SagaMessageTemplatesValidation` class:
```csharp
// Example 1: Validate a saga creation
var validationMessages = SagaMessageTemplatesValidation.ValidateSagaCreated;
foreach (var message in validationMessages)
{
    Console.WriteLine(message);
}

// Example 2: Validate a step completion
var validationMessages2 = SagaMessageTemplatesValidation.ValidateStepCompleted;
if (validationMessages2.Count > 0)
{
    Console.WriteLine("Validation failed:");
    foreach (var message in validationMessages2)
    {
        Console.WriteLine(message);
    }
}
```

## Notes
The `SagaMessageTemplatesValidation` class is designed to be thread-safe, as all of its members are static and do not modify any shared state. However, the lists of validation messages returned by these members should not be modified, as they are read-only. Additionally, the validation messages themselves are simply strings and do not contain any sensitive information, so they can be safely logged or displayed to users. It is also worth noting that these validation templates are just a starting point, and developers may need to add additional validation logic specific to their own use cases.
