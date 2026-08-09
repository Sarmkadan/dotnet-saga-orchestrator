using System;
using System.Collections.Generic;
using Xunit;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Tests
{
    public class CompensationTransactionExtensionsTests
    {
        private CompensationTransaction CreateValidTransaction()
        {
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga-123", "step-456", "test-step", 1, "http://example.com/compensate");
            return transaction;
        }

        [Fact]
        public void IsActive_ReturnsCorrectValues_ForAllStatuses()
        {
            // Arrange
            var pending = CreateValidTransaction();
            var inProgress = CreateValidTransaction();
            inProgress.Start();
            var completed = CreateValidTransaction();
            completed.Complete();
            var failed = CreateValidTransaction();
            failed.Fail("Test error");

            // Act & Assert
            Assert.True(pending.IsActive());
            Assert.True(inProgress.IsActive());
            Assert.False(completed.IsActive());
            Assert.False(failed.IsActive());
        }

        [Fact]
        public void IsCompletedSuccessfully_ReturnsCorrectValues()
        {
            // Arrange
            var notCompleted = CreateValidTransaction();
            var completed = CreateValidTransaction();
            completed.Complete();

            // Act & Assert
            Assert.False(notCompleted.IsCompletedSuccessfully());
            Assert.True(completed.IsCompletedSuccessfully());
        }

        [Fact]
        public void IsFailed_ReturnsCorrectValues()
        {
            // Arrange
            var notFailed = CreateValidTransaction();
            var failed = CreateValidTransaction();
            failed.Fail("Test error");

            // Act & Assert
            Assert.False(notFailed.IsFailed());
            Assert.True(failed.IsFailed());
        }

        [Fact]
        public void GetDurationMs_ReturnsValue_WhenCompletedOrFailed()
        {
            // Arrange
            var startTime = DateTime.UtcNow.AddSeconds(-10);
            var completed = CreateValidTransaction();
            completed.InitiatedAt = startTime;
            completed.Complete();

            var failed = CreateValidTransaction();
            failed.InitiatedAt = startTime;
            failed.Fail("Test error");

            // Act
            var completedDuration = completed.GetDurationMs();
            var failedDuration = failed.GetDurationMs();

            // Assert
            Assert.NotNull(completedDuration);
            Assert.NotNull(failedDuration);
            Assert.InRange(completedDuration.Value, 9000, 11000);
            Assert.InRange(failedDuration.Value, 9000, 11000);
        }

        [Fact]
        public void GetDurationMs_ReturnsNull_WhenNotCompletedOrFailed()
        {
            // Arrange
            var pending = CreateValidTransaction();
            var inProgress = CreateValidTransaction();
            inProgress.Start();

            // Act
            var pendingResult = pending.GetDurationMs();
            var inProgressResult = inProgress.GetDurationMs();

            // Assert
            Assert.Null(pendingResult);
            Assert.Null(inProgressResult);
        }

        [Fact]
        public void GetElapsedTimeMs_ReturnsCorrectValues()
        {
            // Arrange
            var startTime = DateTime.UtcNow.AddSeconds(-10);
            var pending = CreateValidTransaction();
            pending.InitiatedAt = startTime;

            var inProgress = CreateValidTransaction();
            inProgress.InitiatedAt = startTime;
            inProgress.Start();

            var completed = CreateValidTransaction();
            completed.InitiatedAt = startTime;
            completed.Complete();

            // Act
            var pendingResult = pending.GetElapsedTimeMs();
            var inProgressResult = inProgress.GetElapsedTimeMs();
            var completedResult = completed.GetElapsedTimeMs();

            // Assert
            Assert.Null(pendingResult);
            Assert.NotNull(inProgressResult);
            Assert.NotNull(completedResult);
            Assert.InRange(inProgressResult.Value, 9000, 11000);
            Assert.InRange(completedResult.Value, 9000, 11000);
        }

        [Fact]
        public void DeepCopy_CreatesIndependentCopy()
        {
            // Arrange
            var transaction = CreateValidTransaction();
            transaction.RequestPayload.Add("key", "value");
            transaction.ResponsePayload.Add("num", 42);
            transaction.Start();

            // Act
            var copy = transaction.DeepCopy();

            // Assert
            Assert.NotSame(transaction, copy);
            Assert.Equal(transaction.Id, copy.Id);
            Assert.Equal(transaction.RequestPayload["key"], copy.RequestPayload["key"]);
            Assert.Equal(transaction.ResponsePayload["num"], copy.ResponsePayload["num"]);
            Assert.Equal(transaction.Status, copy.Status);

            // Modify original to verify independence
            transaction.RequestPayload["key"] = "modified";
            Assert.Equal("value", copy.RequestPayload["key"]);
            Assert.Equal("modified", transaction.RequestPayload["key"]);
        }

        [Fact]
        public void UpdateRequestPayload_WorksCorrectly()
        {
            // Arrange
            var transaction = CreateValidTransaction();
            var updates = new Dictionary<string, object> { ["newKey"] = "newValue" };

            // Act
            transaction.UpdateRequestPayload(updates);

            // Assert
            Assert.Equal("newValue", transaction.RequestPayload["newKey"]);
        }

        [Fact]
        public void HasExceededMaxRetries_ReturnsCorrectValues()
        {
            // Arrange
            var withinLimit = CreateValidTransaction();
            withinLimit.MaxRetries = 3;
            withinLimit.RetryCount = 2;

            var atLimit = CreateValidTransaction();
            atLimit.MaxRetries = 3;
            atLimit.RetryCount = 3;

            // Act
            var withinResult = withinLimit.HasExceededMaxRetries();
            var atLimitResult = atLimit.HasExceededMaxRetries();

            // Assert
            Assert.False(withinResult);
            Assert.True(atLimitResult);
        }

        [Fact]
        public void GetSummary_ReturnsFormattedString()
        {
            // Arrange
            var transaction = CreateValidTransaction();
            transaction.StepName = "test-step";

            // Act
            var result = transaction.GetSummary();

            // Assert
            Assert.Contains($"Id={transaction.Id}", result);
            Assert.Contains("SagaId=saga-123", result);
            Assert.Contains("Step=test-step", result);
            Assert.Contains($"Status={CompensationStatus.Pending}", result);
            Assert.Contains("Order=1", result);
        }

        [Fact]
        public void CanSafelyRetry_ReturnsCorrectValues()
        {
            // Arrange
            var notFailed = CreateValidTransaction();
            notFailed.MaxRetries = 3;
            notFailed.RetryCount = 1;

            var canRetry = CreateValidTransaction();
            canRetry.MaxRetries = 3;
            canRetry.RetryCount = 1;
            canRetry.Fail("Test error");

            var cannotRetry = CreateValidTransaction();
            cannotRetry.MaxRetries = 2;
            cannotRetry.RetryCount = 2;
            cannotRetry.Fail("Test error");

            // Act
            var notFailedResult = notFailed.CanSafelyRetry();
            var canRetryResult = canRetry.CanSafelyRetry();
            var cannotRetryResult = cannotRetry.CanSafelyRetry();

            // Assert
            Assert.False(notFailedResult);
            Assert.True(canRetryResult);
            Assert.False(cannotRetryResult);
        }

        [Fact]
        public void Methods_ThrowArgumentNullException_WhenTransactionIsNull()
        {
            // Arrange
            CompensationTransaction transaction = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => transaction.IsActive());
            Assert.Throws<ArgumentNullException>(() => transaction.IsCompletedSuccessfully());
            Assert.Throws<ArgumentNullException>(() => transaction.IsFailed());
            Assert.Throws<ArgumentNullException>(() => transaction.GetDurationMs());
            Assert.Throws<ArgumentNullException>(() => transaction.GetElapsedTimeMs());
            Assert.Throws<ArgumentNullException>(() => transaction.DeepCopy());
            Assert.Throws<ArgumentNullException>(() => transaction.GetSummary());
            Assert.Throws<ArgumentNullException>(() => transaction.CanSafelyRetry());
            Assert.Throws<ArgumentNullException>(() => transaction.HasExceededMaxRetries());
        }

        [Fact]
        public void UpdateRequestPayload_ThrowsArgumentNullException_WhenPayloadUpdatesIsNull()
        {
            // Arrange
            var transaction = CreateValidTransaction();
            Dictionary<string, object> payloadUpdates = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => transaction.UpdateRequestPayload(payloadUpdates));
        }
    }
}