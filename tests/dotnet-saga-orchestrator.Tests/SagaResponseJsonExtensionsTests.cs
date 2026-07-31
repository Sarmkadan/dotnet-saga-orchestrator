using System;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Tests
{
    public class SagaResponseJsonExtensionsTests
    {
        private static SagaResponse CreateSampleSagaResponse()
        {
            return new SagaResponse
            {
                Id = "saga-123",
                CorrelationId = "corr-123",
                Status = "InProgress",
                DefinitionId = "def-123",
                DefinitionName = "test-definition",
                StartedAt = DateTime.UtcNow,
                StepCount = 1,
                CompletedSteps = 0,
                FailedSteps = 0,
                RetryCount = 0
            };
        }

        [Fact]
        public void ToJson_ValidSagaResponse_ReturnsJson()
        {
            var response = CreateSampleSagaResponse();
            var json = response.ToJson();

            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"id\":\"saga-123\"");
            json.Should().Contain("\"correlationId\":\"corr-123\"");
        }

        [Fact]
        public void ToJson_NullSagaResponse_ThrowsArgumentNullException()
        {
            SagaResponse? response = null;
            
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.Throws<ArgumentNullException>(() => response!.ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsSagaResponse()
        {
            var response = CreateSampleSagaResponse();
            var json = response.ToJson();

            var deserialized = SagaResponseJsonExtensions.FromJson(json);

            deserialized.Should().NotBeNull();
            deserialized!.Id.Should().Be(response.Id);
            deserialized.CorrelationId.Should().Be(response.CorrelationId);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            var invalidJson = "{ invalid json }";

            Assert.Throws<JsonException>(() => SagaResponseJsonExtensions.FromJson(invalidJson));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrue()
        {
            var response = CreateSampleSagaResponse();
            var json = response.ToJson();

            bool success = SagaResponseJsonExtensions.TryFromJson(json, out var deserialized);

            success.Should().BeTrue();
            deserialized.Should().NotBeNull();
            deserialized!.Id.Should().Be(response.Id);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            var invalidJson = "{ invalid json }";

            bool success = SagaResponseJsonExtensions.TryFromJson(invalidJson, out var deserialized);

            success.Should().BeFalse();
            deserialized.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => SagaResponseJsonExtensions.TryFromJson(null!, out _));
        }
    }
}
