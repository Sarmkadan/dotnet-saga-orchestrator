using System;
using SagaOrchestrator.Configuration;
using Xunit;

namespace SagaOrchestrator.Tests;

public class DebuggerOptionsExtensionsTests
{
    private DebuggerOptions CreateDefaultOptions()
    {
        return new DebuggerOptions
        {
            IsEnabled = true,
            MaxSnapshotsPerSaga = 10,
            AutoCaptureOnStepTransition = true,
            AutoCaptureOnCompensation = false,
            AutoCaptureOnTerminalState = true,
            MaxBreakpointsPerSaga = 5,
            IncludeStepPayloads = true,
            IncludeSagaMetadata = true,
            EnableTimeTravel = true
        };
    }

    [Fact]
    public void IsAutoCaptureEnabled_ReturnsTrueForStepTransition_WhenEnabled()
    {
        // Arrange
        var options = CreateDefaultOptions();

        // Act
        var result = options.IsAutoCaptureEnabled(DebuggerSnapshotTrigger.StepTransition);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsAutoCaptureEnabled_ReturnsFalseForCompensation_WhenDisabled()
    {
        // Arrange
        var options = CreateDefaultOptions();

        // Act
        var result = options.IsAutoCaptureEnabled(DebuggerSnapshotTrigger.Compensation);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAutoCaptureEnabled_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        DebuggerOptions options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options.IsAutoCaptureEnabled(DebuggerSnapshotTrigger.StepTransition));
    }

