# ISagaEventObserver

The `ISagaEventObserver` interface defines a contract for observing and reacting to key lifecycle events in a saga orchestration process. Implementations of this interface can be registered with a saga orchestrator to receive asynchronous notifications when significant events occur, such as saga creation, completion, failure, or compensation initiation.

## API

### `public SagaEventObserver`

A marker type indicating that the implementing class is an event observer for saga orchestration events. This type is used internally by the orchestrator to identify and register observers.

### `public async Task OnSagaCreatedAsync`

Notifies the observer that a new saga instance has been created.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: May throw if the observer encounters an unrecoverable error during processing. The saga orchestrator may handle or propagate this exception depending on its configuration.

### `public async Task OnSagaCompletedAsync`

Notifies the observer that a saga instance has completed all its steps successfully.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: May throw if the observer encounters an unrecoverable error during processing. The saga orchestrator may handle or propagate this exception depending on its configuration.

### `public async Task OnSagaFailedAsync`

Notifies the observer that a saga instance has failed, either due to an unhandled exception or a step rejection.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: May throw if the observer encounters an unrecoverable error during processing. The saga orchestrator may handle or propagate this exception depending on its configuration.

### `public async Task OnCompensationStartedAsync`

Notifies the observer that compensation for a failed saga instance has begun.

- **Parameters**: None.
- **Return value**: A `Task` representing the asynchronous operation.
- **Exceptions**: May throw if the observer encounters an unrecoverable error during processing. The saga orchestrator may handle or propagate this exception depending on its configuration.

## Usage

### Example 1: Logging Saga Events
