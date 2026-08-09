using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Moq;

namespace SagaOrchestrator.Core.Domain.Models.Tests
{
    public class SagaEventJsonExtensionsTests
    {
        [Fact]
        public void HappyPath_ToJson_ValidInput()
        {
            // Arrange
            var sagaEvent = new SagaEvent
            {
                Id = "id-123",
                SagaId = "saga-123",
                EventType = "EventType",
                EventName = "EventName",
                Description = "Description",
                Timestamp = DateTime.UtcNow,
                Severity = EventSeverity.Information,
                Data = new Dictionary<string, object>()
                {
                    {"key", "value"}
                },
                StepId = "step-123",
                StepName = "StepName",
                Source = "Source",
                CorrelationId = "CorrelationId"
            };

            // Act
            var json = SagaEventJsonExtensions.ToJson(sagaEvent);

            // Assert
            Assert.NotNull(json);
        }

        [Fact]
        public void HappyPath_FromJson_ValidInput()
        {
            // Arrange
            var json = "{\"id\":\"id-123\",\"sagaId\":\"saga-123\",\"eventType\":\"EventType\",\"eventName\":\"EventName\",\"description\":\"Description\",\"timestamp\":\"2022-01-01T00:00:00\",\"severity\":0,\"data\":{\"key\":\"value\"},\"stepId\":\"step-123\",\"stepName\":\"StepName\",\"source\":\"Source\",\"correlationId\":\"CorrelationId\"}";

            // Act
            var sagaEvent = SagaEventJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(sagaEvent);
            Assert.Equal("id-123", sagaEvent.Id);
            Assert.Equal("saga-123", sagaEvent.SagaId);
            Assert.Equal("EventType", sagaEvent.EventType);
            Assert.Equal("EventName", sagaEvent.EventName);
            Assert.Equal("Description", sagaEvent.Description);
            Assert.Equal(DateTime.Parse("2022-01-01T00:00:00"), sagaEvent.Timestamp);
            Assert.Equal(EventSeverity.Information, sagaEvent.Severity);
            Assert.NotNull(sagaEvent.Data);
            Assert.Single(sagaEvent.Data);
            Assert.Contains("key", sagaEvent.Data.Keys);
            Assert.Equal("value", sagaEvent.Data["key"].ToString());
            Assert.Equal("step-123", sagaEvent.StepId);
            Assert.Equal("StepName", sagaEvent.StepName);
            Assert.Equal("Source", sagaEvent.Source);
            Assert.Equal("CorrelationId", sagaEvent.CorrelationId);
        }

        [Fact]
        public void EdgeCase_ToJson_NullInput()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaEventJsonExtensions.ToJson(null));
        }

        [Fact]
        public void EdgeCase_FromJson_NullInput()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => SagaEventJsonExtensions.FromJson(null));
        }
    }
}