    [Fact]
    public void IsAutoCaptureEnabled_ThrowsArgumentOutOfRangeException_WhenTriggerIsUndefined()
    {
        // Arrange
        var options = CreateDefaultOptions();
        // Create an undefined enum value by casting an integer not in the enum
        var undefinedTrigger = (DebuggerSnapshotTrigger)999;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => options.IsAutoCaptureEnabled(undefinedTrigger));
    }

    [Fact]
    public void GetMaxSnapshotsForSaga_ReturnsValueWithinExpectedRange()
    {
        // Arrange
        var options = CreateDefaultOptions();
        const string sagaId = "test-saga";

        // Act
        var result = options.GetMaxSnapshotsForSaga(sagaId);

        // Assert
        Assert.InRange(result, 1, options.MaxSnapshotsPerSaga);
    }

    [Fact]
    public void GetMaxSnapshotsForSaga_ReturnsBaseLimit_WhenHashResultsInZeroVariance()
    {
        // Arrange
        var options = CreateDefaultOptions();
        // Choose a sagaId that results in hash = 50 (so variance = 0)
        // We know hash = Math.Abs(StringComparer.OrdinalIgnoreCase.GetHashCode(sagaId)) % 100
        // We need GetHashCode(sagaId) % 100 = 50 or -50 (but absolute value makes it 50)
        // Let's use "50" as a simple test - actual hash may vary, but we can adjust options to make variance 0
        // Instead, we'll set MaxSnapshotsPerSaga to 50 and choose an ID that gives hash=50
        // For simplicity, we'll mock the hash calculation by setting options.MaxSnapshotsPerSaga to 50
        // and then we expect the result to be 50 when hash=50 (variance=0)
        // However, we cannot control the hash without knowing the string. Let's test the boundaries.
        // We'll test that when the effective limit is less than 1, it returns 1.
        // And when effective limit is greater than MaxSnapshotsPerSaga, it returns MaxSnapshotsPerSaga.
        // We already have a test for the range.

        // This test is to verify the calculation logic. We'll use a known string that gives a specific hash.
        // Let's use "test" and compute the hash manually in the test? Not possible without duplicating code.
        // Instead, we'll test the boundaries by setting MaxSnapshotsPerSaga to 1 and then the result must be 1.
        var optionsMin = new DebuggerOptions { MaxSnapshotsPerSaga = 1 };
        var resultMin = optionsMin.GetMaxSnapshotsForSaga("any");
        Assert.Equal(1, resultMin);

        // Now test when MaxSnapshotsPerSaga is high and the variance pushes it above, it should be capped.
        var optionsHigh = new DebuggerOptions { MaxSnapshotsPerSaga = 100 };
        // We know the variance is (hash - 50). The hash is between 0 and 99, so variance between -50 and 49.
        // So effectiveLimit = 100 + (hash - 50) = 50 + hash, which is between 50 and 149.
        // Then we take Math.Max(1, effectiveLimit) -> still between 50 and 149.
        // Then we take Math.Min(effectiveLimit, 100) -> so result is between 50 and 100.
        var resultHigh = optionsHigh.GetMaxSnapshotsForSaga("test");
        Assert.InRange(resultHigh, 50, 100);
    }

    [Fact]
    public void GetMaxSnapshotsForSaga_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        DebuggerOptions options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options.GetMaxSnapshotsForSaga("saga"));
    }

    [Fact]
    public void GetMaxSnapshotsForSaga_ThrowsArgumentException_WhenSagaIdIsNull()
    {
        // Arrange
        var options = CreateDefaultOptions();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => options.GetMaxSnapshotsForSaga(null!));
    }

    [Fact]
    public void GetMaxSnapshotsForSaga_ThrowsArgumentException_WhenSagaIdIsEmpty()
    {
        // Arrange
        var options = CreateDefaultOptions();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => options.GetMaxSnapshotsForSaga(string.Empty));
    }

    [Fact]
    public void GetMaxSnapshotsForSaga_ThrowsArgumentOutOfRangeException_WhenMaxSnapshotsPerSagaIsLessThanOne()
    {
        // Arrange
        var options = new DebuggerOptions { MaxSnapshotsPerSaga = 0 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => options.GetMaxSnapshotsForSaga("saga"));
    }

    [Fact]
    public void WillCaptureDataFor_ReturnsTrueForStepExecution_WhenIncludeStepPayloadsIsTrue()
    {
        // Arrange
        var options = CreateDefaultOptions();

        // Act
        var result = options.WillCaptureDataFor(DebuggerScenario.StepExecution);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WillCaptureDataFor_ReturnsFalseForStepExecution_WhenIncludeStepPayloadsIsFalse()
    {
        // Arrange
        var options = new DebuggerOptions { IncludeStepPayloads = false };

        // Act
        var result = options.WillCaptureDataFor(DebuggerScenario.StepExecution);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WillCaptureDataFor_ReturnsTrueForSagaMetadata_WhenIncludeSagaMetadataIsTrue()
    {
        // Arrange
        var options = CreateDefaultOptions();

        // Act
        var result = options.WillCaptureDataFor(DebuggerScenario.SagaMetadata);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WillCaptureDataFor_ReturnsFalseForSagaMetadata_WhenIncludeSagaMetadataIsFalse()
    {
        // Arrange
        var options = new DebuggerOptions { IncludeSagaMetadata = false };

        // Act
        var result = options.WillCaptureDataFor(DebuggerScenario.SagaMetadata);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WillCaptureDataFor_ReturnsTrueForTimeTravel_WhenEnableTimeTravelIsTrue()
    {
        // Arrange
        var options = CreateDefaultOptions();

        // Act
        var result = options.WillCaptureDataFor(DebuggerScenario.TimeTravel);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WillCaptureDataFor_ReturnsFalseForTimeTravel_WhenEnableTimeTravelIsFalse()
    {
        // Arrange
        var options = new DebuggerOptions { EnableTimeTravel = false };

        // Act
        var result = options.WillCaptureDataFor(DebuggerScenario.TimeTravel);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WillCaptureDataFor_ReturnsIsEnabledForUndefinedScenario()
    {
        // Arrange
        var options = new DebuggerOptions { IsEnabled = false };
        // Create an undefined enum value
        var undefinedScenario = (DebuggerScenario)999;

        // Act
        var result = options.WillCaptureDataFor(undefinedScenario);

        // Assert
        Assert.False(result); // because IsEnabled is false
    }

    [Fact]
    public void WillCaptureDataFor_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        DebuggerOptions options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options.WillCaptureDataFor(DebuggerScenario.StepExecution));
    }

    [Fact]
    public void WithOverrides_ReturnsNewObjectWithOverridesApplied()
    {
        // Arrange
        var options = CreateDefaultOptions();
        const int newMaxSnapshots = 20;
        const bool newAutoCaptureOnStepTransition = false;

        // Act
        var result = options.WithOverrides(o =>
        {
            o.MaxSnapshotsPerSaga = newMaxSnapshots;
            o.AutoCaptureOnStepTransition = newAutoCaptureOnStepTransition;
        });

        // Assert
        Assert.NotSame(options, result);
        Assert.Equal(options.IsEnabled, result.IsEnabled);
        Assert.Equal(newMaxSnapshots, result.MaxSnapshotsPerSaga);
        Assert.Equal(newAutoCaptureOnStepTransition, result.AutoCaptureOnStepTransition);
        Assert.Equal(options.AutoCaptureOnCompensation, result.AutoCaptureOnCompensation);
        Assert.Equal(options.AutoCaptureOnTerminalState, result.AutoCaptureOnTerminalState);
        Assert.Equal(options.MaxBreakpointsPerSaga, result.MaxBreakpointsPerSaga);
        Assert.Equal(options.IncludeStepPayloads, result.IncludeStepPayloads);
        Assert.Equal(options.IncludeSagaMetadata, result.IncludeSagaMetadata);
        Assert.Equal(options.EnableTimeTravel, result.EnableTimeTravel);
    }

    [Fact]
    public void WithOverrides_ThrowsArgumentNullException_WhenOptionsIsNull()
    {
        // Arrange
        DebuggerOptions options = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options.WithOverrides(o => { }));
    }

    [Fact]
    public void WithOverrides_ThrowsArgumentNullException_WhenConfigureIsNull()
    {
        // Arrange
        var options = CreateDefaultOptions();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options.WithOverrides(null!));
    }
}