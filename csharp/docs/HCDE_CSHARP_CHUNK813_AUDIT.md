# Chunk 813 — RETIRED (`p_pspr.cpp` header bands; no whole-function credit)

## Whole functions claimed in original `nativeRanges`

| Symbol (tally label) | Native lines | Verdict |
|----------------------|-------------|---------|
| *(none)* | — | **No function bodies** were credited correctly. |

### Line-by-line classification of former `nativeRanges`

| Former range | Native lines | Content | Credit? |
|--------------|-------------|---------|---------|
| `p_pspr header constants` | **1–53** | License, `#include`, `LOWERSPEED` macro, `FGenericButtons` / `EWRF_Options` type definitions | **No** (not functions) |
| `FGenericButtons` | **54–102** | Struct/enum continuation, `CVAR(Int, sv_fastweapons, …)` **declaration**, `ButtonChecks[]` static initializer | **No** |
| `DPSprite DEFINE_FIELD bits` | **112–162** | `IMPLEMENT_CLASS`, `IMPLEMENT_POINTERS_*`, `DEFINE_FIELD*` / `DEFINE_FIELD_BIT*` | **No** |

### Adjacent whole functions (not in chunk 813)

| Function | Native lines | Tally |
|----------|-------------|-------|
| `DPSprite::DPSprite` | **163–219** | chunk **765** |
| `player_t::FindPSprite` | **227–242** | chunk **765** |
| `P_CreatePsprite` | **258–270** | uncredited / other rows |
| `P_SetPsprite` | **278–288** | chunk **700** / related |

**Valid whole-function native LOC for chunk 813:** **0** (floor 150 not met → **retired**).

## Uncredited-range check (pre-original port)

```
uncredited_range_check.py --file src/playsim/p_pspr.cpp --start 1 --end 162
  [UNCredited] cli 1-162 (162 lines)
exit 0
```

(UNCredited CLI band ≠ eligible conversion credit when the band is not whole functions.)

## Portfolio delta (correction)

| | Native LOC |
|---|---:|
| **Gross credited (invalid)** | **+153** |
| **Retire trim** | **−153** |
| **Net headline Δ (after retire)** | **0** (94,137 → **93,984**) |

## Land hygiene

- Original port **`6f6172ed`** + tally **`9bb5191c`** ran full `verify_full_solution_tests.sh` before push.
- **811 / 812** did **not** each complete `land_chunk_fast.sh` (reaudit stopped the script); the batched push **`f20fd1d9..9bb5191c`** ran **one** hardened verify at **`9bb5191c`** over the **combined** tree (811+812+813), so 811/812 code was gate-tested before that push, but not in isolation per chunk.

## Retire commit

`VmPPSprWeaponReady.cs` / tests removed; tally row `p-pspr-header-buttons-chunk813` set `retired: true`, `nativeLoc: 0`.
