#nullable enable
namespace SagaOrchestrator.Infrastructure.Resilience;

public sealed class PolicyHttpMessageHandler : DelegatingHandler
{
    private readonly ICircuitBreaker _circuitBreaker;
    public PolicyHttpMessageHandler(ICircuitBreaker circuitBreaker)
    { _circuitBreaker = circuitBreaker ?? throw new ArgumentNullException(nameof(circuitBreaker)); }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var identifier = request.RequestUri?.Host ?? "unknown";
        return await _circuitBreaker.ExecuteAsync(
            async ct => await base.SendAsync(request, ct), identifier, cancellationToken);
    }
}
