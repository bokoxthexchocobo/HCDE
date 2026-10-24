# Behavior-only LOC tally correction

## Findings

- Removed **567** mis-counted native lines from behavior totals: `acs-callfunc-wire-table` (142), `zscript-vm-matrix-accuracy` (45), `action-function-index` (380).
- **Combat trace ordering** (280 native) remains behavior — real `CombatTrace.cs` parity work, not merge-only verification.
- Cumulative behavior before this run: **4134** native / **2420** managed; remaining native **503952** (baseline 508086).

## Verified

- `loc_tally.json` sums match chunk table; inventory chunks carry `nativeLoc: 0`.
