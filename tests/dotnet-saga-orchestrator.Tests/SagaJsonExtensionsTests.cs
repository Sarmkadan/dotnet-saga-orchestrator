using System;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Tests
{
    public class SagaJsonExtensionsTests
    {
        private static Saga CreateSampleSaga()
        {
            // The Saga class is expected to have a parameterless constructor.
            // Populate a few typical properties if they exist; otherwise, the default
            // instance is sufficient for serialization round‑trip tests.
            var saga = new Saga();

            // Attempt to set common properties via reflection if they exist.
            // This avoids compile‑time dependencies on the exact shape of the class.
            var type = typeof(Saga);
            var idProp = type.GetProperty("Id");
            if (idProp != null && idProp.CanWrite)
                idProp.SetValue(saga, Guid.NewGuid().ToString());

            var nameProp = type.GetProperty("Name");
            if (nameProp != null && nameProp.CanWrite)
                nameProp.SetValue(saga, "SampleSaga");

            return saga;
        }

        [Fact]
        public void ToJson_WithValidSaga_ReturnsNonEmptyString()
        {
            // Arrange
            var saga = CreateSampleSaga();

            // Act
            var json = saga.ToJson();

            // Assert
            json.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void ToJson_WithIndentation_ProducesMultilineJson()
        {
            // Arrange
            var saga = CreateSampleSaga();

            // Act
            var json = saga.ToJson(indented: true);

            // Assert
            json.Should().Contain(Environment.NewLine);
        }

        [Fact]
        public void ToJson_NullSaga_ThrowsArgumentNullException()
        {
            // Arrange
            Saga? saga = null;

            // Act
            Action act = () => saga!.ToJson();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsDeserializedSaga()
        {
            // Arrange
            var original = CreateSampleSaga();
            var json = original.ToJson();

            // Act
            var deserialized = SagaJsonExtensions.FromJson(json);

            // Assert
            deserialized.Should().NotBeNull();
            deserialized!.GetType().Should().Be(typeof(Saga));
        }

        [Fact]
        public void FromJson_NullOrEmptyJson_ThrowsArgumentException()
        {
            // Act
            Action actNull = () => SagaJsonExtensions.FromJson(null!);
            Action actEmpty = () => SagaJsonExtensions.FromJson(string.Empty);

            // Assert
            actNull.Should().Throw<ArgumentException>();
            actEmpty.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndOutValue()
        {
            // Arrange
            var original = CreateSampleSaga();
            var json = original.ToJson();

            // Act
            var success = SagaJsonExtensions.TryFromJson(json, out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result!.GetType().Should().Be(typeof(Saga));
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNullOut()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";

            // Act
            var success = SagaJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_NullOrEmptyJson_ThrowsArgumentException()
        {
            // Act
            Action actNull = () => SagaJsonExtensions.TryFromJson(null!, out _);
            Action actEmpty = () => SagaJsonExtensions.TryFromJson(string.Empty, out _);

            // Assert
            actNull.Should().Throw<ArgumentException>();
            actEmpty.Should().Throw<ArgumentException>();
        }
    }
}
