using System;
using Xunit;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Tests
{
    public class InMemorySagaDefinitionRepositoryValidationTests
    {
        [Fact]
        public void Validate_WithNullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            InMemorySagaDefinitionRepository repository = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => repository.Validate());
        }

        [Fact]
        public void Validate_WithValidRepository_ReturnsEmptyList()
        {
            // Arrange
            var repository = new InMemorySagaDefinitionRepository();

            // Act
            var result = repository.Validate();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void IsValid_WithNullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            InMemorySagaDefinitionRepository repository = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => repository.IsValid());
        }

        [Fact]
        public void IsValid_WithValidRepository_ReturnsTrue()
        {
            // Arrange
            var repository = new InMemorySagaDefinitionRepository();

            // Act
            var result = repository.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EnsureValid_WithNullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            InMemorySagaDefinitionRepository repository = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => repository.EnsureValid());
        }

        [Fact]
        public void EnsureValid_WithValidRepository_DoesNotThrow()
        {
            // Arrange
            var repository = new InMemorySagaDefinitionRepository();

            // Act & Assert
            var exception = Record.Exception(() => repository.EnsureValid());
            Assert.Null(exception);
        }
    }
}
