using System;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Tests
{
    public class SagaCommandResultTests
    {
        [Fact]
        public void DefaultConstructor_ShouldInitializeDefaults()
        {
            // Arrange & Act
            var result = new SagaCommandResult();

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().BeEmpty();
            result.Data.Should().BeNull();
            result.Errors.Should().BeEmpty();
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            Guid.TryParse(result.RequestId, out var guid).Should().BeTrue();
            guid.Should().NotBeEmpty();
        }

        [Fact]
        public void SuccessResult_ShouldSetSuccessTrueAndPopulateProperties()
        {
            // Arrange
            var data = new { Id = 1, Name = "test" };
            var customMessage = "All good";

            // Act
            var result = SagaCommandResult.SuccessResult(customMessage, data);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be(customMessage);
            result.Data.Should().BeSameAs(data);
            result.Errors.Should().BeEmpty();
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            Guid.TryParse(result.RequestId, out var guid).Should().BeTrue();
            guid.Should().NotBeEmpty();
        }

        [Fact]
        public void FailureResult_ShouldSetSuccessFalseAndCaptureErrors()
        {
            // Arrange
            var errorMessage = "Something failed";
            var errors = new[] { "Error1", "Error2" };

            // Act
            var result = SagaCommandResult.FailureResult(errorMessage, errors);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be(errorMessage);
            result.Errors.Should().BeEquivalentTo(errors);
            result.Data.Should().BeNull();
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            Guid.TryParse(result.RequestId, out var guid).Should().BeTrue();
            guid.Should().NotBeEmpty();
        }

        [Fact]
        public void ExceptionResult_ShouldCaptureExceptionMessage()
        {
            // Arrange
            var ex = new InvalidOperationException("Invalid operation");

            // Act
            var result = SagaCommandResult.ExceptionResult(ex);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("An error occurred during the operation");
            result.Errors.Should().ContainSingle().Which.Should().Be(ex.Message);
            result.Data.Should().BeNull();
            result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            Guid.TryParse(result.RequestId, out var guid).Should().BeTrue();
            guid.Should().NotBeEmpty();
        }

        [Fact]
        public void ErrorsList_ShouldBeIndependentBetweenInstances()
        {
            // Arrange
            var first = SagaCommandResult.FailureResult("first", "E1");
            var second = SagaCommandResult.FailureResult("second", "E2");

            // Act
            first.Errors.Add("extra");

            // Assert
            first.Errors.Should().Contain("E1").And.Contain("extra");
            second.Errors.Should().ContainSingle().Which.Should().Be("E2");
        }

        [Fact]
        public void SuccessResult_WithDefaultParameters_ShouldUseDefaults()
        {
            // Act
            var result = SagaCommandResult.SuccessResult();

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Operation completed successfully");
            result.Data.Should().BeNull();
            result.Errors.Should().BeEmpty();
        }
    }
}
