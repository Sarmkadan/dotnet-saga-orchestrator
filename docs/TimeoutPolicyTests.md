# TimeoutPolicyTests

Unit tests for the `TimeoutPolicy` class, verifying timeout behavior, elapsed time calculations, and policy creation with different configurations.

## API

### `Constructor_ValidSeconds_InitializesCorrectly`
Verifies that a `TimeoutPolicy` is correctly initialized when constructed with a positive timeout value in seconds.

### `Constructor_LargeTimeout_MarksAsRelaxed`
Ensures that a `TimeoutPolicy` with a large timeout (e.g., 300 seconds) is marked as relaxed.

### `Constructor_SmallTimeout_NotRelaxed`
Confirms that a `TimeoutPolicy` with a small timeout (e.g., 10 seconds) is not marked as relaxed.

### `Constructor_ZeroOrNegative_ThrowsArgumentException`
Validates that constructing a `TimeoutPolicy` with a zero or negative timeout value throws an `ArgumentException`.

### `HasExceeded_WhenElapsedEqualsTimeout_ReturnsTrue`
Checks that `HasExceeded` returns `true` when the elapsed time exactly matches the timeout.

### `HasExceeded_WhenElapsedLessThanTimeout_ReturnsFalse`
Ensures `HasExceeded` returns `false` when the elapsed time is less than the timeout.

### `HasExceeded_WhenElapsedExceedsTimeout_ReturnsTrue`
Confirms that `HasExceeded` returns `true` when the elapsed time exceeds the timeout.

### `HasExceeded_WithBuffer_AdjustsThreshold`
Tests that `HasExceeded` accounts for a buffer, adjusting the threshold accordingly.

### `HasExceeded_WithBuffer_StillBelowThreshold`
Verifies that `HasExceeded` returns `false` when the elapsed time plus buffer is still below the timeout.

### `GetRemainingTime_ReturnsCorrectTimeLeft`
Validates that `GetRemainingTime` returns the correct remaining time before timeout.

### `GetRemainingTime_AfterTimeout_ReturnsZero`
Ensures that `GetRemainingTime` returns zero after the timeout has been exceeded.

### `HasSufficientTime_WithEnoughTime_ReturnsTrue`
Checks that `HasSufficientTime` returns `true` when sufficient time remains before timeout.

### `HasSufficientTime_WithInsufficientTime_ReturnsFalse`
Confirms that `HasSufficientTime` returns `false` when insufficient time remains before timeout.

### `GetElapsedPercentage_AtStart_ReturnsNearZero`
Validates that `GetElapsedPercentage` returns a value near zero at the start of the timeout period.

### `GetElapsedPercentage_MidTimeout_ReturnsApproxFifty`
Ensures that `GetElapsedPercentage` returns approximately 50% when half the timeout has elapsed.

### `GetElapsedPercentage_AfterTimeout_ReturnsCappedAtHundred`
Confirms that `GetElapsedPercentage` returns 100% after the timeout has been exceeded.

### `CreateLenient_CreatesThreeHundredSecondPolicy`
Verifies that `CreateLenient` creates a `TimeoutPolicy` with a 300-second timeout.

### `CreateStandard_CreatesOneMinutePolicy`
Ensures that `CreateStandard` creates a `TimeoutPolicy` with a 60-second timeout.

### `CreateStrict_CreatesTenSecondPolicy`
Confirms that `CreateStrict` creates a `TimeoutPolicy` with a 10-second timeout.

### `Create_CustomSeconds_CreatesCorrectPolicy`
Validates that the `Create` method constructs a `TimeoutPolicy` with the specified timeout in seconds.

## Usage

### Example 1: Basic Timeout Policy Usage
