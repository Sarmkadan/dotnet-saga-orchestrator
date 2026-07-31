using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Tests
{
    public class SagaResponseExtensionsTests
    {
        [Fact]
        public void StatusMethods_ReturnsExpectedBoolean()
        {
            var now = DateTime.UtcNow;
            var response = new SagaResponse
            {
                Status = "Completed",
                StartedAt = now.AddMinutes(-5),
                CompletedAt = now,
                CompletedSteps = 2,
                StepCount = 2,
                FailedSteps = 0
            };

            response.IsCompletedSuccessfully().Should().BeTrue();
            response.IsInProgress().Should().BeFalse();
            response.IsFailed().Should().BeFalse();

            response.Status = "InProgress";
            response.CompletedAt = null;
            response.IsInProgress().Should().BeTrue();
            response.IsCompletedSuccessfully().Should().BeFalse();
            
            response.Status = "Failed";
            response.IsFailed().Should().BeTrue();
        }

        [Fact]
        public void GetDurationMilliseconds_ReturnsCorrectValue()
        {
            var startedAt = new DateTime(2023, 1, 1, 12, 0, 0);
            var completedAt = new DateTime(2023, 1, 1, 12, 0, 5); // 5 seconds = 5000ms
            var response = new SagaResponse { StartedAt = startedAt, CompletedAt = completedAt };

            response.GetDurationMilliseconds().Should().Be(5000);
            response.CompletedAt = null;
            response.GetDurationMilliseconds().Should().BeNull();
        }

        [Fact]
        public void GetCompletionPercentage_ReturnsCorrectPercentage()
        {
            var response = new SagaResponse { CompletedSteps = 1, StepCount = 2 };
            response.GetCompletionPercentage().Should().Be(50);

            response.StepCount = 0;
            response.GetCompletionPercentage().Should().Be(0);
        }

        [Fact]
        public void StepFilteringMethods_ReturnsCorrectSteps()
        {
            var response = new SagaResponse
            {
                Steps = new List<SagaStepResponse>
                {
                    new SagaStepResponse { Status = "Completed" },
                    new SagaStepResponse { Status = "Failed" },
                    new SagaStepResponse { Status = "InProgress" },
                    new SagaStepResponse { Status = "Pending" }
                }
            };

            response.GetCompletedSteps().Should().HaveCount(1);
            response.GetFailedSteps().Should().HaveCount(1);
            response.GetInProgressSteps().Should().HaveCount(1);
            response.GetPendingSteps().Should().HaveCount(1);
        }

        [Fact]
        public void GetAverageStepDurationMilliseconds_ReturnsCorrectAverage()
        {
            var startedAt = new DateTime(2023, 1, 1, 12, 0, 0);
            var completedAt = new DateTime(2023, 1, 1, 12, 0, 10);
            var step1StartedAt = new DateTime(2023, 1, 1, 12, 0, 0);
            var step1CompletedAt = new DateTime(2023, 1, 1, 12, 0, 4); // 4s
            var step2StartedAt = new DateTime(2023, 1, 1, 12, 0, 4);
            var step2CompletedAt = new DateTime(2023, 1, 1, 12, 0, 10); // 6s

            var response = new SagaResponse
            {
                StartedAt = startedAt,
                CompletedAt = completedAt,
                CompletedSteps = 2,
                Steps = new List<SagaStepResponse>
                {
                    new SagaStepResponse { Status = "Completed", StartedAt = step1StartedAt, CompletedAt = step1CompletedAt },
                    new SagaStepResponse { Status = "Completed", StartedAt = step2StartedAt, CompletedAt = step2CompletedAt }
                }
            };

            // Average of 4s and 6s = 5s = 5000ms
            response.GetAverageStepDurationMilliseconds().Should().Be(5000);
        }

        [Fact]
        public void ExtensionMethods_ThrowArgumentNullException_WhenInputIsNull()
        {
            SagaResponse? nullResponse = null;

            Action act = () => nullResponse!.IsCompletedSuccessfully();
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
