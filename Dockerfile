# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /app

# Copy project files
COPY dotnet-saga-orchestrator.csproj .
COPY src/ src/

# Build application
RUN dotnet restore
RUN dotnet build -c Release

# Publish
RUN dotnet publish -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app

# Copy published application
COPY --from=builder /app/publish .

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Expose port
EXPOSE 80

# Set environment
ENV ASPNETCORE_URLS=http://+:80
ENV SAGA_LOG_LEVEL=Information

# Run application
ENTRYPOINT ["dotnet", "SagaOrchestrator.dll"]
