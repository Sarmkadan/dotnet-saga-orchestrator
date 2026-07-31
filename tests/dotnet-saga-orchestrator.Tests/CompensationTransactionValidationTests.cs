using System;
using System.Collections.Generic;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using Xunit;

namespace SagaOrchestrator.Tests
{
    public class CompensationTransactionValidationTests
    {
        private static CompensationTransaction CreateValidTransaction()
        {
            return new CompensationTransaction
            {
                Id = Guid.NewGuid().ToString(),
                SagaId = Guid.NewGuid().ToString(),
                StepId = Guid.NewGuid().ToString(),
                StepName = "SampleStep",
                Order = 1,
                Status = CompensationStatus.Pending,
                CompensationUrl = "https://example.com/compensate",
                RequestPayload = new Dictionary<string, object> { ["key"] = "value" },
                ResponsePayload = new Dictionary<string, object> { ["result"] = "ok" },
                InitiatedAt = DateTime.UtcNow,
                CompletedAt = null,
                FailedAt = null,
                ErrorMessage = null,
                RetryCount = 0,
                MaxRetries = 3,
                TimeoutSeconds = 30
            };
        }

        [Fact]
        public void Validate_HappyPath_ReturnsEmptyList()
        {
            // Arrange
            var tx = CreateValidTransaction();

            // Act
            var errors = tx.Validate();

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void IsValid_HappyPath_ReturnsTrue()
        {
            // Arrange
            var tx = CreateValidTransaction();

            // Act
            var result = tx.IsValid();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void EnsureValid_HappyPath_DoesNotThrow()
        {
            // Arrange
            var tx = CreateValidTransaction();

            // Act
            Action act = () => tx.EnsureValid();

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void Validate_NullArgument_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => CompensationTransactionValidation.Validate(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsValid_NullArgument_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => CompensationTransactionValidation.IsValid(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureValid_InvalidTransaction_ThrowsArgumentException_WithErrors()
        {
            // Arrange: create a transaction with several validation failures
            var tx = new CompensationTransaction
            {
                Id = "not-a-guid",
                SagaId = "",
                StepId = null!,
                StepName = "   ",
                Order = -1,
                Status = (CompensationStatus)999, // invalid enum value
                CompensationUrl = "not-a-url",
                RequestPayload = null!,
                ResponsePayload = null!,
                InitiatedAt = default,
                CompletedAt = DateTime.UtcNow, // will be earlier than InitiatedAt (default)
                FailedAt = DateTime.UtcNow,
                ErrorMessage = "   ",
                RetryCount = 5,
                MaxRetries = 3,
                TimeoutSeconds = 0
            };

            // Act
            Action act = () => tx.EnsureValid();

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("*CompensationTransaction is invalid*")
               .Where(ex => ex.Message.Contains("CompensationTransaction.Id must be a valid GUID format.") &&
                            ex.Message.Contains("CompensationTransaction.SagaId must not be null or whitespace.") &&
                            ex.Message.Contains("CompensationTransaction.StepId must not be null or whitespace.") &&
                            ex.Message.Contains("CompensationTransaction.StepName must not be null or whitespace.") &&
                            ex.Message.Contains("CompensationTransaction.Order must be a non-negative integer.") &&
                            ex.Message.Contains("CompensationTransaction.Status must be a valid CompensationStatus value.") &&
                            ex.Message.Contains("CompensationTransaction.CompensationUrl must be a valid absolute URI.") &&
                            ex.Message.Contains("CompensationTransaction.RequestPayload must not be null.") &&
                            ex.Message.Contains("CompensationTransaction.ResponsePayload must not be null.") &&
                            ex.Message.Contains("CompensationTransaction.InitiatedAt must be set to a valid DateTime.") &&
                            ex.Message.Contains("CompensationTransaction.CompletedAt must be a valid DateTime if set.") &&
                            ex.Message.Contains("CompensationTransaction.FailedAt must be a valid DateTime if set.") &&
                            ex.Message.Contains("CompensationTransaction.ErrorMessage must not be whitespace if set.") &&
                            ex.Message.Contains("CompensationTransaction.RetryCount must not exceed MaxRetries.") &&
                            ex.Message.Contains("CompensationTransaction.MaxRetries must be a positive integer.") &&
                            ex.Message.Contains("CompensationTransaction.TimeoutSeconds must be a positive integer."));
        }
    }
}
