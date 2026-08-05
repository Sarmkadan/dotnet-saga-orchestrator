using System;
using System.Collections.Generic;
using SagaOrchestrator.Configuration;
using Xunit;

namespace dotnet_saga_orchestrator.Tests
{
    public class DebuggerOptionsValidationTests
    {
        [Fact]
        public void Validate_ValidOptions_ReturnsEmptyList()
        {
            var options = new DebuggerOptions { MaxSnapshotsPerSaga = 50, MaxBreakpointsPerSaga = 20 };
            var errors = options.Validate();
            Assert.Empty(errors);
        }

        [Fact]
        public void Validate_InvalidOptions_ReturnsErrors()
        {
            var options = new DebuggerOptions { MaxSnapshotsPerSaga = 0, MaxBreakpointsPerSaga = 101 };
            var errors = options.Validate();
            Assert.Equal(2, errors.Count);
            Assert.Contains(errors, e => e.Contains("MaxSnapshotsPerSaga must be at least 1"));
            Assert.Contains(errors, e => e.Contains("MaxBreakpointsPerSaga must be at most 100"));
        }

        [Fact]
        public void Validate_NullOptions_ThrowsArgumentNullException()
        {
            DebuggerOptions? options = null;
            Assert.Throws<ArgumentNullException>(() => options.Validate());
        }

        [Fact]
        public void IsValid_ValidOptions_ReturnsTrue()
        {
            var options = new DebuggerOptions { MaxSnapshotsPerSaga = 50, MaxBreakpointsPerSaga = 20 };
            Assert.True(options.IsValid());
        }

        [Fact]
        public void IsValid_InvalidOptions_ReturnsFalse()
        {
            var options = new DebuggerOptions { MaxSnapshotsPerSaga = 0, MaxBreakpointsPerSaga = 20 };
            Assert.False(options.IsValid());
        }

        [Fact]
        public void IsValid_NullOptions_ReturnsFalse()
        {
            DebuggerOptions? options = null;
            Assert.False(options.IsValid());
        }

        [Fact]
        public void EnsureValid_ValidOptions_DoesNotThrow()
        {
            var options = new DebuggerOptions { MaxSnapshotsPerSaga = 50, MaxBreakpointsPerSaga = 20 };
            var exception = Record.Exception(() => options.EnsureValid());
            Assert.Null(exception);
        }

        [Fact]
        public void EnsureValid_InvalidOptions_ThrowsArgumentException()
        {
            var options = new DebuggerOptions { MaxSnapshotsPerSaga = 0, MaxBreakpointsPerSaga = 20 };
            Assert.Throws<ArgumentException>(() => options.EnsureValid());
        }

        [Fact]
        public void EnsureValid_NullOptions_ThrowsArgumentNullException()
        {
            DebuggerOptions? options = null;
            Assert.Throws<ArgumentNullException>(() => options.EnsureValid());
        }
    }
}
