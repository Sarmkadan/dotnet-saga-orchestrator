# ADR 0002: Why is reverse-order the default compensation strategy?

**Status:** Accepted

## Why does `CompensationStrategy` default to `ReverseOrder`?

Because later steps frequently depend on the effects of earlier ones, so undoing them in the opposite order
of execution is the only sequence that stays consistent at every intermediate point.

Consider the order saga: charge payment -> reserve inventory -> schedule shipping. If shipping fails, the
safe unwind is cancel shipping, then release inventory, then refund payment. Releasing inventory before
cancelling the shipment could let a shipment go out against stock we have already handed back.

Reverse order (LIFO) is the standard saga default for exactly this reason: it mirrors how nested resource
acquisition is released, and it never leaves a "downstream" effect standing while its "upstream" cause has
been rolled back.

## Why keep it configurable then?

Some workflows genuinely do not have that dependency chain. Independent reservations (book a hotel, book a
car, book a flight - none depends on the others) can be compensated in any order, and `Parallel` finishes the
rollback faster. Others need `ForwardOrder` or `FromFailurePoint` for domain-specific reasons.

`GetNextCompensationByStrategy` in `CompensationService` selects the next pending compensation purely from the
strategy enum, so switching behaviour is a one-line definition change with no code path duplicated per
strategy. The default is the safe choice; the others are opt-in for teams who know their steps are
independent.
