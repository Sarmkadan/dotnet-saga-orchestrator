using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Tests
{
    public class SagaEventPublisherValidationTests
    {
        [Fact]
        public void Validate_NullSagaEventPublisher_ThrowsArgumentNullException()
        {
            // Arrange
            SagaEventPublisher publisher = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SagaEventPublisherValidation.Validate(publisher));
        }

        [Fact]
        public void Validate_ValidSagaEventPublisher_ReturnsEmptyList()
        {
            // Arrange
            var publisher = new SagaEventPublisher();

            // Act
            var result = SagaEventPublisherValidation.Validate(publisher);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void IsValid_NullSagaEventPublisher_ThrowsArgumentNullException()
        {
            // Arrange
            SagaEventPublisher publisher = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SagaEventPublisherValidation.IsValid(publisher));
        }

        [Fact]
        public void IsValid_ValidSagaEventPublisher_ReturnsTrue()
        {
            // Arrange
            var publisher = new SagaEventPublisher();

            // Act
            var result = SagaEventPublisherValidation.IsValid(publisher);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EnsureValid_NullSagaEventPublisher_ThrowsArgumentNullException()
        {
            // Arrange
            SagaEventPublisher publisher = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SagaEventPublisherValidation.EnsureValid(publisher));
        }

        [Fact]
        public void EnsureValid_ValidSagaEventPublisher_DoesNotThrow()
        {
            // Arrange
            var publisher = new SagaEventPublisher();

            // Act
            var exception = Record.Exception(() => SagaEventPublisherValidation.EnsureValid(publisher));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Validate_NullSagaEvent_ThrowsArgumentNullException()
        {
            // Arrange
            SagaEvent sagaEvent = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => SagaEventPublisherValidation.Validate(sagaEvent));
        }

        [Fact]
        public void Validate_ValidSagaEvent_ReturnsEmptyList()
        {
            // Arrange
            var sagaEvent = new SagaEvent
            {
                Id = Guid.NewGuid().ToString(),
                SagaId = Guid.NewGuid().ToString(),
                EventType = "TestEvent",
                EventName = "Test Event",
                Source = "TestSource",
                Timestamp = DateTime.UtcNow,
                Severity = EventSeverity.Information,
                Data = new Dictionary<string, object> { { "key", "value" } }
            };

            // Act
            var result = SagaEventPublisherValidation.Validate(sagaEvent);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_SagaEventWithInvalidId_ReturnsErrorMessage()
        {
            // Arrange
            var sagaEvent = new SagaEvent
            {
                Id = null, // Invalid: null
                SagaId = Guid.NewGuid().ToString(),
                EventType = "TestEvent",
                EventName = "Test Event",
                Source = "TestSource",
                Timestamp = DateTime.UtcNow,
                Severity = EventSeverity.Information,
                Data = new Dictionary<string, object>()
            };

            // Act
            var result = SagaEventPublisherValidation.Validate(sagaEvent);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains("Id cannot be null or whitespace.", result);
        }

        [Fact]
        public void Validate_SagaEventWithInvalidTimestampNonUtc_ReturnsErrorMessage()
        {
            // Arrange
            var sagaEvent = new SagaEvent
            {
                Id = Guid.NewGuid().ToString(),
                SagaId = Guid.NewGuid().ToString(),
                EventType = "TestEvent",
                EventName = "Test Event",
                Source = "TestSource",
                Timestamp = DateTime.Now, // Not Utc
                Severity = EventSeverity.Information,
                Data = new Dictionary<string, object>()
            };

            // Act
            var result = SagaEventPublisherValidation.Validate(sagaEvent);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains("Timestamp must be in UTC format.", result);
        }
    }
}