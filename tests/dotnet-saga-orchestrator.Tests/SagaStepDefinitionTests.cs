using System;
using System.Text.Json.Serialization;
using SagaOrchestrator.Core.Utilities;
using Xunit;
using Moq;

namespace SagaOrchestrator.Core.Domain.Models.Tests
{
    public class SagaStepDefinitionTests
    {
        [Fact]
        public void HappyPath_CreateStepDefinition()
        {
            // Arrange
            var stepDefinition = new SagaStepDefinition("Test Step", "Test Service", "https://test.com", "https://test.com/compensation");

            // Act

            // Assert
            Assert.NotNull(stepDefinition);
            Assert.Equal("Test Step", stepDefinition.Name);
            Assert.Equal("Test Service", stepDefinition.ServiceName);
            Assert.Equal("https://test.com", stepDefinition.ServiceUrl);
            Assert.Equal("https://test.com/compensation", stepDefinition.CompensationUrl);
        }

        [Fact]
        public void EdgeCase_CreateStepDefinition_WithNullName_ThrowsArgumentNullException()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentNullException>("name", () => new SagaStepDefinition(null, "Test Service", "https://test.com", "https://test.com/compensation"));
        }

        [Fact]
        public void EdgeCase_CreateStepDefinition_WithNullServiceName_ThrowsArgumentNullException()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentNullException>("serviceName", () => new SagaStepDefinition("Test Step", null, "https://test.com", "https://test.com/compensation"));
        }

        [Fact]
        public void EdgeCase_CreateStepDefinition_WithNullServiceUrl_ThrowsArgumentNullException()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentNullException>("serviceUrl", () => new SagaStepDefinition("Test Step", "Test Service", null, "https://test.com/compensation"));
        }

        [Fact]
        public void EdgeCase_CreateStepDefinition_WithNullCompensationUrl_ThrowsArgumentNullException_WhenIsCompensable()
        {
            // Arrange

            // Act and Assert
            Assert.Throws<ArgumentNullException>("compensationUrl", () => new SagaStepDefinition("Test Step", "Test Service", "https://test.com", null) { IsCompensable = true });
        }

        [Fact]
        public void ErrorPath_Validate_ThrowsArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            var stepDefinition = new SagaStepDefinition("", "Test Service", "https://test.com", "https://test.com/compensation");

            // Act and Assert
            Assert.Throws<ArgumentException>(() => stepDefinition.Validate());
        }
    }
}