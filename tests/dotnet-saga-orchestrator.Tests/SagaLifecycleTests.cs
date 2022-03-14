#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using FluentAssertions;
using Xunit;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Contains unit tests for verifying the lifecycle behavior of sagas and their definitions.
/// Tests cover initialization, state transitions, compensation flow, and step management.
/// </summary>
public class SagaLifecycleTests
{
    /// <summary>
    /// Creates a valid saga definition for testing purposes.
    /// </summary>
    /// <param name="name">The name of the saga definition. Defaults to "TestSaga".</param>
    /// <returns>A new <see cref="SagaDefinition"/> instance with the specified name and description.</returns>
    private static SagaDefinition CreateValidDefinition(string name = "TestSaga") =>
        new SagaDefinition(name, $"{name} description");

    /// <summary>
    /// Creates a valid saga step definition for testing purposes.
    /// </summary>
    /// <param name="name">The name of the step. Defaults to "Step1".</param>
    /// <returns>A new <see cref="SagaStepDefinition"/> instance with the specified parameters.</returns>
    private static SagaStepDefinition CreateValidStep(string name = "Step1") =>
        new SagaStepDefinition(name, "my-service", "http://svc/action", "http://svc/compensate");

    /// <summary>
    /// Tests that initializing a saga with a valid definition sets the correct status and configuration.
    /// </summary>
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

    /// <summary>
    /// Tests that initializing a saga with a null definition throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void Initialize_WithNullDefinition_ThrowsArgumentNullException()
    {
        var saga = new Saga();

        var act = () => saga.Initialize(null!);

        act.Should().Throw<ArgumentNullException>().WithMessage("*definition*");
    }

    /// <summary>
    /// Tests that starting a saga transitions it from Initialized to Running state.
    /// </summary>
    [Fact]
    public void Start_WhenInitialized_TransitionsToRunning()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition());

        saga.Start();

        saga.Status.Should().Be(SagaStatus.Running);
    }

    /// <summary>
    /// Tests that starting a saga that is not initialized throws an InvalidOperationException.
    /// </summary>
    [Fact]
    public void Start_WhenNotInitialized_ThrowsInvalidOperationException()
    {
        var saga = new Saga(); // default status is Pending

        var act = () => saga.Start();

        act.Should().Throw<InvalidOperationException>().WithMessage("*Pending*");
    }

    /// <summary>
    /// Tests that failing a saga sets the Failed status and captures the failure reason.
    /// </summary>
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

    /// <summary>
    /// Tests that beginning compensation transitions a failed saga to the Compensating state.
    /// </summary>
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

    /// <summary>
    /// Tests that beginning compensation when the saga is not in Failed state throws an InvalidOperationException.
    /// </summary>
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

    /// <summary>
    /// Tests that CanRetry returns true when the retry count is below max retries.
    /// </summary>
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

    /// <summary>
    /// Tests that CanRetry returns false when the retry count equals max retries.
    /// </summary>
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

    /// <summary>
    /// Tests that CanRetry returns false when the saga status is not Failed.
    /// </summary>
    [Fact]
    public void CanRetry_WhenStatusIsNotFailed_ReturnsFalse()
    {
        var saga = new Saga();
        saga.Initialize(CreateValidDefinition(), maxRetries: 3);
        saga.Start(); // Running, not Failed

        saga.CanRetry().Should().BeFalse();
    }

    /// <summary>
    /// Tests that completing compensation sets the saga to Compensated status.
    /// </summary>
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

    /// <summary>
    /// Tests that adding steps to a saga definition assigns sequential order numbers.
    /// </summary>
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

    /// <summary>
    /// Tests that adding a null step to a saga definition throws an ArgumentNullException.
    /// </summary>
    [Fact]
    public void SagaDefinition_AddStep_WithNull_ThrowsArgumentNullException()
    {
        var definition = CreateValidDefinition();

        var act = () => definition.AddStep(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that GetStepByName returns the matching step when it exists.
    /// </summary>
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

    /// <summary>
    /// Tests that GetStepByName returns null when the step does not exist.
    /// </summary>
    [Fact]
    public void SagaDefinition_GetStepByName_WhenNotFound_ReturnsNull()
    {
        var definition = CreateValidDefinition();

        definition.GetStepByName("NonexistentStep").Should().BeNull();
    }

    /// <summary>
    /// Tests that GetStepByOrder returns the matching step by its order number.
    /// </summary>
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

    /// <summary>
    /// Tests that Clone returns a new instance with identical values but different reference.
    /// </summary>
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

    /// <summary>
    /// Tests that SetTimeout throws an ArgumentException when given a negative value.
    /// </summary>
    [Fact]
    public void SagaStepDefinition_SetTimeout_NegativeValue_ThrowsArgumentException()
    {
        var step = CreateValidStep();

        var act = () => step.SetTimeout(-1);

        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    /// <summary>
    /// Tests that SetRetryPolicy updates the MaxRetries and RetryDelayMilliseconds properties.
    /// </summary>
    [Fact]
    public void SagaStepDefinition_SetRetryPolicy_UpdatesMaxRetriesAndDelay()
    {
        var step = CreateValidStep();

        step.SetRetryPolicy(maxRetries: 5, delayMilliseconds: 2000);

        step.MaxRetries.Should().Be(5);
        step.RetryDelayMilliseconds.Should().Be(2000);
    }

    /// <summary>
    /// Tests that Validate returns false when a compensable step lacks a compensation URL.
    /// </summary>
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