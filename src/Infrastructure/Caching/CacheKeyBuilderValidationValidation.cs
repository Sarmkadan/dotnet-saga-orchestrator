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
        /// Validates the <see cref="CacheKeyBuilderValidation"/> static methods by exercising them with various inputs.
        /// </summary>
        /// <param name="value">The <see cref="CacheKeyBuilderValidation"/> type reference (unused, for API consistency).</param>
        /// <returns>A list of validation error messages; empty if validation succeeds.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        public static IReadOnlyList<string> Validate(this CacheKeyBuilderValidation? value)
        {
            ArgumentNullException.ThrowIfNull(value);

            var errors = new List<string>();

            // Test Validate methods with invalid inputs (should return non-empty error lists)
            var validateSagaIdErrors = CacheKeyBuilderValidation.ValidateSagaId(null);
            if (validateSagaIdErrors.Count == 0)
            {
                errors.Add("ValidateSagaId returned empty errors for null input");
            }

            var validateDefinitionErrors = CacheKeyBuilderValidation.ValidateDefinition(null, null);
            if (validateDefinitionErrors.Count == 0)
            {
                errors.Add("ValidateDefinition returned empty errors for null inputs");
            }

            var validateStatusErrors = CacheKeyBuilderValidation.ValidateStatus(null);
            if (validateStatusErrors.Count == 0)
            {
                errors.Add("ValidateStatus returned empty errors for null input");
            }

            var validateServiceNameErrors = CacheKeyBuilderValidation.ValidateServiceName(null);
            if (validateServiceNameErrors.Count == 0)
            {
                errors.Add("ValidateServiceName returned empty errors for null input");
            }

            var validateRateLimitErrors = CacheKeyBuilderValidation.ValidateRateLimit(null, null);
            if (validateRateLimitErrors.Count == 0)
            {
                errors.Add("ValidateRateLimit returned empty errors for null inputs");
            }

            var validateUserIdErrors = CacheKeyBuilderValidation.ValidateUserId(null);
            if (validateUserIdErrors.Count == 0)
            {
                errors.Add("ValidateUserId returned empty errors for null input");
            }

            var validateSessionIdErrors = CacheKeyBuilderValidation.ValidateSessionId(null);
            if (validateSessionIdErrors.Count == 0)
            {
                errors.Add("ValidateSessionId returned empty errors for null input");
            }

            var validateWebhookIdErrors = CacheKeyBuilderValidation.ValidateWebhookId(null);
            if (validateWebhookIdErrors.Count == 0)
            {
                errors.Add("ValidateWebhookId returned empty errors for null input");
            }

            var validateCacheKeyErrors = CacheKeyBuilderValidation.ValidateCacheKey(null);
            if (validateCacheKeyErrors.Count == 0)
            {
                errors.Add("ValidateCacheKey returned empty errors for null input");
            }

            // Test Validate methods with whitespace inputs (should return non-empty error lists)
            var validateSagaIdWhitespaceErrors = CacheKeyBuilderValidation.ValidateSagaId("   ");
            if (validateSagaIdWhitespaceErrors.Count == 0)
            {
                errors.Add("ValidateSagaId returned empty errors for whitespace input");
            }

            var validateDefinitionWhitespaceErrors = CacheKeyBuilderValidation.ValidateDefinition("   ", "   ");
            if (validateDefinitionWhitespaceErrors.Count == 0)
            {
                errors.Add("ValidateDefinition returned empty errors for whitespace inputs");
            }

            // Test IsValid methods with invalid inputs (should return false)
            if (CacheKeyBuilderValidation.IsValidSagaId(null))
            {
                errors.Add("IsValidSagaId returned true for null input");
            }

            if (CacheKeyBuilderValidation.IsValidDefinition(null, null))
            {
                errors.Add("IsValidDefinition returned true for null inputs");
            }

            if (CacheKeyBuilderValidation.IsValidStatus(null))
            {
                errors.Add("IsValidStatus returned true for null input");
            }

            if (CacheKeyBuilderValidation.IsValidServiceName(null))
            {
                errors.Add("IsValidServiceName returned true for null input");
            }

            if (CacheKeyBuilderValidation.IsValidRateLimit(null, null))
            {
                errors.Add("IsValidRateLimit returned true for null inputs");
            }

            if (CacheKeyBuilderValidation.IsValidUserId(null))
            {
                errors.Add("IsValidUserId returned true for null input");
            }

            if (CacheKeyBuilderValidation.IsValidSessionId(null))
            {
                errors.Add("IsValidSessionId returned true for null input");
            }

            if (CacheKeyBuilderValidation.IsValidWebhookId(null))
            {
                errors.Add("IsValidWebhookId returned true for null input");
            }

            if (CacheKeyBuilderValidation.IsValidCacheKey(null))
            {
                errors.Add("IsValidCacheKey returned true for null input");
            }

            // Test EnsureValid methods with invalid inputs (should throw)
            try
            {
                CacheKeyBuilderValidation.EnsureValidSagaId(null!);
                errors.Add("EnsureValidSagaId did not throw for null input");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }

            try
            {
                CacheKeyBuilderValidation.EnsureValidDefinition(null!, null!);
                errors.Add("EnsureValidDefinition did not throw for null inputs");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }

            try
            {
                CacheKeyBuilderValidation.EnsureValidStatus(null!);
                errors.Add("EnsureValidStatus did not throw for null input");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }

            try
            {
                CacheKeyBuilderValidation.EnsureValidServiceName(null!);
                errors.Add("EnsureValidServiceName did not throw for null input");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }

            try
            {
                CacheKeyBuilderValidation.EnsureValidRateLimit(null!, null!);
                errors.Add("EnsureValidRateLimit did not throw for null inputs");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }

            try
            {
                CacheKeyBuilderValidation.EnsureValidUserId(null!);
                errors.Add("EnsureValidUserId did not throw for null input");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }

            try
            {
                CacheKeyBuilderValidation.EnsureValidSessionId(null!);
                errors.Add("EnsureValidSessionId did not throw for null input");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }

            try
            {
                CacheKeyBuilderValidation.EnsureValidWebhookId(null!);
                errors.Add("EnsureValidWebhookId did not throw for null input");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }

            try
            {
                CacheKeyBuilderValidation.EnsureValidCacheKey(null!);
                errors.Add("EnsureValidCacheKey did not throw for null input");
            }
            catch (ArgumentNullException)
            {
                // Expected
            }

            // Test valid inputs (should not throw and return empty error lists)
            try
            {
                var validSagaIdErrors = CacheKeyBuilderValidation.ValidateSagaId("valid-saga-id");
                if (validSagaIdErrors.Count > 0)
                {
                    errors.Add("ValidateSagaId returned errors for valid input: " + string.Join(", ", validSagaIdErrors));
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateSagaId threw exception for valid input: {ex.Message}");
            }

            try
            {
                var validDefinitionErrors = CacheKeyBuilderValidation.ValidateDefinition("def-id", "def-name");
                if (validDefinitionErrors.Count > 0)
                {
                    errors.Add("ValidateDefinition returned errors for valid inputs: " + string.Join(", ", validDefinitionErrors));
                }
            }
            catch (Exception ex)
            {
                errors.Add($"ValidateDefinition threw exception for valid inputs: {ex.Message}");
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
            ArgumentNullException.ThrowIfNull(value);
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
            ArgumentNullException.ThrowIfNull(value);
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