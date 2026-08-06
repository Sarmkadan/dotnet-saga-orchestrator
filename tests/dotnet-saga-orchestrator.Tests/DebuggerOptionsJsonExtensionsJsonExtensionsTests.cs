using System.Text.Json;
using SagaOrchestrator.Configuration;
using Xunit;

namespace SagaOrchestrator.Tests
{
    public class DebuggerOptionsJsonExtensionsJsonExtensionsTests
    {
        private static DebuggerOptions CreateValidOptions() => new()
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

        [Fact]
        public void ToJson_ValidOptions_ReturnsJsonString()
        {
            var options = CreateValidOptions();
            var json = options.ToJson();
            Assert.Contains("\"isEnabled\":true", json);
            Assert.Contains("\"maxSnapshotsPerSaga\":10", json);
        }

        [Fact]
        public void ToJson_NullOptions_ThrowsArgumentNullException()
        {
            DebuggerOptions? options = null;
            Assert.Throws<ArgumentNullException>(() => options!.ToJson());
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsOptions()
        {
            var json = "{\"isEnabled\":true,\"maxSnapshotsPerSaga\":10}";
            var options = DebuggerOptionsJsonExtensions.FromJson(json);
            Assert.NotNull(options);
            Assert.True(options.IsEnabled);
            Assert.Equal(10, options.MaxSnapshotsPerSaga);
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            var json = "{\"isEnabled\":true,";
            Assert.Throws<JsonException>(() => DebuggerOptionsJsonExtensions.FromJson(json));
        }

        [Fact]
        public void FromJson_NullOrWhitespaceJson_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentNullException>(() => DebuggerOptionsJsonExtensions.FromJson(null!));
            Assert.Throws<ArgumentException>(() => DebuggerOptionsJsonExtensions.FromJson("   "));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrue()
        {
            var json = "{\"isEnabled\":true}";
            var result = DebuggerOptionsJsonExtensions.TryFromJson(json, out var options);
            Assert.True(result);
            Assert.NotNull(options);
            Assert.True(options!.IsEnabled);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            var json = "{\"isEnabled\":true,";
            var result = DebuggerOptionsJsonExtensions.TryFromJson(json, out var options);
            Assert.False(result);
            Assert.Null(options);
        }

        [Fact]
        public void TryFromJson_NullOrEmptyJson_ReturnsTrueAndNull()
        {
            var result1 = DebuggerOptionsJsonExtensions.TryFromJson(null!, out var options1);
            Assert.True(result1);
            Assert.Null(options1);

            var result2 = DebuggerOptionsJsonExtensions.TryFromJson("   ", out var options2);
            Assert.True(result2);
            Assert.Null(options2);
        }
    }
}