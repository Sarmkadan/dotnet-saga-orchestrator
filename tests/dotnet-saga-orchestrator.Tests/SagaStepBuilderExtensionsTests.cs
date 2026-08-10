using SagaOrchestrator.Core.Builders;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Utilities;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System;

namespace SagaOrchestrator.Tests
{
    public class SagaStepBuilderExtensionsTests
    {
        // This helper will likely fail due to the bug in SagaStepBuilder constructor, 
        // but this is the intended way to test the extensions.
        private static SagaStepBuilder CreateValidBuilder()
        {
            return SagaStepBuilder.Create("Test Step", "Test Service", "https://example.com/action");
        }

        [Fact]
        public void WithDescription_ValidInput_ReturnsBuilder()
        {
            var builder = CreateValidBuilder();
            var result = builder.WithDescription("Test Description");
            result.Should().Be(builder);
        }

        [Fact]
        public void WithDescription_NullBuilder_ThrowsArgumentNullException()
        {
            SagaStepBuilder builder = null!;
            Action act = () => builder.WithDescription("Test Description");
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void WithHttpMethod_ValidInput_ReturnsBuilder()
        {
            var builder = CreateValidBuilder();
            var result = builder.WithHttpMethod("GET");
            result.Should().Be(builder);
        }

        [Fact]
        public void WithCompensable_ValidInput_ReturnsBuilder()
        {
            var builder = CreateValidBuilder();
            var result = builder.WithCompensable(true, "https://example.com/compensate");
            result.Should().Be(builder);
        }

        [Fact]
        public void WithRetryPolicyFromDefinition_ValidInput_ReturnsBuilder()
        {
            var builder = CreateValidBuilder();
            var definition = new SagaStepDefinition("Test Step", "Test Service", "https://example.com/action", "https://example.com/compensate")
            {
                RetryPolicy = new RetryPolicy(3, 1000)
            };

            var result = builder.WithRetryPolicyFromDefinition(definition);
            result.Should().Be(builder);
        }

        [Fact]
        public void WithExponentialRetryPolicy_ValidInput_ReturnsBuilder()
        {
            var builder = CreateValidBuilder();
            var result = builder.WithExponentialRetryPolicy(5, 200, true);
            result.Should().Be(builder);
        }

        [Fact]
        public void WithLinearRetryPolicy_ValidInput_ReturnsBuilder()
        {
            var builder = CreateValidBuilder();
            var result = builder.WithLinearRetryPolicy(4, 300);
            result.Should().Be(builder);
        }

        [Fact]
        public void WithNoRetryPolicy_ValidInput_ReturnsBuilder()
        {
            var builder = CreateValidBuilder();
            var result = builder.WithNoRetryPolicy();
            result.Should().Be(builder);
        }

        [Fact]
        public void WithMetadata_ValidInput_ReturnsBuilder()
        {
            var builder = CreateValidBuilder();
            var metadata = new Dictionary<string, string> { { "key1", "value1" } };
            var result = builder.WithMetadata(metadata);
            result.Should().Be(builder);
        }
    }
}
