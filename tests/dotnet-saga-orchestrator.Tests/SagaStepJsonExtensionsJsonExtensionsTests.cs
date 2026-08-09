using System.Text.Json;
using SagaOrchestrator.Core.Domain.Models;
using Xunit;
using System;

namespace SagaOrchestrator.Core.Domain.Models.Tests
{
    public class SagaStepJsonExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_HAPPY_PATH()
        {
            // Arrange
            var sagaStep = new SagaStep();
            // Act
            var json = SagaStepJsonExtensions.ToJson(sagaStep);
            // Assert
            Assert.NotNull(json);
        }

        [Fact]
        public void FromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{}";
            // Act
            var sagaStep = SagaStepJsonExtensions.FromJson(json);
            // Assert
            Assert.NotNull(sagaStep);
        }

        [Fact]
        public void TryFromJson_HAPPY_PATH()
        {
            // Arrange
            var json = "{}";
            SagaStep? sagaStep;
            // Act
            var success = SagaStepJsonExtensions.TryFromJson(json, out sagaStep);
            // Assert
            Assert.True(success);
            Assert.NotNull(sagaStep);
        }

        [Fact]
        public void ToJson_NULL_INPUT()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaStepJsonExtensions.ToJson(null));
        }

        [Fact]
        public void FromJson_NULL_INPUT()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaStepJsonExtensions.FromJson(null));
        }

        [Fact]
        public void TryFromJson_NULL_INPUT()
        {
            // Arrange
            SagaStep? sagaStep;
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaStepJsonExtensions.TryFromJson(null, out sagaStep));
        }
    }
}