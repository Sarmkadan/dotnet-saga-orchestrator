using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests
{
    public class SagaIdGeneratorJsonExtensionsTests
    {
        [Fact]
        public void ToJson_Happy_PATH()
        {
            // Arrange
            var sagaId = "12345";
            var expectedJson = "{\"SagaId\": \"12345\"}";

            // Act
            var actualJson = SagaIdGeneratorJsonExtensions.ToJson(sagaId);

            // Assert
            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void FromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{\"SagaId\": \"12345\"}";
            var expectedSagaId = "12345";

            // Act
            var actualSagaId = SagaIdGeneratorJsonExtensions.FromJson(json);

            // Assert
            Assert.Equal(expectedSagaId, actualSagaId);
        }

        [Fact]
        public void TryFromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{\"SagaId\": \"12345\"}";
            var expectedSagaId = "12345";
            string? actualSagaId = null;

            // Act
            var success = SagaIdGeneratorJsonExtensions.TryFromJson(json, out actualSagaId);

            // Assert
            Assert.True(success);
            Assert.Equal(expectedSagaId, actualSagaId);
        }
    }
}