using System;
using Xunit;
using SagaOrchestrator.Data.Repositories;

namespace dotnet_saga_orchestrator.Tests
{
    public class InMemorySagaRepositoryValidationTests
    {
        [Fact]
        public void Validate_NullInstance_ReturnsNullMessage()
        {
            // Arrange
            InMemorySagaRepository? repository = null;

            // Act
            var result = repository.Validate();

            // Assert
            Assert.Single(result);
            Assert.Equal("InMemorySagaRepository instance is null.", result[0]);
        }

        [Fact]
        public void Validate_ValidInstance_ReturnsEmptyList()
        {
            // Arrange
            var repository = new InMemorySagaRepository();

            // Act
            var result = repository.Validate();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_ValidInstance_ReturnsReadOnlyList()
        {
            // Arrange
            var repository = new InMemorySagaRepository();

            // Act
            var result = repository.Validate();

            // Assert
            Assert.IsAssignableFrom<System.Collections.Generic.IReadOnlyList<string>>(result);
        }

        [Fact]
        public void IsValid_ValidInstance_ReturnsTrue()
        {
            // Arrange
            var repository = new InMemorySagaRepository();

            // Act
            var result = repository.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_NullInstance_ReturnsFalse()
        {
            // Arrange
            InMemorySagaRepository repository = null!;

            // Act
            // Nullable annotations are not enforced at runtime, so the extension
            // method still runs; Validate() treats null as an invalid instance,
            // so IsValid must reflect that instead of throwing.
            var result = repository.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_ValidInstance_DoesNotThrow()
        {
            // Arrange
            var repository = new InMemorySagaRepository();

            // Act
            var exception = Record.Exception(() => repository.EnsureValid());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_NullInstance_ThrowsArgumentNullException()
        {
            // Arrange
            InMemorySagaRepository? repository = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => repository!.EnsureValid());
        }

        [Fact]
        public void EnsureValid_CalledMultipleTimesOnSameInstance_RemainsConsistent()
        {
            // Arrange
            var repository = new InMemorySagaRepository();

            // Act
            var firstCall = Record.Exception(() => repository.EnsureValid());
            var secondCall = Record.Exception(() => repository.EnsureValid());

            // Assert
            Assert.Null(firstCall);
            Assert.Null(secondCall);
        }
    }
}
