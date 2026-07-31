using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Extensions;
using SagaOrchestrator.Core.Utilities;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SagaOrchestrator.Core.Builders.Tests
{
    public class SagaStepBuilderTests
    {
        [Fact]
        public void Create_WithValidInput_ReturnsValidSagaStepBuilder()
        {
            // Arrange
            var name = "TestStep";
            var serviceName = "TestService";
            var action = "https://example.com";

            // Act
            var sagaStepBuilder = SagaStepBuilder.Create(name, serviceName, action);

            // Assert
            Assert.NotNull(sagaStepBuilder);
            Assert.Equal(name, sagaStepBuilder.Build().Name);
            Assert.Equal(serviceName, sagaStepBuilder.Build().ServiceName);
            Assert.Equal(action, sagaStepBuilder.Build().ServiceUrl);
        }

        [Fact]
        public void Create_WithNullName_ThrowsArgumentException()
        {
            // Arrange
            var name = "";
            var serviceName = "TestService";
            var action = "https://example.com";

            // Act and Assert
            Assert.Throws<ArgumentException>(() => SagaStepBuilder.Create(name, serviceName, action));
        }

        [Fact]
        public void Create_WithNullServiceName_ThrowsArgumentException()
        {
            // Arrange
            var name = "TestStep";
            var serviceName = "";
            var action = "https://example.com";

            // Act and Assert
            Assert.Throws<ArgumentException>(() => SagaStepBuilder.Create(name, serviceName, action));
        }

        [Fact]
        public void Create_WithNullAction_ThrowsArgumentException()
        {
            // Arrange
            var name = "TestStep";
            var serviceName = "TestService";
            var action = "";

            // Act and Assert
            Assert.Throws<ArgumentException>(() => SagaStepBuilder.Create(name, serviceName, action));
        }

        [Fact]
        public void Create_WithInvalidAction_ThrowsArgumentException()
        {
            // Arrange
            var name = "TestStep";
            var serviceName = "TestService";
            var action = "invalid-url";

            // Act and Assert
            Assert.Throws<ArgumentException>(() => SagaStepBuilder.Create(name, serviceName, action));
        }

        [Fact]
        public void WithOrder_SetsOrder()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var order = 5;

            // Act
            sagaStepBuilder = sagaStepBuilder.WithOrder(order);

            // Assert
            Assert.Equal(order, sagaStepBuilder.Build().Order);
        }

        [Fact]
        public void WithOrder_WithNegativeOrder_ThrowsArgumentException()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var order = -1;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sagaStepBuilder.WithOrder(order));
        }

        [Fact]
        public void WithCompensation_SetsCompensationUrl()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var compensationUrl = "https://example.com/compensation";

            // Act
            sagaStepBuilder = sagaStepBuilder.WithCompensation(compensationUrl);

            // Assert
            Assert.Equal(compensationUrl, sagaStepBuilder.Build().CompensationUrl);
        }

        [Fact]
        public void WithCompensation_WithNullCompensationUrl_ThrowsArgumentException()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            string compensationUrl = null;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sagaStepBuilder.WithCompensation(compensationUrl));
        }

        [Fact]
        public void WithTimeout_SetsTimeout()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var timeout = 10;

            // Act
            sagaStepBuilder = sagaStepBuilder.WithTimeout(timeout);

            // Assert
            Assert.Equal(timeout, sagaStepBuilder.Build().TimeoutSeconds);
        }

        [Fact]
        public void WithTimeout_WithNegativeTimeout_ThrowsArgumentException()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var timeout = -1;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sagaStepBuilder.WithTimeout(timeout));
        }

        [Fact]
        public void WithRetryPolicy_SetsRetryPolicy()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var maxRetries = 3;
            var delayMs = 100;

            // Act
            sagaStepBuilder = sagaStepBuilder.WithRetryPolicy(maxRetries, delayMs);

            // Assert
            Assert.Equal(maxRetries, sagaStepBuilder.Build().MaxRetries);
            Assert.Equal(delayMs, sagaStepBuilder.Build().RetryDelayMilliseconds);
        }

        [Fact]
        public void WithRetryPolicy_WithNegativeMaxRetries_ThrowsArgumentException()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var maxRetries = -1;
            var delayMs = 100;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sagaStepBuilder.WithRetryPolicy(maxRetries, delayMs));
        }

        [Fact]
        public void WithMetadata_SetsMetadata()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var key = "TestKey";
            var value = "TestValue";

            // Act
            sagaStepBuilder = sagaStepBuilder.WithMetadata(key, value);

            // Assert
            Assert.Equal(value, sagaStepBuilder.Build().Metadata[key]);
        }

        [Fact]
        public void WithMetadata_WithNullKey_ThrowsArgumentException()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            string key = null;
            var value = "TestValue";

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sagaStepBuilder.WithMetadata(key, value));
        }

        [Fact]
        public void WithCircuitBreakerThreshold_SetsCircuitBreakerThreshold()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var failureThreshold = 3;

            // Act
            sagaStepBuilder = sagaStepBuilder.WithCircuitBreakerThreshold(failureThreshold);

            // Assert
            Assert.Equal(failureThreshold.ToString(), sagaStepBuilder.Build().Metadata["circuitBreakerThreshold"]);
        }

        [Fact]
        public void WithCircuitBreakerThreshold_WithNegativeFailureThreshold_ThrowsArgumentException()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");
            var failureThreshold = -1;

            // Act and Assert
            Assert.Throws<ArgumentException>(() => sagaStepBuilder.WithCircuitBreakerThreshold(failureThreshold));
        }

        [Fact]
        public void Async_SetsAsync()
        {
            // Arrange
            var sagaStepBuilder = SagaStepBuilder.Create("TestStep", "TestService", "https://example.com");

            // Act
            sagaStepBuilder = sagaStepBuilder.Async();

            // Assert
            Assert.Equal("true", sagaStepBuilder.Build().Metadata["async"]);
        }
    }
}