using Xunit;
using FluentAssertions;
using SagaOrchestrator.Infrastructure.Messaging;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Tests for SagaMessageTemplates class.
/// </summary>
public class SagaMessageTemplatesTests
{
    #region SagaCreated Tests

    [Fact]
    public void SagaCreated_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaId = "saga_123";
        var sagaName = "OrderProcessing";
        var stepCount = 5;

        // Act
        var result = SagaMessageTemplates.SagaCreated.Format(sagaId, sagaName, stepCount);

        // Assert
        result.Should().Be("Saga 'OrderProcessing' (ID: saga_123) created with 5 steps");
    }

    [Fact]
    public void SagaCreated_Format_ShouldHandleEmptyStrings()
    {
        // Arrange
        var sagaId = "";
        var sagaName = "";
        var stepCount = 0;

        // Act
        var result = SagaMessageTemplates.SagaCreated.Format(sagaId, sagaName, stepCount);

        // Assert
        result.Should().Be("Saga '' (ID: ) created with 0 steps");
    }

    [Fact]
    public void SagaCreated_Detailed_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaId = "saga_123";
        var sagaName = "OrderProcessing";
        var definitionId = "def_456";
        var stepCount = 3;

        // Act
        var result = SagaMessageTemplates.SagaCreated.Detailed(sagaId, sagaName, definitionId, stepCount);

        // Assert
        result.Should().Be("New saga instance created\n  ID: saga_123\n  Name: OrderProcessing\n  Definition: def_456\n  Steps: 3");
    }

    [Fact]
    public void SagaCreated_Detailed_ShouldHandleNullValues()
    {
        // Arrange
        string sagaId = null!;
        string sagaName = null!;
        string definitionId = null!;
        int stepCount = 0;

        // Act
        var result = SagaMessageTemplates.SagaCreated.Detailed(sagaId, sagaName, definitionId, stepCount);

        // Assert
        result.Should().Be("New saga instance created\n  ID: \n  Name: \n  Definition: \n  Steps: 0");
    }

    #endregion

    #region StepStarted Tests

    [Fact]
    public void StepStarted_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var stepName = "ValidateOrder";
        var stepOrder = 2;

        // Act
        var result = SagaMessageTemplates.StepStarted.Format(stepName, stepOrder);

        // Assert
        result.Should().Be("Executing step 2: ValidateOrder");
    }

    [Fact]
    public void StepStarted_Format_ShouldHandleZeroAndNegativeValues()
    {
        // Arrange
        var stepName = "InitialStep";
        var stepOrder = 0;

        // Act
        var result = SagaMessageTemplates.StepStarted.Format(stepName, stepOrder);

        // Assert
        result.Should().Be("Executing step 0: InitialStep");
    }

    [Fact]
    public void StepStarted_Detailed_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaId = "saga_123";
        var stepName = "ProcessPayment";
        var stepOrder = 1;
        var totalSteps = 3;

        // Act
        var result = SagaMessageTemplates.StepStarted.Detailed(sagaId, stepName, stepOrder, totalSteps);

        // Assert
        result.Should().Be("Step execution started\n  Saga: saga_123\n  Step: ProcessPayment\n  Progress: 1/3");
    }

    [Fact]
    public void StepStarted_Detailed_ShouldHandleBoundaryValues()
    {
        // Arrange
        var sagaId = "saga_123";
        var stepName = "FinalStep";
        var stepOrder = 5;
        var totalSteps = 5;

        // Act
        var result = SagaMessageTemplates.StepStarted.Detailed(sagaId, stepName, stepOrder, totalSteps);

        // Assert
        result.Should().Be("Step execution started\n  Saga: saga_123\n  Step: FinalStep\n  Progress: 5/5");
    }

    #endregion

    #region StepCompleted Tests

    [Fact]
    public void StepCompleted_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var stepName = "SendConfirmation";
        var durationMs = 1500L;

        // Act
        var result = SagaMessageTemplates.StepCompleted.Format(stepName, durationMs);

        // Assert
        result.Should().Be("Step 'SendConfirmation' completed in 1500ms");
    }

    [Fact]
    public void StepCompleted_Format_ShouldHandleZeroDuration()
    {
        // Arrange
        var stepName = "InstantStep";
        var durationMs = 0L;

        // Act
        var result = SagaMessageTemplates.StepCompleted.Format(stepName, durationMs);

        // Assert
        result.Should().Be("Step 'InstantStep' completed in 0ms");
    }

    [Fact]
    public void StepCompleted_Detailed_ShouldReturnCorrectMessage()
    {
        // Arrange
        var stepName = "ValidateInventory";
        var durationMs = 2500L;
        var result = "Success";

        // Act
        var detailedResult = SagaMessageTemplates.StepCompleted.Detailed(stepName, durationMs, result);

        // Assert
        detailedResult.Should().Be("Step execution completed\n  Step: ValidateInventory\n  Duration: 2500ms\n  Result: Success");
    }

    [Fact]
    public void StepCompleted_Detailed_ShouldHandleEmptyResult()
    {
        // Arrange
        var stepName = "CheckStatus";
        var durationMs = 100L;
        var result = "";

        // Act
        var detailedResult = SagaMessageTemplates.StepCompleted.Detailed(stepName, durationMs, result);

        // Assert
        detailedResult.Should().Be("Step execution completed\n  Step: CheckStatus\n  Duration: 100ms\n  Result: ");
    }

    #endregion

    #region StepFailed Tests

    [Fact]
    public void StepFailed_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var stepName = "ChargeCustomer";
        var error = "Insufficient funds";

        // Act
        var result = SagaMessageTemplates.StepFailed.Format(stepName, error);

        // Assert
        result.Should().Be("Step 'ChargeCustomer' failed: Insufficient funds");
    }

    [Fact]
    public void StepFailed_Format_ShouldHandleNullError()
    {
        // Arrange
        var stepName = "ProcessRefund";
        string error = null!;

        // Act
        var result = SagaMessageTemplates.StepFailed.Format(stepName, error);

        // Assert
        result.Should().Be("Step 'ProcessRefund' failed: ");
    }

    [Fact]
    public void StepFailed_WithRetry_ShouldReturnCorrectMessage()
    {
        // Arrange
        var stepName = "VerifyAddress";
        var error = "Timeout";
        var retryCount = 2;
        var maxRetries = 3;

        // Act
        var result = SagaMessageTemplates.StepFailed.WithRetry(stepName, error, retryCount, maxRetries);

        // Assert
        result.Should().Be("Step 'VerifyAddress' failed (attempt 2/3): Timeout");
    }

    [Fact]
    public void StepFailed_WithRetry_ShouldHandleFirstAndLastAttempt()
    {
        // Arrange
        var stepName = "RetryableStep";
        var error = "Network error";
        var retryCount = 1;
        var maxRetries = 3;

        // Act
        var result = SagaMessageTemplates.StepFailed.WithRetry(stepName, error, retryCount, maxRetries);

        // Assert
        result.Should().Be("Step 'RetryableStep' failed (attempt 1/3): Network error");
    }

    [Fact]
    public void StepFailed_Detailed_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaId = "saga_123";
        var stepName = "ExternalAPICall";
        var error = "Service unavailable";
        var attemptNumber = 3;

        // Act
        var result = SagaMessageTemplates.StepFailed.Detailed(sagaId, stepName, error, attemptNumber);

        // Assert
        result.Should().Be("Step execution failed\n  Saga: saga_123\n  Step: ExternalAPICall\n  Attempt: 3\n  Error: Service unavailable");
    }

    [Fact]
    public void StepFailed_Detailed_ShouldHandleZeroAttempt()
    {
        // Arrange
        var sagaId = "saga_123";
        var stepName = "FirstAttempt";
        var error = "Initial failure";
        var attemptNumber = 0;

        // Act
        var result = SagaMessageTemplates.StepFailed.Detailed(sagaId, stepName, error, attemptNumber);

        // Assert
        result.Should().Be("Step execution failed\n  Saga: saga_123\n  Step: FirstAttempt\n  Attempt: 0\n  Error: Initial failure");
    }

    #endregion

    #region SagaCompleted Tests

    [Fact]
    public void SagaCompleted_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaName = "CustomerOnboarding";
        var durationMs = 45000L;
        var completedSteps = 5;
        var totalSteps = 5;

        // Act
        var result = SagaMessageTemplates.SagaCompleted.Format(sagaName, durationMs, completedSteps, totalSteps);

        // Assert
        result.Should().Be("Saga 'CustomerOnboarding' completed successfully in 45000ms (5/5 steps)");
    }

    [Fact]
    public void SagaCompleted_Format_ShouldHandlePartialCompletion()
    {
        // Arrange
        var sagaName = "OrderFulfillment";
        var durationMs = 30000L;
        var completedSteps = 3;
        var totalSteps = 5;

        // Act
        var result = SagaMessageTemplates.SagaCompleted.Format(sagaName, durationMs, completedSteps, totalSteps);

        // Assert
        result.Should().Be("Saga 'OrderFulfillment' completed successfully in 30000ms (3/5 steps)");
    }

    [Fact]
    public void SagaCompleted_Detailed_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaId = "saga_789";
        var sagaName = "ProductLaunch";
        var durationMs = 60000L;
        var completedSteps = 4;
        var totalSteps = 4;

        // Act
        var result = SagaMessageTemplates.SagaCompleted.Detailed(sagaId, sagaName, durationMs, completedSteps, totalSteps);

        // Assert
        result.Should().Be("Saga execution completed\n  ID: saga_789\n  Name: ProductLaunch\n  Duration: 60000ms\n  Steps: 4/4\n  Status: SUCCESS");
    }

    [Fact]
    public void SagaCompleted_Detailed_ShouldHandleZeroDuration()
    {
        // Arrange
        var sagaId = "saga_instant";
        var sagaName = "ImmediateTask";
        var durationMs = 0L;
        var completedSteps = 1;
        var totalSteps = 1;

        // Act
        var result = SagaMessageTemplates.SagaCompleted.Detailed(sagaId, sagaName, durationMs, completedSteps, totalSteps);

        // Assert
        result.Should().Be("Saga execution completed\n  ID: saga_instant\n  Name: ImmediateTask\n  Duration: 0ms\n  Steps: 1/1\n  Status: SUCCESS");
    }

    #endregion

    #region SagaFailed Tests

    [Fact]
    public void SagaFailed_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaName = "PaymentProcessing";
        var error = "Card declined";

        // Act
        var result = SagaMessageTemplates.SagaFailed.Format(sagaName, error);

        // Assert
        result.Should().Be("Saga 'PaymentProcessing' failed: Card declined");
    }

    [Fact]
    public void SagaFailed_Format_ShouldHandleEmptyError()
    {
        // Arrange
        var sagaName = "SilentFailure";
        var error = "";

        // Act
        var result = SagaMessageTemplates.SagaFailed.Format(sagaName, error);

        // Assert
        result.Should().Be("Saga 'SilentFailure' failed: ");
    }

    [Fact]
    public void SagaFailed_Detailed_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaId = "saga_failed_123";
        var sagaName = "DataMigration";
        var failedStepName = "TransformData";
        var error = "Invalid data format";

        // Act
        var result = SagaMessageTemplates.SagaFailed.Detailed(sagaId, sagaName, failedStepName, error);

        // Assert
        result.Should().Be("Saga execution failed\n  ID: saga_failed_123\n  Name: DataMigration\n  Failed Step: TransformData\n  Error: Invalid data format\n  Status: FAILED");
    }

    [Fact]
    public void SagaFailed_Detailed_ShouldHandleNullValues()
    {
        // Arrange
        string sagaId = null!;
        string sagaName = null!;
        string failedStepName = null!;
        string error = null!;

        // Act
        var result = SagaMessageTemplates.SagaFailed.Detailed(sagaId, sagaName, failedStepName, error);

        // Assert
        result.Should().Be("Saga execution failed\n  ID: \n  Name: \n  Failed Step: \n  Error: \n  Status: FAILED");
    }

    #endregion

    #region Compensation Tests

    [Fact]
    public void CompensationStarted_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var strategy = "rollback";
        var stepsToCompensate = 3;

        // Act
        var result = SagaMessageTemplates.CompensationStarted.Format(strategy, stepsToCompensate);

        // Assert
        result.Should().Be("Compensation started (rollback strategy) for 3 steps");
    }

    [Fact]
    public void CompensationStarted_Format_ShouldHandleZeroSteps()
    {
        // Arrange
        var strategy = "none";
        var stepsToCompensate = 0;

        // Act
        var result = SagaMessageTemplates.CompensationStarted.Format(strategy, stepsToCompensate);

        // Assert
        result.Should().Be("Compensation started (none strategy) for 0 steps");
    }

    [Fact]
    public void CompensationStarted_Detailed_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaId = "saga_comp_456";
        var strategy = "semantic";
        var stepsToCompensate = 2;

        // Act
        var result = SagaMessageTemplates.CompensationStarted.Detailed(sagaId, strategy, stepsToCompensate);

        // Assert
        result.Should().Be("Compensation initiated\n  Saga: saga_comp_456\n  Strategy: semantic\n  Steps to compensate: 2");
    }

    [Fact]
    public void CompensationCompleted_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var compensatedSteps = 4;
        var durationMs = 15000L;

        // Act
        var result = SagaMessageTemplates.CompensationCompleted.Format(compensatedSteps, durationMs);

        // Assert
        result.Should().Be("Compensation completed for 4 steps in 15000ms");
    }

    [Fact]
    public void CompensationCompleted_Detailed_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaId = "saga_comp_789";
        var compensatedSteps = 3;
        var durationMs = 8000L;

        // Act
        var result = SagaMessageTemplates.CompensationCompleted.Detailed(sagaId, compensatedSteps, durationMs);

        // Assert
        result.Should().Be("Compensation completed\n  Saga: saga_comp_789\n  Steps compensated: 3\n  Duration: 8000ms");
    }

    #endregion

    #region Timeout Tests

    [Fact]
    public void SagaTimeout_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var sagaName = "LongRunningProcess";
        var timeoutSeconds = 300;

        // Act
        var result = SagaMessageTemplates.SagaTimeout.Format(sagaName, timeoutSeconds);

        // Assert
        result.Should().Be("Saga 'LongRunningProcess' exceeded timeout limit of 300 seconds");
    }

    [Fact]
    public void SagaTimeout_Format_ShouldHandleZeroTimeout()
    {
        // Arrange
        var sagaName = "InstantTimeout";
        var timeoutSeconds = 0;

        // Act
        var result = SagaMessageTemplates.SagaTimeout.Format(sagaName, timeoutSeconds);

        // Assert
        result.Should().Be("Saga 'InstantTimeout' exceeded timeout limit of 0 seconds");
    }

    [Fact]
    public void SagaTimeout_StepTimeout_ShouldReturnCorrectMessage()
    {
        // Arrange
        var stepName = "ExternalServiceCall";
        var timeoutSeconds = 30;

        // Act
        var result = SagaMessageTemplates.SagaTimeout.StepTimeout(stepName, timeoutSeconds);

        // Assert
        result.Should().Be("Step 'ExternalServiceCall' exceeded timeout limit of 30 seconds");
    }

    #endregion

    #region DefinitionInvalid Tests

    [Fact]
    public void DefinitionInvalid_Format_ShouldReturnCorrectMessage()
    {
        // Arrange
        var reason = "Missing required step";

        // Act
        var result = SagaMessageTemplates.DefinitionInvalid.Format(reason);

        // Assert
        result.Should().Be("Saga definition is invalid: Missing required step");
    }

    [Fact]
    public void DefinitionInvalid_Format_ShouldHandleEmptyReason()
    {
        // Arrange
        var reason = "";

        // Act
        var result = SagaMessageTemplates.DefinitionInvalid.Format(reason);

        // Assert
        result.Should().Be("Saga definition is invalid: ");
    }

    [Fact]
    public void DefinitionInvalid_MissingSteps_ShouldReturnCorrectMessage()
    {
        // Act
        var result = SagaMessageTemplates.DefinitionInvalid.MissingSteps();

        // Assert
        result.Should().Be("Saga definition must contain at least one step");
    }

    [Fact]
    public void DefinitionInvalid_InvalidStep_ShouldReturnCorrectMessage()
    {
        // Arrange
        var stepName = "InvalidStep";
        var reason = "Step configuration is malformed";

        // Act
        var result = SagaMessageTemplates.DefinitionInvalid.InvalidStep(stepName, reason);

        // Assert
        result.Should().Be("Step 'InvalidStep' is invalid: Step configuration is malformed");
    }

    [Fact]
    public void DefinitionInvalid_InvalidStep_ShouldHandleEmptyValues()
    {
        // Arrange
        var stepName = "";
        var reason = "";

        // Act
        var result = SagaMessageTemplates.DefinitionInvalid.InvalidStep(stepName, reason);

        // Assert
        result.Should().Be("Step '' is invalid: ");
    }

    #endregion

    #region Miscellaneous Tests

    [Fact]
    public void ServiceHealth_ShouldReturnCorrectMessage_WhenHealthy()
    {
        // Arrange
        var serviceName = "PaymentGateway";
        var isHealthy = true;

        // Act
        var result = SagaMessageTemplates.ServiceHealth(serviceName, isHealthy);

        // Assert
        result.Should().Be("Service 'PaymentGateway' is healthy");
    }

    [Fact]
    public void ServiceHealth_ShouldReturnCorrectMessage_WhenUnhealthy()
    {
        // Arrange
        var serviceName = "InventoryService";
        var isHealthy = false;

        // Act
        var result = SagaMessageTemplates.ServiceHealth(serviceName, isHealthy);

        // Assert
        result.Should().Be("Service 'InventoryService' is unhealthy");
    }

    [Fact]
    public void WebhookDelivery_ShouldReturnCorrectMessage_WhenSuccess()
    {
        // Arrange
        var url = "https://example.com/webhook";
        var eventType = "OrderCreated";
        var success = true;

        // Act
        var result = SagaMessageTemplates.WebhookDelivery(url, eventType, success);

        // Assert
        result.Should().Be("Webhook delivery succeeded for OrderCreated to https://example.com/webhook");
    }

    [Fact]
    public void WebhookDelivery_ShouldReturnCorrectMessage_WhenFailure()
    {
        // Arrange
        var url = "https://failed-endpoint.com/callback";
        var eventType = "PaymentFailed";
        var success = false;

        // Act
        var result = SagaMessageTemplates.WebhookDelivery(url, eventType, success);

        // Assert
        result.Should().Be("Webhook delivery failed for PaymentFailed to https://failed-endpoint.com/callback");
    }

    #endregion
}