// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

public class SagaLifecycleTests
{
    private static SagaDefinition CreateValidDefinition(string name = "TestSaga") =>
        new SagaDefinition(name, $"{name} description");

    private static SagaStepDefinition CreateValidStep(string name = "Step1") =>
        new SagaStepDefinition(name, "my-service", "http://svc/action", "http://svc/compensate");

    [Fact]
    public void Initialize_WithValidDefinition_SetsStatusToInitialized()
    {
        var saga = new Saga();
        var definition = CreateValidDefinition();

        saga.Initialize(definition, maxRetries: 5, timeoutSeconds: 120);

        saga.Status.Should().Be(SagaStatus.Initialized);
        saga.MaxRetries.Should().Be(5);
        saga.TimeoutSeconds.Should().Be(120);
        saga.Definition.Should().BeSameAs(definition);
    }

    [Fact]
    public void Initialize_WithNullDefinition_ThrowsArgumentNullException()
    {
        var saga = new Saga();

        var act = () => saga.Initialize(null!);

        act.Should().Throw<ArgumentNullException>().WithMessage("*definition*");
    }

    [Fact]
    public void Start_WhenInitialized_TransitionsToRunning()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition());

        saga.Start();

        saga.Status.Should().Be(SagaStatus.Running);
    }

    [Fact]
    public void Start_WhenNotInitialized_ThrowsInvalidOperationException()
    {
        var saga = new Saga(); // default status is Pending

        var act = () => saga.Start();

        act.Should().Throw<InvalidOperationException>().WithMessage("*Pending*");
    }

    [Fact]
    public void Fail_SetsFailedStatusAndCapturesReason()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition());
        saga.Start();

        saga.Fail("Payment service unavailable");

        saga.Status.Should().Be(SagaStatus.Failed);
        saga.FailureReason.Should().Be("Payment service unavailable");
        saga.FailedAt.Should().NotBeNull();
    }

    [Fact]
    public void BeginCompensation_WhenFailed_TransitionsToCompensating()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition());
        saga.Start();
        saga.Fail("Step failed");

        saga.BeginCompensation();

        saga.Status.Should().Be(SagaStatus.Compensating);
        saga.CompensationStartedAt.Should().NotBeNull();
    }

    [Fact]
    public void BeginCompensation_WhenNotFailed_ThrowsInvalidOperationException()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition());
        saga.Start(); // Running, not Failed

        var act = () => saga.BeginCompensation();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Can only compensate failed sagas");
    }

    [Fact]
    public void CanRetry_WhenBelowMaxRetries_ReturnsTrue()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition(), maxRetries: 3);
        saga.Start();
        saga.Fail("Transient error");
        saga.RetryCount = 2;

        saga.CanRetry().Should().BeTrue();
    }

    [Fact]
    public void CanRetry_WhenAtMaxRetries_ReturnsFalse()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition(), maxRetries: 3);
        saga.Start();
        saga.Fail("Persistent error");
        saga.RetryCount = 3;

        saga.CanRetry().Should().BeFalse();
    }

    [Fact]
    public void CanRetry_WhenStatusIsNotFailed_ReturnsFalse()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition(), maxRetries: 3);
        saga.Start(); // Running, not Failed

        saga.CanRetry().Should().BeFalse();
    }

    [Fact]
    public void CompleteCompensation_SetsCompensatedStatus()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition());
        saga.Start();
        saga.Fail("Error");
        saga.BeginCompensation();

        saga.CompleteCompensation();

        saga.Status.Should().Be(SagaStatus.Compensated);
        saga.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void SagaDefinition_AddStep_AssignsSequentialOrder()
    {
        var definition = CreateValidDefinition();
        var step1 = CreateValidStep("Step1");
        var step2 = CreateValidStep("Step2");

        definition.AddStep(step1);
        definition.AddStep(step2);

        step1.Order.Should().Be(1);
        step2.Order.Should().Be(2);
        definition.Steps.Should().HaveCount(2);
    }

    [Fact]
    public void SagaDefinition_AddStep_WithNull_ThrowsArgumentNullException()
    {
        var definition = CreateValidDefinition();

        var act = () => definition.AddStep(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SagaDefinition_GetStepByName_ReturnsMatchingStep()
    {
        var definition = CreateValidDefinition();
        var step = new SagaStepDefinition("PaymentStep", "payment-svc", "http://pay/action", "http://pay/compensate");
        definition.AddStep(step);

        var found = definition.GetStepByName("PaymentStep");

        found.Should().NotBeNull();
        found!.ServiceName.Should().Be("payment-svc");
    }

    [Fact]
    public void SagaDefinition_GetStepByName_WhenNotFound_ReturnsNull()
    {
        var definition = CreateValidDefinition();

        definition.GetStepByName("NonexistentStep").Should().BeNull();
    }

    [Fact]
    public void SagaDefinition_GetStepByOrder_ReturnsMatchingStep()
    {
        var definition = CreateValidDefinition();
        definition.AddStep(CreateValidStep("First"));
        definition.AddStep(CreateValidStep("Second"));

        var found = definition.GetStepByOrder(2);

        found.Should().NotBeNull();
        found!.Name.Should().Be("Second");
    }

    [Fact]
    public void SagaStepDefinition_Clone_ReturnsNewInstanceWithIdenticalValues()
    {
        var step = new SagaStepDefinition("OriginalStep", "my-service", "http://svc/action", "http://svc/compensate");
        step.TimeoutSeconds = 60;
        step.MaxRetries = 5;

        var clone = step.Clone();

        clone.Should().NotBeSameAs(step);
        clone.Name.Should().Be(step.Name);
        clone.ServiceName.Should().Be(step.ServiceName);
        clone.ServiceUrl.Should().Be(step.ServiceUrl);
        clone.TimeoutSeconds.Should().Be(step.TimeoutSeconds);
        clone.MaxRetries.Should().Be(step.MaxRetries);
    }

    [Fact]
    public void SagaStepDefinition_SetTimeout_NegativeValue_ThrowsArgumentException()
    {
        var step = CreateValidStep();

        var act = () => step.SetTimeout(-1);

        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    [Fact]
    public void SagaStepDefinition_SetRetryPolicy_UpdatesMaxRetriesAndDelay()
    {
        var step = CreateValidStep();

        step.SetRetryPolicy(maxRetries: 5, delayMilliseconds: 2000);

        step.MaxRetries.Should().Be(5);
        step.RetryDelayMilliseconds.Should().Be(2000);
    }

    [Fact]
    public void SagaStepDefinition_Validate_WhenCompensableWithoutUrl_ReturnsFalse()
    {
        var step = new SagaStepDefinition
        {
            Name = "Step",
            ServiceName = "svc",
            ServiceUrl = "http://svc/action",
            IsCompensable = true,
            CompensationUrl = string.Empty, // missing
            TimeoutSeconds = 30,
            MaxRetries = 3
        };

        step.Validate().Should().BeFalse();
    }
}
