#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using SagaOrchestrator.Core.Extensions;

namespace SagaOrchestrator.Infrastructure.Integration;

/// <summary>
/// Registry for tracking external microservices used by saga steps.
/// Maintains service endpoints, health status, and metadata.
/// </summary>
public interface IServiceRegistry
{
    Task RegisterServiceAsync(ServiceDescriptor service);
    Task<ServiceDescriptor?> GetServiceAsync(string name);
    Task<List<ServiceDescriptor>> GetAllServicesAsync();
    Task<bool> IsServiceHealthyAsync(string name);
    Task UpdateServiceHealthAsync(string name, bool isHealthy);
    Task UnregisterServiceAsync(string name);
}

public class ServiceRegistry : IServiceRegistry
{
    private readonly Dictionary<string, ServiceDescriptor> _services;
    private readonly ILogger<ServiceRegistry> _logger;
    private readonly object _lock = new();

    public ServiceRegistry(ILogger<ServiceRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _services = new();
    }

    public async Task RegisterServiceAsync(ServiceDescriptor service)
    {
        service.NotNull(nameof(service));
        service.Name.NotNullOrEmpty(nameof(service.Name));

        await Task.Run(() =>
        {
            lock (_lock)
            {
                _services[service.Name] = service;
            }
        });

        _logger.LogInformation("Service registered | Name: {ServiceName}, Url: {Url}",
            service.Name, service.Url);
    }

    public async Task<ServiceDescriptor?> GetServiceAsync(string name)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                _services.TryGetValue(name, out var service);
                return service;
            }
        });
    }

    public async Task<List<ServiceDescriptor>> GetAllServicesAsync()
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                return _services.Values.ToList();
            }
        });
    }

    public async Task<bool> IsServiceHealthyAsync(string name)
    {
        var service = await GetServiceAsync(name);
        return service?.IsHealthy ?? false;
    }

    public async Task UpdateServiceHealthAsync(string name, bool isHealthy)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_services.TryGetValue(name, out var service))
                {
                    service.IsHealthy = isHealthy;
                    service.LastHealthCheckTime = DateTime.UtcNow;

                    _logger.LogInformation("Service health updated | Name: {ServiceName}, Healthy: {IsHealthy}",
                        name, isHealthy);
                }
            }
        });
    }

    public async Task UnregisterServiceAsync(string name)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                _services.Remove(name);
            }
        });

        _logger.LogInformation("Service unregistered | Name: {ServiceName}", name);
    }
}

public class ServiceDescriptor
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public int Timeout { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool IsHealthy { get; set; } = true;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime LastHealthCheckTime { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Metadata { get; set; } = new();

    public ServiceDescriptor() { }

    public ServiceDescriptor(string name, string url)
    {
        Name = name;
        Url = url;
    }

    public override string ToString() =>
        $"Service: {Name} ({Url}), Healthy: {IsHealthy}";
}
