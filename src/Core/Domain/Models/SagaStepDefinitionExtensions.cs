using System;
using System.Collections.Generic;

namespace SagaOrchestrator.Core.Domain.Models
{
    /// <summary>
    /// Provides extension methods for <see cref="SagaStepDefinition"/> to facilitate common operations.
    /// </summary>
    public static class SagaStepDefinitionExtensions
    {
        /// <summary>
        /// Determines whether the specified step definition is valid.
        /// </summary>
        /// <param name="stepDefinition">The step definition to validate.</param>
        /// <returns><see langword="true"/> if the step definition is valid; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepDefinition"/> is <see langword="null"/>.</exception>
        public static bool IsValid(this SagaStepDefinition stepDefinition)
        {
            ArgumentNullException.ThrowIfNull(stepDefinition);

            return !string.IsNullOrEmpty(stepDefinition.Id)
                && !string.IsNullOrEmpty(stepDefinition.Name)
                && !string.IsNullOrEmpty(stepDefinition.ServiceName)
                && stepDefinition.Order >= 0
                && stepDefinition.TimeoutSeconds >= 0
                && stepDefinition.MaxRetries >= 0
                && stepDefinition.RetryDelayMilliseconds >= 0;
        }

        /// <summary>
        /// Converts the step definition to a metadata dictionary, optionally prefixing the keys.
        /// </summary>
        /// <param name="stepDefinition">The step definition to convert.</param>
        /// <param name="keyPrefix">Optional prefix for metadata keys. If specified, keys will be formatted as "{prefix}.{propertyName}".</param>
        /// <returns>A dictionary containing the step definition's properties as key-value pairs.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stepDefinition"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="keyPrefix"/> is neither <see langword="null"/> nor empty but contains invalid characters.</exception>
        public static Dictionary<string, string> ToMetadataDictionary(this SagaStepDefinition stepDefinition, string keyPrefix = "")
        {
            ArgumentNullException.ThrowIfNull(stepDefinition);

            var metadata = new Dictionary<string, string>(StringComparer.Ordinal);

            string GetKey(string propertyName) => string.IsNullOrEmpty(keyPrefix)
                ? propertyName
                : $"{keyPrefix}.{propertyName}";

            metadata.Add(GetKey("Id"), stepDefinition.Id);
            metadata.Add(GetKey("Name"), stepDefinition.Name);
            metadata.Add(GetKey("Description"), stepDefinition.Description);
            metadata.Add(GetKey("ServiceName"), stepDefinition.ServiceName);
            metadata.Add(GetKey("ServiceUrl"), stepDefinition.ServiceUrl);
            metadata.Add(GetKey("CompensationUrl"), stepDefinition.CompensationUrl);
            metadata.Add(GetKey("TimeoutSeconds"), stepDefinition.TimeoutSeconds.ToString());
            metadata.Add(GetKey("MaxRetries"), stepDefinition.MaxRetries.ToString());
            metadata.Add(GetKey("RetryDelayMilliseconds"), stepDefinition.RetryDelayMilliseconds.ToString());
            metadata.Add(GetKey("IsCompensable"), stepDefinition.IsCompensable.ToString());
            metadata.Add(GetKey("IsAsync"), stepDefinition.IsAsync.ToString());
            metadata.Add(GetKey("HttpMethod"), stepDefinition.HttpMethod ?? "POST");

            return metadata;
        }
    }
}