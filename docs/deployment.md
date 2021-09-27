# Deployment Guide

Instructions for deploying Saga Orchestrator in various environments.

## Local Development

### Prerequisites

- .NET 10 SDK
- Git
- Text editor or IDE

### Setup

```bash
# Clone repository
git clone https://github.com/Sarmkadan/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

### Configuration

Create `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "SagaOrchestrator": "Debug"
    }
  },
  "SagaOrchestrator": {
    "DefaultSagaTimeoutSeconds": 300,
    "DefaultStepTimeoutSeconds": 30,
    "DefaultMaxRetries": 3,
    "CachingEnabled": true,
    "WebhooksEnabled": true
  }
}
```

## Docker Deployment

### Build Docker Image

```bash
docker build -t saga-orchestrator:latest .
```

### Run Container

```bash
docker run -d \
  --name saga-orchestrator \
  -p 5000:80 \
  -e SAGA_TIMEOUT_SECONDS=300 \
  -e SAGA_ENABLE_WEBHOOKS=true \
  saga-orchestrator:latest
```

### Docker Compose

```bash
docker-compose up -d
```

See `docker-compose.yml` for full configuration.

## Production Deployment

### Prerequisites

- .NET 10 runtime
- Persistent database (optional; see repository interfaces for extension points)
- Load balancer (optional)
- Monitoring system (Prometheus, Grafana)

### Build

```bash
# Build release binary
dotnet publish -c Release -o ./publish

# Create artifact
zip -r saga-orchestrator-release.zip ./publish
```

### Configuration

Create `appsettings.Production.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SagaOrchestrator": "Information"
    }
  },
  "SagaOrchestrator": {
    "DefaultSagaTimeoutSeconds": 600,
    "DefaultStepTimeoutSeconds": 45,
    "DefaultMaxRetries": 5,
    "CachingEnabled": true,
    "CacheTtlMinutes": 10,
    "WebhooksEnabled": true,
    "TimeoutWorkerEnabled": true,
    "TimeoutCheckIntervalSeconds": 30
  },
  "Database": {
    "Provider": "SqlServer",
    "ConnectionString": "Server=prod-db;Database=SagaOrchestrator;..."
  }
}
```

### Environment Variables

```bash
# Saga configuration
export SAGA_TIMEOUT_SECONDS=600
export SAGA_STEP_TIMEOUT_SECONDS=45
export SAGA_MAX_RETRIES=5

# Feature flags
export SAGA_ENABLE_CACHING=true
export SAGA_ENABLE_WEBHOOKS=true
export SAGA_ENABLE_TIMEOUT_WORKER=true

# Logging
export SAGA_LOG_LEVEL=Information

# Database (optional)
export DB_PROVIDER=SqlServer
export DB_CONNECTION_STRING="Server=prod-db;Database=SagaOrchestrator;..."
```

### Start Service

```bash
cd publish
./SagaOrchestrator
```

### Health Checks

```bash
# Check system health
curl http://localhost:5000/health

# Expected response
{
  "status": "Healthy",
  "activeSagas": 42,
  "uptime": "2d 5h 30m",
  "timestamp": "2026-05-04T10:30:00Z"
}
```

## Kubernetes Deployment

### Build Container Image

```bash
docker build -t myregistry.azurecr.io/saga-orchestrator:1.0.0 .
docker push myregistry.azurecr.io/saga-orchestrator:1.0.0
```

### Kubernetes Manifest

Create `k8s-deployment.yaml`:

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: saga-orchestrator
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: saga-orchestrator
  namespace: saga-orchestrator
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
      containers:
      - name: saga-orchestrator
        image: myregistry.azurecr.io/saga-orchestrator:1.0.0
        ports:
        - containerPort: 80
        env:
        - name: SAGA_TIMEOUT_SECONDS
          value: "600"
        - name: SAGA_ENABLE_WEBHOOKS
          value: "true"
        - name: SAGA_LOG_LEVEL
          value: "Information"
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 80
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 80
          initialDelaySeconds: 10
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: saga-orchestrator
  namespace: saga-orchestrator
spec:
  selector:
    app: saga-orchestrator
  ports:
  - protocol: TCP
    port: 80
    targetPort: 80
  type: LoadBalancer
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: saga-orchestrator-hpa
  namespace: saga-orchestrator
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: saga-orchestrator
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

Deploy:

```bash
kubectl apply -f k8s-deployment.yaml
kubectl get pods -n saga-orchestrator
kubectl logs -n saga-orchestrator -f deployment/saga-orchestrator
```

## AWS Deployment

### Using AWS AppRunner

```bash
# Create connection (one-time)
aws apprunner create-connection --provider-type GITHUB

