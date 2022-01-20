using System;
using System.Collections.Generic;
using SagaOrchestrator.Tests;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides extension methods for <see cref="StringExtensionsTests"/>.
/// </summary>
public static class StringExtensionsTestsExtensions
{
    /// <summary>
    /// Asserts that the provided operation on an input string matches the expected output.
    /// </summary>
    /// <param name="tests">The test class instance.</param>
    /// <param name="input">The input string.</param>
    /// <param name="expected">The expected output.</param>
    /// <param name="operation">The operation to perform.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> or <paramref name="operation"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the result of the operation does not match the expected value.</exception>
    public static void AssertStringOperation(this StringExtensionsTests tests, string input, string expected, Func<string, string> operation)
    {
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(operation);

        var result = operation(input);
        if (result != expected)
        {
            throw new InvalidOperationException($"Expected {expected} but got {result}");
        }
    }

    /// <summary>
    /// Returns a collection of sample inputs based on the specified scenario.
    /// </summary>
    /// <param name="tests">The test class instance.</param>
    /// <param name="scenario">The test scenario.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of sample strings.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="tests"/> is null.</exception>
    public static IEnumerable<string> GetSampleInputs(this StringExtensionsTests tests, string scenario)
    {
        ArgumentNullException.ThrowIfNull(tests);

        return scenario switch
        {
            "PascalCase" => new List<string> { "OrderProcessing", "SagaOrchestrator" },
            "SingleWord" => new List<string> { "Saga", "A" },
            _ => Array.Empty<string>()
        };
    }
}
