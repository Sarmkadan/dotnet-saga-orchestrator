# InMemorySagaStepRepository
The `InMemorySagaStepRepository` is a repository implementation that stores saga steps in memory, providing a simple and lightweight way to manage saga steps. It is designed to be used in scenarios where data persistence is not required or where the data can be easily recreated. This repository provides basic CRUD operations for saga steps, allowing for creation, retrieval, update, and deletion of saga steps.

## API
The `InMemorySagaStepRepository` provides the following public members:
* `GetByIdAsync`: Retrieves a saga step by its ID. Returns a `SagaStep` object if found, or `null` if not found. Throws an exception if the ID is invalid.
* `CreateAsync`: Creates a new saga step. Returns the created `SagaStep` object. Throws an exception if the creation fails.
* `UpdateAsync`: Updates an existing saga step. Returns the updated `SagaStep` object. Throws an exception if the update fails.
* `DeleteAsync`: Deletes a saga step by its ID. Returns `true` if the deletion is successful, `false` otherwise. Throws an exception if the ID is invalid.
* `GetBySagaIdAsync`: Retrieves a list of saga steps by saga ID. Returns a list of `SagaStep` objects. Throws an exception if the saga ID is invalid.
* `GetAllAsync`: Retrieves all saga steps. Returns a list of `SagaStep` objects.
* `GetByOrderAsync`: Retrieves a saga step by its order. Returns a `SagaStep` object if found, or `null` if not found. Throws an exception if the order is invalid.
* `GetByStatusAsync`: Retrieves a list of saga steps by status. Returns a list of `SagaStep` objects. Throws an exception if the status is invalid.

## Usage
Here are two examples of using the `InMemorySagaStepRepository`:
```csharp
// Create a new saga step
var repository = new InMemorySagaStepRepository();
var sagaStep = await repository.CreateAsync(new SagaStep { Id = Guid.NewGuid(), SagaId = Guid.NewGuid(), Order = 1, Status = SagaStepStatus.Pending });

// Retrieve all saga steps
var allSagaSteps = await repository.GetAllAsync();
foreach (var step in allSagaSteps)
{
    Console.WriteLine($"Saga Step ID: {step.Id}, Saga ID: {step.SagaId}, Order: {step.Order}, Status: {step.Status}");
}
```

## Notes
The `InMemorySagaStepRepository` stores data in memory, which means that all data will be lost when the application restarts. This repository is not suitable for production use where data persistence is required. Additionally, this repository is not thread-safe, and concurrent access to the repository may result in unexpected behavior. It is recommended to use a thread-safe repository implementation in multi-threaded environments. The `InMemorySagaStepRepository` is intended for testing and development purposes only.
