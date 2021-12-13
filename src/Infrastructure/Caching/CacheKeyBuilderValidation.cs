using System;
using System.Collections.Generic;
using System.Text;

namespace SagaOrchestrator.Infrastructure.Caching
{
    /// <summary>
    /// Validation helpers for CacheKeyBuilder static methods.
    /// Provides validation for all parameters passed to CacheKeyBuilder methods.
    /// </summary>
    public static class CacheKeyBuilderValidation
    {
        /// <summary>
        /// Validates sagaId parameter for BuildSagaKey method.
        /// </summary>
        public static IReadOnlyList<string> ValidateSagaId(string sagaId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(sagaId))
            {
                errors.Add("sagaId cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates definitionId and name parameters for BuildDefinitionKey and BuildDefinitionByNameKey methods.
        /// </summary>
        public static IReadOnlyList<string> ValidateDefinition(string definitionId, string name)
        {
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
        /// Validates status parameter for BuildSagasByStatusKey method.
        /// </summary>
        public static IReadOnlyList<string> ValidateStatus(string status)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(status))
            {
                errors.Add("status cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates serviceName parameter for BuildServiceKey method.
        /// </summary>
        public static IReadOnlyList<string> ValidateServiceName(string serviceName)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                errors.Add("serviceName cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates identifier and resource parameters for BuildRateLimitKey method.
        /// </summary>
        public static IReadOnlyList<string> ValidateRateLimit(string identifier, string resource)
        {
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
        /// Validates userId parameter for BuildUserCacheKey method.
        /// </summary>
        public static IReadOnlyList<string> ValidateUserId(string userId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(userId))
            {
                errors.Add("userId cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates sessionId parameter for BuildSessionKey method.
        /// </summary>
        public static IReadOnlyList<string> ValidateSessionId(string sessionId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                errors.Add("sessionId cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates webhookId parameter for BuildWebhookKey method.
        /// </summary>
        public static IReadOnlyList<string> ValidateWebhookId(string webhookId)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(webhookId))
            {
                errors.Add("webhookId cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Validates key parameter for IsSagaKey, IsDefinitionKey, and ExtractIdFromKey methods.
        /// </summary>
        public static IReadOnlyList<string> ValidateCacheKey(string key)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(key))
            {
                errors.Add("key cannot be null or whitespace");
            }

            return errors;
        }

        /// <summary>
        /// Checks if sagaId is valid for BuildSagaKey method.
        /// </summary>
        public static bool IsValidSagaId(string sagaId)
        {
            return ValidateSagaId(sagaId).Count == 0;
        }

        /// <summary>
        /// Checks if definitionId and name are valid for BuildDefinitionKey and BuildDefinitionByNameKey methods.
        /// </summary>
        public static bool IsValidDefinition(string definitionId, string name)
        {
            return ValidateDefinition(definitionId, name).Count == 0;
        }

        /// <summary>
        /// Checks if status is valid for BuildSagasByStatusKey method.
        /// </summary>
        public static bool IsValidStatus(string status)
        {
            return ValidateStatus(status).Count == 0;
        }

        /// <summary>
        /// Checks if serviceName is valid for BuildServiceKey method.
        /// </summary>
        public static bool IsValidServiceName(string serviceName)
        {
            return ValidateServiceName(serviceName).Count == 0;
        }

        /// <summary>
        /// Checks if identifier and resource are valid for BuildRateLimitKey method.
        /// </summary>
        public static bool IsValidRateLimit(string identifier, string resource)
        {
            return ValidateRateLimit(identifier, resource).Count == 0;
        }

        /// <summary>
        /// Checks if userId is valid for BuildUserCacheKey method.
        /// </summary>
        public static bool IsValidUserId(string userId)
        {
            return ValidateUserId(userId).Count == 0;
        }

        /// <summary>
        /// Checks if sessionId is valid for BuildSessionKey method.
        /// </summary>
        public static bool IsValidSessionId(string sessionId)
        {
            return ValidateSessionId(sessionId).Count == 0;
        }

        /// <summary>
        /// Checks if webhookId is valid for BuildWebhookKey method.
        /// </summary>
        public static bool IsValidWebhookId(string webhookId)
        {
            return ValidateWebhookId(webhookId).Count == 0;
        }

        /// <summary>
        /// Checks if key is valid for IsSagaKey, IsDefinitionKey, and ExtractIdFromKey methods.
        /// </summary>
        public static bool IsValidCacheKey(string key)
        {
            return ValidateCacheKey(key).Count == 0;
        }

        /// <summary>
        /// Ensures sagaId is valid for BuildSagaKey method, throwing an exception if not.
        /// </summary>
        public static void EnsureValidSagaId(string sagaId)
        {
            var errors = ValidateSagaId(sagaId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("sagaId validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures definitionId and name are valid for BuildDefinitionKey and BuildDefinitionByNameKey methods, throwing an exception if not.
        /// </summary>
        public static void EnsureValidDefinition(string definitionId, string name)
        {
            var errors = ValidateDefinition(definitionId, name);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("definitionId and name validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures status is valid for BuildSagasByStatusKey method, throwing an exception if not.
        /// </summary>
        public static void EnsureValidStatus(string status)
        {
            var errors = ValidateStatus(status);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("status validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures serviceName is valid for BuildServiceKey method, throwing an exception if not.
        /// </summary>
        public static void EnsureValidServiceName(string serviceName)
        {
            var errors = ValidateServiceName(serviceName);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("serviceName validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures identifier and resource are valid for BuildRateLimitKey method, throwing an exception if not.
        /// </summary>
        public static void EnsureValidRateLimit(string identifier, string resource)
        {
            var errors = ValidateRateLimit(identifier, resource);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("identifier and resource validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures userId is valid for BuildUserCacheKey method, throwing an exception if not.
        /// </summary>
        public static void EnsureValidUserId(string userId)
        {
            var errors = ValidateUserId(userId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("userId validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures sessionId is valid for BuildSessionKey method, throwing an exception if not.
        /// </summary>
        public static void EnsureValidSessionId(string sessionId)
        {
            var errors = ValidateSessionId(sessionId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("sessionId validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures webhookId is valid for BuildWebhookKey method, throwing an exception if not.
        /// </summary>
        public static void EnsureValidWebhookId(string webhookId)
        {
            var errors = ValidateWebhookId(webhookId);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("webhookId validation failed", errors));
            }
        }

        /// <summary>
        /// Ensures key is valid for IsSagaKey, IsDefinitionKey, and ExtractIdFromKey methods, throwing an exception if not.
        /// </summary>
        public static void EnsureValidCacheKey(string key)
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