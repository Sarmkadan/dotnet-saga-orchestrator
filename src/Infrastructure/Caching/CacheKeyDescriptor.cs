namespace SagaOrchestrator.Infrastructure.Caching;

/// <summary>
/// Represents a strongly-typed cache key descriptor.
/// </summary>
/// <param name="Prefix">The prefix for the cache key (e.g., "saga", "definition").</param>
/// <param name="Id">The unique identifier for the item.</param>
public record CacheKeyDescriptor(string Prefix, string Id)
{
    public override string ToString() => $"{Prefix}:{Id}";
}
