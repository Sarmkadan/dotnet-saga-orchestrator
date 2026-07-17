using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;

namespace SagaOrchestrator.Data.Repositories
{
    /// <summary>
    /// Provides validation helpers for <see cref="InMemoryCompensationTransactionRepository"/> and <see cref="CompensationTransaction"/>.
    /// </summary>
    public static class InMemoryCompensationTransactionRepositoryValidation
    {
        /// <summary>
        /// Validates the specified <see cref="InMemoryCompensationTransactionRepository"/> instance.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this InMemoryCompensationTransactionRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = new List<string>();

            // Repository-level validations would go here if there were any state to validate
            // For now, we validate the public contract through the methods

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified <see cref="InMemoryCompensationTransactionRepository"/> instance is valid.
        /// </summary>
        /// <param name="value">The repository instance to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this InMemoryCompensationTransactionRepository value)
        {
            return value?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified <see cref="InMemoryCompensationTransactionRepository"/> instance is valid.
        /// </summary>
        /// <param name="value">The repository instance to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this InMemoryCompensationTransactionRepository value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var problems = value.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"Repository is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }

        /// <summary>
        /// Validates a compensation transaction.
        /// </summary>
        /// <param name="transaction">The transaction to validate.</param>
        /// <returns>A list of validation problems; empty if valid.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="transaction"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this CompensationTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            var problems = new List<string>();

            // Validate required properties
            if (string.IsNullOrWhiteSpace(transaction.Id))
            {
                problems.Add("CompensationTransaction.Id must be a non-empty string.");
            }

            if (string.IsNullOrWhiteSpace(transaction.SagaId))
            {
                problems.Add("CompensationTransaction.SagaId must be a non-empty string.");
            }

            if (string.IsNullOrWhiteSpace(transaction.StepId))
            {
                problems.Add("CompensationTransaction.StepId must be a non-empty string.");
            }

            if (string.IsNullOrWhiteSpace(transaction.StepName))
            {
                problems.Add("CompensationTransaction.StepName must be a non-empty string.");
            }

            if (transaction.Order < 0)
            {
                problems.Add("CompensationTransaction.Order must be a non-negative integer.");
            }

            if (transaction.Status is not (CompensationStatus.Pending or CompensationStatus.InProgress or CompensationStatus.Completed or CompensationStatus.Failed))
            {
                problems.Add($"CompensationTransaction.Status has invalid value: {transaction.Status}.");
            }

            if (string.IsNullOrWhiteSpace(transaction.CompensationUrl))
            {
                problems.Add("CompensationTransaction.CompensationUrl must be a non-empty string.");
            }

            if (transaction.InitiatedAt == default)
            {
                problems.Add("CompensationTransaction.InitiatedAt must be set to a valid DateTime.");
            }

            if (transaction.Status == CompensationStatus.Completed && transaction.CompletedAt == null)
            {
                problems.Add("CompensationTransaction.CompletedAt must be set when Status is Completed.");
            }

            if (transaction.Status == CompensationStatus.Completed && transaction.CompletedAt < transaction.InitiatedAt)
            {
                problems.Add("CompensationTransaction.CompletedAt cannot be earlier than InitiatedAt.");
            }

            if (transaction.Status == CompensationStatus.Failed && transaction.FailedAt == null)
            {
                problems.Add("CompensationTransaction.FailedAt must be set when Status is Failed.");
            }

            if (transaction.Status == CompensationStatus.Failed && transaction.FailedAt < transaction.InitiatedAt)
            {
                problems.Add("CompensationTransaction.FailedAt cannot be earlier than InitiatedAt.");
            }

            if (transaction.Status == CompensationStatus.Failed && string.IsNullOrWhiteSpace(transaction.ErrorMessage))
            {
                problems.Add("CompensationTransaction.ErrorMessage must be set when Status is Failed.");
            }

            if (transaction.RetryCount < 0)
            {
                problems.Add("CompensationTransaction.RetryCount must be a non-negative integer.");
            }

            if (transaction.MaxRetries < 0)
            {
                problems.Add("CompensationTransaction.MaxRetries must be a non-negative integer.");
            }

            if (transaction.TimeoutSeconds <= 0)
            {
                problems.Add("CompensationTransaction.TimeoutSeconds must be a positive integer.");
            }

            // Validate payloads if present
            if (transaction.RequestPayload != null)
            {
                if (transaction.RequestPayload.Count == 0)
                {
                    problems.Add("CompensationTransaction.RequestPayload should not be empty if initialized.");
                }
            }

            if (transaction.ResponsePayload != null)
            {
                if (transaction.ResponsePayload.Count == 0)
                {
                    problems.Add("CompensationTransaction.ResponsePayload should not be empty if initialized.");
                }
            }

            return problems.AsReadOnly();
        }

        /// <summary>
        /// Determines whether the specified compensation transaction is valid.
        /// </summary>
        /// <param name="transaction">The transaction to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="transaction"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this CompensationTransaction transaction)
        {
            return transaction?.Validate().Count == 0;
        }

        /// <summary>
        /// Ensures that the specified compensation transaction is valid.
        /// </summary>
        /// <param name="transaction">The transaction to validate.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="transaction"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="transaction"/> is not valid, containing a list of problems.</exception>
        public static void EnsureValid(this CompensationTransaction transaction)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            var problems = transaction.Validate();
            if (problems.Count > 0)
            {
                throw new ArgumentException(
                    $"CompensationTransaction is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
            }
        }
    }
}