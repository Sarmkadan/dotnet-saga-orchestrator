using System;
using System.Collections.Generic;
using System.Linq;

namespace SagaOrchestrator.Core.Domain.Models
{
    public static class SagaStepDefinitionExtensions
    {
        public static bool IsValid(this SagaStepDefinition stepDefinition)
        {
            if (stepDefinition == null) throw new ArgumentNullException(nameof(stepDefinition));

            return !string.IsNullOrEmpty(stepDefinition.Id) 
                && !string.IsNullOrEmpty(stepDefinition.Name) 
                && !string.IsNullOrEmpty(stepDefinition.ServiceName) 
                && stepDefinition.Order >= 0 
                && stepDefinition.TimeoutSeconds >= 0 
                && stepDefinition.MaxRetries >= 0 
                && stepDefinition.RetryDelayMilliseconds >= 0;
        }

        public static Dictionary<string, string> ToMetadataDictionary(this SagaStepDefinition stepDefinition, string keyPrefix = "")
        {
            if (stepDefinition == null) throw new ArgumentNullException(nameof(stepDefinition));

            var metadata = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(keyPrefix))
            {
                metadata.Add($"{keyPrefix}.Id", stepDefinition.Id);
                metadata.Add($"{keyPrefix}.Name", stepDefinition.Name);
                metadata.Add($"{keyPrefix}.Description", stepDefinition.Description);
                metadata.Add($"{keyPrefix}.ServiceName", stepDefinition.ServiceName);
                metadata.Add($"{keyPrefix}.ServiceUrl", stepDefinition.ServiceUrl);
                metadata.Add($"{keyPrefix}.CompensationUrl", stepDefinition.CompensationUrl);
                metadata.Add($"{keyPrefix}.TimeoutSeconds", stepDefinition.TimeoutSeconds.ToString());
                metadata.Add($"{keyPrefix}.MaxRetries", stepDefinition.MaxRetries.ToString());
                metadata.Add($"{keyPrefix}.RetryDelayMilliseconds", stepDefinition.RetryDelayMilliseconds.ToString());
                metadata.Add($"{keyPrefix}.IsCompensable", stepDefinition.IsCompensable.ToString());
                metadata.Add($"{keyPrefix}.IsAsync", stepDefinition.IsAsync.ToString());
                metadata.Add($"{keyPrefix}.HttpMethod", stepDefinition.HttpMethod);
            }
            else
            {
                metadata.Add("Id", stepDefinition.Id);
                metadata.Add("Name", stepDefinition.Name);
                metadata.Add("Description", stepDefinition.Description);
                metadata.Add("ServiceName", stepDefinition.ServiceName);
                metadata.Add("ServiceUrl", stepDefinition.ServiceUrl);
                metadata.Add("CompensationUrl", stepDefinition.CompensationUrl);
                metadata.Add("TimeoutSeconds", stepDefinition.TimeoutSeconds.ToString());
                metadata.Add("MaxRetries", stepDefinition.MaxRetries.ToString());
                metadata.Add("RetryDelayMilliseconds", stepDefinition.RetryDelayMilliseconds.ToString());
                metadata.Add("IsCompensable", stepDefinition.IsCompensable.ToString());
                metadata.Add("IsAsync", stepDefinition.IsAsync.ToString());
                metadata.Add("HttpMethod", stepDefinition.HttpMethod);
            }

            return metadata;
        }
    }
}
