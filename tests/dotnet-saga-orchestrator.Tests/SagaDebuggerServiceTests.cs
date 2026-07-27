using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SagaOrchestrator.Application.Services;
using SagaOrchestrator.Configuration;
using SagaOrchestrator.Core.Domain.Models;
using SagaOrchestrator.Data.Repositories;
using SagaOrchestrator.Infrastructure.Debugging;
using Xunit;

namespace SagaOrchestrator.Tests.Infrastructure.Debugging;

public class SagaDebuggerServiceTests
{
    [Fact]
    public async Task CaptureSnapshotAsync_HappyPath_ReturnsSnapshot()
    {
        // Arrange
        var sagaRepository = Mock.Of<ISagaRepository>();
        var sagaStepRepository = Mock.Of<ISagaStepRepository>();
        var eventPublisher = new SagaEventPublisher();
        var options = new DebuggerOptions { IsEnabled = true };
        var service = new SagaDebuggerService(sagaRepository, sagaStepRepository, eventPublisher, options);

        // Act
        var snapshot = await service.CaptureSnapshotAsync("sagaId");

        // Assert
        Assert.NotNull(snapshot);
    }

    [Fact]
    public async Task GetSnapshotsAsync_HappyPath_ReturnsSnapshots()
    {
        // Arrange
        var sagaRepository = Mock.Of<ISagaRepository>();
        var sagaStepRepository = Mock.Of<ISagaStepRepository>();
        var eventPublisher = new SagaEventPublisher();
        var options = new DebuggerOptions { IsEnabled = true };
        var service = new SagaDebuggerService(sagaRepository, sagaStepRepository, eventPublisher, options);

        // Act
        var snapshots = await service.GetSnapshotsAsync("sagaId");

        // Assert
        Assert.NotNull(snapshots);
    }

    [Fact]
    public async Task TravelToAsync_HappyPath_ReturnsSnapshot()
    {
        // Arrange
        var sagaRepository = Mock.Of<ISagaRepository>();
        var sagaStepRepository = Mock.Of<ISagaStepRepository>();
        var eventPublisher = new SagaEventPublisher();
        var options = new DebuggerOptions { IsEnabled = true };
        var service = new SagaDebuggerService(sagaRepository, sagaStepRepository, eventPublisher, options);

        // Act
        var snapshot = await service.TravelToAsync("sagaId", "snapshotId");

        // Assert
        Assert.NotNull(snapshot);
    }

    [Fact]
    public async Task SetBreakpointAsync_HappyPath_ReturnsBreakpoint()
    {
        // Arrange
        var sagaRepository = Mock.Of<ISagaRepository>();
        var sagaStepRepository = Mock.Of<ISagaStepRepository>();
        var eventPublisher = new SagaEventPublisher();
        var options = new DebuggerOptions { IsEnabled = true };
        var service = new SagaDebuggerService(sagaRepository, sagaStepRepository, eventPublisher, options);

        // Act
        var breakpoint = await service.SetBreakpointAsync("sagaId", "stepName");

        // Assert
        Assert.NotNull(breakpoint);
    }

    [Fact]
    public async Task RemoveBreakpointAsync_HappyPath_ReturnsTrue()
    {
        // Arrange
        var sagaRepository = Mock.Of<ISagaRepository>();
        var sagaStepRepository = Mock.Of<ISagaStepRepository>();
        var eventPublisher = new SagaEventPublisher();
        var options = new DebuggerOptions { IsEnabled = true };
        var service = new SagaDebuggerService(sagaRepository, sagaStepRepository, eventPublisher, options);

        // Act
        var breakpoint = await service.SetBreakpointAsync("sagaId", "stepName");
        var result = await service.RemoveBreakpointAsync(breakpoint.BreakpointId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CaptureSnapshotAsync_DisabledDebugger_ThrowsException()
    {
        // Arrange
        var sagaRepository = Mock.Of<ISagaRepository>();
        var sagaStepRepository = Mock.Of<ISagaStepRepository>();
        var eventPublisher = new SagaEventPublisher();
        var options = new DebuggerOptions { IsEnabled = false };
        var service = new SagaDebuggerService(sagaRepository, sagaStepRepository, eventPublisher, options);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CaptureSnapshotAsync("sagaId"));
    }

    [Fact]
    public async Task TravelToAsync_InvalidSnapshotId_ThrowsException()
    {
        // Arrange
        var sagaRepository = Mock.Of<ISagaRepository>();
        var sagaStepRepository = Mock.Of<ISagaStepRepository>();
        var eventPublisher = new SagaEventPublisher();
        var options = new DebuggerOptions { IsEnabled = true };
        var service = new SagaDebuggerService(sagaRepository, sagaStepRepository, eventPublisher, options);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.TravelToAsync("sagaId", "invalidSnapshotId"));
    }
}
