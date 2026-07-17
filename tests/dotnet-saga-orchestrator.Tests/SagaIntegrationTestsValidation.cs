#nullable enable

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace SagaOrchestrator.Tests;

/// <summary>
/// Provides validation helpers for <see cref="SagaIntegrationTests"/> to ensure test data integrity
/// and provide meaningful error messages when validation fails.
/// </summary>
public static class SagaIntegrationTestsValidation
{
    /// <summary>
    /// Validates the specified <see cref="SagaIntegrationTests"/> instance and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A read-only list of validation problems. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this SagaIntegrationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        ValidateServiceProvider(value, problems);
        ValidateTestMethods(value, problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="SagaIntegrationTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this SagaIntegrationTests value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="SagaIntegrationTests"/> instance is valid,
    /// throwing an <see cref="ArgumentException"/> with detailed error messages if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing a list of problems.</exception>
    public static void EnsureValid(this SagaIntegrationTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"SagaIntegrationTests validation failed:{Environment.NewLine}- {
            string.Join($"{Environment.NewLine}- ", problems)
            }",
            nameof(value));
    }

    private static void ValidateServiceProvider(SagaIntegrationTests tests, List<string> problems)
    {
        try
        {
            var createServiceProviderMethod = typeof(SagaIntegrationTests).GetMethod(
                "CreateServiceProvider",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (createServiceProviderMethod is null)
            {
                problems.Add("CreateServiceProvider method not found on SagaIntegrationTests.");
                return;
            }

            var provider = (IServiceProvider?)createServiceProviderMethod.Invoke(tests, null);
            if (provider is null)
            {
                problems.Add("Service provider creation returned null.");
            }
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            problems.Add($"Service provider creation failed: {ex.Message}");
        }
    }

    private static void ValidateTestMethods(SagaIntegrationTests tests, List<string> problems)
    {
        var testMethods = tests.GetType()
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Where(m => m.GetCustomAttributes(typeof(FactAttribute), false).Length > 0)
            .ToList();

        if (testMethods.Count == 0)
        {
            problems.Add("No test methods with [Fact] attribute found.");
        }

        foreach (var method in testMethods)
        {
            ValidateTestMethod(method, problems);
        }
    }

    private static void ValidateTestMethod(System.Reflection.MethodInfo method, List<string> problems)
    {
        if (method.ReturnType != typeof(void) &&
            method.ReturnType != typeof(Task) &&
            method.ReturnType != typeof(ValueTask))
        {
            problems.Add($"Test method '{method.Name}' has invalid return type '{method.ReturnType.Name}'. Expected void, Task, or ValueTask.");
        }

        if (method.GetParameters().Length > 0)
        {
            problems.Add($"Test method '{method.Name}' should not have parameters.");
        }

        if (!method.IsPublic)
        {
            problems.Add($"Test method '{method.Name}' must be public.");
        }
    }
}