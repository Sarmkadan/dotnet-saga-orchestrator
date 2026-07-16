# ADR 0003: How does the circuit breaker decide a service has recovered?

**Status:** Accepted

## Why a half-open probe instead of just re-closing after a timeout?

Because a timeout only tells us that *time* has passed, not that the downstream service is actually healthy.
Slamming a recovering (or still-broken) service with the full request load the instant the timeout expires is
how you turn a brief outage into a flapping outage.

The breaker (`CircuitBreaker`) has three states:

- **Closed** - requests flow normally; failures are counted.
- **Open** - once failures reach `failureThreshold`, all calls are rejected immediately for `timeoutSeconds`.
- **HalfOpen** - after the open window elapses, exactly one trial request is allowed through.

The half-open probe is the recovery test. If that single request **succeeds**, `RecordSuccess` transitions
the breaker back to Closed and resets the failure count - traffic resumes. If it **fails**, `RecordFailure`
sends the breaker straight back to Open and restarts the timeout window, so we wait again instead of
hammering.

## Why is the state re-evaluated on read, not on a timer?

`GetState` and `CanExecute` compute the Open -> HalfOpen transition lazily from `LastFailureTime` and the
configured timeout. This avoids a background timer per identifier (there can be thousands of endpoint
identifiers) and keeps the breaker allocation-light: state only changes when someone actually asks. The
trade-off is that a breaker "recovers" on the next access rather than at a precise wall-clock instant, which
is fine for a fault-tolerance guard.

Stale identifiers are reclaimed separately by `EvictStaleEntries`, called periodically, so the metrics
dictionary does not grow without bound.
