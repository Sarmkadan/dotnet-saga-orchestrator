using System;
using System.Collections.Generic;
using SagaOrchestrator.Core.Domain.Enums;
using SagaOrchestrator.Core.Domain.Models;
using Xunit;

namespace SagaOrchestrator.Tests;

public class SagaDebugSnapshotExtensionsTests
{
    [Fact]
    public void ToJson_ShouldSerializeSnapshot()
    {
        var snapshot = CreateSnapshot("1");
        
        var json = snapshot.ToJson();
        
        Assert.Contains("\"snapshotId\":", json);
        Assert.Contains("\"sagaId\":", json);
    }

    [Fact]
    public void DiffAgainst_ShouldReturnDifferences()
    {
        var snapshot1 = CreateSnapshot("1");
        var snapshot2 = snapshot1 with { SagaStatus = SagaStatus.Failed, RetryCount = 1 };
        
        var diffs = snapshot1.DiffAgainst(snapshot2);
        
        Assert.Contains("SagaStatus", diffs);
        Assert.Contains("RetryCount", diffs);
        Assert.DoesNotContain("Trigger", diffs);
    }

    private static SagaDebugSnapshot CreateSnapshot(string id) => new()
    {
        SnapshotId = id,
        SagaId = "saga-1",
        SagaName = "TestSaga",
        DefinitionId = "def-1",
        CorrelationId = "corr-1",
        SagaStatus = SagaStatus.Running,
        Trigger = SnapshotTrigger.Manual,
        CapturedAt = DateTime.UtcNow,
        SagaStartedAt = DateTime.UtcNow,
        RetryCount = 0,
        MaxRetries = 3,
        Steps = new List<SagaStepDebugState>().AsReadOnly(),
        Metadata = new Dictionary<string, object>(),
        SequenceNumber = 1
    };
}
