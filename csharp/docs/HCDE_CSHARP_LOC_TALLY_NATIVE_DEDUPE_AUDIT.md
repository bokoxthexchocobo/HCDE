# LOC tally native-range dedupe audit

## Policy

Each native source line may appear in **at most one** `nativeRanges` entry across all conversion and recognition chunks. When ranges overlapped, credit stays on the **earliest** chunk in `loc_tally.json` (first port / first recorded conversion).

## Method

- `csharp/tools/dedupe_loc_tally_ranges.py` walks chunks in file order, trims later ranges to uncredited lines, and splits ranges when needed.
- `csharp/tools/generate_native_managed_matrix.py` recomputed headline fields via `sync_loc_tally`.
- `LocTallyIntegrityTests.LocTallyNativeRangesDoNotOverlapAcrossChunks` guards against regressions.
- `append_tally_chunk.py` now rejects new ranges that touch an already-credited line.

## Before / after

| Metric | Before | After | Delta |
|--------|--------|-------|-------|
| Overlapping native lines | 1,166 | 0 | −1,166 |
| Extra line credits (Σ overlaps−1) | 1,171 | 0 | −1,171 |
| `convertedBehaviorNative` | 21,625 | 20,575 | −1,050 |
| `recognitionBehaviorNative` | 5,353 | 5,232 | −121 |
| `combinedBehaviorNative` | 26,978 | 25,807 | −1,171 |
| `remainingNative` | 481,108 | 482,279 | +1,171 |

`convertedBehaviorManaged` and per-chunk `managedLoc` were **not** changed; only native credit ranges were deduped.

## Chunks fully stripped of native credit

These chunks retain managed LOC and commits but no longer claim native lines (duplicate of an earlier chunk):

- `sector-changeflags-gap-audit`
- `action-dispatch-damage-wiring`
- `action-combat-dispatch-batch2`
- `thing-projectile-175-178`
- `p-map-trymove-line-cross-chunk4`
- `p-enemy-chase-lost-sight-nosight-chunk4`
- `thingdef-translation-chunk32`
- `d-net-replication-reset-chunk40`
- `p-mobj-freeze-tick-chunk47`
- `p-mobj-nointeraction-tick-chunk52`
- `p-zmovement-smooth-step-chunk63`
- `gameconfig-lastrun227-chunk134`
- `stringtable-langtags-chunk157`
- `stringtable-binary-default-chunk163`
- `stringtable-gender-macros-chunk164`
- `tgatexture-chunk101-complete`

## Largest partial trims (native LOC removed)

| Chunk | Lines removed |
|-------|----------------|
| `action-combat-dispatch-batch2` | 208 |
| `tgatexture-chunk101-complete` | 125 |
| `p-acs-callfunc-gameplay-band-chunk66-recognition` | 93 |
| `p-pusher-lighttransfer-chunk6` | 52 |
| `p-mobj-nointeraction-tick-chunk52` | 51 |
| `pngtexture-chunk111-complete` | 49 |
| `thingdef-translation-chunk32` | 44 |
| `p-map-trymove-line-cross-chunk4` | 43 |
| `pcxtexture-chunk100-complete` | 41 |

(Full machine log: run `python3 csharp/tools/dedupe_loc_tally_ranges.py` on a pre-dedupe tree; this repo is post-dedupe.)

## Verification

`dotnet build csharp/HCDE.sln -warnaserror`  
`dotnet test csharp/HCDE.sln --filter "FullyQualifiedName~LocTallyIntegrityTests"`
