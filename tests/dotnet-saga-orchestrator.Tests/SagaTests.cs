using System;
using Xunit;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Tests
{
    public class SagaTests
    {
        [Fact]
        public void Constructor_SetsDefaultValues()
        {
            var saga = new Saga();
            
            Assert.NotNull(saga.Id);
            Assert.NotNull(saga.CorrelationId);
            Assert.Equal(SagaStatus.Pending, saga.Status);
            Assert.NotNull(saga.Definition);
            Assert.True(saga.StartedAt <= DateTime.UtcNow);
        }

        [Fact]
        public void Initialize_SetsValuesCorrectly()
        {
            var saga = new Saga();
            var definition = new SagaDefinition { Id = "def-1", Name = "TestSaga" };
            
            saga.Initialize(definition, 5, 60);
            
            Assert.Equal(definition, saga.Definition);
            Assert.Equal(5, saga.MaxRetries);
            Assert.Equal(60, saga.TimeoutSeconds);
            Assert.Equal(SagaStatus.Initialized, saga.Status);
        }

        [Fact]
        public void Initialize_ThrowsArgumentNullException_WhenDefinitionIsNull()
        {
            var saga = new Saga();
            Assert.Throws<ArgumentNullException>(() => saga.Initialize(null!));
        }

        [Fact]
        public void Start_ChangesStatusToRunning()
        {
            var saga = new Saga();
            saga.Initialize(new SagaDefinition());
            
            saga.Start();
            
            Assert.Equal(SagaStatus.Running, saga.Status);
        }

        [Fact]
        public void Start_ThrowsInvalidOperationException_IfStatusNotInitialized()
        {
            var saga = new Saga();
            // Default status is Pending, not Initialized
            Assert.Throws<InvalidOperationException>(() => saga.Start());
        }

        [Fact]
        public void Fail_SetsStatusToFailedAndSetsReason()
        {
            var saga = new Saga();
            var reason = "Something went wrong";
            
            saga.Fail(reason);
            
            Assert.Equal(SagaStatus.Failed, saga.Status);
            Assert.Equal(reason, saga.FailureReason);
            Assert.NotNull(saga.FailedAt);
        }

        [Fact]
        public void BeginCompensation_ThrowsInvalidOperationException_IfNotFailed()
        {
            var saga = new Saga();
            Assert.Throws<InvalidOperationException>(() => saga.BeginCompensation());
        }

        [Fact]
        public void IsTimedOut_ReturnsTrue_WhenTimeoutExceeded()
        {
            var saga = new Saga();
            saga.Initialize(new SagaDefinition(), 3, 1); // 1 second timeout
            saga.Start();
            
            // Simulate time passing
            System.Threading.Thread.Sleep(1100);
            
            Assert.True(saga.IsTimedOut());
        }
    }
}
