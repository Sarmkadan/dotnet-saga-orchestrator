using System;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Extensions;

namespace SagaOrchestrator.Tests
{
    public class DateTimeExtensionsValidationTests
    {
        [Fact]
        public void Validate_UtcNow_ReturnsEmptyList()
        {
            // Arrange
            var value = DateTime.UtcNow;

            // Act
            var result = value.Validate();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Validate_DefaultDateTime_ReturnsErrorList()
        {
            // Arrange
            var value = default(DateTime);

            // Act
            var result = value.Validate();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().ContainSingle();
            result.First().Should().Be("DateTime value is default (uninitialized).");
        }

        [Fact]
        public void Validate_NonUtcDateTime_ReturnsErrorList()
        {
            // Arrange
            var value = DateTime.Now; // Local time

            // Act
            var result = value.Validate();

            // Assert
            result.Should().NotBeEmpty();
            // Expect at least one error about DateTimeKind
            result.Should().Contain(s => s.Contains("DateTimeKind") && s.Contains("expected UTC"));
        }

        [Fact]
        public void IsValid_UtcNow_ReturnsTrue()
        {
            // Arrange
            var value = DateTime.UtcNow;

            // Act
            var result = value.IsValid();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsValid_DefaultDateTime_ReturnsFalse()
        {
            // Arrange
            var value = default(DateTime);

            // Act
            var result = value.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_NonUtcDateTime_ReturnsFalse()
        {
            // Arrange
            var value = DateTime.Now; // Local time

            // Act
            var result = value.IsValid();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void EnsureValid_UtcNow_DoesNotThrow()
        {
            // Arrange
            var value = DateTime.UtcNow;

            // Act
            Action act = () => value.EnsureValid();

            // Assert
            act.Should().NotThrow<ArgumentException>();
        }

        [Fact]
        public void EnsureValid_DefaultDateTime_ThrowsArgumentException()
        {
            // Arrange
            var value = default(DateTime);

            // Act
            Action act = () => value.EnsureValid();

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("*DateTime value is default (uninitialized).*");
        }
    }
}