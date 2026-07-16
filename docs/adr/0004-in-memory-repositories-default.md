# ADR 0004: Why ship in-memory repositories as the default persistence?

**Status:** Accepted

## Why does `AddSagaOrchestrator()` register in-memory repositories out of the box?

Because the orchestrator depends on repository *interfaces* (`ISagaRepository`, `ISagaStepRepository`,
`ICompensationTransactionRepository`, `ISagaDefinitionRepository`), and shipping a working in-memory
implementation lets a developer clone the repo, register the services, and run a real saga end to end with
zero infrastructure.

That matters for two audiences:

1. **Evaluators and the test suite.** Every integration test in this repo runs against the in-memory
   repositories. They are fast, deterministic, and require no database container, which keeps CI simple.
2. **New adopters.** The quickstart in the README is copy-paste runnable. First impressions of a saga library
   should not begin with "provision a database".

## What is the migration path to production?

Persistence is a seam, not a decision baked into the core. Because everything is coded against the interfaces,
a production deployment supplies its own EF Core / SQL / document-store implementations and registers them
*after* (or instead of) `AddSagaOrchestrator()`, overriding the in-memory singletons. `AddSagaServices()` and
`AddSagaRepositories()` exist precisely to let callers compose the service layer and the persistence layer
independently.

The one property that a durable implementation must preserve is the ordering contract that compensation
relies on: `GetBySagaIdAsync` returns transactions so the strategy can pick the correct next one. The
in-memory version documents that expectation by ordering results, and the tests assert it.
