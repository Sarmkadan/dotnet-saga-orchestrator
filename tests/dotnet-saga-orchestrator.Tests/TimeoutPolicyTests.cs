#nullable enable

using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Test data transfer object for timeout policy testing scenarios.
/// </summary>
public record TimeoutPolicyTests(
int TimeoutSeconds,
bool IsRelaxed
)
{
/// <summary>
/// Creates a standard timeout policy test data (60 seconds, not relaxed).
/// </summary>
public static TimeoutPolicyTests Standard => new(60, false);

/// <summary>
/// Creates a lenient timeout policy test data (300 seconds, relaxed).
/// </summary>
public static TimeoutPolicyTests Lenient => new(300, true);

/// <summary>
/// Creates a strict timeout policy test data (10 seconds, not relaxed).
/// </summary>
public static TimeoutPolicyTests Strict => new(10, false);

/// <summary>
/// Converts the timeout policy test data to a TimeoutPolicy instance.
/// </summary>
/// <returns>A TimeoutPolicy instance.</returns>
public TimeoutPolicy ToPolicy() => new(this.TimeoutSeconds);
}
