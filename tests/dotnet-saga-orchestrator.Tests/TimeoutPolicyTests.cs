#nullable enable

using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Test data transfer object for timeout policy testing scenarios.
/// Provides predefined timeout configurations and conversion to <see cref="TimeoutPolicy"/>.
/// </summary>
public record TimeoutPolicyTests(
    /// <summary>
    /// Gets the timeout duration in seconds.
    /// </summary>
    int TimeoutSeconds,

    /// <summary>
    /// Gets a value indicating whether the timeout policy is relaxed.
    /// </summary>
    bool IsRelaxed
)
{
    /// <summary>
    /// Gets a standard timeout policy test data configuration (60 seconds, not relaxed).
    /// </summary>
    public static TimeoutPolicyTests Standard => new(60, false);

    /// <summary>
    /// Gets a lenient timeout policy test data configuration (300 seconds, relaxed).
    /// </summary>
    public static TimeoutPolicyTests Lenient => new(300, true);

    /// <summary>
    /// Gets a strict timeout policy test data configuration (10 seconds, not relaxed).
    /// </summary>
    public static TimeoutPolicyTests Strict => new(10, false);

    /// <summary>
    /// Converts the timeout policy test data to a <see cref="TimeoutPolicy"/> instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="TimeoutPolicy"/> instance initialized with the <see cref="TimeoutSeconds"/>.
    /// The <see cref="IsRelaxed"/> property is not used in this conversion.
    /// </returns>
    public TimeoutPolicy ToPolicy() => new(this.TimeoutSeconds);
}
