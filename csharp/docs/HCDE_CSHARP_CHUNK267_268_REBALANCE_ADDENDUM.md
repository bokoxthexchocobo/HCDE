# Addendum: chunks 267–268 native credit rebalance

## Issue

Chunk **268** credited **183** native lines but only **66** managed lines in commit `aed28a93`. The **Bits tail** (`d_dehacked.cpp` 1721–1822) was implemented in **`VmDehackedPatchThingBits`** on chunk **267**’s code commit (`4dd14c9f`), not in 268’s finalize commit.

## Resolution

| Chunk | Change |
|------:|--------|
| **267** | Extended Bits credit from 1638–1720 to **1638–1822** (`nativeLoc` **151 → 253**). Tests list includes `ResolveTranslucentAlpha_ReturnsHalf`. `managedLoc` unchanged (**236**, measured on 267 commit files). |
| **268** | Removed Bits tail range; credit only **1831–1905** PatchThing post-loop finalize (`nativeLoc` **183 → 75**). Matches `VmDehackedPatchThingFinalize` in `aed28a93`. |

**Net conversion native LOC:** unchanged (102 lines moved from 268 to 267).

Chunk 268 is intentionally **below** the 150-line chunk target: it is a tally correction aligned to an already-landed commit, not new port work.

## Header vs per-chunk sum drift (−160 native)

Auditors summing only chunks with an explicit `"bucket": "conversion"` field under-counted by **160** native LOC versus `convertedBehaviorNative` in the tally header. Three early conversion chunks omitted the `bucket` field but were still counted by `sync_loc_tally()` / `chunk_bucket()` (implicit conversion):

| Chunk | Native LOC |
|-------|----------:|
| `p-map-radius-thrust-chunk2` | 58 |
| `p-enemy-move-blocked-chunk2` | 21 |
| `p-mobj-missile-spawn-live` | 81 |

**Fix:** set `"bucket": "conversion"` on those entries, run `python3 csharp/tools/sync_loc_tally.py`, and enforce `LocTallyHeaderTotalsMatchChunkSums` plus `LocTallyConversionChunksDeclareExplicitBucket` in `HCDE.MigrationMatrix.Tests` (prevents spreadsheet audits that filter on explicit `bucket` from drifting again).

**Chunk 276 seam:** initial `PatchPointer` credit ended at **2560**, overlapping `PatchMisc` at **2537**. Tally now credits **2439–2536** (SetPointer + PatchPointer + PatchCheats) beside PatchSound **1913–1964**.
