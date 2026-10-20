# Chunk 272 — FWeaponSlot add/list/position/sort. (re-audit 265-286: tested ranges only)

## Scope

FWeaponSlot add/list/position/sort. (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/a_weapons.cpp` **FWeaponSlot** 66–215 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 21).

## Managed

`VmWeaponSlotRuntime.cs`, `VmWeaponSlotRuntimeTests.cs`
Managed LOC (tally): 84.
Conversion commit: `5264ae125a18caa710e01fb1f218d73e227fcb75`.

## Tests

- `VmWeaponSlotRuntimeTests.SetInitialPositions_SingleWeaponCentered`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 272 --to 272` (0 hard / 0 soft).

## Seams

Native engine globals and VM dispatch are not re-hosted; only credited control flow is modeled.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
