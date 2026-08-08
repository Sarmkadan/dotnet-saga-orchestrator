using Xunit;
using SagaOrchestrator.Core.Exceptions;

namespace SagaOrchestrator.Tests
{
    public class InvalidSagaDefinitionExceptionTests
    {
        [Fact]
        public void Constructor_Message()
        {
            // Arrange and Act
            var exception = new InvalidSagaDefinitionException("Test message");

            // Assert
            Assert.Equal("Test message", exception.Message);
        }

        [Fact]
        public void Constructor_MessageAndInnerException()
        {
            // Arrange
            var innerException = new Exception("Inner exception");

            // Act
            var exception = new InvalidSagaDefinitionException("Test message", innerException);

            // Assert
            Assert.Equal("Test message", exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void Constructor_DefinitionIdAndMessage()
        {
            // Act
            var exception = new InvalidSagaDefinitionException("definitionId", "Test message");

            // Assert
            Assert.Equal($"Saga definition 'definitionId' is invalid: Test message", exception.Message);
        }

        [Fact]
        public void Constructor_NullMessage_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new InvalidSagaDefinitionException(null));
        }

        [Fact]
        public void Constructor_NullDefinitionId_ThrowsArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => new InvalidSagaDefinitionException(null, "Test message"));
        }
    }
}
