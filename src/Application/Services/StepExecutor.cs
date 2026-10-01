#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;

namespace SagaOrchestrator.Application.Services;

/// <summary>
/// Executes individual saga steps with retry logic, timeout enforcement,
/// and status tracking. Each step execution is atomic and idempotent.
/// </summary>
public class StepExecutor
{
    private readonly ISagaStepRepository _stepRepository;
    private readonly ILogger<StepExecutor> _logger;
    private readonly SagaOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="StepExecutor"/> class.
    /// </summary>
    public StepExecutor(
        ISagaStepRepository stepRepository,
        ILogger<StepExecutor> logger,
        IOptions<SagaOptions> options)
    {
        _stepRepository = stepRepository ?? throw new ArgumentNullException(nameof(stepRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new SagaOptions();
    }

    /// <summary>
    /// Executes a single saga step, updating its status and recording execution time.
    /// Applies retry policy on transient failures.
    /// </summary>
    /// <param name="step">The step to execute.</param>
    /// <param name="cancellationToken">Token to cancel execution.</param>
    /// <returns>True if the step completed successfully; false if it failed after all retries.</returns>
    public async Task<bool> ExecuteStepAsync(SagaStep step, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(step);

        var maxRetries = _options.RetryPolicies.DefaultMaxRetries;
        var attempt = 0;

        while (attempt <= maxRetries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;

            try
            {
                step.Status = SagaStepStatus.Executing;
                step.StartedAt = DateTime.UtcNow;
                await _stepRepository.UpdateAsync(step);

                _logger.LogDebug("Executing step {StepId} (attempt {Attempt}/{MaxRetries})",
                    step.Id, attempt, maxRetries + 1);

                var timeout = TimeSpan.FromSeconds(_options.TimeoutPolicies.DefaultStepTimeoutSeconds);
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeout);

                var sw = Stopwatch.StartNew();
                await ExecuteStepActionAsync(step, timeoutCts.Token);
                sw.Stop();

                step.Status = SagaStepStatus.Completed;
                step.CompletedAt = DateTime.UtcNow;
                await _stepRepository.UpdateAsync(step);

                _logger.LogInformation("Step {StepId} completed in {Duration}ms", step.Id, sw.ElapsedMilliseconds);
                return true;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Step {StepId} timed out on attempt {Attempt}", step.Id, attempt);
                step.Status = SagaStepStatus.TimedOut;
                await _stepRepository.UpdateAsync(step);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Step {StepId} failed on attempt {Attempt}", step.Id, attempt);

                if (attempt <= maxRetries)
                {
                    step.Status = SagaStepStatus.WaitingForRetry;
                    await _stepRepository.UpdateAsync(step);
                    var delay = CalculateRetryDelay(attempt);
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    step.Status = SagaStepStatus.Failed;
                    step.ErrorMessage = ex.Message;
                    await _stepRepository.UpdateAsync(step);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Validates whether a step is in a valid state to be executed.
    /// </summary>
    /// <param name="step">The step to validate.</param>
    /// <returns>True if the step can be executed.</returns>
    public bool CanExecute(SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        return step.Status is SagaStepStatus.Pending or SagaStepStatus.WaitingForRetry;
    }

    /// <summary>
    /// Marks a step as skipped without executing it.
    /// </summary>
    /// <param name="step">The step to skip.</param>
    public async Task SkipStepAsync(SagaStep step)
    {
        ArgumentNullException.ThrowIfNull(step);
        step.Status = SagaStepStatus.Skipped;
        step.CompletedAt = DateTime.UtcNow;
        await _stepRepository.UpdateAsync(step);
        _logger.LogInformation("Step {StepId} skipped", step.Id);
    }

    /// <summary>
    /// Calculates exponential backoff delay for retry attempts.
    /// </summary>
    /// <param name="attempt">Current attempt number (1-based).</param>
    /// <returns>The delay before the next retry.</returns>
    public TimeSpan CalculateRetryDelay(int attempt)
    {
        var baseDelayMs = _options.RetryPolicies.DefaultRetryDelayMs;
        var delayMs = Math.Min(baseDelayMs * Math.Pow(2, attempt - 1), _options.RetryPolicies.MaxBackoffDelayMs);
        return TimeSpan.FromMilliseconds(delayMs);
    }

    private Task ExecuteStepActionAsync(SagaStep step, CancellationToken ct)
    {
        // Placeholder: in a real implementation this would invoke the step's
        // registered action delegate or send a command to the target service.
        return Task.CompletedTask;
    }
}
