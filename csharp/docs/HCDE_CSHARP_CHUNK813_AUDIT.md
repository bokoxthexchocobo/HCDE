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

## Credit rationale

Credits the still-uncredited `p_pspr.cpp` header without overlapping chunk 765.
