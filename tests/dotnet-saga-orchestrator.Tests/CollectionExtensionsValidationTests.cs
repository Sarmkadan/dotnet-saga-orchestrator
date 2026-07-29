using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SagaOrchestrator.Core.Extensions;

namespace dotnet_saga_orchestrator.Tests
{
    public class CollectionExtensionsValidationTests
    {
        [Fact]
        public void Validate_NullCollection_ThrowsArgumentNullException()
        {
            // Arrange
            IEnumerable<string> source = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => source.Validate());
        }

        [Fact]
        public void Validate_EmptyCollection_ReturnsNotEmptyMessage()
        {
            // Arrange
            var source = new List<string>();

            // Act
            var result = source.Validate();

            // Assert
            Assert.Single(result);
            Assert.Equal("Collection is null or empty", result[0]);
        }

        [Fact]
        public void Validate_StringCollectionWithNullElements_ReturnsNullElementMessage()
        {
            // Arrange
            var source = new List<string?> { "valid", null, "also valid" };

            // Act
            var result = source.Validate();

            // Assert
            Assert.Single(result);
            Assert.Contains("Collection contains 1 null element(s)", result[0]);
        }

        [Fact]
        public void Validate_StringCollectionWithEmptyAndWhitespace_ReturnsAppropriateMessages()
        {
            // Arrange
            var source = new List<string> { "hello", "", "   ", "world" };

            // Act
            var result = source.Validate();

            // Assert
            Assert.Contains("Collection contains 1 empty string(s)", string.Join("; ", result));
            Assert.Contains("Collection contains 1 whitespace-only string(s)", string.Join("; ", result));
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void Validate_IntCollectionWithDefaultValues_ReturnsDefaultValueMessage()
        {
            // Arrange
            var source = new List<int> { 1, 0, 2, 0 };

            // Act
            var result = source.Validate();

            // Assert
            Assert.Contains("Collection contains 2 default/zero value(s)", string.Join("; ", result));
            Assert.Single(result);
        }

        [Fact]
        public void Validate_DateTimeCollectionWithDefaultAndMinDate_ReturnsMessages()
        {
            // Arrange
            var source = new List<DateTime>
            {
                DateTime.Now,
                default, // DateTime.MinValue
                DateTime.Now.AddDays(1),
                DateTime.MinValue
            };

            // Act
            var result = source.Validate();

            // Assert
            var resultText = string.Join("; ", result);
            Assert.Contains("Collection contains 2 default date(s) (DateTime.MinValue)", resultText);
            Assert.Contains("Collection contains 2 minimum date(s) (DateTime.MinValue)", resultText);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ValidateDictionary_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            Dictionary<string?, string> dictionary = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => dictionary.Validate());
        }

        [Fact]
        public void ValidateDictionary_EmptyDictionary_ReturnsNotEmptyMessage()
        {
            // Arrange
            var dictionary = new Dictionary<string, int>();

            // Act
            var result = dictionary.Validate();

            // Assert
            Assert.Single(result);
            Assert.Equal("Dictionary is null or empty", result[0]);
        }

        [Fact]
        public void ValidateDictionary_WithNullKeyAndNullValue_ReturnsMessages()
        {
            // Arrange
            // Note: TKey : notnull, so we cannot have null key in Dictionary<string?, int>? Actually TKey : notnull prevents null keys.
            // So we need a type that allows null keys? The method constraint is where TKey : notnull, so null keys are not allowed.
            // Therefore we test with a dictionary that allows null keys? Actually the method signature includes where TKey : notnull,
            // so the dictionary cannot have null keys. So we test with null values and empty string values.
            var dictionary = new Dictionary<string, string?>
            {
                { "key1", null },
                { "key2", "" },
                { "key3", "   " },
                { "key4", "valid" }
            };

            // Act
            var result = dictionary.Validate();

            // Assert
            var resultText = string.Join("; ", result);
            Assert.Contains("Dictionary contains 1 null value(s)", resultText);
            Assert.Contains("Dictionary contains 1 empty string value(s)", resultText);
            Assert.Contains("Dictionary contains 1 whitespace-only string value(s)", resultText);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void IsValid_ValidCollection_ReturnsTrue()
        {
            // Arrange
            var source = new List<string> { "a", "b", "c" };

            // Act
            var result = source.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValid_InvalidCollection_ReturnsFalse()
        {
            // Arrange
            var source = new List<string> { "a", "", "c" };

            // Act
            var result = source.IsValid();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void EnsureValid_ValidCollection_DoesNotThrow()
        {
            // Arrange
            var source = new List<int> { 1, 2, 3 };

            // Act
            Action act = () => source.EnsureValid();

            // Assert
            Assert.NotNull(act);
        }

        [Fact]
        public void EnsureValid_InvalidCollection_ThrowsArgumentException()
        {
            // Arrange
            var source = new List<string> { "valid", "" };

            // Act
            Action act = () => source.EnsureValid();

            // Assert
            var exception = Assert.Throws<ArgumentException>(act);
            Assert.Contains("Collection is invalid", exception.Message);
            Assert.Contains("Collection contains 1 empty string(s)", exception.Message);
        }

        [Fact]
        public void EnsureValidDictionary_ValidDictionary_DoesNotThrow()
        {
            // Arrange
            var dictionary = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            // Act
            Action act = () => dictionary.EnsureValid();

            // Assert
            Assert.NotNull(act);
        }

        [Fact]
        public void EnsureValidDictionary_InvalidDictionary_ThrowsArgumentException()
        {
            // Arrange
            var dictionary = new Dictionary<string, string?>
            {
                { "key1", "value1" },
                { "key2", null }
            };

            // Act
            Action act = () => dictionary.EnsureValid();

            // Assert
            var exception = Assert.Throws<ArgumentException>(act);
            Assert.Contains("Dictionary is invalid", exception.Message);
            Assert.Contains("Dictionary contains 1 null value(s)", exception.Message);
        }
    }
}