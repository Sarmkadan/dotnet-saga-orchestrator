using System;
using System.Collections.Generic;
using System.Globalization;

namespace SagaOrchestrator.Application.DTOs
{
    /// <summary>
    /// Extension methods for <see cref="CreateSagaRequest"/>.
    /// </summary>
    public static class CreateSagaRequestExtensions
    {
        /// <summary>
        /// Determines whether the <see cref="CreateSagaRequest"/> has a valid timeout.
        /// </summary>
        /// <param name="request">The <see cref="CreateSagaRequest"/> to check.</param>
        /// <returns><c>true</c> if the request has a valid timeout; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
        public static bool HasValidTimeout(this CreateSagaRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return request.TimeoutSeconds.HasValue && request.TimeoutSeconds.Value >= 0;
        }

        /// <summary>
        /// Gets a read-only dictionary of metadata for the <see cref="CreateSagaRequest"/>.
        /// </summary>
        /// <param name="request">The <see cref="CreateSagaRequest"/> to get metadata from.</param>
        /// <returns>A read-only dictionary of metadata.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
        public static IReadOnlyDictionary<string, object> GetMetadata(this CreateSagaRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return request.Metadata?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// Tries to parse the <see cref="CreateSagaRequest.Data"/> as a decimal value.
        /// </summary>
        /// <param name="request">The <see cref="CreateSagaRequest"/> to parse data from.</param>
        /// <param name="result">The parsed decimal value.</param>
        /// <returns><c>true</c> if the data was successfully parsed; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <c>null</c>.</exception>
        public static bool TryParseDataAsDecimal(this CreateSagaRequest request, out decimal result)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrEmpty(request.Data))
            {
                result = default;
                return false;
            }

            return decimal.TryParse(request.Data, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
        }
    }
}
