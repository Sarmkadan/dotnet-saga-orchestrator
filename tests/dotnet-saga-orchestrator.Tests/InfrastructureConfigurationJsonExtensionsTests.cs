using System;
using Xunit;
using SagaOrchestrator.Configuration;
using System.Text.Json;

namespace SagaOrchestrator.Tests
{
    public class InfrastructureConfigurationJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithNullValue_ThrowsArgumentNullException()
        {
            // Arrange
            InfrastructureConfiguration? value = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => InfrastructureConfigurationJsonExtensions.ToJson(value!));
        }

        [Fact]
        public void ToJson_WithDefaultConfiguration_ReturnsValidJson()
        {
            // Arrange
            var configuration = InfrastructureConfiguration.Default;

            // Act
            var json = InfrastructureConfigurationJsonExtensions.ToJson(configuration);

            // Assert
            Assert.NotNull(json);
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.Contains("\"enableCaching\":true", json);
            Assert.Contains("\"enableHttpClients\":true", json);
            Assert.Contains("\"enableEventBus\":true", json);
            Assert.Contains("\"enableFormatting\":true", json);
            Assert.Contains("\"enableLogging\":true", json);
            Assert.Contains("\"enableIntegration\":true", json);
            Assert.Contains("\"enableRateLimiting\":true", json);
            Assert.Contains("\"enableBackgroundWorkers\":true", json);
        }

        [Fact]
        public void ToJson_WithIndentedTrue_ReturnsIndentedJson()
        {
            // Arrange
            var configuration = InfrastructureConfiguration.Default;

            // Act
            var json = InfrastructureConfigurationJsonExtensions.ToJson(configuration, indented: true);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("{\n  ", json); // Check for indentation (Unix line endings)
        }

        [Fact]
        public void FromJson_WithNullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => InfrastructureConfigurationJsonExtensions.FromJson(json!));
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsConfiguration()
        {
            // Arrange
            var json = @"{
                ""enableCaching"": false,
                ""enableHttpClients"": true,
                ""enableEventBus"": false,
                ""enableFormatting"": true,
                ""enableLogging"": false,
                ""enableIntegration"": true,
                ""enableRateLimiting"": false,
                ""enableBackgroundWorkers"": true
            }";

            // Act
            var configuration = InfrastructureConfigurationJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(configuration);
            Assert.False(configuration.EnableCaching);
            Assert.True(configuration.EnableHttpClients);
            Assert.False(configuration.EnableEventBus);
            Assert.True(configuration.EnableFormatting);
            Assert.False(configuration.EnableLogging);
            Assert.True(configuration.EnableIntegration);
            Assert.False(configuration.EnableRateLimiting);
            Assert.True(configuration.EnableBackgroundWorkers);
        }

        [Fact]
        public void FromJson_WithInvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "invalid json {";

            // Act & Assert
            Assert.Throws<JsonException>(() => InfrastructureConfigurationJsonExtensions.FromJson(json));
        }

        [Fact]
        public void FromJson_WithEmptyString_ThrowsJsonException()
        {
            // Arrange
            var json = "";

            // Act & Assert
            Assert.Throws<JsonException>(() => InfrastructureConfigurationJsonExtensions.FromJson(json));
        }

        [Fact]
        public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;
            InfrastructureConfiguration? value;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => InfrastructureConfigurationJsonExtensions.TryFromJson(json!, out value));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndConfiguration()
        {
            // Arrange
            var json = @"{
                ""enableCaching"": false,
                ""enableHttpClients"": true
            }";
            InfrastructureConfiguration? value;

            // Act
            var result = InfrastructureConfigurationJsonExtensions.TryFromJson(json, out value);

            // Assert
            Assert.True(result);
            Assert.NotNull(value);
            Assert.False(value.EnableCaching);
            Assert.True(value.EnableHttpClients);
            // Other properties should be true (default)
            Assert.True(value.EnableEventBus);
            Assert.True(value.EnableFormatting);
            Assert.True(value.EnableLogging);
            Assert.True(value.EnableIntegration);
            Assert.True(value.EnableRateLimiting);
            Assert.True(value.EnableBackgroundWorkers);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "invalid json {";
            InfrastructureConfiguration? value;

            // Act
            var result = InfrastructureConfigurationJsonExtensions.TryFromJson(json, out value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        [Fact]
        public void TryFromJson_WithEmptyString_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "";
            InfrastructureConfiguration? value;

            // Act
            var result = InfrastructureConfigurationJsonExtensions.TryFromJson(json, out value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }
    }
}