# Chunk 270 — PatchSprite, PatchAmmo, PatchWeapon head. Retired (2026-10-24): `--band` hull overlaps existing credit (d-dehacked-patch-weapon-full-chunk514 src/gamedata/d_dehacked.cpp 2253-2437).

## Scope

PatchSprite, PatchAmmo, PatchWeapon head. Retired (2026-10-24): `--band` hull overlaps existing credit (d-dehacked-patch-weapon-full-chunk514 src/gamedata/d_dehacked.cpp 2253-2437).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `cd1740611f5f7b50ad08e90be408a1b1c7ef23aa`.

## Padding review (2026-10-21)



### Out-of-body uncredit (270)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **0** |
| **Honest** | **160** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 270 --to 270` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
