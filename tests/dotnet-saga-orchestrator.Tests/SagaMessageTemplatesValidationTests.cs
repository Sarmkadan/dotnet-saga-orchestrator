using Xunit;
using FluentAssertions;
using SagaOrchestrator.Infrastructure.Messaging;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Tests for SagaMessageTemplatesValidation class.
/// </summary>
public class SagaMessageTemplatesValidationTests
{
    private readonly SagaMessageTemplatesValidation _validator = new();

    #region ValidateSagaCreated Tests

    [Fact]
    public void ValidateSagaCreated_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var sagaId = "saga_123";
        var sagaName = "OrderProcessing";
        var definitionId = "def_456";
        var stepCount = 5;

        // Act
        var result = _validator.ValidateSagaCreated(sagaId, sagaName, definitionId, stepCount);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSagaCreated_ShouldThrowArgumentNullException_WhenSagaIdIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaCreated(null!, "ValidName", "ValidDef", 1);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void ValidateSagaCreated_ShouldThrowArgumentNullException_WhenSagaNameIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaCreated("ValidId", null!, "ValidDef", 1);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaName");
    }

    [Fact]
    public void ValidateSagaCreated_ShouldThrowArgumentNullException_WhenDefinitionIdIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaCreated("ValidId", "ValidName", null!, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("definitionId");
    }

    [Fact]
    public void ValidateSagaCreated_ShouldReturnError_WhenSagaIdExceedsMaxLength()
    {
        // Arrange
        var longId = new string('a', 201); // MaxSagaNameLength is 200

        // Act
        var result = _validator.ValidateSagaCreated(longId, "ValidName", "ValidDef", 1);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Saga ID length cannot exceed 200 characters");
    }

    [Fact]
    public void ValidateSagaCreated_ShouldReturnError_WhenSagaNameExceedsMaxLength()
    {
        // Arrange
        var longName = new string('b', 201); // MaxSagaNameLength is 200

        // Act
        var result = _validator.ValidateSagaCreated("ValidId", longName, "ValidDef", 1);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Saga name length cannot exceed 200 characters");
    }

    [Fact]
    public void ValidateSagaCreated_ShouldReturnError_WhenDefinitionIdExceedsMaxLength()
    {
        // Arrange
        var longDefId = new string('c', 201); // MaxDefinitionIdLength is 200

        // Act
        var result = _validator.ValidateSagaCreated("ValidId", "ValidName", longDefId, 1);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Definition ID length cannot exceed 200 characters");
    }

    [Fact]
    public void ValidateSagaCreated_ShouldReturnError_WhenStepCountIsNegative()
    {
        // Act
        var result = _validator.ValidateSagaCreated("ValidId", "ValidName", "ValidDef", -1);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Step count cannot be negative.");
    }

    #endregion

    #region ValidateStepStarted Tests

    [Fact]
    public void ValidateStepStarted_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var sagaId = "saga_123";
        var stepName = "ProcessPayment";
        var stepOrder = 2;
        var totalSteps = 5;

        // Act
        var result = _validator.ValidateStepStarted(sagaId, stepName, stepOrder, totalSteps);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateStepStarted_ShouldThrowArgumentNullException_WhenSagaIdIsNull()
    {
        // Act
        Action act = () => _validator.ValidateStepStarted(null!, "ValidStep", 0, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void ValidateStepStarted_ShouldThrowArgumentNullException_WhenStepNameIsNull()
    {
        // Act
        Action act = () => _validator.ValidateStepStarted("ValidId", null!, 0, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("stepName");
    }

    [Fact]
    public void ValidateStepStarted_ShouldReturnError_WhenStepOrderIsNegative()
    {
        // Act
        var result = _validator.ValidateStepStarted("ValidId", "ValidStep", -1, 5);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Step order cannot be negative.");
    }

    [Fact]
    public void ValidateStepStarted_ShouldReturnError_WhenTotalStepsIsZeroOrNegative()
    {
        // Act
        var result = _validator.ValidateStepStarted("ValidId", "ValidStep", 0, 0);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Total steps must be positive.");
    }

    [Fact]
    public void ValidateStepStarted_ShouldReturnError_WhenStepOrderExceedsTotalSteps()
    {
        // Act
        var result = _validator.ValidateStepStarted("ValidId", "ValidStep", 5, 5);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Step order must be less than total steps.");
    }

    #endregion

    #region ValidateStepCompleted Tests

    [Fact]
    public void ValidateStepCompleted_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var stepName = "SendConfirmation";
        var durationMs = 1500L;

        // Act
        var result = _validator.ValidateStepCompleted(stepName, durationMs);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateStepCompleted_ShouldThrowArgumentNullException_WhenStepNameIsNull()
    {
        // Act
        Action act = () => _validator.ValidateStepCompleted(null!, 1000L);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("stepName");
    }

    [Fact]
    public void ValidateStepCompleted_ShouldReturnError_WhenStepNameExceedsMaxLength()
    {
        // Arrange
        var longStepName = new string('x', 101); // MaxStepNameLength is 100

        // Act
        var result = _validator.ValidateStepCompleted(longStepName, 1000L);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Step name length cannot exceed 100 characters");
    }

    [Fact]
    public void ValidateStepCompleted_ShouldReturnError_WhenDurationIsNegative()
    {
        // Act
        var result = _validator.ValidateStepCompleted("ValidStep", -1L);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Duration cannot be negative.");
    }

    #endregion

    #region ValidateStepFailed Tests

    [Fact]
    public void ValidateStepFailed_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var sagaId = "saga_123";
        var stepName = "ChargeCustomer";
        var error = "Insufficient funds";
        var attemptNumber = 1;
        var maxRetries = 3;

        // Act
        var result = _validator.ValidateStepFailed(sagaId, stepName, error, attemptNumber, maxRetries);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateStepFailed_ShouldThrowArgumentNullException_WhenSagaIdIsNull()
    {
        // Act
        Action act = () => _validator.ValidateStepFailed(null!, "ValidStep", "ValidError", 0);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void ValidateStepFailed_ShouldThrowArgumentNullException_WhenStepNameIsNull()
    {
        // Act
        Action act = () => _validator.ValidateStepFailed("ValidId", null!, "ValidError", 0);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("stepName");
    }

    [Fact]
    public void ValidateStepFailed_ShouldThrowArgumentNullException_WhenErrorIsNull()
    {
        // Act
        Action act = () => _validator.ValidateStepFailed("ValidId", "ValidStep", null!, 0);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("error");
    }

    [Fact]
    public void ValidateStepFailed_ShouldReturnError_WhenErrorExceedsMaxLength()
    {
        // Arrange
        var longError = new string('e', 1001); // MaxErrorMessageLength is 1000

        // Act
        var result = _validator.ValidateStepFailed("ValidId", "ValidStep", longError, 0);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Error message length cannot exceed 1000 characters");
    }

    [Fact]
    public void ValidateStepFailed_ShouldReturnError_WhenAttemptNumberIsNegative()
    {
        // Act
        var result = _validator.ValidateStepFailed("ValidId", "ValidStep", "ValidError", -1);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Attempt number cannot be negative.");
    }

    [Fact]
    public void ValidateStepFailed_ShouldReturnError_WhenAttemptNumberExceedsMaxRetries()
    {
        // Act
        var result = _validator.ValidateStepFailed("ValidId", "ValidStep", "ValidError", 5, 3);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Attempt number cannot exceed max retries");
    }

    #endregion

    #region ValidateSagaCompleted Tests

    [Fact]
    public void ValidateSagaCompleted_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var sagaId = "saga_123";
        var sagaName = "OrderProcessing";
        var durationMs = 45000L;
        var completedSteps = 5;
        var totalSteps = 5;

        // Act
        var result = _validator.ValidateSagaCompleted(sagaId, sagaName, durationMs, completedSteps, totalSteps);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSagaCompleted_ShouldThrowArgumentNullException_WhenSagaIdIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaCompleted(null!, "ValidName", 0L, 0, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void ValidateSagaCompleted_ShouldThrowArgumentNullException_WhenSagaNameIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaCompleted("ValidId", null!, 0L, 0, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaName");
    }

    [Fact]
    public void ValidateSagaCompleted_ShouldReturnError_WhenDurationIsNegative()
    {
        // Act
        var result = _validator.ValidateSagaCompleted("ValidId", "ValidName", -1L, 0, 1);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Duration cannot be negative.");
    }

    [Fact]
    public void ValidateSagaCompleted_ShouldReturnError_WhenCompletedStepsIsNegative()
    {
        // Act
        var result = _validator.ValidateSagaCompleted("ValidId", "ValidName", 0L, -1, 5);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Completed steps cannot be negative.");
    }

    [Fact]
    public void ValidateSagaCompleted_ShouldReturnError_WhenTotalStepsIsZeroOrNegative()
    {
        // Act
        var result = _validator.ValidateSagaCompleted("ValidId", "ValidName", 0L, 0, 0);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Total steps must be positive.");
    }

    [Fact]
    public void ValidateSagaCompleted_ShouldReturnError_WhenCompletedStepsExceedsTotalSteps()
    {
        // Act
        var result = _validator.ValidateSagaCompleted("ValidId", "ValidName", 0L, 6, 5);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Completed steps cannot exceed total steps.");
    }

    #endregion

    #region ValidateSagaFailed Tests

    [Fact]
    public void ValidateSagaFailed_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var sagaId = "saga_123";
        var sagaName = "OrderProcessing";
        var failedStepName = "PaymentStep";
        var error = "Card declined";

        // Act
        var result = _validator.ValidateSagaFailed(sagaId, sagaName, failedStepName, error);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSagaFailed_ShouldThrowArgumentNullException_WhenSagaIdIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaFailed(null!, "ValidName", "ValidStep", "ValidError");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void ValidateSagaFailed_ShouldThrowArgumentNullException_WhenSagaNameIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaFailed("ValidId", null!, "ValidStep", "ValidError");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaName");
    }

    [Fact]
    public void ValidateSagaFailed_ShouldThrowArgumentNullException_WhenFailedStepNameIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaFailed("ValidId", "ValidName", null!, "ValidError");

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("failedStepName");
    }

    [Fact]
    public void ValidateSagaFailed_ShouldThrowArgumentNullException_WhenErrorIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaFailed("ValidId", "ValidName", "ValidStep", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("error");
    }

    [Fact]
    public void ValidateSagaFailed_ShouldReturnError_WhenSagaNameExceedsMaxLength()
    {
        // Arrange
        var longSagaName = new string('s', 201); // MaxSagaNameLength is 200

        // Act
        var result = _validator.ValidateSagaFailed("ValidId", longSagaName, "ValidStep", "ValidError");

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Saga name length cannot exceed 200 characters");
    }

    [Fact]
    public void ValidateSagaFailed_ShouldReturnError_WhenFailedStepNameExceedsMaxLength()
    {
        // Arrange
        var longStepName = new string('t', 101); // MaxStepNameLength is 100

        // Act
        var result = _validator.ValidateSagaFailed("ValidId", "ValidName", longStepName, "ValidError");

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Failed step name length cannot exceed 100 characters");
    }

    [Fact]
    public void ValidateSagaFailed_ShouldReturnError_WhenErrorExceedsMaxLength()
    {
        // Arrange
        var longError = new string('e', 1001); // MaxErrorMessageLength is 1000

        // Act
        var result = _validator.ValidateSagaFailed("ValidId", "ValidName", "ValidStep", longError);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Error message length cannot exceed 1000 characters");
    }

    #endregion

    #region ValidateCompensationStarted Tests

    [Fact]
    public void ValidateCompensationStarted_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var sagaId = "saga_123";
        var strategy = "rollback";
        var stepsToCompensate = 3;

        // Act
        var result = _validator.ValidateCompensationStarted(sagaId, strategy, stepsToCompensate);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCompensationStarted_ShouldThrowArgumentNullException_WhenSagaIdIsNull()
    {
        // Act
        Action act = () => _validator.ValidateCompensationStarted(null!, "ValidStrategy", 1);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void ValidateCompensationStarted_ShouldThrowArgumentNullException_WhenStrategyIsNull()
    {
        // Act
        Action act = () => _validator.ValidateCompensationStarted("ValidId", null!, 1);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("strategy");
    }

    [Fact]
    public void ValidateCompensationStarted_ShouldReturnError_WhenStrategyExceedsMaxLength()
    {
        // Arrange
        var longStrategy = new string('r', 51); // MaxStrategyLength is 50

        // Act
        var result = _validator.ValidateCompensationStarted("ValidId", longStrategy, 1);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Compensation strategy length cannot exceed 50 characters");
    }

    [Fact]
    public void ValidateCompensationStarted_ShouldReturnError_WhenStepsToCompensateIsNegative()
    {
        // Act
        var result = _validator.ValidateCompensationStarted("ValidId", "ValidStrategy", -1);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Steps to compensate cannot be negative.");
    }

    #endregion

    #region ValidateCompensationCompleted Tests

    [Fact]
    public void ValidateCompensationCompleted_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var sagaId = "saga_123";
        var compensatedSteps = 3;
        var durationMs = 15000L;

        // Act
        var result = _validator.ValidateCompensationCompleted(sagaId, compensatedSteps, durationMs);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateCompensationCompleted_ShouldThrowArgumentNullException_WhenSagaIdIsNull()
    {
        // Act
        Action act = () => _validator.ValidateCompensationCompleted(null!, 1, 1000L);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaId");
    }

    [Fact]
    public void ValidateCompensationCompleted_ShouldReturnError_WhenCompensatedStepsIsNegative()
    {
        // Act
        var result = _validator.ValidateCompensationCompleted("ValidId", -1, 1000L);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Compensated steps cannot be negative.");
    }

    [Fact]
    public void ValidateCompensationCompleted_ShouldReturnError_WhenDurationIsNegative()
    {
        // Act
        var result = _validator.ValidateCompensationCompleted("ValidId", 1, -1L);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Duration cannot be negative.");
    }

    #endregion

    #region ValidateSagaTimeout Tests

    [Fact]
    public void ValidateSagaTimeout_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var sagaName = "OrderProcessing";
        var stepName = "ValidateOrder";
        var timeoutSeconds = 300;

        // Act
        var result = _validator.ValidateSagaTimeout(sagaName, stepName, timeoutSeconds);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateSagaTimeout_ShouldThrowArgumentNullException_WhenSagaNameIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaTimeout(null!, "ValidStep", 300);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("sagaName");
    }

    [Fact]
    public void ValidateSagaTimeout_ShouldThrowArgumentNullException_WhenStepNameIsNull()
    {
        // Act
        Action act = () => _validator.ValidateSagaTimeout("ValidName", null!, 300);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("stepName");
    }

    [Fact]
    public void ValidateSagaTimeout_ShouldReturnError_WhenTimeoutIsZeroOrNegative()
    {
        // Act
        var result = _validator.ValidateSagaTimeout("ValidName", "ValidStep", 0);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Be("Timeout must be positive.");
    }

    #endregion

    #region ValidateDefinitionInvalid Tests

    [Fact]
    public void ValidateDefinitionInvalid_ShouldReturnEmptyList_WhenAllParametersAreValid()
    {
        // Arrange
        var reason = "Missing required step";

        // Act
        var result = _validator.ValidateDefinitionInvalid(reason);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ValidateDefinitionInvalid_ShouldThrowArgumentNullException_WhenReasonIsNull()
    {
        // Act
        Action act = () => _validator.ValidateDefinitionInvalid(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("reason");
    }

    [Fact]
    public void ValidateDefinitionInvalid_ShouldReturnError_WhenReasonExceedsMaxLength()
    {
        // Arrange
        var longReason = new string('m', 501); // MaxReasonLength is 500

        // Act
        var result = _validator.ValidateDefinitionInvalid(longReason);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Reason length cannot exceed 500 characters");
    }

    [Fact]
    public void ValidateDefinitionInvalid_ShouldReturnError_WhenStepNameExceedsMaxLength()
    {
        // Arrange
        var reason = "Invalid configuration";
        var longStepName = new string('s', 101); // MaxStepNameLength is 100

        // Act
        var result = _validator.ValidateDefinitionInvalid(reason, longStepName);

        // Assert
        result.Should().ContainSingle()
            .Which.Should().Contain("Step name length cannot exceed 100 characters");
    }

    #endregion
}