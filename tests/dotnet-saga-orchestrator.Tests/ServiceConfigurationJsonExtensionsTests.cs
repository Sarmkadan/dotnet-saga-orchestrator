using System;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Configuration;

namespace SagaOrchestrator.Tests
{
    public class ServiceConfigurationJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ValidOptions_ReturnsExpectedJson()
        {
            // Arrange
            var options = new SagaOptions();

            // Act
            string json = ServiceConfigurationJsonExtensions.ToJson(options);

            // Assert
            json.Should().Contain("\"retryPolicies\":");
            json.Should().Contain("\"timeoutPolicies\":");
            json.Should().Contain("\"cachePolicies\":");
        }

        [Fact]
        public void ToJson_Indented_ReturnsIndentedJson()
        {
            // Arrange
            var options = new SagaOptions();

            // Act
            string json = ServiceConfigurationJsonExtensions.ToJson(options, indented: true);

            // Assert
            json.Should().Contain(Environment.NewLine);
        }

        [Fact]
        public void ToJson_NullInput_ThrowsArgumentNullException()
        {
            // Arrange
            SagaOptions options = null!;

            // Act
            Action act = () => ServiceConfigurationJsonExtensions.ToJson(options);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsSagaOptions()
        {
            // Arrange
            var options = new SagaOptions { RetryPolicies = new RetryPolicies { DefaultMaxRetries = 5 } };
            string json = ServiceConfigurationJsonExtensions.ToJson(options);

            // Act
            var result = ServiceConfigurationJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.RetryPolicies.DefaultMaxRetries.Should().Be(5);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            string json = "{ invalid json }";

            // Act
            Action act = () => ServiceConfigurationJsonExtensions.FromJson(json);

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndOptions()
        {
            // Arrange
            var options = new SagaOptions { CachePolicies = new CachePolicies { MaxCacheSize = 999 } };
            string json = ServiceConfigurationJsonExtensions.ToJson(options);

            // Act
            bool success = ServiceConfigurationJsonExtensions.TryFromJson(json, out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result!.CachePolicies.MaxCacheSize.Should().Be(999);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            string json = "{ invalid }";

            // Act
            bool success = ServiceConfigurationJsonExtensions.TryFromJson(json, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }
    }
}
