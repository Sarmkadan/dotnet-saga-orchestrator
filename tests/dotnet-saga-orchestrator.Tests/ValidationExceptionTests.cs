using System;
using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class ValidationExceptionTests
    {
        [Fact]
        public void Constructor_NoParameters_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException());
        }

        [Fact]
        public void Constructor_Message_ThrowsArgumentNullException_WhenMessageIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException(null));
        }

        [Fact]
        public void Constructor_Message_ThrowsArgumentNullException_WhenMessageIsEmpty()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException(""));
        }

        [Fact]
        public void Constructor_Message_ThrowsArgumentNullException_WhenMessageIsWhiteSpace()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException("   "));
        }

        [Fact]
        public void Constructor_Message_ThrowsArgumentNullException_WhenMessageIsNullAndInnerExceptionIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException(null, null));
        }

        [Fact]
        public void Constructor_Message_ThrowsArgumentNullException_WhenMessageIsEmptyAndInnerExceptionIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException("", null));
        }

        [Fact]
        public void Constructor_Message_ThrowsArgumentNullException_WhenMessageIsWhiteSpaceAndInnerExceptionIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException("   ", null));
        }

        [Fact]
        public void Constructor_Message_ThrowsArgumentNullException_WhenMessageIsNullAndInnerExceptionIsNotNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException(null, new Exception()));
        }

        [Fact]
        public void Constructor_Message_ThrowsArgumentNullException_WhenMessageIsEmptyAndInnerExceptionIsNotNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException("", new Exception()));
        }

        [Fact]
        public void Constructor_Message_ThrowsArgumentNullException_WhenMessageIsWhiteSpaceAndInnerExceptionIsNotNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ValidationException("   ", new Exception()));
        }
    }
}
