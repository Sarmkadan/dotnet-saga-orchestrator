# TimeoutPolicyTestsExtensions

Utility class providing extension methods for testing `TimeoutPolicy` instances in unit tests. These methods simplify assertions around timeout behavior, elapsed time, and remaining time within test scenarios.

## API

### `GetLenientPolicy`

Returns a `TimeoutPolicy` configured with lenient timeout settings suitable for testing scenarios where slight delays should not cause failures.

- **Parameters**: None
- **Return value**: `TimeoutPolicy` – A policy with relaxed timeout thresholds.
- **Exceptions**: None

### `AssertHasExceeded(TimeoutPolicy policy)`

Asserts that the given timeout policy has exceeded its configured timeout threshold.

- **Parameters**:
  - `policy` (`TimeoutPolicy`) – The policy instance to check.
- **Return value**: None
- **Exceptions**:
  - Throws `XunitException` if the policy has not exceeded its timeout.
  - Throws `ArgumentNullException` if `policy` is `null`.

### `AssertRemainingTime(TimeoutPolicy policy, TimeSpan expected)`

Asserts that the remaining time in the given policy matches the expected duration within a small tolerance.

- **Parameters**:
  - `policy` (`TimeoutPolicy`) – The policy instance to check.
  - `expected` (`TimeSpan`) – The expected remaining time.
- **Return value**: None
- **Exceptions**:
  - Throws `XunitException` if the remaining time does not match the expected value within tolerance.
  - Throws `ArgumentNullException` if `policy` is `null`.

### `AssertElapsedPercentage(TimeoutPolicy policy, double expectedPercentage)`

Asserts that the elapsed time percentage of the policy is approximately equal to the expected value.

- **Parameters**:
  - `policy` (`TimeoutPolicy`) – The policy instance to check.
  - `expectedPercentage` (`double`) – The expected elapsed time percentage (0.0 to 1.0).
- **Return value**: None
- **Exceptions**:
  - Throws `XunitException` if the elapsed percentage is outside an acceptable tolerance.
  - Throws `ArgumentNullException` if `policy` is `null`.
  - Throws `ArgumentOutOfRangeException` if `expectedPercentage` is not between 0.0 and 1.0.

## Usage
