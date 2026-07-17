using System;
using System.Threading.Tasks;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides extension methods for the <see cref="SagaDefinitionValidatorTests"/> class.
/// </summary>
public static class SagaDefinitionValidatorTestsExtensions
{
    /// <summary>
    /// Runs a set of basic validation tests.
    /// </summary>
    /// <param name="instance">The test instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public static async Task RunBasicValidationTestsAsync(this SagaDefinitionValidatorTests instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        await instance.ValidateAsync_WithValidDefinition_DoesNotThrow();
        await instance.ValidateAsync_WithInvalidDefinition_Throws();
        await instance.ValidateAsync_ThrowsWithAllErrors_InExceptionMessage();
    }

    /// <summary>
    /// Runs a set of field validation tests.
    /// </summary>
    /// <param name="instance">The test instance.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public static async Task RunFieldValidationTestsAsync(this SagaDefinitionValidatorTests instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        await instance.ValidateAndGetErrorsAsync_NullName_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_NameTooLong_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_NoSteps_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_TooManySteps_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_InvalidStepName_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_InvalidServiceUrl_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_InvalidCompensationUrl_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_TimeoutZero_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_TimeoutTooLarge_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_NegativeRetries_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_TooManyRetries_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_DuplicateStepOrder_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_OrderDoesNotStartAtOne_ReturnsError();
        await instance.ValidateAndGetErrorsAsync_MultipleErrors_ReturnsAll();
    }
}