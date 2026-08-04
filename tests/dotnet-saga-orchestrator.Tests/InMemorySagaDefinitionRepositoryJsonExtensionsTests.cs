using System;
using System.Text.Json;
using SagaOrchestrator.Data.Repositories;
using Xunit;

namespace SagaOrchestrator.Tests
{
    public class InMemorySagaDefinitionRepositoryJsonExtensionsTests
    {
        // Helper to create a minimal repository instance.
        // The actual InMemorySagaDefinitionRepository type is not shown,
        // but it is assumed to have a public parameterless constructor.
        private static InMemorySagaDefinitionRepository CreateRepository()
        {
            // If the repository requires initialization of internal collections,
            // they can be left at their defaults – the purpose of the tests is
            // to verify (de)serialization behaviour, not business logic.
            return new InMemorySagaDefinitionRepository();
        }

        [Fact]
        public void ToJson_WithValidRepository_ReturnsNonEmptyJson()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var json = repo.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            // The JSON should be parseable by System.Text.Json
            using var doc = JsonDocument.Parse(json);
            Assert.NotNull(doc.RootElement);
        }

        [Fact]
        public void ToJson_WithIndentation_ProducesReadableJson()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var json = repo.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json); // indented JSON contains line breaks
        }

        [Fact]
        public void ToJson_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            InMemorySagaDefinitionRepository? repo = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => repo!.ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsRepository()
        {
            // Arrange
            var original = CreateRepository();
            var json = original.ToJson();

            // Act
            var deserialized = InMemorySagaDefinitionRepositoryJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            // At minimum the round‑trip should give an object of the same type.
            Assert.IsType<InMemorySagaDefinitionRepository>(deserialized);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => InMemorySagaDefinitionRepositoryJsonExtensions.FromJson(json!));
        }

        [Fact]
        public void FromJson_EmptyOrWhiteSpaceJson_ReturnsNull()
        {
            // Arrange
            var empty = "";
            var whitespace = "   \t\r\n";

            // Act
            var resultEmpty = InMemorySagaDefinitionRepositoryJsonExtensions.FromJson(empty);
            var resultWhite = InMemorySagaDefinitionRepositoryJsonExtensions.FromJson(whitespace);

            // Assert
            Assert.Null(resultEmpty);
            Assert.Null(resultWhite);
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";

            // Act
            var result = InMemorySagaDefinitionRepositoryJsonExtensions.FromJson(invalidJson);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndRepository()
        {
            // Arrange
            var repo = CreateRepository();
            var json = repo.ToJson();

            // Act
            var success = InMemorySagaDefinitionRepositoryJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserialized);
            Assert.IsType<InMemorySagaDefinitionRepository>(deserialized);
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                InMemorySagaDefinitionRepositoryJsonExtensions.TryFromJson(json!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyJson_ReturnsFalse()
        {
            // Arrange
            var empty = "";

            // Act
            var success = InMemorySagaDefinitionRepositoryJsonExtensions.TryFromJson(empty, out var deserialized);

            // Assert
            Assert.False(success);
            Assert.Null(deserialized);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var invalidJson = "[ not a valid json";

            // Act
            var success = InMemorySagaDefinitionRepositoryJsonExtensions.TryFromJson(invalidJson, out var deserialized);

            // Assert
            Assert.False(success);
            Assert.Null(deserialized);
        }
    }
}
