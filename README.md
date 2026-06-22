# dotnet-saga-orchestrator

A production-ready distributed saga orchestrator for .NET microservices implementing the Saga pattern with compensating transactions.

![Build](https://github.com/sarmkadan/dotnet-saga-orchestrator/actions/workflows/build.yml/badge.svg)
![License](https://img.shields.io/badge/License-MIT-blue.svg)

## Installation

### Method 1: Clone from Repository

```bash
git clone https://github.com/Sarmkadan/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator
dotnet build
```

### Method 2: Add NuGet Package

```bash
dotnet add package Zaiets.dotnet.saga.orchestrator
```

## Quick Start

```csharp
using Microsoft.Extensions.DependencyInjection;
using SagaOrchestrator.Application.Services;

// Setup dependency injection
var services = new ServiceCollection();
services.AddSagaOrchestrator();
var provider = services.BuildServiceProvider();

// Get services
var definitionService = provider.GetRequiredService<SagaDefinitionService>();
var orchestration = provider.GetRequiredService<SagaOrchestrationService>();

// Create saga definition
var definition = await definitionService.CreateDefinitionAsync("Order Processing", "Process orders");

// Add a step
await definitionService.AddStepAsync(definition.Id, new SagaStepDefinition(
    "Reserve Inventory", "inventory-service", "http://inventory/reserve", "http://inventory/release"));

// Execute saga
var saga = await orchestration.CreateSagaAsync(definition);
await orchestration.StartSagaAsync(saga.Id);
await orchestration.ExecuteNextStepAsync(saga.Id);
```

## Configuration

The orchestrator can be configured via `ServiceCollection` extensions:

```csharp
var services = new ServiceCollection();
services.AddSagaOrchestrator()
    .WithDefaultStepTimeout(60)
    .WithDefaultMaxRetries(5)
    .Build();
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
