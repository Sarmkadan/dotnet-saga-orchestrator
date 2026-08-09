using System;
using System.Collections.Generic;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class ExceptionMapperTests
    {
        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var errorResponse = new ErrorResponse
            {
                Code = "code",
                Message = "message",
                Timestamp = DateTime.UtcNow,
                RequestId = "requestId"
            };

            // Act
            var result = ExceptionMapperValidation.Validate(errorResponse);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ExceptionMapperValidation.Validate(null));
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var errorResponse = new ErrorResponse
            {
                Code = "code",
                Message = "message",
                Timestamp = DateTime.UtcNow,
                RequestId = "requestId"
            };

            // Act
            var result = ExceptionMapperValidation.IsValid(errorResponse);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_NullInput_ReturnsFalse()
        {
            // Act
            var result = ExceptionMapperValidation.IsValid(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var errorResponse = new ErrorResponse
            {
                Code = "code",
                Message = "message",
                Timestamp = DateTime.UtcNow,
                RequestId = "requestId"
            };

            // Act and Assert
            ExceptionMapperValidation.EnsureValid(errorResponse);
        }

        [Fact]
        public void EnsureValid_NullInput_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => ExceptionMapperValidation.EnsureValid(null));
        }

        [Fact]
        public void EnsureValid_InvalidInput_ThrowsArgumentException()
        {
            // Arrange
            var errorResponse = new ErrorResponse
            {
                Code = string.Empty,
                Message = "message",
                Timestamp = DateTime.UtcNow,
                RequestId = "requestId"
            };

            // Act and Assert
            Assert.Throws<ArgumentException>(() => ExceptionMapperValidation.EnsureValid(errorResponse));
        }
    }
}