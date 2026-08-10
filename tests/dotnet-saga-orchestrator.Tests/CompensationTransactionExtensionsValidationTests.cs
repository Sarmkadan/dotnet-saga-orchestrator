using System;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Tests
{
    public class CompensationTransactionExtensionsValidationTests
    {
        [Fact]
        public void ValidateExtensionMethods_NullTransaction_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => CompensationTransactionExtensionsValidation.ValidateExtensionMethods(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ValidateExtensionMethods_ValidTransaction_ReturnsNoErrors()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga1", "step1", "Step 1", 1, "http://example.com");
            transaction.Status = CompensationStatus.Completed;
            transaction.InitiatedAt = DateTime.UtcNow.AddSeconds(-10); // Started 10 seconds ago
            transaction.CompletedAt = DateTime.UtcNow.AddSeconds(-5); // Completed 5 seconds ago

            // Act
            var errors = CompensationTransactionExtensionsValidation.ValidateExtensionMethods(transaction);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateExtensionMethods_TransactionWithNegativeElapsedTime_ReturnsErrorForGetElapsedTimeMs()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga1", "step1", "Step 1", 1, "http://example.com");
            transaction.Status = CompensationStatus.InProgress;
            transaction.InitiatedAt = DateTime.UtcNow.AddSeconds(10); // Future initiation

            // Act
            var errors = CompensationTransactionExtensionsValidation.ValidateExtensionMethods(transaction);

            // Assert
            errors.Should().ContainSingle()
                .Which.Should().Contain(nameof(CompensationTransactionExtensions.GetElapsedTimeMs));
        }

        [Fact]
        public void ValidateExtensionMethods_TransactionWithNegativeDuration_ReturnsErrorForGetDurationMs()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga1", "step1", "Step 1", 1, "http://example.com");
            transaction.Status = CompensationStatus.Completed;
            transaction.InitiatedAt = DateTime.UtcNow.AddSeconds(-5); // Started 5 seconds ago
            transaction.CompletedAt = DateTime.UtcNow.AddSeconds(-10); // Completed 10 seconds ago (before initiated)

            // Act
            var errors = CompensationTransactionExtensionsValidation.ValidateExtensionMethods(transaction);

            // Assert
            errors.Should().ContainSingle()
                .Which.Should().Contain(nameof(CompensationTransactionExtensions.GetDurationMs));
        }

        [Fact]
        public void IsValidExtensionMethods_NullTransaction_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => CompensationTransactionExtensionsValidation.IsValidExtensionMethods(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void IsValidExtensionMethods_ValidTransaction_ReturnsTrue()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga1", "step1", "Step 1", 1, "http://example.com");
            transaction.Status = CompensationStatus.Completed;
            transaction.InitiatedAt = DateTime.UtcNow.AddSeconds(-10); // Started 10 seconds ago
            transaction.CompletedAt = DateTime.UtcNow.AddSeconds(-5); // Completed 5 seconds ago

            // Act
            var isValid = CompensationTransactionExtensionsValidation.IsValidExtensionMethods(transaction);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void IsValidExtensionMethods_InvalidTransaction_ReturnsFalse()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga1", "step1", "Step 1", 1, "http://example.com");
            transaction.Status = CompensationStatus.InProgress;
            transaction.InitiatedAt = DateTime.UtcNow.AddSeconds(10); // Future initiation

            // Act
            var isValid = CompensationTransactionExtensionsValidation.IsValidExtensionMethods(transaction);

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void EnsureValidExtensionMethods_NullTransaction_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => CompensationTransactionExtensionsValidation.EnsureValidExtensionMethods(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void EnsureValidExtensionMethods_ValidTransaction_DoesNotThrow()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga1", "step1", "Step 1", 1, "http://example.com");
            transaction.Status = CompensationStatus.Completed;
            transaction.InitiatedAt = DateTime.UtcNow.AddSeconds(-10); // Started 10 seconds ago
            transaction.CompletedAt = DateTime.UtcNow.AddSeconds(-5); // Completed 5 seconds ago

            // Act
            Action act = () => CompensationTransactionExtensionsValidation.EnsureValidExtensionMethods(transaction);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void EnsureValidExtensionMethods_InvalidTransaction_ThrowsArgumentExceptionWithErrors()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga1", "step1", "Step 1", 1, "http://example.com");
            transaction.Status = CompensationStatus.InProgress;
            transaction.InitiatedAt = DateTime.UtcNow.AddSeconds(10); // Future initiation

            // Act
            Action act = () => CompensationTransactionExtensionsValidation.EnsureValidExtensionMethods(transaction);

            // Assert
            act.Should().Throw<ArgumentException>()
                .Which.Message.Should().Contain(nameof(CompensationTransactionExtensions.GetElapsedTimeMs));
        }
    }
}