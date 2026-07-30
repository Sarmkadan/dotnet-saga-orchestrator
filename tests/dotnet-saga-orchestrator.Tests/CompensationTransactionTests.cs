using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Tests
{
    public class CompensationTransactionTests
    {
        [Fact]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Act
            var transaction = new CompensationTransaction();

            // Assert
            transaction.Id.Should().NotBeNullOrEmpty();
            transaction.Status.Should().Be(CompensationStatus.Pending);
            transaction.InitiatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            transaction.RetryCount.Should().Be(0);
            transaction.MaxRetries.Should().Be(3);
        }

        [Fact]
        public void Initialize_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            var sagaId = "saga-123";
            var stepId = "step-456";
            var stepName = "PaymentStep";
            var order = 1;
            var url = "http://compensation.url";

            // Act
            transaction.Initialize(sagaId, stepId, stepName, order, url);

            // Assert
            transaction.SagaId.Should().Be(sagaId);
            transaction.StepId.Should().Be(stepId);
            transaction.StepName.Should().Be(stepName);
            transaction.Order.Should().Be(order);
            transaction.CompensationUrl.Should().Be(url);
        }

        [Fact]
        public void Initialize_WithNullSagaId_ShouldThrowArgumentNullException()
        {
            // Arrange
            var transaction = new CompensationTransaction();

            // Act
            Action act = () => transaction.Initialize(null!, "stepId", "name", 1, "url");

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("sagaId");
        }

        [Fact]
        public void Start_ShouldChangeStatusToInProgress()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga", "step", "name", 1, "url");

            // Act
            transaction.Start();

            // Assert
            transaction.Status.Should().Be(CompensationStatus.InProgress);
        }

        [Fact]
        public void Start_WhenNotPending_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga", "step", "name", 1, "url");
            transaction.Start(); // Status is now InProgress

            // Act
            Action act = () => transaction.Start();

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*Cannot start compensation*");
        }

        [Fact]
        public void Complete_ShouldSetStatusAndResponsePayload()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga", "step", "name", 1, "url");
            transaction.Start();
            var payload = new Dictionary<string, object> { { "result", "success" } };

            // Act
            transaction.Complete(payload);

            // Assert
            transaction.Status.Should().Be(CompensationStatus.Completed);
            transaction.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
            transaction.ResponsePayload["result"].Should().Be("success");
        }

        [Fact]
        public void Fail_And_PrepareForRetry_ShouldUpdateStateCorrectly()
        {
            // Arrange
            var transaction = new CompensationTransaction();
            transaction.Initialize("saga", "step", "name", 1, "url");
            transaction.Start();
            transaction.Fail("Error occurred");

            // Act
            transaction.PrepareForRetry();

            // Assert
            transaction.Status.Should().Be(CompensationStatus.Pending);
            transaction.RetryCount.Should().Be(1);
        }
    }
}
