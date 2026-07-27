#!/bin/bash

# Build the SagaActivitySource library
dotnet build SagaActivitySource.csproj

# Run the unit tests
dotnet test SagaActivitySourceTests.csproj
