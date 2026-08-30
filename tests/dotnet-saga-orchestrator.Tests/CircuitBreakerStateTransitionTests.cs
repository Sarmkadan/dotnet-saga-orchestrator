using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using SagaOrchestrator.Infrastructure.Resilience;

namespace SagaOrchestrator.Tests
{
    public class CircuitBreakerStateTransitionTests
    {
        [Fact]
        public async Task ExecuteAsync_SuccessfulAction_KeepsBreakerClosed()
        {
            // Arrange
            var breaker = new CircuitBreaker();

            // Act
            var result = await breaker.ExecuteAsync(() => Task.CompletedTask, "service-a");

            // Assert
            result.Should().BeTrue();
            breaker.GetState("service-a").Should().Be(CircuitBreakerState.Closed);
        }

        [Fact]
        public async Task ExecuteAsync_FailureThresholdReached_OpensBreakerAndRejectsFurtherAction()
        {
            // Arrange
            var breaker = new CircuitBreaker(failureThreshold: 2, timeoutSeconds: 60);
            var invocationCount = 0;
            Func<Task> failingAction = () => throw new InvalidOperationException("failure");

            // Act
            for (var attempt = 0; attempt < 2; attempt++)
            {
                Func<Task> act = async () => await breaker.ExecuteAsync(failingAction, "service-a");
                await act.Should().ThrowAsync<InvalidOperationException>();
            }

            var result = await breaker.ExecuteAsync(
                () =>
                {
                    invocationCount++;
                    return Task.CompletedTask;
                },
                "service-a");

            // Assert
            breaker.GetState("service-a").Should().Be(CircuitBreakerState.Open);
            result.Should().BeFalse();
            invocationCount.Should().Be(0);
        }

        [Fact]
        public async Task ExecuteAsync_GenericActionWhileOpen_ThrowsInvalidOperationException()
        {
            // Arrange
            var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 60);
            Func<Task> failingAction = () => throw new InvalidOperationException("failure");
            Func<Task> tripBreaker = async () => await breaker.ExecuteAsync(failingAction, "service-a");
            await tripBreaker.Should().ThrowAsync<InvalidOperationException>();

            // Act
            Func<Task> act = async () => await breaker.ExecuteAsync(() => Task.FromResult(42), "service-a");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Circuit breaker is open*");
        }

        [Fact]
        public async Task ExecuteAsync_SuccessfulHalfOpenProbe_ClosesBreaker()
        {
            // Arrange
            var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 0);
            Func<Task> failingAction = () => throw new InvalidOperationException("failure");
            Func<Task> tripBreaker = async () => await breaker.ExecuteAsync(failingAction, "service-a");
            await tripBreaker.Should().ThrowAsync<InvalidOperationException>();
            breaker.GetState("service-a").Should().Be(CircuitBreakerState.HalfOpen);

            // Act
            var result = await breaker.ExecuteAsync(() => Task.CompletedTask, "service-a");

            // Assert
            result.Should().BeTrue();
            breaker.GetState("service-a").Should().Be(CircuitBreakerState.Closed);
        }

        [Fact]
        public async Task ExecuteAsync_FailedHalfOpenProbe_ReopensBreaker()
        {
            // Arrange
            var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 1);
            Func<Task> failingAction = () => throw new InvalidOperationException("failure");
            Func<Task> tripBreaker = async () => await breaker.ExecuteAsync(failingAction, "service-a");
            await tripBreaker.Should().ThrowAsync<InvalidOperationException>();
            await Task.Delay(TimeSpan.FromMilliseconds(1100));
            breaker.GetState("service-a").Should().Be(CircuitBreakerState.HalfOpen);

            // Act
            Func<Task> probe = async () => await breaker.ExecuteAsync(failingAction, "service-a");

            // Assert
            await probe.Should().ThrowAsync<InvalidOperationException>();
            breaker.GetState("service-a").Should().Be(CircuitBreakerState.Open);
        }

        [Fact]
        public async Task Reset_OpenBreaker_ReturnsBreakerToClosed()
        {
            // Arrange
            var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 60);
            Func<Task> failingAction = () => throw new InvalidOperationException("failure");
            Func<Task> tripBreaker = async () => await breaker.ExecuteAsync(failingAction, "service-a");
            await tripBreaker.Should().ThrowAsync<InvalidOperationException>();
            breaker.GetState("service-a").Should().Be(CircuitBreakerState.Open);

            // Act
            breaker.Reset("service-a");

            // Assert
            breaker.GetState("service-a").Should().Be(CircuitBreakerState.Closed);
        }

        [Fact]
        public async Task ExecuteAsync_DifferentIdentifiers_MaintainIndependentState()
        {
            // Arrange
            var breaker = new CircuitBreaker(failureThreshold: 1, timeoutSeconds: 60);
            Func<Task> failingAction = () => throw new InvalidOperationException("failure");

            // Act
            Func<Task> tripFirstBreaker = async () => await breaker.ExecuteAsync(failingAction, "service-a");
            await tripFirstBreaker.Should().ThrowAsync<InvalidOperationException>();
            var secondResult = await breaker.ExecuteAsync(() => Task.CompletedTask, "service-b");

            // Assert
            breaker.GetState("service-a").Should().Be(CircuitBreakerState.Open);
            breaker.GetState("service-b").Should().Be(CircuitBreakerState.Closed);
            secondResult.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteAsync_NullAction_ThrowsArgumentNullException()
        {
            // Arrange
            var breaker = new CircuitBreaker();

            // Act
            Func<Task> act = async () => await breaker.ExecuteAsync((Func<Task>)null!, "service-a");

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ExecuteAsync_NullOrEmptyIdentifier_ThrowsArgumentException(string? identifier)
        {
            // Arrange
            var breaker = new CircuitBreaker();

            // Act
            Func<Task> act = async () => await breaker.ExecuteAsync(() => Task.CompletedTask, identifier!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_NonPositiveFailureThreshold_ThrowsArgumentOutOfRangeException(int failureThreshold)
        {
            // Act
            Action act = () => new CircuitBreaker(failureThreshold);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
