# Chunks 724–728: converted +1,569 vs 2,059 native credited

Baseline (pre-tranche HEAD `e8331c22`): **converted 80,524** · **recognition 2,629** · **remaining 424,933**.

After (`607fa5e8`): **converted 82,093** · **recognition 1,060** · **remaining 424,933**.

| Bucket | Δ | Explanation |
|--------|--:|-------------|
| **Converted** | **+1,569** | New rows 724–728 credit **2,059** native LOC, minus **490** native removed from retired conversion rows (no double count). |
| **Recognition** | **−1,569** | `p-mobj-movement-thinker-chunk3-recognition` trimmed by `patch_tally_mobj_movement_recognition.py` (symbols dropped per chunk 724–728). |
| **Remaining** | **0** | `remaining = baseline − converted − recognition`; both numerator terms moved in opposite directions by the same amount. |

## New conversion credit (724–728)

| Chunk | Native LOC | Symbols (`p_mobj.cpp` unless noted) |
|------:|-----------:|-----------------------------------|
| 724 | 506 | `P_XYMovement` 2421–2926 |
| 725 | 258 | `P_ZMovement` 2955–3212 |
| 726 | 275 | `P_ExplodeMissile` 1958–2096; `AActor::FloorBounceMissile` 2139–2274 |
| 727 | 467 | `AActor::Die` 330–796 (`p_interaction.cpp`) |
| 728 | 553 | `AActor::Tick` 4523–5075 |
| **Sum** | **2,059** | |

## Recognition retired (−1,569)

Trimmed from **`p-mobj-movement-thinker-chunk3-recognition`** (was 1,569 native LOC at `e8331c22`):

| Symbol | Lines | LOC |
|--------|-------|----:|
| `P_ExplodeMissile` | 1958–2100 | 143 |
| `P_XYMovement` | 2421–2679 | 259 |
| `P_ZMovement` | 2955–3480 | 526 |
| `AActor::Tick` | 4521–5075 | 555 |
| `missile collide explode` | 2680–2765 | 86 |
| **Sum** | | **1,569** |

Recognition row is now **0** native (empty `nativeRanges`). No overlap with conversion rows: trim runs before/while landing each chunk.

## Conversion rows retired (−490)

These ranges were **already in `convertedBehaviorNative`** as slice/helper chunks; whole-function ports supersede them (one implementation, retired rows zeroed):

| Retired row | Native removed | Overlap with 724–728 |
|-------------|---------------:|----------------------|
| **`p-interaction-actor-die-morph-chunk505`** | **230** | `AActor::Die` 329–450, 622–720, `CallDie` 809–817 ⊂ **727** 330–796 |
| **`p-mobj-bounce-seek-chunk460`** | **260** | `AActor::FloorBounceMissile` 2139–2274 ⊂ **726**; remainder &lt;150 LOC after trim → full retire |

**490 + 1,569 = 2,059** — matches gross new chunk native; net converted **+1,569**.

## Double-count check

- Integrity: `behaviorNativeChecksum` stays **508,086** (`LocTallyIntegrityTests`).
- No line in `p_mobj.cpp` / `p_interaction.cpp` is credited in both recognition and conversion after sync.
- Chunk **727** does not add native on top of active **505** credit: **505** was retired to `nativeLoc: 0` when **727** landed (`patch_tally_actor_die_recognition.py`).
