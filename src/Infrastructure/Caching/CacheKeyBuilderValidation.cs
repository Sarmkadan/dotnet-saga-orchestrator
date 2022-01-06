using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SagaOrchestrator.Infrastructure.Caching
{
    /// <summary>
    /// Validation helpers for <see cref="CacheKeyBuilder"/> static methods.
    /// Provides validation for all parameters passed to <see cref="CacheKeyBuilder"/> methods.
    /// </summary>
    public sealed class CacheKeyBuilderValidation
    {
        /// <summary>
        /// Validates <paramref name="sagaId"/> parameter for <see cref="CacheKeyBuilder.BuildSagaKey"/> method.
        /// </summary>
        /// <param name="sagaId">The saga identifier to validate.</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sagaId"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> ValidateSagaId([NotNullWhen(false)] string? sagaId)
        {
            ArgumentNullException.ThrowIfNull(sagaId);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(sagaId))
            {
                errors.Add("sagaId cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates <paramref name="definitionId"/> and <paramref name="name"/> parameters for
        /// <see cref="CacheKeyBuilder.BuildDefinitionKey"/> and <see cref="CacheKeyBuilder.BuildDefinitionByNameKey"/> methods.
        /// </summary>
        /// <param name="definitionId">The definition identifier to validate.</param>
        /// <param name="name">The name to validate.</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="definitionId"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> ValidateDefinition([NotNullWhen(false)] string? definitionId, [NotNullWhen(false)] string? name)
        {
            ArgumentNullException.ThrowIfNull(definitionId);
            ArgumentNullException.ThrowIfNull(name);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(definitionId))
            {
                errors.Add("definitionId cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add("name cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates <paramref name="status"/> parameter for <see cref="CacheKeyBuilder.BuildSagasByStatusKey"/> method.
        /// </summary>
        /// <param name="status">The status to validate.</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="status"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> ValidateStatus([NotNullWhen(false)] string? status)
        {
            ArgumentNullException.ThrowIfNull(status);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(status))
            {
                errors.Add("status cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates <paramref name="serviceName"/> parameter for <see cref="CacheKeyBuilder.BuildServiceKey"/> method.
        /// </summary>
        /// <param name="serviceName">The service name to validate.</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serviceName"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> ValidateServiceName([NotNullWhen(false)] string? serviceName)
        {
            ArgumentNullException.ThrowIfNull(serviceName);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                errors.Add("serviceName cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates <paramref name="identifier"/> and <paramref name="resource"/> parameters for
        /// <see cref="CacheKeyBuilder.BuildRateLimitKey"/> method.
        /// </summary>
        /// <param name="identifier">The identifier to validate.</param>
        /// <param name="resource">The resource to validate.</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> or <paramref name="resource"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> ValidateRateLimit([NotNullWhen(false)] string? identifier, [NotNullWhen(false)] string? resource)
        {
            ArgumentNullException.ThrowIfNull(identifier);
            ArgumentNullException.ThrowIfNull(resource);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(identifier))
            {
                errors.Add("identifier cannot be null or whitespace");
            }

            if (string.IsNullOrWhiteSpace(resource))
            {
                errors.Add("resource cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates <paramref name="userId"/> parameter for <see cref="CacheKeyBuilder.BuildUserCacheKey"/> method.
        /// </summary>
        /// <param name="userId">The user identifier to validate.</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="userId"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> ValidateUserId([NotNullWhen(false)] string? userId)
        {
            ArgumentNullException.ThrowIfNull(userId);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(userId))
            {
                errors.Add("userId cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates <paramref name="sessionId"/> parameter for <see cref="CacheKeyBuilder.BuildSessionKey"/> method.
        /// </summary>
        /// <param name="sessionId">The session identifier to validate.</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sessionId"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> ValidateSessionId([NotNullWhen(false)] string? sessionId)
        {
            ArgumentNullException.ThrowIfNull(sessionId);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                errors.Add("sessionId cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates <paramref name="webhookId"/> parameter for <see cref="CacheKeyBuilder.BuildWebhookKey"/> method.
        /// </summary>
        /// <param name="webhookId">The webhook identifier to validate.</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="webhookId"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> ValidateWebhookId([NotNullWhen(false)] string? webhookId)
        {
            ArgumentNullException.ThrowIfNull(webhookId);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(webhookId))
            {
                errors.Add("webhookId cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates <paramref name="key"/> parameter for <see cref="CacheKeyBuilder.IsSagaKey"/>,
        /// <see cref="CacheKeyBuilder.IsDefinitionKey"/>, and <see cref="CacheKeyBuilder.ExtractIdFromKey"/> methods.
        /// </summary>
        /// <param name="key">The cache key to validate.</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> ValidateCacheKey([NotNullWhen(false)] string? key)
        {
            ArgumentNullException.ThrowIfNull(key);

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(key))
            {
                errors.Add("key cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Checks if <paramref name="sagaId"/> is valid for <see cref="CacheKeyBuilder.BuildSagaKey"/> method.
        /// </summary>
        /// <param name="sagaId">The saga identifier to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sagaId"/> is <see langword="null"/>.</exception>
        public static bool IsValidSagaId([NotNullWhen(true)] string? sagaId)
        {
            return ValidateSagaId(sagaId).Count == 0;
        }

        /// <summary>
        /// Checks if <paramref name="definitionId"/> and <paramref name="name"/> are valid for
        /// <see cref="CacheKeyBuilder.BuildDefinitionKey"/> and <see cref="CacheKeyBuilder.BuildDefinitionByNameKey"/> methods.
        /// </summary>
        /// <param name="definitionId">The definition identifier to check.</param>
        /// <param name="name">The name to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="definitionId"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
        public static bool IsValidDefinition([NotNullWhen(true)] string? definitionId, [NotNullWhen(true)] string? name)
        {
            return ValidateDefinition(definitionId, name).Count == 0;
        }

        /// <summary>
        /// Checks if <paramref name="status"/> is valid for <see cref="CacheKeyBuilder.BuildSagasByStatusKey"/> method.
        /// </summary>
        /// <param name="status">The status to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="status"/> is <see langword="null"/>.</exception>
        public static bool IsValidStatus([NotNullWhen(true)] string? status)
        {
            return ValidateStatus(status).Count == 0;
        }

        /// <summary>
        /// Checks if <paramref name="serviceName"/> is valid for <see cref="CacheKeyBuilder.BuildServiceKey"/> method.
        /// </summary>
        /// <param name="serviceName">The service name to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="serviceName"/> is <see langword="null"/>.</exception>
        public static bool IsValidServiceName([NotNullWhen(true)] string? serviceName)
        {
            return ValidateServiceName(serviceName).Count == 0;
        }

        /// <summary>
        /// Checks if <paramref name="identifier"/> and <paramref name="resource"/> are valid for
        /// <see cref="CacheKeyBuilder.BuildRateLimitKey"/> method.
        /// </summary>
        /// <param name="identifier">The identifier to check.</param>
        /// <param name="resource">The resource to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> or <paramref name="resource"/> is <see langword="null"/>.</exception>
        public static bool IsValidRateLimit([NotNullWhen(true)] string? identifier, [NotNullWhen(true)] string? resource)
        {
            return ValidateRateLimit(identifier, resource).Count == 0;
        }

        /// <summary>
        /// Checks if <paramref name="userId"/> is valid for <see cref="CacheKeyBuilder.BuildUserCacheKey"/> method.
        /// </summary>
        /// <param name="userId">The user identifier to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="userId"/> is <see langword="null"/>.</exception>
        public static bool IsValidUserId([NotNullWhen(true)] string? userId)
        {
            return ValidateUserId(userId).Count == 0;
        }

        /// <summary>
        /// Checks if <paramref name="sessionId"/> is valid for <see cref="CacheKeyBuilder.BuildSessionKey"/> method.
        /// </summary>
        /// <param name="sessionId">The session identifier to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sessionId"/> is <see langword="null"/>.</exception>
        public static bool IsValidSessionId([NotNullWhen(true)] string? sessionId)
        {
            return ValidateSessionId(sessionId).Count == 0;
        }

        /// <summary>
        /// Checks if <paramref name="webhookId"/> is valid for <see cref="CacheKeyBuilder.BuildWebhookKey"/> method.
        /// </summary>
        /// <param name="webhookId">The webhook identifier to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="webhookId"/> is <see langword="null"/>.</exception>
        public static bool IsValidWebhookId([NotNullWhen(true)] string? webhookId)
        {
            return ValidateWebhookId(webhookId).Count == 0;
        }

        /// <summary>
        /// Checks if <paramref name="key"/> is valid for <see cref="CacheKeyBuilder.IsSagaKey"/>,
        /// <see cref="CacheKeyBuilder.IsDefinitionKey"/>, and <see cref="CacheKeyBuilder.ExtractIdFromKey"/> methods.
        /// </summary>
        /// <param name="key">The cache key to check.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public static bool IsValidCacheKey([NotNullWhen(true)] string? key)
        {
            return ValidateCacheKey(key).Count == 0;
        }

        /// <summary>
        /// Ensures <paramref name="sagaId"/> is valid for <see cref="CacheKeyBuilder.BuildSagaKey"/> method,
        /// throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="sagaId">The saga identifier to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sagaId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="sagaId"/> is null or whitespace.</exception>
        public static void EnsureValidSagaId([NotNull] string sagaId)
        {
            var errors = ValidateSagaId(sagaId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("sagaId validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures <paramref name="definitionId"/> and <paramref name="name"/> are valid for
        /// <see cref="CacheKeyBuilder.BuildDefinitionKey"/> and <see cref="CacheKeyBuilder.BuildDefinitionByNameKey"/> methods,
        /// throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="definitionId">The definition identifier to validate.</param>
        /// <param name="name">The name to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="definitionId"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="definitionId"/> or <paramref name="name"/> is null or whitespace.</exception>
        public static void EnsureValidDefinition([NotNull] string definitionId, [NotNull] string name)
        {
            var errors = ValidateDefinition(definitionId, name);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("definitionId and name validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures <paramref name="status"/> is valid for <see cref="CacheKeyBuilder.BuildSagasByStatusKey"/> method,
        /// throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="status">The status to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="status"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="status"/> is null or whitespace.</exception>
        public static void EnsureValidStatus([NotNull] string status)
        {
            var errors = ValidateStatus(status);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("status validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures <paramref name="serviceName"/> is valid for <see cref="CacheKeyBuilder.BuildServiceKey"/> method,
        /// throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="serviceName">The service name to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="serviceName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="serviceName"/> is null or whitespace.</exception>
        public static void EnsureValidServiceName([NotNull] string serviceName)
        {
            var errors = ValidateServiceName(serviceName);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("serviceName validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures <paramref name="identifier"/> and <paramref name="resource"/> are valid for
        /// <see cref="CacheKeyBuilder.BuildRateLimitKey"/> method, throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="identifier">The identifier to validate.</param>
        /// <param name="resource">The resource to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> or <paramref name="resource"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="identifier"/> or <paramref name="resource"/> is null or whitespace.</exception>
        public static void EnsureValidRateLimit([NotNull] string identifier, [NotNull] string resource)
        {
            var errors = ValidateRateLimit(identifier, resource);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("identifier and resource validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures <paramref name="userId"/> is valid for <see cref="CacheKeyBuilder.BuildUserCacheKey"/> method,
        /// throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="userId">The user identifier to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="userId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="userId"/> is null or whitespace.</exception>
        public static void EnsureValidUserId([NotNull] string userId)
        {
            var errors = ValidateUserId(userId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("userId validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures <paramref name="sessionId"/> is valid for <see cref="CacheKeyBuilder.BuildSessionKey"/> method,
        /// throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="sessionId">The session identifier to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sessionId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="sessionId"/> is null or whitespace.</exception>
        public static void EnsureValidSessionId([NotNull] string sessionId)
        {
            var errors = ValidateSessionId(sessionId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("sessionId validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures <paramref name="webhookId"/> is valid for <see cref="CacheKeyBuilder.BuildWebhookKey"/> method,
        /// throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="webhookId">The webhook identifier to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="webhookId"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="webhookId"/> is null or whitespace.</exception>
        public static void EnsureValidWebhookId([NotNull] string webhookId)
        {
            var errors = ValidateWebhookId(webhookId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("webhookId validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures <paramref name="key"/> is valid for <see cref="CacheKeyBuilder.IsSagaKey"/>,
        /// <see cref="CacheKeyBuilder.IsDefinitionKey"/>, and <see cref="CacheKeyBuilder.ExtractIdFromKey"/> methods,
        /// throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="key">The cache key to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="key"/> is null or whitespace.</exception>
        public static void EnsureValidCacheKey([NotNull] string key)
        {
            var errors = ValidateCacheKey(key);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("key validation failed", errors));
            }
        }

        private static string FormatErrors(string context, IReadOnlyList<string> errors)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine($"{context}:");
            foreach (var error in errors)
            {
                errorMessage.AppendLine($"- {error}");
            }
            return errorMessage.ToString();
        }
    }
}