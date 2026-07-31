using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xunit;
using Moq;

namespace SagaOrchestrator.Core.Domain.Models.Tests
{
    public class SagaEventTests
    {
        [Fact]
        public void HappyPath_CreateLifecycleEvent_ValidInput()
        {
            // Arrange
            var sagaEvent = SagaEvent.CreateLifecycleEvent("saga-123", "eventName", "description");

            // Act

            // Assert
            Assert.NotNull(sagaEvent);
            Assert.Equal("saga-123", sagaEvent.SagaId);
            Assert.Equal("eventName", sagaEvent.EventName);
            Assert.Equal("description", sagaEvent.Description);
            Assert.Equal(EventSeverity.Information, sagaEvent.Severity);
        }

        [Fact]
        public void HappyPath_CreateStepEvent_ValidInput()
        {
            // Arrange
            var sagaEvent = SagaEvent.CreateStepEvent("saga-123", "step-123", "stepName", "eventName", "description");

            // Act

            // Assert
            Assert.NotNull(sagaEvent);
            Assert.Equal("saga-123", sagaEvent.SagaId);
            Assert.Equal("step-123", sagaEvent.StepId);
            Assert.Equal("stepName", sagaEvent.StepName);
            Assert.Equal("eventName", sagaEvent.EventName);
            Assert.Equal("description", sagaEvent.Description);
            Assert.Equal(EventSeverity.Information, sagaEvent.Severity);
        }

        [Fact]
        public void HappyPath_CreateErrorEvent_ValidInput()
        {
            // Arrange
            var sagaEvent = SagaEvent.CreateErrorEvent("saga-123", "stepName", "errorMessage");

            // Act

            // Assert
            Assert.NotNull(sagaEvent);
            Assert.Equal("saga-123", sagaEvent.SagaId);
            Assert.Equal("stepName", sagaEvent.StepName);
            Assert.Equal("Error", sagaEvent.EventType);
            Assert.Equal("ExecutionError", sagaEvent.EventName);
            Assert.Equal("errorMessage", sagaEvent.Description);
            Assert.Equal(EventSeverity.Error, sagaEvent.Severity);
        }

        [Fact]
        public void EdgeCase_CreateLifecycleEvent_NullInput()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => SagaEvent.CreateLifecycleEvent(null, "eventName", "description"));
        }

        [Fact]
        public void EdgeCase_CreateStepEvent_NullInput()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => SagaEvent.CreateStepEvent("saga-123", null, "stepName", "eventName", "description"));
        }

        [Fact]
        public void EdgeCase_CreateErrorEvent_NullInput()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => SagaEvent.CreateErrorEvent("saga-123", null, "errorMessage"));
        }
    }
}