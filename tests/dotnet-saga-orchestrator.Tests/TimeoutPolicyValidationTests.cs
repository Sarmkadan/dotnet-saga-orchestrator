using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests
{
    public class TimeoutPolicyValidationTests
    {
        [Fact]
        public void Validate_ValidPolicy_ReturnsNoErrors()
        {
            // Arrange
            var policy = TimeoutPolicy.CreateStandard();

            // Act
            var errors = policy.Validate();

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_NullPolicy_ThrowsArgumentNullException()
        {
            // Arrange
            TimeoutPolicy? policy = null;

            // Act
            Action act = () => policy.Validate();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsValid_ValidPolicy_ReturnsTrue()
        {
            // Arrange
            var policy = TimeoutPolicy.CreateStrict();

            // Act
            var isValid = policy.IsValid();

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void EnsureValid_ValidPolicy_DoesNotThrow()
        {
            // Arrange
            var policy = TimeoutPolicy.CreateLenient();

            // Act
            Action act = () => policy.EnsureValid();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void EnsureValid_NullPolicy_ThrowsArgumentNullException()
        {
            // Arrange
            TimeoutPolicy? policy = null;

            // Act
            Action act = () => policy.EnsureValid();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
