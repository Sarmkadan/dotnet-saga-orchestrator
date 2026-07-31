using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Tests
{
    public class CreateSagaRequestTests
    {
        [Fact]
        public void CreateSagaRequest_DefaultValues_ShouldBeInitializedCorrectly()
        {
            // Act
            var request = new CreateSagaRequest();

            // Assert
            request.DefinitionId.Should().Be(string.Empty);
            request.DefinitionName.Should().BeNull();
            request.MaxRetries.Should().BeNull();
            request.TimeoutSeconds.Should().BeNull();
            request.Metadata.Should().BeNull();
            request.Data.Should().BeNull();
        }

        [Fact]
        public void IsValid_WithValidDefinitionId_ShouldReturnTrue()
        {
            // Arrange
            var request = new CreateSagaRequest { DefinitionId = "valid-id" };

            // Act
            var isValid = request.IsValid();

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WithValidDefinitionName_ShouldReturnTrue()
        {
            // Arrange
            var request = new CreateSagaRequest { DefinitionName = "valid-name" };

            // Act
            var isValid = request.IsValid();

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void IsValid_WithEmptyDefinitionIdAndName_ShouldReturnFalse()
        {
            // Arrange
            var request = new CreateSagaRequest { DefinitionId = "", DefinitionName = "" };

            // Act
            var isValid = request.IsValid();

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WithNegativeMaxRetries_ShouldReturnFalse()
        {
            // Arrange
            var request = new CreateSagaRequest { DefinitionId = "id", MaxRetries = -1 };

            // Act
            var isValid = request.IsValid();

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WithNonPositiveTimeoutSeconds_ShouldReturnFalse()
        {
            // Arrange
            var request = new CreateSagaRequest { DefinitionId = "id", TimeoutSeconds = 0 };

            // Act
            var isValid = request.IsValid();

            // Assert
            isValid.Should().BeFalse();
        }

        [Fact]
        public void Properties_ShouldBeSettable()
        {
            // Arrange
            var request = new CreateSagaRequest();
            var metadata = new Dictionary<string, object> { { "key", "value" } };

            // Act
            request.DefinitionId = "id";
            request.DefinitionName = "name";
            request.MaxRetries = 3;
            request.TimeoutSeconds = 30;
            request.Metadata = metadata;
            request.Data = "{ \"foo\": \"bar\" }";

            // Assert
            request.DefinitionId.Should().Be("id");
            request.DefinitionName.Should().Be("name");
            request.MaxRetries.Should().Be(3);
            request.TimeoutSeconds.Should().Be(30);
            request.Metadata.Should().BeSameAs(metadata);
            request.Data.Should().Be("{ \"foo\": \"bar\" }");
        }
    }
}
