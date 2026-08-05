using System;
using System.Text.Json;
using SagaOrchestrator.Data.Repositories;
using Xunit;

namespace SagaOrchestrator.Tests
{
    public class InMemorySagaRepositoryJsonExtensionsTests
    {
        private static InMemorySagaRepository CreateRepository()
        {
            return new InMemorySagaRepository();
        }

        [Fact]
        public void ToJson_WithValidRepository_ReturnsParseableJson()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var json = repo.ToJson();

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            using var doc = JsonDocument.Parse(json);
            Assert.NotNull(doc.RootElement);
        }

        [Fact]
        public void ToJson_WithIndentation_ProducesMultiLineJson()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var json = repo.ToJson(indented: true);

            // Assert
            Assert.Contains("\n", json);
        }

        [Fact]
        public void ToJson_NullRepository_ThrowsArgumentNullException()
        {
            // Arrange
            InMemorySagaRepository? repo = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => repo!.ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsRepositoryInstance()
        {
            // Arrange
            var original = CreateRepository();
            var json = original.ToJson();

            // Act
            var deserialized = InMemorySagaRepositoryJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.IsType<InMemorySagaRepository>(deserialized);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => InMemorySagaRepositoryJsonExtensions.FromJson(json!));
        }

        [Fact]
        public void FromJson_EmptyOrWhitespaceJson_ThrowsArgumentException()
        {
            // Arrange
            var whitespace = "   \t\r\n";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => InMemorySagaRepositoryJsonExtensions.FromJson(string.Empty));
            Assert.Throws<ArgumentException>(() => InMemorySagaRepositoryJsonExtensions.FromJson(whitespace));
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";

            // Act
            var result = InMemorySagaRepositoryJsonExtensions.FromJson(invalidJson);

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
            var success = InMemorySagaRepositoryJsonExtensions.TryFromJson(json, out var deserialized);

            // Assert
            Assert.True(success);
            Assert.NotNull(deserialized);
            Assert.IsType<InMemorySagaRepository>(deserialized);
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                InMemorySagaRepositoryJsonExtensions.TryFromJson(json!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyJson_ThrowsArgumentException()
        {
            // Arrange
            var empty = string.Empty;

            // Act & Assert
            Assert.Throws<ArgumentException>(() => InMemorySagaRepositoryJsonExtensions.TryFromJson(empty, out _));
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var invalidJson = "[ not a valid json";

            // Act
            var success = InMemorySagaRepositoryJsonExtensions.TryFromJson(invalidJson, out var deserialized);

            // Assert
            Assert.False(success);
            Assert.Null(deserialized);
        }
    }
}
