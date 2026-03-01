# Migration Guide: v1.x to v2.0

This document covers breaking changes and migration steps for upgrading from v1.x to v2.0 of the Distributed Saga Orchestrator.

## Breaking Changes

### 1. Default Port Changed from 80 to 8080

The container now listens on port **8080** instead of port 80. This follows the .NET 8+ convention of running containers as non-root users, where binding to ports below 1024 requires elevated privileges.

**Before (v1.x):**

```yaml
ports:
  - "5000:80"
```

**After (v2.0):**

```yaml
ports:
  - "8080:8080"
```

If you have a reverse proxy or load balancer pointing to the old port, update accordingly.

### 2. Docker Base Image Changed to `aspnet`

The runtime base image switched from `mcr.microsoft.com/dotnet/runtime:10.0` to `mcr.microsoft.com/dotnet/aspnet:10.0` to support the upcoming REST API endpoints and health check middleware.

### 3. Docker Compose Schema Version Removed

The `version: '3.8'` key has been removed from `docker-compose.yml` per the Compose Specification. Docker Compose v2+ ignores this field and emits a warning. No action needed unless you are running Compose v1 (EOL).

### 4. `ASPNETCORE_URLS` Environment Variable

The environment variable `ASPNETCORE_URLS` now defaults to `http://+:8080`. If you override this variable, ensure the port matches what you expose in your Dockerfile or Compose file.

## Migration Steps

### Step 1 - Update Docker Port Mappings

In your `docker-compose.yml` or `docker run` commands, change port mappings from `80` to `8080`:

```bash
# Old
docker run -p 5000:80 saga-orchestrator

# New
docker run -p 8080:8080 saga-orchestrator
```

### Step 2 - Update Health Check URLs

If you have external health checks configured (Kubernetes liveness/readiness probes, load balancer checks), update the port:

```yaml
# Kubernetes example
livenessProbe:
  httpGet:
    path: /health
    port: 8080
```

### Step 3 - Update NuGet Package Reference

```bash
dotnet add package Zaiets.dotnet.saga.orchestrator --version 2.0.0
```

### Step 4 - Rebuild Docker Images

```bash
docker compose build --no-cache
docker compose up -d
```

### Step 5 - Verify

```bash
curl http://localhost:8080/health
```

## New Features in v2.0

- Multi-stage Docker build with layer caching for faster rebuilds
- Non-root container user with restricted shell (`/sbin/nologin`)
- Improved HEALTHCHECK with longer start period (10s) for cold-start scenarios
- ASP.NET base image ready for REST API and health check middleware
- Docker Compose v2 compatible (no deprecated `version` key)

## Compatibility

| Component | v1.x | v2.0 |
|-----------|------|------|
| .NET SDK | 10.0 | 10.0 |
| Container port | 80 | 8080 |
| Runtime image | `dotnet/runtime` | `dotnet/aspnet` |
| Compose schema | 3.8 | Compose Spec |
| Non-root user | Yes | Yes (hardened) |

## Rollback

If you need to revert to v1.x behavior, pin the package version and restore the old port mapping:

```bash
dotnet add package Zaiets.dotnet.saga.orchestrator --version 1.0.0
```

Restore the `ASPNETCORE_URLS=http://+:80` environment variable and update port mappings back to `80`.
