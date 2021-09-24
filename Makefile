# =============================================================================
# Makefile for Saga Orchestrator
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build clean test run restore publish docker-build docker-run docker-clean lint format check-format

# Default target
help:
	@echo "Saga Orchestrator - Build Targets"
	@echo "=================================="
	@echo ""
	@echo "make build            Build the project"
	@echo "make clean            Remove build artifacts"
	@echo "make restore          Restore NuGet packages"
	@echo "make test             Run all tests"
	@echo "make run              Run the application"
	@echo "make publish          Publish release build"
	@echo "make docker-build     Build Docker image"
	@echo "make docker-run       Run Docker container"
	@echo "make docker-clean     Remove Docker image"
	@echo "make docker-compose   Run full stack with docker-compose"
	@echo "make lint             Run code analysis"
	@echo "make format           Format code"
	@echo "make check-format     Check code formatting"
	@echo "make examples         Run example projects"
	@echo "make docs             Generate documentation"
	@echo "make clean-all        Remove all artifacts and builds"
	@echo "make ci               Run CI pipeline (build, test, lint)"
	@echo ""

# Build target
build:
	@echo "Building project..."
	dotnet build -c Release
	@echo "✓ Build complete"

# Clean target
clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf bin/ obj/
	rm -rf src/*/bin src/*/obj
	@echo "✓ Clean complete"

# Restore packages
restore:
	@echo "Restoring NuGet packages..."
	dotnet restore
	@echo "✓ Restore complete"

# Run tests
test:
	@echo "Running tests..."
	dotnet test -c Release --no-build --verbosity minimal
	@echo "✓ Tests complete"

# Run application
run:
	@echo "Running application..."
	dotnet run

# Publish release
publish:
	@echo "Publishing release build..."
	dotnet publish -c Release -o ./publish
	@echo "✓ Publish complete (output: ./publish)"

# Build Docker image
docker-build:
	@echo "Building Docker image..."
	docker build -t saga-orchestrator:latest .
	docker tag saga-orchestrator:latest saga-orchestrator:1.2.0
	@echo "✓ Docker image built"
	@echo "  Tags: saga-orchestrator:latest, saga-orchestrator:1.2.0"

# Run Docker container
docker-run:
	@echo "Running Docker container..."
	docker run -d \
		--name saga-orchestrator \
		-p 5000:80 \
		-e SAGA_TIMEOUT_SECONDS=300 \
		-e SAGA_LOG_LEVEL=Information \
		saga-orchestrator:latest
	@echo "✓ Container running on http://localhost:5000"

# Stop and remove Docker container
docker-clean:
	@echo "Cleaning Docker..."
	docker stop saga-orchestrator 2>/dev/null || true
	docker rm saga-orchestrator 2>/dev/null || true
	docker rmi saga-orchestrator:latest saga-orchestrator:1.2.0 2>/dev/null || true
	@echo "✓ Docker cleaned"

# Docker Compose - full stack
docker-compose-up:
	@echo "Starting full stack with docker-compose..."
	docker-compose up -d
	@echo "✓ Services running:"
	@echo "  Saga Orchestrator: http://localhost:5000"
	@echo "  Redis: localhost:6379"
	@echo "  Prometheus: http://localhost:9090"
	@echo "  Grafana: http://localhost:3000"

docker-compose-down:
	@echo "Stopping docker-compose stack..."
	docker-compose down
	@echo "✓ Stack stopped"

docker-compose-logs:
	docker-compose logs -f

# Code analysis
lint:
	@echo "Running code analysis..."
	dotnet build /p:EnforceCodeStyleInBuild=true
	@echo "✓ Code analysis complete"

# Format code
format:
	@echo "Formatting code..."
	dotnet format
	@echo "✓ Code formatted"

# Check formatting
check-format:
	@echo "Checking code formatting..."
	dotnet format --verify-no-changes --verbosity diagnostic
	@echo "✓ Code format check complete"

# Run examples
examples:
	@echo "Running examples..."
	@echo ""
	@echo "1. Order Processing Example"
	dotnet run --project examples/OrderProcessing.cs
	@echo ""
	@echo "2. Money Transfer Example"
	dotnet run --project examples/MoneyTransfer.cs
	@echo ""
	@echo "3. Travel Booking Example"
	dotnet run --project examples/TravelBooking.cs

# Generate documentation
docs:
	@echo "Documentation:"
	@echo "  README.md - Project overview and usage examples"
	@echo "  docs/getting-started.md - Getting started guide"
	@echo "  docs/architecture.md - Architecture deep dive"
	@echo "  docs/api-reference.md - Complete API documentation"
	@echo "  docs/deployment.md - Deployment guides"
	@echo "  docs/faq.md - Frequently asked questions"
	@echo ""
	@echo "To view: open in text editor or web browser"

# Clean everything
clean-all: clean docker-clean
	@echo "Removing all artifacts..."
	rm -rf publish/
	rm -rf .vs/
	rm -rf .vscode/
	find . -name "*.user" -delete
	@echo "✓ Complete cleanup done"

# CI pipeline
ci: restore lint build test
	@echo "✓ CI pipeline complete"

# Info target
info:
	@echo "Saga Orchestrator Project Information"
	@echo "====================================="
	@echo ""
	@echo "Framework: .NET 10"
	@echo "Language: C# 14"
	@echo "Version: 1.2.0"
	@echo "Author: Vladyslav Zaiets"
	@echo "Website: https://sarmkadan.com"
	@echo ""
	@echo "Project Statistics:"
	@echo ""
	dotnet build /p:GenerateDocumentationFile=false --no-restore -q
	@find src -name "*.cs" | wc -l | xargs echo "Total C# files:"
	@find src -name "*.cs" -exec wc -l {} + | tail -1 | xargs echo "Total lines of code:"

# Version check
version:
	@echo "Project Information:"
	dotnet --version
	@echo ""
	@echo "Project Version: 1.2.0"
	@grep -m 1 "Version" dotnet-saga-orchestrator.csproj || echo "Version not found in csproj"

# Watch mode - rebuild on file changes
watch:
	@echo "Watching for file changes (Ctrl+C to stop)..."
	dotnet watch run

# Run with verbose logging
run-verbose:
	@echo "Running with verbose logging..."
	SAGA_LOG_LEVEL=Debug dotnet run

# Development setup
setup:
	@echo "Setting up development environment..."
	dotnet restore
	dotnet build
	@echo "✓ Development environment ready"
	@echo ""
	@echo "Next steps:"
	@echo "  make run              - Run the application"
	@echo "  make examples         - Run examples"
	@echo "  make test             - Run tests"

# Performance test
perf:
	@echo "Running performance test..."
	dotnet build -c Release
	@echo "Execute: time dotnet bin/Release/net10.0/SagaOrchestrator"

.DEFAULT_GOAL := help
