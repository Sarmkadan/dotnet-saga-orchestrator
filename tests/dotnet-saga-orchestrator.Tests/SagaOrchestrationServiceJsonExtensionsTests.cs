using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Domain.Models;
using Xunit;

namespace SagaOrchestrator.Application.Services.Tests
{
    public class SagaOrchestrationServiceJsonExtensionsTests
    {
        [Fact]
        public void ToJson_Happy_PATH()
        {
            // Given
            var saga = new Saga();
            // When
            var json = SagaOrchestrationServiceJsonExtensions.ToJson(saga);
            // Then
            Assert.NotNull(json);
        }

        [Fact]
        public void FromJson_HAPPY_PATH()
        {
            // Given
            var json = "{}";
            // When
            var saga = SagaOrchestrationServiceJsonExtensions.FromJson(json);
            // Then
            Assert.NotNull(saga);
        }

        [Fact]
        public void TryFromJson_SUCCESS()
        {
            // Given
            var json = "{}";
            Saga? saga;
            // When
            var success = SagaOrchestrationServiceJsonExtensions.TryFromJson(json, out saga);
            // Then
            Assert.True(success);
            Assert.NotNull(saga);
        }
    }
}