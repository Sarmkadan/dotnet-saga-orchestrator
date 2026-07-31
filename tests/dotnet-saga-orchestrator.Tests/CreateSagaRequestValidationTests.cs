using System;
using System.Collections.Generic;
using Xunit;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Tests
{
    public class CreateSagaRequestValidationTests
    {
        private static CreateSagaRequest ValidRequest => new CreateSagaRequest
        {
            DefinitionId = "def-123",
            DefinitionName = null,
            MaxRetries = 3,
            TimeoutSeconds = 30
        };

        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var request = ValidRequest;

            // Act
            IReadOnlyList<string> errors = request.Validate();

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_MissingDefinition_ReturnsError()
        {
            // Arrange
            var request = new CreateSagaRequest
            {
                DefinitionId = null,
                DefinitionName = null,
                MaxRetries = 0,
                TimeoutSeconds = 10
            };

            // Act
            IReadOnlyList<string> errors = request.Validate();

            // Assert
            Assert.Contains("Either DefinitionId or DefinitionName must be provided.", errors);
        }

        [Fact]
        public void Validate_NegativeMaxRetries_ReturnsError()
        {
            // Arrange
            var request = new CreateSagaRequest
            {
                DefinitionId = "def-123",
                MaxRetries = -1,
                TimeoutSeconds = 10
            };

            // Act
            IReadOnlyList<string> errors = request.Validate();

            // Assert
            Assert.Contains(
                "MaxRetries must be greater than or equal to 0, but was -1.",
                errors);
        }

        [Fact]
        public void Validate_NonPositiveTimeoutSeconds_ReturnsError()
        {
            // Arrange
            var request = new CreateSagaRequest
            {
                DefinitionId = "def-123",
                MaxRetries = 0,
                TimeoutSeconds = 0
            };

            // Act
            IReadOnlyList<string> errors = request.Validate();

            // Assert
            Assert.Contains(
                "TimeoutSeconds must be greater than 0, but was 0.",
                errors);
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var request = ValidRequest;

            // Act
            bool isValid = request.IsValid();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_InvalidRequest_ReturnsFalse()
        {
            // Arrange
            var request = new CreateSagaRequest
            {
                DefinitionId = null,
                DefinitionName = null,
                MaxRetries = -5,
                TimeoutSeconds = -10
            };

            // Act
            bool isValid = request.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var request = ValidRequest;

            // Act / Assert
            var exception = Record.Exception(() => request.EnsureValid());
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_InvalidRequest_ThrowsArgumentException()
        {
            // Arrange
            var request = new CreateSagaRequest
            {
                DefinitionId = null,
                DefinitionName = null,
                MaxRetries = -1,
                TimeoutSeconds = 0
            };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => request.EnsureValid());
            Assert.Contains("Either DefinitionId or DefinitionName must be provided.", ex.Message);
            Assert.Contains("MaxRetries must be greater than or equal to 0, but was -1.", ex.Message);
            Assert.Contains("TimeoutSeconds must be greater than 0, but was 0.", ex.Message);
        }

        [Fact]
        public void Validate_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            CreateSagaRequest? request = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => request!.Validate());
        }

        [Fact]
        public void EnsureValid_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            CreateSagaRequest? request = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => request!.EnsureValid());
        }

        [Fact]
        public void IsValid_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            CreateSagaRequest? request = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => request!.IsValid());
        }
    }
}
