#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace SagaOrchestrator.Infrastructure.Telemetry;

/// <summary>
/// Validation extension methods for <see cref="SagaActivitySourceExtensions"/> to validate method arguments
/// and provide validation helpers for telemetry operations.
/// </summary>
public static class SagaActivitySourceExtensionsValidation
{
    /// <summary>
    /// Validates the parameters for <see cref="SagaActivitySourceExtensions.StartSaga"/> method.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="definitionId">The saga definition identifier.</param>
    /// <param name="correlationId">Optional correlation identifier for distributed tracing.</param>
    /// <param name="sagaType">Optional saga type/category for filtering and grouping.</param>
    /// <param name="tenantId">Optional tenant identifier for multi-tenancy scenarios.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateStartSaga(
        string sagaId,
        string definitionId,
        string? correlationId = null,
        string? sagaType = null,
        string? tenantId = null)
    {
        var problems = new List<string>();

        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="SagaActivitySourceExtensions.RecordSagaComplete"/> method.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="finalStatus">The final saga status (e.g., "Completed", "Compensated", "Failed").</param>
    /// <param name="totalSteps">Total number of steps in the saga.</param>
    /// <param name="duration">Total execution duration for performance tracking.</param>
    /// <param name="completedSteps">Number of successfully completed steps.</param>
    /// <param name="failedSteps">Number of failed steps.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateRecordSagaComplete(
        string sagaId,
        string finalStatus,
        int totalSteps,
        TimeSpan duration,
        int completedSteps = 0,
        int failedSteps = 0)
    {
        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(sagaId))
        {
            problems.Add("Saga identifier cannot be null, empty, or whitespace.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(finalStatus);

        if (totalSteps < 0)
        {
            problems.Add("Total steps cannot be negative.");
        }

        if (completedSteps < 0)
        {
            problems.Add("Completed steps cannot be negative.");
        }

        if (failedSteps < 0)
        {
            problems.Add("Failed steps cannot be negative.");
        }

        if (completedSteps + failedSteps > totalSteps)
        {
            problems.Add("Sum of completed steps and failed steps cannot exceed total steps.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="SagaActivitySourceExtensions.StartStep"/> method.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="stepName">Human-readable step name.</param>
    /// <param name="order">Execution order position in the saga.</param>
    /// <param name="attempt">Retry attempt number (default: 1).</param>
    /// <param name="stepType">Optional step type/category for filtering.</param>
    /// <param name="serviceName">Optional service name where the step executes.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateStartStep(
        string sagaId,
        string stepId,
        string stepName,
        int order,
        int attempt = 1,
        string? stepType = null,
        string? serviceName = null)
    {
        var problems = new List<string>();

        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);

        if (order < 0)
        {
            problems.Add("Step order cannot be negative.");
        }

        if (attempt < 1)
        {
            problems.Add("Retry attempt must be at least 1.");
        }


        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="SagaActivitySourceExtensions.RecordStepFailure"/> method.
    /// </summary>
    /// <param name="activity">The activity to record the failure on.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="exception">Optional exception containing detailed error information.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateRecordStepFailure(
        Activity? activity,
        string errorMessage,
        Exception? exception = null)
    {
        var problems = new List<string>();

        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="SagaActivitySourceExtensions.StartCompensation"/> method.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="compensationId">The compensation transaction identifier.</param>
    /// <param name="stepName">Name of the step being compensated.</param>
    /// <param name="stepOrder">Execution order of the step being compensated.</param>
    /// <param name="compensationType">Optional compensation type/category.</param>
    /// <param name="compensatingService">Optional service performing the compensation.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateStartCompensation(
        string sagaId,
        string compensationId,
        string stepName,
        int stepOrder,
        string? compensationType = null,
        string? compensatingService = null)
    {
        var problems = new List<string>();

        ArgumentException.ThrowIfNullOrWhiteSpace(sagaId);
        ArgumentException.ThrowIfNullOrWhiteSpace(compensationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);

        if (stepOrder < 0)
        {
            problems.Add("Step order cannot be negative.");
        }


        return problems.AsReadOnly();
    }

    /// <summary>
    /// Validates the parameters for <see cref="SagaActivitySourceExtensions.RecordCompensationFailure"/> method.
    /// </summary>
    /// <param name="activity">The activity to record the failure on.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="exception">Optional exception containing detailed error information.</param>
    /// <returns>List of validation problems; empty list if valid.</returns>
    public static IReadOnlyList<string> ValidateRecordCompensationFailure(
        Activity? activity,
        string errorMessage,
        Exception? exception = null)
    {
        var problems = new List<string>();

        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified saga operation parameters are valid.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="definitionId">The saga definition identifier.</param>
    /// <param name="correlationId">Optional correlation identifier for distributed tracing.</param>
    /// <param name="sagaType">Optional saga type/category for filtering and grouping.</param>
    /// <param name="tenantId">Optional tenant identifier for multi-tenancy scenarios.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaId"/> or <paramref name="definitionId"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/>, <paramref name="definitionId"/>, <paramref name="correlationId"/>, <paramref name="sagaType"/>, or <paramref name="tenantId"/> is empty or whitespace.</exception>
    public static bool IsValidStartSaga(
        string sagaId,
        string definitionId,
        string? correlationId = null,
        string? sagaType = null,
        string? tenantId = null)
        => ValidateStartSaga(sagaId, definitionId, correlationId, sagaType, tenantId).Count == 0;

    /// <summary>
    /// Determines whether the specified saga completion parameters are valid.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="finalStatus">The final saga status (e.g., "Completed", "Compensated", "Failed").</param>
    /// <param name="totalSteps">Total number of steps in the saga.</param>
    /// <param name="duration">Total execution duration for performance tracking.</param>
    /// <param name="completedSteps">Number of successfully completed steps.</param>
    /// <param name="failedSteps">Number of failed steps.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaId"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> or <paramref name="finalStatus"/> is empty or whitespace.</exception>
    public static bool IsValidRecordSagaComplete(
        string sagaId,
        string finalStatus,
        int totalSteps,
        TimeSpan duration,
        int completedSteps = 0,
        int failedSteps = 0)
        => ValidateRecordSagaComplete(sagaId, finalStatus, totalSteps, duration, completedSteps, failedSteps).Count == 0;

    /// <summary>
    /// Determines whether the specified step execution parameters are valid.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="stepName">Human-readable step name.</param>
    /// <param name="order">Execution order position in the saga.</param>
    /// <param name="attempt">Retry attempt number (default: 1).</param>
    /// <param name="stepType">Optional step type/category for filtering.</param>
    /// <param name="serviceName">Optional service name where the step executes.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaId"/>, <paramref name="stepId"/>, or <paramref name="stepName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/>, <paramref name="stepId"/>, or <paramref name="stepName"/> is empty or whitespace.</exception>
    public static bool IsValidStartStep(
        string sagaId,
        string stepId,
        string stepName,
        int order,
        int attempt = 1,
        string? stepType = null,
        string? serviceName = null)
        => ValidateStartStep(sagaId, stepId, stepName, order, attempt, stepType, serviceName).Count == 0;

        /// <summary>
    /// Determines whether the specified compensation parameters are valid.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="compensationId">The compensation transaction identifier.</param>
    /// <param name="stepName">Name of the step being compensated.</param>
    /// <param name="stepOrder">Execution order of the step being compensated.</param>
    /// <param name="compensationType">Optional compensation type/category.</param>
    /// <param name="compensatingService">Optional service performing the compensation.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaId"/>, <paramref name="compensationId"/>, or <paramref name="stepName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/>, <paramref name="compensationId"/>, or <paramref name="stepName"/> is empty or whitespace.</exception>
    public static bool IsValidStartCompensation(
        string sagaId,
        string compensationId,
        string stepName,
        int stepOrder,
        string? compensationType = null,
        string? compensatingService = null)
        => ValidateStartCompensation(sagaId, compensationId, stepName, stepOrder, compensationType, compensatingService).Count == 0;

    /// <summary>
    /// Determines whether the specified compensation failure parameters are valid.
    /// </summary>
    /// <param name="activity">The activity to record the failure on.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="exception">Optional exception containing detailed error information.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errorMessage"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorMessage"/> is empty or whitespace.</exception>
    public static bool IsValidRecordStepFailure(
        Activity? activity,
        string errorMessage,
        Exception? exception = null)
        => ValidateRecordStepFailure(activity, errorMessage, exception).Count == 0;

    /// <summary>
    /// Ensures that the specified saga operation parameters are valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="definitionId">The saga definition identifier.</param>
    /// <param name="correlationId">Optional correlation identifier for distributed tracing.</param>
    /// <param name="sagaType">Optional saga type/category for filtering and grouping.</param>
    /// <param name="tenantId">Optional tenant identifier for multi-tenancy scenarios.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaId"/> or <paramref name="definitionId"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/>, <paramref name="definitionId"/>, <paramref name="correlationId"/>, <paramref name="sagaType"/>, or <paramref name="tenantId"/> is empty or whitespace.</exception>
    public static void EnsureValidStartSaga(
        string sagaId,
        string definitionId,
        string? correlationId = null,
        string? sagaType = null,
        string? tenantId = null)
    {
        var problems = ValidateStartSaga(sagaId, definitionId, correlationId, sagaType, tenantId);
        if (problems.Count > 0)
        {
            throw new ArgumentException("SagaActivitySourceExtensions validation failed. " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified saga completion parameters are valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="finalStatus">The final saga status (e.g., "Completed", "Compensated", "Failed").</param>
    /// <param name="totalSteps">Total number of steps in the saga.</param>
    /// <param name="duration">Total execution duration for performance tracking.</param>
    /// <param name="completedSteps">Number of successfully completed steps.</param>
    /// <param name="failedSteps">Number of failed steps.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaId"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/> or <paramref name="finalStatus"/> is empty or whitespace.</exception>
    public static void EnsureValidRecordSagaComplete(
        string sagaId,
        string finalStatus,
        int totalSteps,
        TimeSpan duration,
        int completedSteps = 0,
        int failedSteps = 0)
    {
        var problems = ValidateRecordSagaComplete(sagaId, finalStatus, totalSteps, duration, completedSteps, failedSteps);
        if (problems.Count > 0)
        {
            throw new ArgumentException("SagaActivitySourceExtensions validation failed. " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified step execution parameters are valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="stepId">The step identifier.</param>
    /// <param name="stepName">Human-readable step name.</param>
    /// <param name="order">Execution order position in the saga.</param>
    /// <param name="attempt">Retry attempt number (default: 1).</param>
    /// <param name="stepType">Optional step type/category for filtering.</param>
    /// <param name="serviceName">Optional service name where the step executes.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaId"/>, <paramref name="stepId"/>, or <paramref name="stepName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/>, <paramref name="stepId"/>, or <paramref name="stepName"/> is empty or whitespace.</exception>
    public static void EnsureValidStartStep(
        string sagaId,
        string stepId,
        string stepName,
        int order,
        int attempt = 1,
        string? stepType = null,
        string? serviceName = null)
    {
        var problems = ValidateStartStep(sagaId, stepId, stepName, order, attempt, stepType, serviceName);
        if (problems.Count > 0)
        {
            throw new ArgumentException("SagaActivitySourceExtensions validation failed. " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified compensation parameters are valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="sagaId">The saga identifier.</param>
    /// <param name="compensationId">The compensation transaction identifier.</param>
    /// <param name="stepName">Name of the step being compensated.</param>
    /// <param name="stepOrder">Execution order of the step being compensated.</param>
    /// <param name="compensationType">Optional compensation type/category.</param>
    /// <param name="compensatingService">Optional service performing the compensation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sagaId"/>, <paramref name="compensationId"/>, or <paramref name="stepName"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sagaId"/>, <paramref name="compensationId"/>, or <paramref name="stepName"/> is empty or whitespace.</exception>
    public static void EnsureValidStartCompensation(
        string sagaId,
        string compensationId,
        string stepName,
        int stepOrder,
        string? compensationType = null,
        string? compensatingService = null)
    {
        var problems = ValidateStartCompensation(sagaId, compensationId, stepName, stepOrder, compensationType, compensatingService);
        if (problems.Count > 0)
        {
            throw new ArgumentException("SagaActivitySourceExtensions validation failed. " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Ensures that the specified compensation failure parameters are valid, throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="activity">The activity to record the failure on.</param>
    /// <param name="errorMessage">The error message describing the failure.</param>
    /// <param name="exception">Optional exception containing detailed error information.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errorMessage"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorMessage"/> is empty or whitespace.</exception>
    public static void EnsureValidRecordStepFailure(
        Activity? activity,
        string errorMessage,
        Exception? exception = null)
    {
        var problems = ValidateRecordStepFailure(activity, errorMessage, exception);
        if (problems.Count > 0)
        {
            throw new ArgumentException("SagaActivitySourceExtensions validation failed. " + string.Join(" ", problems));
        }
    }
}