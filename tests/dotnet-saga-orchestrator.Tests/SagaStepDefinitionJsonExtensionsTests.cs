using System;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Tests
{
    public class SagaStepDefinitionJsonExtensionsTests
    {
        private static SagaStepDefinition CreateSampleStep()
            => new SagaStepDefinition { Name = "SampleStep" };

        [Fact]
        public void ToJson_WithValidObject_ReturnsCamelCaseJson()
        {
            // Arrange
            var step = CreateSampleStep();

            // Act
            var json = step.ToJson();

            // Assert
            json.Should().NotBeNullOrWhiteSpace();
            json.Should().Contain("\"name\":\"SampleStep\""); // camelCase property name
        }

        [Fact]
        public void ToJson_WithIndentation_ProducesMultilineJson()
        {
            // Arrange
            var step = CreateSampleStep();

            // Act
            var json = step.ToJson(indented: true);

            // Assert
            json.Should().Contain(Environment.NewLine);
        }

        [Fact]
        public void ToJson_NullArgument_ThrowsArgumentNullException()
        {
            // Arrange
            SagaStepDefinition? step = null;

            // Act
            Action act = () => step!.ToJson();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsDeserializedObject()
        {
            // Arrange
            var original = CreateSampleStep();
            var json = original.ToJson();

            // Act
            var result = SagaStepDefinitionJsonExtensions.FromJson(json);

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(original.Name);
        }

        [Fact]
        public void FromJson_NullOrEmptyJson_ThrowsArgumentException()
        {
            // Act
            Action actNull = () => SagaStepDefinitionJsonExtensions.FromJson(null!);
            Action actEmpty = () => SagaStepDefinitionJsonExtensions.FromJson(string.Empty);

            // Assert
            actNull.Should().Throw<ArgumentException>();
            actEmpty.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndOutValue()
        {
            // Arrange
            var original = CreateSampleStep();
            var json = original.ToJson();

            // Act
            var success = SagaStepDefinitionJsonExtensions.TryFromJson(json, out var result);

            // Assert
            success.Should().BeTrue();
            result.Should().NotBeNull();
            result!.Name.Should().Be(original.Name);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNullOut()
        {
            // Arrange
            var invalidJson = "{ this is not valid json }";

            // Act
            var success = SagaStepDefinitionJsonExtensions.TryFromJson(invalidJson, out var result);

            // Assert
            success.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_NullOrEmptyJson_ThrowsArgumentException()
        {
            // Act
            Action actNull = () => SagaStepDefinitionJsonExtensions.TryFromJson(null!, out _);
            Action actEmpty = () => SagaStepDefinitionJsonExtensions.TryFromJson(string.Empty, out _);

            // Assert
            actNull.Should().Throw<ArgumentException>();
            actEmpty.Should().Throw<ArgumentException>();
        }
    }
}
