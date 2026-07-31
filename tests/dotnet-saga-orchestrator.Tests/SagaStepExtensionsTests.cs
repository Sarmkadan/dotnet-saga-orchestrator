using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests
{
    public class SagaStepExtensionsTests
    {
        [Fact]
        public void IsTerminal_WhenStatusIsCompleted_ReturnsTrue()
        {
            var step = new SagaStep { Status = SagaStepStatus.Completed };
            step.IsTerminal().Should().BeTrue();
        }

        [Fact]
        public void IsTerminal_WhenStatusIsPending_ReturnsFalse()
        {
            var step = new SagaStep { Status = SagaStepStatus.Pending };
            step.IsTerminal().Should().BeFalse();
        }

        [Fact]
        public void IsRetryable_WhenFailedAndCanRetry_ReturnsTrue()
        {
            var step = new SagaStep { Status = SagaStepStatus.Failed, MaxRetries = 1, RetryCount = 0 };
            step.IsRetryable().Should().BeTrue();
        }

        [Fact]
        public void GetExecutionDurationMs_WithValidTiming_ReturnsExpectedDuration()
        {
            var startedAt = DateTime.UtcNow.AddMinutes(-5);
            var completedAt = DateTime.UtcNow;
            var step = new SagaStep { StartedAt = startedAt, CompletedAt = completedAt };
            
            var expected = (long)(completedAt - startedAt).TotalMilliseconds;
            step.GetExecutionDurationMs().Should().Be(expected);
        }

        [Fact]
        public void Clone_CreatesDeepCopy()
        {
            var step = new SagaStep 
            { 
                Name = "TestStep",
                Payload = new Dictionary<string, object> { { "key", "value" } }
            };
            
            var clone = step.Clone();
            
            clone.Should().NotBeSameAs(step);
            clone.Id.Should().NotBe(step.Id);
            clone.Name.Should().Be(step.Name);
            clone.Payload.Should().NotBeSameAs(step.Payload);
            clone.Payload["key"].Should().Be("value");
        }

        [Fact]
        public void UpdatePayload_MergesDataCorrectly()
        {
            var step = new SagaStep { Payload = new Dictionary<string, object> { { "key1", "old" } } };
            var newData = new Dictionary<string, object> { { "key2", "new" } };
            
            step.UpdatePayload(newData);
            
            step.Payload.Should().ContainKey("key1").WhoseValue.Should().Be("old");
            step.Payload.Should().ContainKey("key2").WhoseValue.Should().Be("new");
        }

        [Fact]
        public void IsTerminal_NullStep_ThrowsArgumentNullException()
        {
            SagaStep step = null!;
            Action act = () => step.IsTerminal();
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
