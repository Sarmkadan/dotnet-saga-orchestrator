using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Tests
{
    public class SagaStepTests
    {
        [Fact]
        public void Initialize_ShouldSetPropertiesCorrectly()
        {
            var step = new SagaStep();
            step.Initialize("Step1", 1, "url1", "compUrl1");

            step.Name.Should().Be("Step1");
            step.Order.Should().Be(1);
            step.ServiceUrl.Should().Be("url1");
            step.CompensationUrl.Should().Be("compUrl1");
        }

        [Fact]
        public void Start_ShouldChangeStatusToExecuting()
        {
            var step = new SagaStep();
            step.Initialize("Step1", 1, "url1", "compUrl1");
            step.Start();

            step.Status.Should().Be(SagaStepStatus.Executing);
            step.StartedAt.Should().NotBeNull();
        }

        [Fact]
        public void Complete_ShouldChangeStatusToCompleted()
        {
            var step = new SagaStep();
            step.Initialize("Step1", 1, "url1", "compUrl1");
            step.Start();
            step.Complete();

            step.Status.Should().Be(SagaStepStatus.Completed);
            step.CompletedAt.Should().NotBeNull();
        }

        [Fact]
        public void Fail_ShouldChangeStatusToFailed()
        {
            var step = new SagaStep();
            step.Initialize("Step1", 1, "url1", "compUrl1");
            step.Start();
            step.Fail("Error message");

            step.Status.Should().Be(SagaStepStatus.Failed);
            step.ErrorMessage.Should().Be("Error message");
        }

        [Fact]
        public void CanRetry_ShouldReturnTrue_WhenFailedAndRetriesAvailable()
        {
            var step = new SagaStep();
            step.Initialize("Step1", 1, "url1", "compUrl1");
            step.Start();
            step.Fail("Error");
            step.MaxRetries = 3;

            step.CanRetry().Should().BeTrue();
        }

        [Fact]
        public void PrepareForRetry_ShouldIncreaseRetryCount()
        {
            var step = new SagaStep();
            step.Initialize("Step1", 1, "url1", "compUrl1");
            step.Start();
            step.Fail("Error");
            step.PrepareForRetry();

            step.RetryCount.Should().Be(1);
            step.Status.Should().Be(SagaStepStatus.WaitingForRetry);
        }

        [Fact]
        public void Compensate_ShouldChangeStatusToCompensated()
        {
            var step = new SagaStep();
            step.Initialize("Step1", 1, "url1", "compUrl1");
            step.Start();
            step.Complete();
            step.Compensate();

            step.Status.Should().Be(SagaStepStatus.Compensated);
            step.CompensatedAt.Should().NotBeNull();
        }
    }
}
