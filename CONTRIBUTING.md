# Contributing to dotnet-saga-orchestrator

Thanks for helping out. This guide covers the basics: setup, build and test commands, code conventions, and the PR process.

## Prerequisites

- **.NET SDK 10.0** (the project targets `net10.0`; `global.json` pins `10.0.100` with `rollForward: latestMinor`)
- Git

Check your setup with `dotnet --version`.

## Build

```bash
dotnet restore
dotnet build -c Release    # or: make build
```

To run the demo in `Program.cs`: `dotnet run`.

## Test

```bash
dotnet test                # or: make test (after a Release build)
dotnet test --filter "FullyQualifiedName~SagaLifecycleTests"   # single test class
```

Tests live in `tests/dotnet-saga-orchestrator.Tests/` (xUnit, FluentAssertions, Moq, NSubstitute). Add tests for every feature and bug fix. Note that `TypeNamingConventionTests` fails the build if a public type has a doubled suffix (for example `FooExtensionsExtensions`).

## Code conventions

- **Style:** follow `.editorconfig` (4-space indentation, `var` only when the type is apparent, PascalCase for types and members, camelCase for locals). Run `dotnet format` before committing; `dotnet format --verify-no-changes` (`make check-format`) is the check.
- **Nullable:** `Nullable` is enabled. Do not suppress nullable warnings without a reason.
- **Warnings:** do not add new compiler or analyzer warnings. Check the build output for warnings in the files you touched.
- **Namespaces:** use file-scoped namespaces (`namespace SagaOrchestrator.Core.Utilities;`). Root namespace is `SagaOrchestrator`; sub-namespaces mirror folders.
- **Async:** async methods end in `Async`.
- **Docs:** add XML doc comments to public members (`GenerateDocumentationFile` is on). New public types also need a page under `docs/<TypeName>.md` and a short section in `README.md`.
- **Layout:** the main project is the single `dotnet-saga-orchestrator.csproj` at the repo root. Put new production code under `src/` and tests under `tests/`. Read `CLAUDE.md` for the full folder map.

## Branches and commits

- Branch from `main`. Do not commit directly to `main`.
- Name branches `<type>/<short-kebab-description>`, for example `feat/retry-jitter`, `fix/null-get-by-id`, or `docs/contributing`.
- Write commit messages in the Conventional Commits style used in the history: `feat: ...`, `fix: ...`, `docs: ...`, `chore: ...`.

## Pull requests

1. Keep PRs small and focused on one change. Split unrelated work into separate PRs.
2. Open the PR against `main`.
3. Describe what changed and why, and link any related issue.
4. Make sure CI passes. The `CI` workflow (`.github/workflows/ci.yml`) runs restore, a Release build, and the tests. A PR with a failing build or tests will not be merged.
5. Respond to review comments by pushing new commits to the same branch.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
