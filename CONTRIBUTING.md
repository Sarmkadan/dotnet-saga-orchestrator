# Contributing to dotnet-saga-orchestrator

Thank you for considering contributing to dotnet-saga-orchestrator! It's people like you that make the open-source community such a great place to learn, inspire, and create.

## Development Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- A code editor with C# support (Visual Studio, VS Code with C# extension, or JetBrains Rider)

## Building Locally

```bash
# Clone the repository
git clone https://github.com/sarmkadan/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator

# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build --configuration Release
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with TRX log output
dotnet test --logger "trx;LogFileName=test-results.trx"

# Run a specific test project
dotnet test tests/dotnet-saga-orchestrator.Tests/
```

## How to Contribute

### 1. Fork and Clone

Fork the repository on GitHub, then clone your fork locally:

```bash
git clone https://github.com/your-username/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator
```

### 2. Create a Branch

Create a branch for your feature or bug fix:

```bash
git checkout -b feature/your-feature-name
```

### 3. Make Your Changes

- Follow the existing code style defined in `.editorconfig`.
- Include XML documentation comments for all public APIs and classes.
- Write or update unit tests for any logic you add or modify.
- Ensure all existing tests continue to pass.

### 4. Code Style

This project uses `.editorconfig` to enforce consistent formatting. Key conventions:

- 4-space indentation for C# files
- PascalCase for public members, types, and constants
- camelCase for private/protected members
- Prefix interfaces with `I`, type parameters with `T`
- Prefer expression bodies and pattern matching where appropriate

### 5. Submit a Pull Request

Push your branch to your fork and open a Pull Request against `main`. In your PR description:

- Clearly describe what you changed and why
- Reference any related issues (e.g., `Fixes #123`)
- Include any relevant test results or screenshots

## Reporting Issues

Use GitHub Issues to report bugs or suggest features. When reporting a bug, include:

- A clear and descriptive title
- Steps to reproduce the issue
- Expected vs. actual behavior
- .NET version and OS details
- Any relevant logs or stack traces

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
