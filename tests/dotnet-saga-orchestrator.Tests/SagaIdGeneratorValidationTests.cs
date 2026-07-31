using System;
using Xunit;
using SagaOrchestrator.Core.Utilities;

namespace SagaOrchestrator.Tests
{
    public class SagaIdGeneratorValidationTests
    {
        [Fact]
        public void Validate_ReturnsEmptyList_WhenAllGeneratorsAreValid()
        {
            // Act
            var result = SagaIdGeneratorValidation.Validate();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Validate_ReturnsReadOnlyList()
        {
            // Act
            var result = SagaIdGeneratorValidation.Validate();

            // Assert
            Assert.IsAssignableFrom<IReadOnlyList<string>>(result);
        }

        [Fact]
        public void IsValid_ReturnsTrue_WhenAllGeneratorsAreValid()
        {
            // Act
            var result = SagaIdGeneratorValidation.IsValid();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void EnsureValid_DoesNotThrow_WhenAllGeneratorsAreValid()
        {
            // Act & Assert
            SagaIdGeneratorValidation.EnsureValid();
        }
    }
}
