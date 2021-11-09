# Docker Guide for Saga Orchestrator

This guide provides comprehensive instructions for running the Saga Orchestrator in Docker containers, including production deployment best practices.

## Table of Contents

- [Quick Start](#quick-start)
- [Docker Compose](#docker-compose)
- [Environment Variables](#environment-variables)
- [Production Deployment](#production-deployment)
- [Health Checks](#health-checks)
- [Monitoring and Logging](#monitoring-and-logging)
- [Security Considerations](#security-considerations)
- [Troubleshooting](#troubleshooting)
- [Performance Tuning](#performance-tuning)

## Quick Start

### Prerequisites

- Docker Engine 20.10+ or Docker Desktop
- Docker Compose v2+ (recommended)
- .NET 10 SDK (for building custom images)

### Pull and Run Official Image

```bash
# Pull the latest image from GitHub Container Registry
docker pull ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest

# Run the container with default configuration
docker run -d --name saga-orchestrator \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

### Verify Installation

```bash
# Check container status
docker ps

# Test health endpoint
curl http://localhost:8080/health

# Expected response:
# {"status":"healthy","service":"Saga Orchestrator","version":"2.0.0"}
```

### Basic CLI Usage

```bash
# Execute a saga step
docker exec saga-orchestrator dotnet run -- execute --saga <saga-id>

# List all sagas
docker exec saga-orchestrator dotnet run -- list

# Show saga status
docker exec saga-orchestrator dotnet run -- status --saga <saga-id>
```

## Docker Compose

### Development Setup

Create a `docker-compose.dev.yml` file for development:

```yaml
version: '3.9'

services:
  saga-orchestrator:
    image: ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
    container_name: saga-orchestrator-dev
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - SAGA_LOG_LEVEL=Debug
      - SAGA_TIMEOUT_WORKER_INTERVAL=10
      - SAGA_COMPENSATION_WORKER_INTERVAL=5
    volumes:
      - ./data:/app/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
```

### Start Development Environment

```bash
# Start services
docker compose -f docker-compose.dev.yml up -d

# View logs
docker compose -f docker-compose.dev.yml logs -f saga-orchestrator

# Stop services
docker compose -f docker-compose.dev.yml down
```

### Production Setup

Create a `docker-compose.yml` file for production:

```yaml
version: '3.9'

services:
  saga-orchestrator:
    image: ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
    container_name: saga-orchestrator-prod
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - SAGA_LOG_LEVEL=Information
      - SAGA_TIMEOUT_WORKER_INTERVAL=15
      - SAGA_COMPENSATION_WORKER_INTERVAL=10
      - SAGA_ENABLE_CACHING=true
      - SAGA_CACHE_TTL_MINUTES=10
      - SAGA_WEBHOOKS_ENABLED=true
    volumes:
      - saga-data:/app/data
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M

volumes:
  saga-data:
```

### Start Production Environment

```bash
# Start services
docker compose up -d

# Scale for high availability (multiple instances)
docker compose up -d --scale saga-orchestrator=3

# Stop services
docker compose down -v
```

### Custom Image Build

Build your own image from source:

```bash
# Clone repository
git clone https://github.com/sarmkadan/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator

# Build image
make docker-build

# Or manually:
docker build -t my-saga-orchestrator .

# Run custom image
docker run -d --name my-saga -p 8080:8080 my-saga-orchestrator
```

## Environment Variables

### Core Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Production` | ASP.NET environment (Development/Production) |
| `ASPNETCORE_URLS` | `http://+:8080` | Server URLs |
| `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT` | `false` | Use invariant globalization |

### Saga Orchestrator Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `SAGA_LOG_LEVEL` | `Information` | Log level (Debug/Information/Warning/Error) |
| `SAGA_CORRELATION_ID_HEADER` | `X-Correlation-ID` | HTTP header for correlation IDs |
| `SAGA_TIMEOUT_SECONDS` | `300` | Default saga timeout in seconds |
| `SAGA_STEP_TIMEOUT_SECONDS` | `30` | Default step timeout in seconds |
| `SAGA_MAX_RETRIES` | `3` | Default maximum retry attempts |
| `SAGA_RETRY_DELAY_MS` | `1000` | Default retry delay in milliseconds |

### Feature Flags

| Variable | Default | Description |
|----------|---------|-------------|
| `SAGA_ENABLE_CACHING` | `true` | Enable in-memory caching |
| `SAGA_CACHE_TTL_MINUTES` | `5` | Cache time-to-live in minutes |
| `SAGA_ENABLE_WEBHOOKS` | `false` | Enable webhook notifications |
| `SAGA_ENABLE_TIMEOUT_WORKER` | `true` | Enable timeout monitoring worker |
| `SAGA_ENABLE_COMPENSATION_WORKER` | `true` | Enable compensation worker |
| `SAGA_ENABLE_EVENT_PROCESSING_WORKER` | `true` | Enable event processing worker |
| `SAGA_EXPONENTIAL_BACKOFF_ENABLED` | `true` | Enable exponential backoff for retries |

### Background Workers

| Variable | Default | Description |
|----------|---------|-------------|
| `SAGA_TIMEOUT_WORKER_INTERVAL` | `15` | Timeout worker check interval in seconds |
| `SAGA_COMPENSATION_WORKER_INTERVAL` | `10` | Compensation worker check interval in seconds |
| `SAGA_EVENT_PROCESSING_WORKER_BATCH_SIZE` | `100` | Event processing batch size |
| `SAGA_EVENT_PROCESSING_WORKER_MAX_WAIT_SECONDS` | `5` | Max wait time for batch processing |

### Performance Tuning

| Variable | Default | Description |
|----------|---------|-------------|
| `SAGA_CACHE_MAX_ENTRIES` | `1000` | Maximum cache entries |
| `SAGA_CIRCUIT_BREAKER_FAILURE_THRESHOLD` | `5` | Circuit breaker failure threshold |
| `SAGA_CIRCUIT_BREAKER_TIMEOUT_SECONDS` | `30` | Circuit breaker timeout in seconds |
| `SAGA_RATE_LIMIT_PER_SERVICE` | `100` | Default rate limit per service |

### Example: Production Configuration

```bash
docker run -d --name saga-prod \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e SAGA_LOG_LEVEL=Warning \
  -e SAGA_TIMEOUT_SECONDS=600 \
  -e SAGA_STEP_TIMEOUT_SECONDS=60 \
  -e SAGA_MAX_RETRIES=5 \
  -e SAGA_ENABLE_CACHING=true \
  -e SAGA_CACHE_TTL_MINUTES=15 \
  -e SAGA_ENABLE_WEBHOOKS=true \
  -e SAGA_TIMEOUT_WORKER_INTERVAL=20 \
  -e SAGA_COMPENSATION_WORKER_INTERVAL=15 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

## Production Deployment

### Kubernetes Deployment

#### Deployment Manifest

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: saga-orchestrator
  labels:
    app: saga-orchestrator
spec:
  replicas: 3
  selector:
    matchLabels:
      app: saga-orchestrator
  template:
    metadata:
      labels:
        app: saga-orchestrator
    spec:
      securityContext:
        runAsNonRoot: true
        runAsUser: 1000
        fsGroup: 2000
      containers:
      - name: saga-orchestrator
        image: ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
        ports:
        - containerPort: 8080
          name: http
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: SAGA_LOG_LEVEL
          value: "Warning"
        - name: SAGA_TIMEOUT_SECONDS
          value: "600"
        - name: SAGA_ENABLE_CACHING
          value: "true"
        - name: SAGA_CACHE_TTL_MINUTES
          value: "15"
        resources:
          requests:
            cpu: "500m"
            memory: "256Mi"
          limits:
            cpu: "1000m"
            memory: "512Mi"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 60
          periodSeconds: 30
          timeoutSeconds: 10
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 15
          timeoutSeconds: 5
          failureThreshold: 3
        volumeMounts:
        - name: saga-data
          mountPath: /app/data
      volumes:
      - name: saga-data
        persistentVolumeClaim:
          claimName: saga-data-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: saga-orchestrator
spec:
  selector:
    app: saga-orchestrator
  ports:
  - name: http
    port: 80
    targetPort: 8080
  type: ClusterIP
---
apiVersion: v1
kind: PersistentVolumeClaim
metadata:
  name: saga-data-pvc
spec:
  accessModes:
    - ReadWriteOnce
  resources:
    requests:
      storage: 1Gi
```

#### Ingress Configuration

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: saga-orchestrator-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  ingressClassName: nginx
  tls:
  - hosts:
    - saga.example.com
    secretName: saga-tls-secret
  rules:
  - host: saga.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: saga-orchestrator
            port:
              number: 80
```

### Docker Swarm

#### Stack File

```yaml
version: '3.8'

services:
  saga-orchestrator:
    image: ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
        window: 120s
      resources:
        limits:
          cpus: '1.0'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - SAGA_LOG_LEVEL=Warning
      - SAGA_TIMEOUT_SECONDS=600
    volumes:
      - saga-data:/app/data
    networks:
      - saga-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s

volumes:
  saga-data:
    driver: local
    driver_opts:
      type: none
      device: /mnt/saga-data
      o: bind

networks:
  saga-network:
    driver: overlay
    attachable: true
```

#### Deploy Stack

```bash
docker stack deploy -c docker-stack.yml saga-orchestrator
```

### AWS ECS

#### Task Definition

```json
{
  "family": "saga-orchestrator",
  "networkMode": "awsvpc",
  "executionRoleArn": "arn:aws:iam::123456789012:role/ecsTaskExecutionRole",
  "containerDefinitions": [
    {
      "name": "saga-orchestrator",
      "image": "ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest",
      "essential": true,
      "portMappings": [
        {
          "containerPort": 8080,
          "hostPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        },
        {
          "name": "SAGA_LOG_LEVEL",
          "value": "Warning"
        },
        {
          "name": "SAGA_TIMEOUT_SECONDS",
          "value": "600"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/saga-orchestrator",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": [
          "CMD-SHELL",
          "curl -f http://localhost:8080/health || exit 1"
        ],
        "interval": 30,
        "timeout": 10,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ],
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024"
}
```

## Health Checks

### Endpoints

| Endpoint | Description | Expected Response |
|----------|-------------|-----------------|
| `/health` | Overall health status | `{"status":"healthy"}` |
| `/health/ready` | Readiness probe | `{"status":"ready"}` |
| `/health/live` | Liveness probe | `{"status":"alive"}` |

### Health Check Configuration

#### Docker Healthcheck

```yaml
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 3
  start_period: 60s
```

#### Kubernetes Liveness Probe

```yaml
livenessProbe:
  httpGet:
    path: /health
    port: 8080
  initialDelaySeconds: 60
  periodSeconds: 30
  timeoutSeconds: 10
  failureThreshold: 3
```

#### Kubernetes Readiness Probe

```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 15
  timeoutSeconds: 5
  failureThreshold: 3
```

### Health Monitoring

```bash
# Check container health
docker inspect --format='{{json .State.Health}}' saga-orchestrator

# Get health status via API
curl http://localhost:8080/health

# Get detailed metrics
curl http://localhost:8080/metrics
```

## Monitoring and Logging

### Prometheus Metrics

Enable metrics endpoint:

```bash
docker run -d --name saga-metrics \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e SAGA_ENABLE_METRICS=true \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

Access metrics:

```bash
curl http://localhost:8080/metrics
```

### Grafana Dashboard

Import the provided Grafana dashboard template:

```json
{
  "dashboard": {
    "id": null,
    "title": "Saga Orchestrator Metrics",
    "tags": ["saga", "orchestrator", "metrics"],
    "timezone": "browser",
    "panels": [
      {
        "title": "Active Sagas",
        "type": "stat",
        "targets": [{
          "expr": "saga_active_sagas",
          "legendFormat": "Active"
        }]
      },
      {
        "title": "Saga Success Rate",
        "type": "gauge",
        "targets": [{
          "expr": "rate(saga_completed_total[5m]) / rate(saga_started_total[5m])",
          "legendFormat": "Success Rate"
        }]
      },
      {
        "title": "Step Execution Time",
        "type": "graph",
        "targets": [{
          "expr": "histogram_quantile(0.95, saga_step_duration_seconds_bucket)",
          "legendFormat": "P95 Duration"
        }]
      }
    ]
  }
}
```

### Logging Configuration

#### Docker Logging Driver

```bash
docker run -d --name saga-logs \
  -p 8080:8080 \
  --log-driver=json-file \
  --log-opt max-size=10m \
  --log-opt max-file=3 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

#### AWS CloudWatch Logs

```bash
docker run -d --name saga-cloudwatch \
  -p 8080:8080 \
  --log-driver=awslogs \
  --log-opt awslogs-group=/saga-orchestrator \
  --log-opt awslogs-region=us-east-1 \
  --log-opt awslogs-stream-prefix=ecs \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

#### ELK Stack

Configure Filebeat for log shipping:

```yaml
filebeat.inputs:
- type: container
  paths:
    - /var/lib/docker/containers/*/*.log

output.elasticsearch:
  hosts: ['elasticsearch:9200']
  index: 'saga-orchestrator-%{+yyyy.MM.dd}'
```

### Metrics Collection

Enable metrics collection:

```bash
docker run -d --name saga-metrics \
  -p 8080:8080 \
  -p 9090:9090 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e SAGA_ENABLE_METRICS=true \
  -e SAGA_METRICS_PORT=9090 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

Access metrics on port 9090.

## Security Considerations

### Container Security

#### Non-Root User

The container runs as non-root user (UID 1000) by default:

```dockerfile
USER 1000
```

#### Read-Only Filesystem

For enhanced security, run with read-only filesystem:

```bash
docker run -d --name saga-secure \
  --read-only \
  --tmpfs /tmp:size=100M \
  -p 8080:8080 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

#### Capabilities

Drop unnecessary Linux capabilities:

```bash
docker run -d --name saga-capabilities \
  --cap-drop=ALL \
  --cap-add=NET_BIND_SERVICE \
  -p 8080:8080 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

### Network Security

#### Internal Network Only

```bash
docker network create saga-internal

docker run -d --name saga-internal \
  --network saga-internal \
  -p 127.0.0.1:8080:8080 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

#### Use Custom Bridge Network

```bash
docker network create --driver=bridge --subnet=172.20.0.0/16 saga-bridge

docker run -d --name saga-bridge \
  --network saga-bridge \
  -p 8080:8080 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

### Secrets Management

#### Docker Secrets

```bash
echo "my-secret-password" | docker secret create saga-db-password -

docker service create \
  --name saga-orchestrator \
  --secret saga-db-password \
  -e DB_PASSWORD_FILE=/run/secrets/saga-db-password \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

#### Environment Variables

Use Docker secrets for sensitive configuration:

```bash
# Create secret
docker secret create saga-db-password ./db-password.txt

# Use in container
docker run -d --name saga-secrets \
  --secret db-password \
  -e DB_PASSWORD_FILE=/run/secrets/db-password \
  -p 8080:8080 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

### TLS/SSL

#### Self-Signed Certificate

```bash
# Generate certificate
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes

# Create volume for certificates
docker volume create saga-certs

# Copy certificates to volume
docker run --rm -v saga-certs:/certs -v $(pwd):/certs-src alpine \
  sh -c "cp /certs-src/*.pem /certs/"

# Run with TLS
docker run -d --name saga-tls \
  -p 8443:8443 \
  -e ASPNETCORE_Kestrel__Endpoints__Https__Url=https://+:8443 \
  -e ASPNETCORE_Kestrel__Endpoints__Https__Certificate__Path=/app/certs/cert.pem \
  -e ASPNETCORE_Kestrel__Endpoints__Https__Certificate__KeyPath=/app/certs/key.pem \
  -v saga-certs:/app/certs \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

## Troubleshooting

### Common Issues

#### Container Fails to Start

**Symptom:** Container exits immediately with error

**Solution:** Check logs:
```bash
docker logs saga-orchestrator
```

Common causes:
- Port already in use: Change port mapping
- Missing environment variables: Add required variables
- Volume permission issues: Check volume permissions
- Resource constraints: Increase memory/CPU limits

#### Health Check Fails

**Symptom:** Health check returns unhealthy

**Solution:** Check startup time:
```bash
# Increase start period in healthcheck
healthcheck:
  test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
  interval: 30s
  timeout: 10s
  retries: 5
  start_period: 120s  # Increased from 60s
```

#### Port Binding Errors

**Symptom:** `Failed to bind to address http://[::]:8080`

**Solution:** Check for port conflicts:
```bash
# Find processes using port
sudo lsof -i :8080

# Kill conflicting process
sudo kill -9 <PID>

# Or change port
sudo docker run -p 9090:8080 ...
```

#### Memory Issues

**Symptom:** Container OOM killed

**Solution:** Increase memory limits:
```yaml
resources:
  limits:
    memory: 1024M
```

### Debugging Commands

```bash
# View container logs
docker logs saga-orchestrator

# View last 100 lines
docker logs --tail=100 saga-orchestrator

# Follow logs in real-time
docker logs -f saga-orchestrator

# View container details
docker inspect saga-orchestrator

# View container resource usage
docker stats saga-orchestrator

# Enter container shell
docker exec -it saga-orchestrator sh
```

### Performance Diagnostics

```bash
# Check container resource usage
docker stats saga-orchestrator

# Check .NET runtime metrics
curl http://localhost:8080/metrics

# Check system metrics
docker system df

# Check container disk usage
docker ps --size
```

## Performance Tuning

### Resource Allocation

#### CPU Tuning

```yaml
resources:
  limits:
    cpus: '2.0'
    memory: 1024M
  reservations:
    cpus: '1.0'
    memory: 512M
```

#### Memory Tuning

```bash
docker run -d --name saga-highmem \
  -p 8080:8080 \
  --memory=2g \
  --memory-swap=2g \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

### Scaling Strategies

#### Horizontal Scaling

```bash
# Scale to 5 instances
docker compose up -d --scale saga-orchestrator=5

# Load balancing
# Use a reverse proxy (Nginx, Traefik) in front of containers
```

#### Vertical Scaling

```yaml
resources:
  limits:
    cpus: '2.0'
    memory: 2048M
```

### Cache Optimization

```bash
docker run -d --name saga-cache \
  -p 8080:8080 \
  -e SAGA_ENABLE_CACHING=true \
  -e SAGA_CACHE_TTL_MINUTES=30 \
  -e SAGA_CACHE_MAX_ENTRIES=5000 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

### Background Worker Tuning

```bash
docker run -d --name saga-workers \
  -p 8080:8080 \
  -e SAGA_TIMEOUT_WORKER_INTERVAL=10 \
  -e SAGA_COMPENSATION_WORKER_INTERVAL=10 \
  -e SAGA_EVENT_PROCESSING_WORKER_BATCH_SIZE=200 \
  ghcr.io/sarmkadan/dotnet-saga-orchestrator:latest
```

## Backup and Recovery

### Data Backup

```bash
# Backup volume
docker run --rm -v saga-data:/data -v $(pwd):/backup alpine \
  tar cvf /backup/saga-data-$(date +%Y%m%d).tar /data

# Restore volume
docker run --rm -v saga-data:/data -v $(pwd):/backup alpine \
  sh -c "rm -rf /data/* && tar xvf /backup/saga-data-20250518.tar -C /"
```

### Disaster Recovery

```bash
# Export saga definitions
docker exec saga-orchestrator dotnet run -- export-definitions > definitions.json

# Backup configuration
docker exec saga-orchestrator cat /app/appsettings.json > appsettings-backup.json

# Import definitions
docker cp definitions.json saga-orchestrator:/app/definitions.json
```

## Best Practices

### Development Best Practices

1. **Use docker-compose.dev.yml** for development
2. **Enable debug logging** for troubleshooting
3. **Mount local volumes** for code changes
4. **Use health checks** for container monitoring

### Production Best Practices

1. **Use specific image tags** (not latest)
2. **Enable all security features** (non-root, read-only, capabilities)
3. **Configure proper resource limits**
4. **Enable health checks** and monitoring
5. **Use secrets management** for sensitive data
6. **Implement backup strategy**
7. **Enable TLS/SSL** for production traffic

### Monitoring Best Practices

1. **Enable metrics collection**
2. **Configure logging aggregation**
3. **Set up alerts** for health check failures
4. **Monitor resource usage**
5. **Track saga performance** metrics

## Version Compatibility

| Docker Image Tag | .NET Version | Compatibility |
|-----------------|--------------|---------------|
| `latest` | 10.0 | ✅ Latest features |
| `2.0.0` | 10.0 | ✅ v2.0 features |
| `1.0.0` | 8.0 | ⚠️ v1.x features |

## Support

For Docker-related issues:
- Check [Docker documentation](https://docs.docker.com/)
- Review [GitHub Issues](https://github.com/sarmkadan/dotnet-saga-orchestrator/issues)
- Consult [Docker Community Forums](https://forums.docker.com/)

---

**Last Updated:** May 2026

**Next Review:** June 2026