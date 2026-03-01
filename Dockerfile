# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /src

# Restore dependencies (layer caching)
COPY dotnet-saga-orchestrator.csproj .
RUN dotnet restore

# Copy source and build
COPY src/ src/
COPY Program.cs .
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=builder /app/publish .

# Non-root user
RUN groupadd -r appuser && useradd -r -g appuser -s /sbin/nologin appuser
USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV SAGA_LOG_LEVEL=Information

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -sf http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SagaOrchestrator.dll"]
