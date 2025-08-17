// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace SagaOrchestrator.Core.Constants;

/// <summary>
/// Contains configuration constants for saga orchestration.
/// </summary>
public static class SagaConstants
{
    /// <summary>
    /// Default timeout for saga execution in seconds
    /// </summary>
    public const int DefaultSagaTimeoutSeconds = 300;

    /// <summary>
    /// Default timeout for individual saga steps in seconds
    /// </summary>
    public const int DefaultStepTimeoutSeconds = 30;

    /// <summary>
    /// Default maximum number of retries for saga steps
    /// </summary>
    public const int DefaultMaxRetries = 3;

    /// <summary>
    /// Default maximum number of retries for compensation transactions
    /// </summary>
    public const int DefaultCompensationMaxRetries = 3;

    /// <summary>
    /// Default retry delay in milliseconds
    /// </summary>
    public const int DefaultRetryDelayMs = 1000;

    /// <summary>
    /// Default backoff multiplier for exponential retry delays
    /// </summary>
    public const double DefaultBackoffMultiplier = 2.0;

    /// <summary>
    /// Default maximum backoff delay in milliseconds
    /// </summary>
    public const int DefaultMaxBackoffDelayMs = 60000;

    /// <summary>
    /// Default polling interval for saga status checks in milliseconds
    /// </summary>
    public const int DefaultPollingIntervalMs = 5000;

    /// <summary>
    /// Maximum polling interval for saga status checks in milliseconds
    /// </summary>
    public const int MaxPollingIntervalMs = 30000;

    /// <summary>
    /// Header key for correlation ID propagation
    /// </summary>
    public const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Header key for saga ID propagation
    /// </summary>
    public const string SagaIdHeader = "X-Saga-ID";

    /// <summary>
    /// Header key for request trace ID
    /// </summary>
    public const string TraceIdHeader = "X-Trace-ID";

    /// <summary>
    /// Content type for JSON requests
    /// </summary>
    public const string JsonContentType = "application/json";

    /// <summary>
    /// Maximum concurrent saga executions
    /// </summary>
    public const int MaxConcurrentSagas = 100;

    /// <summary>
    /// Maximum number of steps per saga definition
    /// </summary>
    public const int MaxStepsPerSaga = 50;

    /// <summary>
    /// Minimum step order value
    /// </summary>
    public const int MinStepOrder = 1;

    /// <summary>
    /// Default persistence storage location
    /// </summary>
    public const string DefaultStoragePath = "./data/sagas";

    /// <summary>
    /// Cleanup interval for completed sagas in seconds
    /// </summary>
    public const int CleanupIntervalSeconds = 3600;

    /// <summary>
    /// Retention period for completed saga records in days
    /// </summary>
    public const int SagaRetentionDays = 30;
}
