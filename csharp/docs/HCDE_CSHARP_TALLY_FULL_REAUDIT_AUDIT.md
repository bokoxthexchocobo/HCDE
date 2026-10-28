# HCDE C# tally full re-audit (runs 7+ and related chunks)

Self-checking `nativeLoc` is now `sum(nativeRanges)` via `sync_loc_tally` / `generate_native_managed_matrix.py`.
MigrationMatrix `LocTallyIntegrityTests` fails on range/test violations.

## Cumulative totals (after re-audit commit)

| Metric | Before (headline @ ae1b7b36) | After (range-sum synced) |
| --- | ---: | ---: |
| convertedBehaviorNative | 18,887 | 12,690 |
| recognitionBehaviorNative | 5,440 | 4,645 |
| remainingNative | 483,759 | 490,751 |
| convertedBehaviorManaged | 10,809 | 10,809 |

## Per-chunk before/after

| Chunk | Before native (headline) | Before range sum | After native | After range sum | Before managed | After managed | Notes |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| `acs-absent-callfunc-chunk11` | 684 | 84 | 84 | 84 | 500 | 500 | before headline≠ranges after synced |
| `acs-absent-opcodes-callfunc-chunk10` | 842 | 114 | 114 | 114 | 278 | 278 | before headline≠ranges after synced |
| `acs-eternity-stack-chunk17` | 22 | 22 | 22 | 22 | 52 | 52 | after synced |
| `acs-print-hud-chunk14` | 56 | 55 | 55 | 55 | 380 | 380 | before headline≠ranges after synced |
| `acs-soundsequence-spawn-facing-chunk6` | 175 | 76 | 76 | 76 | 110 | 110 | before headline≠ranges after synced |
| `acs-wire-printbind-chunk16` | 20 | 20 | 20 | 20 | 58 | 58 | after synced |
| `d-net-cutscene-runtime-chunk13` | 137 | 137 | 137 | 137 | 420 | 420 | after synced |
| `d-net-gameplay-envelope-room-recognition` | 20 | - | 5 | 5 | 0 | 0 | after synced |
| `d-net-lag-waiting-chunk15` | 37 | 37 | 37 | 37 | 120 | 120 | after synced |
| `d-net-reset-commands-chunk12` | 1528 | 148 | 148 | 148 | 180 | 180 | before headline≠ranges after synced |
| `g-game-headless-ticker-chunk9` | 1612 | 1080 | 1080 | 1080 | 116 | 116 | before headline≠ranges after synced |
| `g-level-headless-flow-chunk8` | 1588 | 1022 | 1008 | 1008 | 471 | 471 | before headline≠ranges after synced |
| `hcde-edf-thingtype-chunk18` | 88 | 147 | 97 | 97 | 108 | 108 | before headline≠ranges after synced |
| `p-enemy-chase-core-chunk3-recognition` | 1499 | 1441 | 1406 | 1406 | 0 | 0 | before headline≠ranges after synced |
| `p-enemy-chase-lost-sight-nosight-chunk4` | 23 | 23 | 23 | 23 | 8 | 8 | after synced |
| `p-enemy-move-blocked-chunk2` | 22 | 21 | 21 | 21 | 180 | 180 | before headline≠ranges after synced |
| `p-floor-waggle-donut-elevator-chunk6` | 469 | 152 | 152 | 152 | 195 | 195 | before headline≠ranges after synced |
| `p-lights-ev-neighbor-chunk5` | 68 | 68 | 68 | 68 | 54 | 54 | after synced |
| `p-lights-thinkers-chunk5-recognition` | 752 | 68 | 68 | 68 | 0 | 0 | before headline≠ranges after synced |
| `p-map-radius-thrust-chunk2` | 69 | 69 | 69 | 69 | 210 | 210 | after synced |
| `p-map-try-use-lineattack-chunk3-recognition` | 1555 | 1577 | 1577 | 1577 | 0 | 0 | before headline≠ranges after synced |
| `p-mobj-missile-spawn-live` | 81 | 81 | 81 | 81 | 75 | 75 | after synced |
| `p-mobj-movement-thinker-chunk3-recognition` | 1594 | 1838 | 1569 | 1569 | 0 | 0 | before headline≠ranges after synced |
| `p-pusher-lighttransfer-chunk6` | 524 | 224 | 224 | 224 | 175 | 175 | before headline≠ranges after synced |
| `p-saveg-acs-archive-chunk10` | 1512 | 687 | 687 | 687 | 550 | 550 | before headline≠ranges after synced |
| `p-spec-sector-friction-push-chunk4` | 97 | 113 | 113 | 113 | 62 | 62 | before headline≠ranges after synced |
| `p-switch-textures-button-chunk5` | 839 | 185 | 185 | 185 | 210 | 210 | before headline≠ranges after synced |
| `p-teleport-switch-range-chunk6` | 378 | 204 | 204 | 204 | 95 | 95 | before headline≠ranges after synced |
| `pr77-classify-actor-consolidation` | 66 | 69 | 69 | 69 | 49 | 49 | before headline≠ranges after synced |
| `zscript-vm-matrix-recognition-chunk7` | 20 | 20 | 20 | 20 | 0 | 0 | after synced |
| `zscript-vm-pointer-arith-chunk7` | 52 | 18 | 18 | 18 | 110 | 110 | before headline≠ranges after synced |

## Trims applied in this pass

- Removed overlapping native range double-counts (`g-level` RunIntermission vs `G_DoCompleted`, `p-mobj` `P_XYMovement` vs missile collide, `p-enemy` visibility vs targetable player).
- `hcde-edf-thingtype-chunk18`: `ParseThingBlock` credit narrowed to the flat property dispatch loop (977–980).
- `d-net-gameplay-envelope-room-recognition`: credited lines tied to `GameplayEnvelopeCodecTests`.
- Added missing named tests on `p-map-radius-thrust-chunk2`, `p-enemy-move-blocked-chunk2`, `p-mobj-missile-spawn-live`.

## Verified

`python3 csharp/tools/generate_native_managed_matrix.py`; `dotnet build -warnaserror`; full `dotnet test`.