# Deploy
aws apprunner create-service \
  --service-name saga-orchestrator \
  --source-configuration '{
    "RepositoryType": "GITHUB",
    "GitHubRepositorySourceConfiguration": {
      "RepositoryUrl": "https://github.com/Sarmkadan/dotnet-saga-orchestrator",
      "Branch": "main",
      "ConfigurationSource": "REPOSITORY"
    }
  }' \
  --instance-configuration '{
    "InstanceRoleArn": "arn:aws:iam::ACCOUNT:role/AppRunnerRole",
    "InstanceType": "db.t3.small"
  }'
```

### Using EC2

```bash
# Launch instance
aws ec2 run-instances \
  --image-id ami-0c55b159cbfafe1f0 \
  --instance-type t3.medium \
  --security-groups saga-orchestrator-sg

# SSH and setup
ssh -i key.pem ec2-user@instance-ip

# Install .NET
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Deploy
cd /opt
git clone https://github.com/Sarmkadan/dotnet-saga-orchestrator.git
cd dotnet-saga-orchestrator
dotnet build -c Release
./publish/SagaOrchestrator
```

## Azure Deployment

### Azure App Service

```bash
# Create resource group
az group create --name saga-orchestrator --location eastus

# Create App Service plan
az appservice plan create \
  --name saga-orchestrator-plan \
  --resource-group saga-orchestrator \
  --sku B2

# Deploy
az webapp deployment source config-zip \
  --resource-group saga-orchestrator \
  --name saga-orchestrator \
  --src saga-orchestrator-release.zip

# Configure app settings
az webapp config appsettings set \
  --name saga-orchestrator \
  --resource-group saga-orchestrator \
  --settings \
    SAGA_TIMEOUT_SECONDS=600 \
    SAGA_ENABLE_WEBHOOKS=true
```

## Google Cloud Deployment

### Cloud Run

```bash
# Build and push image
gcloud builds submit \
  --tag gcr.io/PROJECT-ID/saga-orchestrator

# Deploy
gcloud run deploy saga-orchestrator \
  --image gcr.io/PROJECT-ID/saga-orchestrator \
  --platform managed \
  --region us-central1 \
  --memory 512Mi \
  --cpu 1 \
  --set-env-vars SAGA_TIMEOUT_SECONDS=600,SAGA_ENABLE_WEBHOOKS=true
```

## Monitoring & Observability

### Logging

Configure structured logging:

```csharp
services.AddLogging(config =>
{
    config.AddConsole();
    config.AddFile("/var/log/saga-orchestrator/logs.txt");
});
```

### Health Checks

```bash
# Basic health
curl http://localhost:5000/health

# Detailed health
curl http://localhost:5000/health/details

# Readiness probe
curl http://localhost:5000/health/ready
```

### Metrics

Access metrics endpoint:

```bash
curl http://localhost:5000/metrics
```

## Scaling Considerations

### Horizontal Scaling

1. Replace in-memory repositories with database
2. Use distributed cache (Redis)
3. Configure load balancer
4. Set up session affinity if needed

### Vertical Scaling

1. Increase timeout worker intervals
2. Tune cache expiration
3. Adjust retry policies
4. Monitor resource usage

## Backup & Recovery

### State Backup

```bash
# Export sagas to JSON
dotnet run -- export --output sagas.json

# Export metrics
dotnet run -- metrics export --output metrics.json
```

### Restore

```bash
# Restore from backup
dotnet run -- import --input sagas.json
```

## Troubleshooting

### Port Already in Use

```bash
# Find process using port 5000
lsof -i :5000

# Kill process
kill -9 PID
```

### High Memory Usage

```bash
# Check memory
dotnet-trace collect --output trace.nettrace

# Analyze
dotnet-gcdump analyze
```

### Slow Startup

```bash
# Profile startup
dotnet run --verbose

# Check logs
tail -f /var/log/saga-orchestrator/logs.txt
```

## CI/CD Integration

### GitHub Actions

See `.github/workflows/build.yml` for CI/CD pipeline.

```yaml
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - uses: actions/setup-dotnet@v1
      - run: dotnet build
      - run: dotnet test
```

## Rollback Procedure

```bash
# Check current version
curl http://localhost:5000/version

# Rollback to previous version
docker run --rm -d \
  --name saga-orchestrator \
  -p 5000:80 \
  saga-orchestrator:v1.0.0
```

## Cost Optimization

### For Cloud Deployments

1. **Use spot instances** - Save up to 90% on compute
2. **Enable auto-scaling** - Scale down during off-hours
3. **Cache aggressively** - Reduce database load
4. **Compress responses** - Lower bandwidth costs
5. **Archive old data** - Move to cheaper storage

### Example Cost Estimate (AWS)

- t3.medium EC2: ~$30/month
- RDS db.t3.small: ~$35/month
- CloudWatch: ~$10/month
- Data transfer: ~$5/month
- **Total: ~$80/month**

## Support & Troubleshooting

For deployment issues:
1. Check logs: `dotnet run -- logs`
2. Run health checks: `curl /health`
3. Review configuration: `appsettings.json`
4. Enable debug logging: `--log-level Debug`
