using System.Diagnostics;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Data.Repositories;
using FluentAssertions;
using Xunit;
using SagaOrchestrator.Infrastructure.Telemetry;

namespace SagaOrchestrator.Tests
{
    public class SagaActivitySourceExtensionsTests
    {
        [Fact]
        public void ValidateStartSaga_ValidParameters_ReturnsEmptyList()
        {
            var problems = SagaActivitySourceExtensionsValidation.ValidateStartSaga("saga1", "def1");
            problems.Should().BeEmpty();
        }

        [Fact]
        public void ValidateStartSaga_InvalidSagaId_ThrowsArgumentException()
        {
            Action act = () => SagaActivitySourceExtensionsValidation.ValidateStartSaga("", "def1");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ValidateRecordSagaComplete_ValidParameters_ReturnsEmptyList()
        {
            var problems = SagaActivitySourceExtensionsValidation.ValidateRecordSagaComplete("saga1", "Completed", 5, TimeSpan.FromSeconds(10), 5, 0);
            problems.Should().BeEmpty();
        }

        [Fact]
        public void ValidateRecordSagaComplete_NegativeSteps_ReturnsProblems()
        {
            var problems = SagaActivitySourceExtensionsValidation.ValidateRecordSagaComplete("saga1", "Completed", -1, TimeSpan.FromSeconds(10));
            problems.Should().Contain("Total steps cannot be negative.");
        }

        [Fact]
        public void ValidateStartStep_ValidParameters_ReturnsEmptyList()
        {
            var problems = SagaActivitySourceExtensionsValidation.ValidateStartStep("saga1", "step1", "stepName", 1);
            problems.Should().BeEmpty();
        }

        [Fact]
        public void ValidateStartStep_InvalidOrder_ReturnsProblems()
        {
            var problems = SagaActivitySourceExtensionsValidation.ValidateStartStep("saga1", "step1", "stepName", -1);
            problems.Should().Contain("Step order cannot be negative.");
        }

        [Fact]
        public void ValidateRecordStepFailure_InvalidErrorMessage_ThrowsArgumentException()
        {
            // Activity is nullable, so passing null is valid if errorMessage is invalid
            Action act = () => SagaActivitySourceExtensionsValidation.ValidateRecordStepFailure(null, "");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void IsValidStartSaga_ValidParameters_ReturnsTrue()
        {
            var isValid = SagaActivitySourceExtensionsValidation.IsValidStartSaga("saga1", "def1");
            isValid.Should().BeTrue();
        }
    }
}
