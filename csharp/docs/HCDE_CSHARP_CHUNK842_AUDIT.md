# Chunk 842 — Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **120** (< **150** floor). Prior tally **178** included banned separator padding. Matrix credit **0**.

## Scope

Retired (2026-10-23): separator `nativeRanges` removed per policy; honest whole-function credit **120** (< **150** floor). Prior tally **178** included banned separator padding. Matrix credit **0**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `df333f73fe95a63812a4d7ac7a6bc3e6eccb70c2`.

## Padding review (2026-10-21)

### Separator uncredit → retire

| | `nativeLoc` |
|--|----------:|
| **Before** (incl. separators) | **178** |
| **After uncredit** (functions only) | **120** |
| **After retire** (matrix) | **0** |

Removed **58** separator lines across **8** `nativeRanges` entries.


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 842 --to 842` (0 hard / 0 soft).

## Seams

No new `ParserContext` Action/Func forwarders. FraggleScript `actorvalue` mobjs map to managed `Actor` (`Thrust`, `ReactionTime`, `VelocityX/Y/Z`, `SetTargetPointer`/`SeeState`/`JustHit`, `Simulation` for target read). In-band trig: `SfPointToAngle` / `SfPointToDist`.

## Credit rationale

Separator padding between `SF_*` bands must not count toward `nativeLoc`. After uncredit, band is below floor → retire (behavior may remain off-tally).
