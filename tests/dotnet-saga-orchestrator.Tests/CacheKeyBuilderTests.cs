using System;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Infrastructure.Caching;

namespace SagaOrchestrator.Tests
{
    public class CacheKeyBuilderTests
    {
        [Fact]
        public void BuildSagaKey_ValidInput_ReturnsExpectedKey()
        {
            // Arrange
            string sagaId = "saga123";
            string expected = "saga:saga123";

            // Act
            string actual = CacheKeyBuilder.BuildSagaKey(sagaId);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildSagaKey_NullInput_ReturnsKeyWithEmptyValue()
        {
            // Arrange
            string sagaId = null;
            string expected = "saga:"; // null becomes empty string

            // Act
            string actual = CacheKeyBuilder.BuildSagaKey(sagaId);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildDefinitionKey_ValidInput_ReturnsExpectedKey()
        {
            // Arrange
            string definitionId = "def456";
            string expected = "definition:def456";

            // Act
            string actual = CacheKeyBuilder.BuildDefinitionKey(definitionId);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildDefinitionKey_NullInput_ReturnsKeyWithEmptyValue()
        {
            // Arrange
            string definitionId = null;
            string expected = "definition:"; // null becomes empty string

            // Act
            string actual = CacheKeyBuilder.BuildDefinitionKey(definitionId);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildAllSagasKey_ReturnsExpectedKey()
        {
            // Act
            string actual = CacheKeyBuilder.BuildAllSagasKey();

            // Assert
            actual.Should().Be("sagas:all");
        }

        [Fact]
        public void BuildSagasByStatusKey_ValidInput_ReturnsExpectedKey()
        {
            // Arrange
            string status = "Running";
            string expected = "sagas:status:Running";

            // Act
            string actual = CacheKeyBuilder.BuildSagasByStatusKey(status);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildSagasByStatusKey_NullInput_ReturnsKeyWithEmptyValue()
        {
            // Arrange
            string status = null;
            string expected = "sagas:status:"; // null becomes empty string

            // Act
            string actual = CacheKeyBuilder.BuildSagasByStatusKey(status);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildCompensationKey_ValidInput_ReturnsExpectedKey()
        {
            // Arrange
            string sagaId = "saga789";
            string expected = "compensation:saga789";

            // Act
            string actual = CacheKeyBuilder.BuildCompensationKey(sagaId);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildCompensationKey_NullInput_ReturnsKeyWithEmptyValue()
        {
            // Arrange
            string sagaId = null;
            string expected = "compensation:"; // null becomes empty string

            // Act
            string actual = CacheKeyBuilder.BuildCompensationKey(sagaId);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildEventHistoryKey_ValidInput_ReturnsExpectedKey()
        {
            // Arrange
            string sagaId = "sagaXYZ";
            string expected = "events:sagaXYZ";

            // Act
            string actual = CacheKeyBuilder.BuildEventHistoryKey(sagaId);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildEventHistoryKey_NullInput_ReturnsKeyWithEmptyValue()
        {
            // Arrange
            string sagaId = null;
            string expected = "events:"; // null becomes empty string

            // Act
            string actual = CacheKeyBuilder.BuildEventHistoryKey(sagaId);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildServiceKey_ValidInput_ReturnsExpectedKey()
        {
            // Arrange
            string serviceName = "PaymentService";
            string expected = "service:PaymentService";

            // Act
            string actual = CacheKeyBuilder.BuildServiceKey(serviceName);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildServiceKey_NullInput_ReturnsKeyWithEmptyValue()
        {
            // Arrange
            string serviceName = null;
            string expected = "service:"; // null becomes empty string

            // Act
            string actual = CacheKeyBuilder.BuildServiceKey(serviceName);

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void BuildHealthCheckKey_ReturnsExpectedKey()
        {
            // Act
            string actual = CacheKeyBuilder.BuildHealthCheckKey();

            // Assert
            actual.Should().Be("health:check");
        }
    }
}