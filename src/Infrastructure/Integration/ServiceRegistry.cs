#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<string, ServiceDescriptor> _services;
    private readonly ILogger<ServiceRegistry> _logger;

    public ServiceRegistry(ILogger<ServiceRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _services = new ConcurrentDictionary<string, ServiceDescriptor>();
    }

    public async Task RegisterServiceAsync(ServiceDescriptor service)
    {
        service.NotNull(nameof(service));
        service.Name.NotNullOrEmpty(nameof(service.Name));

        await Task.Run(() =>
        {
            _services[service.Name] = service;
        });

        _logger.LogInformation("Service registered | Name: {ServiceName}, Url: {Url}",
            service.Name, service.Url);
    }

    public async Task<ServiceDescriptor?> GetServiceAsync(string name)
    {
        return await Task.Run(() =>
        {
            _services.TryGetValue(name, out var service);
            return service;
        });
    }

    public async Task<List<ServiceDescriptor>> GetAllServicesAsync()
    {
        return await Task.Run(() =>
        {
            return _services.Values.ToList();
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
            bool updated = false;
            while (!updated)
            {
                if (!_services.TryGetValue(name, out var currentService))
                {
                    // Service not found, exit
                    return;
                }
                var updatedService = new ServiceDescriptor
                {
                    Name = currentService.Name,
                    Url = currentService.Url,
                    ApiKey = currentService.ApiKey,
                    Timeout = currentService.Timeout,
                    MaxRetries = currentService.MaxRetries,
                    IsHealthy = isHealthy,
                    RegisteredAt = currentService.RegisteredAt,
                    LastHealthCheckTime = DateTime.UtcNow,
                    Metadata = new Dictionary<string, string>(currentService.Metadata)
                };
                updated = _services.TryUpdate(name, updatedService, currentService);
            }

            _logger.LogInformation("Service health updated | Name: {ServiceName}, Healthy: {IsHealthy}",
                name, isHealthy);
        });
    }

    public async Task UnregisterServiceAsync(string name)
    {
        await Task.Run(() =>
        {
            _services.TryRemove(name, out _);
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