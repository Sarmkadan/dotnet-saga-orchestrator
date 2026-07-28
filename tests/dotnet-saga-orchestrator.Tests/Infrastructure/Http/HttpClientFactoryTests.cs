using System.Net;
using SagaOrchestrator.Infrastructure.Http;
using Xunit;
using Moq;
using Moq.Protected;
using System.Net.Http;

namespace SagaOrchestrator.Tests.Infrastructure.Http;

public class HttpClientFactoryTests
{
    /// <summary>
    /// Verifies that the factory caches client instances by name.
    /// </summary>
    [Fact]
    public void CreateClient_ShouldCacheClientByName()
    {
        var factory = new HttpClientFactory();
        var config = new HttpClientConfiguration { BaseUrl = "http://localhost" };
        
        var client1 = factory.CreateClient("test", config);
        var client2 = factory.CreateClient("test", config);
        
        Assert.Same(client1, client2);
    }

    [Fact]
    public void HttpClientConfiguration_Validate_ShouldThrowOnInvalidValues()
    {
        var config = new HttpClientConfiguration { BaseUrl = "" };
        Assert.Throws<ArgumentException>(() => config.Validate());
        
        config.BaseUrl = "http://test.com";
        config.TimeoutSeconds = 0;
        Assert.Throws<ArgumentException>(() => config.Validate());
        
        config.TimeoutSeconds = 10;
        config.Policy = new PolicyOptions { MaxRetries = -1 };
        Assert.Throws<ArgumentException>(() => config.Validate());
    }

    [Fact]
    public async Task PolicyHttpMessageHandler_RetriesOnTransientFailure()
    {
        var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

        var handler = new PolicyHttpMessageHandler("test", new PolicyOptions { MaxRetries = 2, BackoffBaseMs = 1 })
        {
            InnerHandler = mockHandler.Object
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        request.AllowRetry(); // Need to ensure request is allowed to retry (GET is inherently retryable, but good to be sure)

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        
        // Verification: The mock should have been called MaxRetries + 1 times (initial + retries)
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public void CreateClient_ShouldSetTimeout()
    {
        var factory = new HttpClientFactory();
        var config = new HttpClientConfiguration { BaseUrl = "http://localhost", TimeoutSeconds = 5 };
        
        var client = factory.CreateClient("test", config);
        
        Assert.Equal(TimeSpan.FromSeconds(5), client.Timeout);
    }
}
