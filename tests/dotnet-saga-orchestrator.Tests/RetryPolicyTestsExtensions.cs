using System;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides extension methods for the <see cref="RetryPolicyTests"/> class to aggregate test execution.
/// </summary>
public static class RetryPolicyTestsExtensions
{
    /// <summary>
    /// Runs all constructor-related tests.
    /// </summary>
    /// <param name="instance">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public static void RunConstructorTests(this RetryPolicyTests instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        instance.Constructor_NegativeMaxRetries_ThrowsArgumentException();
        instance.Constructor_NegativeInitialDelay_ThrowsArgumentException();
        instance.Constructor_BackoffMultiplierBelowOne_ThrowsArgumentException();
        instance.Constructor_MaxDelayLessThanInitialDelay_ThrowsArgumentException();
    }

    /// <summary>
    /// Runs all delay calculation-related tests.
    /// </summary>
    /// <param name="instance">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public static void RunCalculateDelayTests(this RetryPolicyTests instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        instance.CalculateDelay_FirstAttempt_ReturnsInitialDelay();
        instance.CalculateDelay_SecondAttempt_AppliesExponentialBackoff();
        instance.CalculateDelay_ThirdAttempt_SquaresTheMultiplier();
        instance.CalculateDelay_LargeAttemptNumber_CapsAtMaxDelay();
        instance.CalculateDelay_AttemptBelowOne_ThrowsArgumentException(0);
        instance.CalculateDelay_AttemptExceedsMaxRetries_ThrowsInvalidOperationException();
    }

    /// <summary>
    /// Runs all policy creation factory method tests.
    /// </summary>
    /// <param name="instance">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public static void RunFactoryMethodTests(this RetryPolicyTests instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        instance.CreateLinear_AllAttemptsReturnSameFixedDelay();
        instance.CreateNoRetry_SetsZeroMaxRetriesAndDelay();
        instance.CreateExponential_UsesDoubleBackoffMultiplier();
    }

    /// <summary>
    /// Runs all retry capability tests.
    /// </summary>
    /// <param name="instance">The test instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public static void RunCanRetryTests(this RetryPolicyTests instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        instance.CanRetry_WhenBelowMaxRetries_ReturnsTrue();
        instance.CanRetry_WhenAtMaxRetries_ReturnsFalse();
    }
}
