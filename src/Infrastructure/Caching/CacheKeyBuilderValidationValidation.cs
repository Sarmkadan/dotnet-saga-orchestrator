using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SagaOrchestrator.Infrastructure.Caching
{
    /// <summary>
    /// Validation helpers for the <see cref="CacheKeyBuilderValidation"/> type.
    /// Provides comprehensive validation for <see cref="CacheKeyBuilderValidation"/> static methods.
    /// </summary>
    public static class CacheKeyBuilderValidationValidation
    {
        /// <summary>
        /// Validates the <see cref="CacheKeyBuilderValidation"/> static methods.
        /// </summary>
        /// <param name="value">The <see cref="CacheKeyBuilderValidation"/> type reference (unused, for API consistency).</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this CacheKeyBuilderValidation? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Validate that all static methods exist and are callable by invoking them with null
            // This ensures the methods exist and have the correct signatures
            try
            {
                // Test Validate methods with null inputs
                var validateSagaIdErrors = CacheKeyBuilderValidation.ValidateSagaId(null);
                if (validateSagaIdErrors.Count == 0)
                {
                    errors.Add("ValidateSagaId returned empty errors for null input - method signature may be incorrect");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateSagaId method call failed: {ex.Message}");
            }

            try
            {
                var validateDefinitionErrors = CacheKeyBuilderValidation.ValidateDefinition(null, null);
                if (validateDefinitionErrors.Count == 0)
                {
                    errors.Add("ValidateDefinition returned empty errors for null inputs - method signature may be incorrect");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateDefinition method call failed: {ex.Message}");
            }

            try
            {
                var validateStatusErrors = CacheKeyBuilderValidation.ValidateStatus(null);
                if (validateStatusErrors.Count == 0)
                {
                    errors.Add("ValidateStatus returned empty errors for null input - method signature may be incorrect");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateStatus method call failed: {ex.Message}");
            }

            try
            {
                var validateServiceNameErrors = CacheKeyBuilderValidation.ValidateServiceName(null);
                if (validateServiceNameErrors.Count == 0)
                {
                    errors.Add("ValidateServiceName returned empty errors for null input - method signature may be incorrect");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateServiceName method call failed: {ex.Message}");
            }

            try
            {
                var validateRateLimitErrors = CacheKeyBuilderValidation.ValidateRateLimit(null, null);
                if (validateRateLimitErrors.Count == 0)
                {
                    errors.Add("ValidateRateLimit returned empty errors for null inputs - method signature may be incorrect");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateRateLimit method call failed: {ex.Message}");
            }

            try
            {
                var validateUserIdErrors = CacheKeyBuilderValidation.ValidateUserId(null);
                if (validateUserIdErrors.Count == 0)
                {
                    errors.Add("ValidateUserId returned empty errors for null input - method signature may be incorrect");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateUserId method call failed: {ex.Message}");
            }

            try
            {
                var validateSessionIdErrors = CacheKeyBuilderValidation.ValidateSessionId(null);
                if (validateSessionIdErrors.Count == 0)
                {
                    errors.Add("ValidateSessionId returned empty errors for null input - method signature may be incorrect");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateSessionId method call failed: {ex.Message}");
            }

            try
            {
                var validateWebhookIdErrors = CacheKeyBuilderValidation.ValidateWebhookId(null);
                if (validateWebhookIdErrors.Count == 0)
                {
                    errors.Add("ValidateWebhookId returned empty errors for null input - method signature may be incorrect");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateWebhookId method call failed: {ex.Message}");
            }

            try
            {
                var validateCacheKeyErrors = CacheKeyBuilderValidation.ValidateCacheKey(null);
                if (validateCacheKeyErrors.Count == 0)
                {
                    errors.Add("ValidateCacheKey returned empty errors for null input - method signature may be incorrect");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateCacheKey method call failed: {ex.Message}");
            }

            // Test IsValid methods
            try
            {
                var isValidSagaId = CacheKeyBuilderValidation.IsValidSagaId(null);
            }
            catch (Exception ex)
            {
                errors.Add($"IsValidSagaId method call failed: {ex.Message}");
            }

            try
            {
                var isValidDefinition = CacheKeyBuilderValidation.IsValidDefinition(null, null);
            }
            catch (Exception ex)
            {
                errors.Add($"IsValidDefinition method call failed: {ex.Message}");
            }

            try
            {
                var isValidStatus = CacheKeyBuilderValidation.IsValidStatus(null);
            }
            catch (Exception ex)
            {
                errors.Add($"IsValidStatus method call failed: {ex.Message}");
            }

            try
            {
                var isValidServiceName = CacheKeyBuilderValidation.IsValidServiceName(null);
            }
            catch (Exception ex)
            {
                errors.Add($"IsValidServiceName method call failed: {ex.Message}");
            }

            try
            {
                var isValidRateLimit = CacheKeyBuilderValidation.IsValidRateLimit(null, null);
            }
            catch (Exception ex)
            {
                errors.Add($"IsValidRateLimit method call failed: {ex.Message}");
            }

            try
            {
                var isValidUserId = CacheKeyBuilderValidation.IsValidUserId(null);
            }
            catch (Exception ex)
            {
                errors.Add($"IsValidUserId method call failed: {ex.Message}");
            }

            try
            {
                var isValidSessionId = CacheKeyBuilderValidation.IsValidSessionId(null);
            }
            catch (Exception ex)
            {
                errors.Add($"IsValidSessionId method call failed: {ex.Message}");
            }

            try
            {
                var isValidWebhookId = CacheKeyBuilderValidation.IsValidWebhookId(null);
            }
            catch (Exception ex)
            {
                errors.Add($"IsValidWebhookId method call failed: {ex.Message}");
            }

            try
            {
                var isValidCacheKey = CacheKeyBuilderValidation.IsValidCacheKey(null);
            }
            catch (Exception ex)
            {
                errors.Add($"IsValidCacheKey method call failed: {ex.Message}");
            }

            // Test EnsureValid methods
            try
            {
                CacheKeyBuilderValidation.EnsureValidSagaId("test");
            }
            catch (Exception ex)
            {
                errors.Add($"EnsureValidSagaId method call failed: {ex.Message}");
            }

            try
            {
                CacheKeyBuilderValidation.EnsureValidDefinition("test", "test");
            }
            catch (Exception ex)
            {
                errors.Add($"EnsureValidDefinition method call failed: {ex.Message}");
            }

            return errors;
        }

        /// <summary>
        /// Checks if the <see cref="CacheKeyBuilderValidation"/> static methods are valid.
        /// </summary>
        /// <param name="value">The <see cref="CacheKeyBuilderValidation"/> type reference.</param>
        /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this CacheKeyBuilderValidation value)
        {
            return Validate(value).Count == 0;
        }

        /// <summary>
        /// Ensures the <see cref="CacheKeyBuilderValidation"/> static methods are valid,
        /// throwing an <see cref="ArgumentException"/> if not.
        /// </summary>
        /// <param name="value">The <see cref="CacheKeyBuilderValidation"/> type reference.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="value"/> contains invalid methods.</exception>
        public static void EnsureValid(this CacheKeyBuilderValidation value)
        {
            var errors = Validate(value);
            if (errors.Count > 0)
            {
                throw new ArgumentException(FormatErrors("CacheKeyBuilderValidation validation failed", errors));
            }
        }

        private static string FormatErrors(string context, IReadOnlyList<string> errors)
        {
            var errorMessage = new System.Text.StringBuilder();
            errorMessage.AppendLine($"{context}:");
            foreach (var error in errors)
            {
                errorMessage.AppendLine($"- {error}");
            }
            return errorMessage.ToString();
        }
    }
}