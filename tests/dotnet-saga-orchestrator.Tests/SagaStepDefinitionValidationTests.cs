using System;
using System.Collections.Generic;
using Xunit;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests
{
    public class SagaStepDefinitionValidationTests
    {
        [Fact]
        public void Validate_ValidStep_ReturnsEmptyList()
        {
            var step = new SagaStepDefinition("Valid Step", "Service", "http://service.com", "http://compensation.com")
            {
                Id = Guid.NewGuid().ToString()
            };

            var errors = SagaStepDefinitionValidation.Validate(step);

            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_InvalidId_ReturnsError()
        {
            var step = new SagaStepDefinition("Step", "Service", "http://service.com", "http://compensation.com")
            {
                Id = "invalid-guid"
            };

            var errors = SagaStepDefinitionValidation.Validate(step);

            Assert.Contains("Id must be a valid GUID.", errors);
        }

        [Fact]
        public void Validate_InvalidName_ReturnsError()
        {
            // Name must be between 1 and 256. If I set it to a very long string it should fail.
            var longName = new string('a', 257);
            var step = new SagaStepDefinition(longName, "Service", "http://service.com", "http://compensation.com");

            var errors = SagaStepDefinitionValidation.Validate(step);

            Assert.Contains("Name cannot exceed 256 characters.", errors);
        }

        [Fact]
        public void Validate_InvalidServiceUrl_ReturnsError()
        {
            // The existing test used "not-a-url", but apparently that's valid.
            // Let's see if we can make it invalid. Maybe it needs to be an absolute URL?
            // The validation code says:
            // Uri.IsWellFormedUriString(value.ServiceUrl, UriKind.Absolute) && !Uri.IsWellFormedUriString(value.ServiceUrl, UriKind.Relative)
            // It uses AND? No, it uses ||?
            // else if (!Uri.IsWellFormedUriString(value.ServiceUrl, UriKind.Absolute) && !Uri.IsWellFormedUriString(value.ServiceUrl, UriKind.Relative))
            // So if BOTH are false, it is invalid.

            // How about something that is not well formed?
            // "http://[invalid-url]"?
            var step = new SagaStepDefinition("Step", "Service", "http://[invalid-url]", "http://compensation.com");

            var errors = SagaStepDefinitionValidation.Validate(step);

            Assert.Contains("ServiceUrl must be a valid URI.", errors);
        }

        [Fact]
        public void IsValid_ReturnsCorrectResult()
        {
            var validStep = new SagaStepDefinition("Valid", "Service", "http://s.com", "http://c.com") { Id = Guid.NewGuid().ToString() };
            var invalidStep = new SagaStepDefinition("Invalid", "Service", "http://[invalid-url]", "http://c.com");

            Assert.True(SagaStepDefinitionValidation.IsValid(validStep));
            Assert.False(SagaStepDefinitionValidation.IsValid(invalidStep));
        }

        [Fact]
        public void EnsureValid_ThrowsArgumentException_WhenInvalid()
        {
            var step = new SagaStepDefinition("Step", "Service", "http://[invalid-url]", "http://compensation.com");

            Assert.Throws<ArgumentException>(() => SagaStepDefinitionValidation.EnsureValid(step));
        }

        [Fact]
        public void EnsureValid_DoesNotThrow_WhenValid()
        {
            var step = new SagaStepDefinition("Valid", "Service", "http://s.com", "http://c.com") { Id = Guid.NewGuid().ToString() };

            var exception = Record.Exception(() => SagaStepDefinitionValidation.EnsureValid(step));

            Assert.Null(exception);
        }
    }
}
