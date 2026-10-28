# Tally headline identity (`loc_tally.json`)

## Identity (unchanged baseline)

`baselineNative` is fixed at **508,086** for this matrix run (full native LOC under `src/` per generator inventory; it does not move when conversion chunks land).

Headline fields are recomputed by `sync_loc_tally()` in `generate_native_managed_matrix.py`:

```
remainingNative = baselineNative - convertedBehaviorNative - recognitionBehaviorNative
behaviorNativeChecksum = convertedBehaviorNative + recognitionBehaviorNative + remainingNative
```

`behaviorNativeChecksum` must equal `baselineNative` (hard error on sync if not).

- **convertedBehaviorNative** — union of all non-retired **conversion** `nativeRanges` per file (overlapping chunk rows do not double-count lines).
- **recognitionBehaviorNative** — sum of **recognition** chunk `nativeLoc` values (no union across recognition rows today).
- **remainingNative** — explicit subtraction, not an independent scan.

## Why 76,832 + 426,232 + **5,591** ≠ 508,086

That sum is **508,655** because **5,591** is stale recognition.

After chunks **702–703** landed whole `A_DoChase` / `P_Move`, `patch_tally_chase_recognition.py` removed the overlapping **`P_Move`** and **`A_DoChase`** bands from `p-enemy-chase-core-chunk3-recognition` (−569 recognition native). Recognition fell **5,591 → 5,022** while conversion rose **75,796 → 76,832** (+1,036 net new conversion credit for 699–703, modulo union).

| Snapshot (committed) | converted | recognition | remaining | Sum |
|---------------------|----------:|------------:|----------:|----:|
| After chunk **699** (`3d3147e9`) | 75,796 | 5,591 | 426,699 | 508,086 |
| After chunk **703** (`1a17584e`) | 76,832 | 5,022 | 426,232 | 508,086 |

`recognitionThisRunNative` is **−418** on this branch because `runStartRecognitionBehaviorNative` (5,440) predates the chase recognition trim; use **`recognitionBehaviorNative`** (5,022) for balance checks, not the run-start delta alone.

## Chunks 699–703

No double-count in conversion union: per-chunk `nativeLoc` sums to **76,832** and matches the union headline. Recognition trim is the paired bookkeeping move for chase/move VM ports, not missing baseline LOC.
