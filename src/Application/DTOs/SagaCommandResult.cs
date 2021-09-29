#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json.Serialization;

namespace SagaOrchestrator.Application.DTOs;

/// <summary>
/// Standardized command result DTO for all saga operations.
/// Provides consistent response format with metadata and error handling.
/// </summary>
public class SagaCommandResult
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    public static SagaCommandResult SuccessResult(string message = "Operation completed successfully", object? data = null)
    {
        return new SagaCommandResult
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    public static SagaCommandResult FailureResult(string message, params string[] errors)
    {
        return new SagaCommandResult
        {
            Success = false,
            Message = message,
            Errors = errors.ToList(),
            Timestamp = DateTime.UtcNow
        };
    }

    public static SagaCommandResult ExceptionResult(Exception ex)
    {
        return new SagaCommandResult
        {
            Success = false,
            Message = "An error occurred during the operation",
            Errors = new() { ex.Message },
            Timestamp = DateTime.UtcNow
        };
    }
}

public class SagaCommandResult<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    public static SagaCommandResult<T> SuccessResult(T data, string message = "Operation completed successfully")
    {
        return new SagaCommandResult<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }

    public static SagaCommandResult<T> FailureResult(string message, params string[] errors)
    {
        return new SagaCommandResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors.ToList(),
            Timestamp = DateTime.UtcNow
        };
    }

    public static SagaCommandResult<T> ExceptionResult(Exception ex)
    {
        return new SagaCommandResult<T>
        {
            Success = false,
            Message = "An error occurred during the operation",
            Errors = new() { ex.Message },
            Timestamp = DateTime.UtcNow
        };
    }
}

public class PaginatedResult<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => PageNumber < TotalPages;

    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => PageNumber > 1;

    public static PaginatedResult<T> Create(List<T> items, int pageNumber, int pageSize, int totalCount)
    {
        return new PaginatedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

public class HealthCheckResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = "healthy";

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("version")]
    public string Version { get; set; } = "2.0.0";

    [JsonPropertyName("uptime")]
    public TimeSpan Uptime { get; set; }

    [JsonPropertyName("activeSagas")]
    public int ActiveSagas { get; set; }

    [JsonPropertyName("totalSagas")]
    public int TotalSagas { get; set; }
}
