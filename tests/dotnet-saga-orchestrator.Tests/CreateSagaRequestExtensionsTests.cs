using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Tests
{
    public class CreateSagaRequestExtensionsTests
    {
        [Fact]
        public void HasValidTimeout_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            CreateSagaRequest request = null!;

            // Act
            Action act = () => request.HasValidTimeout();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(10, true)]
        [InlineData(-1, false)]
        public void HasValidTimeout_VariousValues_ReturnsExpectedResult(int? timeout, bool expected)
        {
            // Arrange
            var request = new CreateSagaRequest { TimeoutSeconds = timeout };

            // Act
            bool actual = request.HasValidTimeout();

            // Assert
            actual.Should().Be(expected);
        }

        [Fact]
        public void HasValidTimeout_NullTimeout_ReturnsFalse()
        {
            // Arrange
            var request = new CreateSagaRequest { TimeoutSeconds = null };

            // Act
            bool actual = request.HasValidTimeout();

            // Assert
            actual.Should().BeFalse();
        }

        [Fact]
        public void GetMetadata_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            CreateSagaRequest request = null!;

            // Act
            Action act = () => request.GetMetadata();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GetMetadata_NoMetadata_ReturnsEmptyDictionary()
        {
            // Arrange
            var request = new CreateSagaRequest { Metadata = null };

            // Act
            var actual = request.GetMetadata();

            // Assert
            actual.Should().BeEmpty();
        }

        [Fact]
        public void GetMetadata_HasMetadata_ReturnsMetadata()
        {
            // Arrange
            var metadata = new Dictionary<string, object> { { "key", "value" } };
            var request = new CreateSagaRequest { Metadata = metadata };

            // Act
            var actual = request.GetMetadata();

            // Assert
            actual.Should().HaveCount(1).And.ContainKey("key").WhoseValue.Should().Be("value");
        }

        [Fact]
        public void TryParseDataAsDecimal_NullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            CreateSagaRequest request = null!;
            decimal result;

            // Act
            Action act = () => request.TryParseDataAsDecimal(out result);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("abc", false)]
        [InlineData("12.34", true)]
        public void TryParseDataAsDecimal_VariousData_ReturnsExpectedResult(string? data, bool expected)
        {
            // Arrange
            var request = new CreateSagaRequest { Data = data };

            // Act
            bool actual = request.TryParseDataAsDecimal(out decimal result);

            // Assert
            actual.Should().Be(expected);
            if (expected)
            {
                result.Should().Be(12.34m);
            }
        }
    }
}
