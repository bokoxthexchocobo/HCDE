# Chunk 813 — `WRF_` / `ButtonChecks` and `DPSprite` flag metadata from `p_pspr.cpp` (1–162); ctor/`FindPSprite` remain on chunk 765.

## Scope

`WRF_` / `ButtonChecks` and `DPSprite` flag metadata from `p_pspr.cpp` (1–162); ctor/`FindPSprite` remain on chunk 765.

## Native ranges

- `src/playsim/p_pspr.cpp` **p_pspr header constants** 1–53 (53 lines)
- `src/playsim/p_pspr.cpp` **FGenericButtons** 54–102 (49 lines)
- `src/playsim/p_pspr.cpp` **DPSprite DEFINE_FIELD bits** 112–162 (51 lines)

**Credited native LOC:** 153 (tally `nativeLoc` 153).

## Managed

`VmPPSprWeaponReady.cs`
Managed LOC (tally): 105.
Conversion commit: `6f6172ede90fbe2347a7d3fa2472f9f4617a5a59`.

## Padding review (2026-10-21)

| Managed | Native (`p_pspr.cpp`) |
|---------|----------------------|
| `LowerSpeed` / `sv_fastweapons` | **1–53** |
| `WrfOptions` / `ButtonChecks` | **54–102** |
| `DPSprite` field bits | **112–162** |

**Post-review ratio:** 105/153 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmPPSprWeaponReadyTests.LowerSpeed_MatchesNative`
- `VmPPSprWeaponReadyTests.ButtonChecks_HasZoomReloadUserSlots`
- `VmPPSprWeaponReadyTests.PspfWeaponBobFlags_MatchNativeBits`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 813 --to 813` (0 hard / 0 soft).

## Seams

`IMPLEMENT_CLASS` / `DEFINE_FIELD` registration and `DPSprite` list mutation stay on chunk 765.

## Uncredited-range check (pre-port)

```
uncredited_range_check.py --file src/playsim/p_pspr.cpp --start 1 --end 162
  [UNCredited] cli … 1-162 (162 lines)
uncredited_range_check.py --chunk 813  (ranges 1-53, 54-102, 112-162 only)
  [UNCredited] all bands; no overlap with chunk765 (163-242)
exit 0
```

## Portfolio delta

| | Native LOC |
|---|---:|
| **Gross** (`p-pspr-header-buttons-chunk813`) | **+153** |
| **Overlap trim** | **0** (skipped **163–242** — chunk **765** `p-psprite-construct-bob-buttons-chunk765`) |
| **Net headline Δ** | **+153** (93,984 → **94,137**) |

## Land hygiene

**Compliant:** `land_chunk_fast.sh 813` end-to-end — port **`6f6172ed`**, tally **`9bb5191c`**, full `verify_full_solution_tests.sh`, push (same multi-chunk push batch as 811/812).

## Credit rationale

Credits the still-uncredited `p_pspr.cpp` header without overlapping chunk 765.
