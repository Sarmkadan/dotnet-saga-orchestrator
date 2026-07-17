#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace SagaOrchestrator.Application.DTOs;

/// <summary>
/// Extension methods for <see cref="SagaCommandResult"/> and <see cref="SagaCommandResult{T}"/>
/// providing common operations and conversions.
/// </summary>
public static class SagaCommandResultExtensions
{
    private const string DefaultSuccessMessage = "Operation completed successfully";
    /// <summary>
    /// Converts a <see cref="SagaCommandResult"/> to a <see cref="SagaCommandResult{T}"/> with the specified data type.
    /// </summary>
    /// <typeparam name="T">The type of data to include in the typed result.</typeparam>
    /// <param name="result">The source result to convert.</param>
    /// <param name="data">The data to include in the typed result.</param>
    /// <returns>A new typed result with the same success state and errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static SagaCommandResult<T> ToTypedResult<T>(this SagaCommandResult result, T? data = default)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new()
        {
            Success = result.Success,
            Message = result.Message,
            Data = data,
            Errors = result.Errors,
            Timestamp = result.Timestamp,
            RequestId = result.RequestId
        };
    }

    /// <summary>
    /// Converts a <see cref="SagaCommandResult{T}"/> to a <see cref="SagaCommandResult"/>
    /// by extracting the data as an anonymous object.
    /// </summary>
    /// <param name="result">The source typed result to convert.</param>
    /// <param name="dataSelector">Optional selector to transform the data before conversion.</param>
    /// <returns>A new untyped result with the same success state and errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static SagaCommandResult ToUntypedResult(this SagaCommandResult<object?> result, Func<object?, object?>? dataSelector = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        var data = dataSelector is null ? result.Data : dataSelector(result.Data);

        return new()
        {
            Success = result.Success,
            Message = result.Message,
            Data = data,
            Errors = result.Errors,
            Timestamp = result.Timestamp,
            RequestId = result.RequestId
        };
    }

    /// <summary>
    /// Adds an error message to the result's errors collection if the result is not successful.
    /// </summary>
    /// <param name="result">The result to potentially add an error to.</param>
    /// <param name="errorMessage">The error message to add.</param>
    /// <returns>The same result instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static SagaCommandResult WithError(this SagaCommandResult result, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        if (!result.Success)
        {
            result.Errors.Add(errorMessage);
        }

        return result;
    }

    /// <summary>
    /// Adds an error message to the result's errors collection if the result is not successful.
    /// </summary>
    /// <typeparam name="T">The type of data in the result.</typeparam>
    /// <param name="result">The result to potentially add an error to.</param>
    /// <param name="errorMessage">The error message to add.</param>
    /// <returns>The same result instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static SagaCommandResult<T> WithError<T>(this SagaCommandResult<T> result, string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        if (!result.Success)
        {
            result.Errors.Add(errorMessage);
        }

        return result;
    }

    /// <summary>
    /// Creates a paginated result from a successful typed result.
    /// </summary>
    /// <typeparam name="T">The type of items in the paginated result.</typeparam>
    /// <param name="result">The source result containing data to paginate.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A paginated result containing the data from the source result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the result is not successful or data is null.</exception>
    public static PaginatedResult<T> ToPaginatedResult<T>(this SagaCommandResult<IEnumerable<T>> result, int pageNumber, int pageSize)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(result.Data);

        if (!result.Success)
        {
            throw new ArgumentException("Cannot paginate a failed result.", nameof(result));
        }

        var items = result.Data.ToList();
        var totalCount = items.Count;

        return PaginatedResult<T>.Create(
            items.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            pageNumber,
            pageSize,
            totalCount
        );
    }

    /// <summary>
    /// Combines multiple results into a single result using logical AND operation.
    /// The combined result is successful only if all input results are successful.
    /// </summary>
    /// <param name="results">The results to combine.</param>
    /// <returns>A new result representing the combination of all input results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="results"/> is null.</exception>
    public static SagaCommandResult Combine(this IEnumerable<SagaCommandResult> results)
{
    ArgumentNullException.ThrowIfNull(results);

    var resultList = results.ToList();
    return resultList.Count switch
    {
        0 => SagaCommandResult.FailureResult("Cannot combine empty result collection."),
        _ => resultList.All(r => r.Success)
            ? SagaCommandResult.SuccessResult(DefaultSuccessMessage)
            : SagaCommandResult.FailureResult("One or more operations failed", resultList.SelectMany(r => r.Errors).ToArray())
    };
}

    /// <summary>
    /// Creates a failure result with the same error collection as the source result.
    /// </summary>
    /// <param name="result">The source result to create a failure from.</param>
    /// <param name="newMessage">Optional new message for the failure result.</param>
    /// <returns>A new failure result with the same errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static SagaCommandResult AsFailure(this SagaCommandResult result, string? newMessage = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return SagaCommandResult.FailureResult(
            newMessage ?? result.Message,
            result.Errors.ToArray()
        );
    }

    /// <summary>
    /// Creates a failure result with the same error collection as the source typed result.
    /// </summary>
    /// <typeparam name="T">The type of data in the result.</typeparam>
    /// <param name="result">The source result to create a failure from.</param>
    /// <param name="newMessage">Optional new message for the failure result.</param>
    /// <returns>A new failure result with the same errors.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static SagaCommandResult<T> AsFailure<T>(this SagaCommandResult<T> result, string? newMessage = null)
    {
        ArgumentNullException.ThrowIfNull(result);

        return SagaCommandResult<T>.FailureResult(
            newMessage ?? result.Message,
            result.Errors.ToArray()
        );
    }

    /// <summary>
    /// Determines whether the result contains any of the specified error messages.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <param name="errorMessages">The error messages to search for.</param>
    /// <returns>True if any error message matches; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static bool HasError(this SagaCommandResult result, params string[] errorMessages)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(errorMessages);

        return result.Errors.Any(error => errorMessages.Contains(error, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether the result contains any of the specified error messages.
    /// </summary>
    /// <typeparam name="T">The type of data in the result.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <param name="errorMessages">The error messages to search for.</param>
    /// <returns>True if any error message matches; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="result"/> is null.</exception>
    public static bool HasError<T>(this SagaCommandResult<T> result, params string[] errorMessages)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(errorMessages);

        return result.Errors.Any(error => errorMessages.Contains(error, StringComparer.OrdinalIgnoreCase));
    }
}