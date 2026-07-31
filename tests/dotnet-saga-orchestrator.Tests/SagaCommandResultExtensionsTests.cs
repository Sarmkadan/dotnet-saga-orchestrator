using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Tests
{
    public class SagaCommandResultExtensionsTests
    {
        [Fact]
        public void ToTypedResult_ValidInput_ReturnsTypedResult()
        {
            // Arrange
            var untyped = SagaCommandResult.SuccessResult("Success", "data");
            
            // Act
            var typed = untyped.ToTypedResult<string>("typedData");

            // Assert
            typed.Success.Should().Be(untyped.Success);
            typed.Data.Should().Be("typedData");
            typed.RequestId.Should().Be(untyped.RequestId);
        }

        [Fact]
        public void ToUntypedResult_ValidInput_ReturnsUntypedResult()
        {
            // Arrange
            var typed = SagaCommandResult<object?>.SuccessResult("data", "Success");
            
            // Act
            var untyped = typed.ToUntypedResult();

            // Assert
            untyped.Success.Should().Be(typed.Success);
            untyped.Data.Should().Be(typed.Data);
            untyped.RequestId.Should().Be(typed.RequestId);
        }

        [Fact]
        public void WithError_AddsErrorToFailedResult()
        {
            // Arrange
            var result = SagaCommandResult.FailureResult("Failed");
            
            // Act
            result.WithError("New Error");

            // Assert
            result.Errors.Should().Contain("New Error");
        }

        [Fact]
        public void ToPaginatedResult_ValidInput_ReturnsPaginatedResult()
        {
            // Arrange
            var data = Enumerable.Range(1, 10).Select(i => i.ToString()).ToList();
            var result = SagaCommandResult<IEnumerable<string>>.SuccessResult(data, "Success");
            
            // Act
            var paginated = result.ToPaginatedResult(1, 5);

            // Assert
            paginated.Items.Should().HaveCount(5);
            paginated.TotalCount.Should().Be(10);
        }

        [Fact]
        public void Combine_MultipleSuccessfulResults_ReturnsSuccess()
        {
            // Arrange
            var results = new List<SagaCommandResult>
            {
                SagaCommandResult.SuccessResult(),
                SagaCommandResult.SuccessResult()
            };
            
            // Act
            var combined = results.Combine();

            // Assert
            combined.Success.Should().BeTrue();
        }

        [Fact]
        public void AsFailure_ReturnsFailureResultWithSameErrors()
        {
            // Arrange
            var result = SagaCommandResult.FailureResult("Original", "Error1");
            
            // Act
            var failure = result.AsFailure("New Message");

            // Assert
            failure.Success.Should().BeFalse();
            failure.Message.Should().Be("New Message");
            failure.Errors.Should().Contain("Error1");
        }

        [Fact]
        public void HasError_ReturnsTrueIfErrorExists()
        {
            // Arrange
            var result = SagaCommandResult.FailureResult("Failed", "Error1", "Error2");
            
            // Act
            var hasError = result.HasError("Error1");

            // Assert
            hasError.Should().BeTrue();
        }
    }
}
