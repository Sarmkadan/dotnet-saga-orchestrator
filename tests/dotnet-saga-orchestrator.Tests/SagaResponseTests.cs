using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Application.DTOs;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Core.Domain.Enums;

namespace SagaOrchestrator.Tests
{
    public class SagaResponseTests
    {
        [Fact]
        public void FromSaga_ValidSaga_ShouldMapPropertiesCorrectly()
        {
            // Arrange
            var definition = new SagaDefinition("TestSaga", "Test description");
            var saga = new Saga();
            saga.Initialize(definition);
            saga.Start();
            
            // Act
            var response = SagaResponse.FromSaga(saga);

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(saga.Id);
            response.CorrelationId.Should().Be(saga.CorrelationId);
            response.Status.Should().Be(saga.Status.ToString());
            response.DefinitionId.Should().Be(saga.Definition.Id);
            response.DefinitionName.Should().Be(saga.Definition.Name);
            response.StartedAt.Should().Be(saga.StartedAt);
        }

        [Fact]
        public void FromSaga_NullSaga_ShouldThrowArgumentNullException()
        {
            // Act
            Action act = () => SagaResponse.FromSaga(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
               .WithParameterName("saga");
        }

        [Fact]
        public void SagaResponse_DefaultConstructor_ShouldInitializeWithExpectedDefaults()
        {
            // Act
            var response = new SagaResponse();

            // Assert
            response.Id.Should().Be(string.Empty);
            response.CorrelationId.Should().Be(string.Empty);
            response.Status.Should().Be(string.Empty);
            response.DefinitionId.Should().Be(string.Empty);
            response.DefinitionName.Should().Be(string.Empty);
            response.Steps.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void SagaResponse_Properties_ShouldBeSettable()
        {
            // Arrange
            var response = new SagaResponse();
            var now = DateTime.UtcNow;

            // Act
            response.Id = "new-id";
            response.Status = "Running";
            response.StartedAt = now;

            // Assert
            response.Id.Should().Be("new-id");
            response.Status.Should().Be("Running");
            response.StartedAt.Should().Be(now);
        }
    }
}
