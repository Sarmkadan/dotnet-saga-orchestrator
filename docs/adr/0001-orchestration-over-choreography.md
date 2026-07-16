# ADR 0001: Why orchestration instead of choreography?

**Status:** Accepted

## Why did we pick a central orchestrator rather than event choreography?

Because the workflows this library targets - order processing, money transfer, travel booking - have
non-trivial ordering, conditional rollback, and a strong need to answer "where is this transaction right
now?" at any moment.

With choreography each service reacts to events and there is no single place that knows the whole saga's
state. That is attractive for loose coupling, but it makes three things hard that we care about a lot:

1. **Observability.** A support engineer asking "why did order #123 get stuck?" has to reconstruct the flow
   from scattered event logs across services.
2. **Compensation ordering.** Reverse-order rollback needs a component that knows the exact sequence of
   completed steps. In choreography that knowledge is implicit and duplicated.
3. **Testability.** A deterministic state machine in one process is straightforward to unit- and
   integration-test; an emergent flow across N brokers is not.

The orchestrator (`SagaOrchestrationService`) owns the state machine explicitly: it advances one step at a
time, persists the saga after every transition, and hands rollback to `CompensationService`.

## What is the cost we accepted?

The orchestrator is a component that must stay available and can become a bottleneck. We mitigate this by
keeping it stateless between calls (all state lives in the repositories) so it scales horizontally, and by
persisting after each step so a crashed orchestrator can resume from the last checkpoint.
