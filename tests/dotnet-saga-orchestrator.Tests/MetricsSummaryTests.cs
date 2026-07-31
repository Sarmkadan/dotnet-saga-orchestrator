using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SagaOrchestrator.Application.DTOs;

namespace SagaOrchestrator.Tests
{
    public class MetricsSummaryTests
    {
        [Fact]
        public void Properties_ShouldSetAndGetValuesCorrectly()
        {
            // Arrange
            var metrics = new MetricsSummary();
            var now = DateTime.UtcNow;
            var byStatus = new Dictionary<string, int> { { "Running", 5 }, { "Completed", 10 } };

            // Act
            metrics.TotalSagas = 15;
            metrics.ByStatus = byStatus;
            metrics.AverageDurationSeconds = 12.5;
            metrics.CompensationRate = 0.05;
            metrics.Timestamp = now;

            // Assert
            metrics.TotalSagas.Should().Be(15);
            metrics.ByStatus.Should().HaveCount(2);
            metrics.ByStatus["Running"].Should().Be(5);
            metrics.ByStatus["Completed"].Should().Be(10);
            metrics.AverageDurationSeconds.Should().Be(12.5);
            metrics.CompensationRate.Should().Be(0.05);
            metrics.Timestamp.Should().Be(now);
        }

        [Fact]
        public void ByStatus_DefaultValue_ShouldBeEmptyDictionary()
        {
            // Arrange
            var metrics = new MetricsSummary();

            // Assert
            metrics.ByStatus.Should().NotBeNull();
            metrics.ByStatus.Should().BeEmpty();
        }

        [Fact]
        public void Properties_BoundaryValues_ShouldWork()
        {
            // Arrange
            var metrics = new MetricsSummary();

            // Act
            metrics.TotalSagas = int.MaxValue;
            metrics.AverageDurationSeconds = double.MaxValue;
            metrics.CompensationRate = 1.0;

            // Assert
            metrics.TotalSagas.Should().Be(int.MaxValue);
            metrics.AverageDurationSeconds.Should().Be(double.MaxValue);
            metrics.CompensationRate.Should().Be(1.0);
        }
    }
}
