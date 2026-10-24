# Chunk 317 — FinishDehPatch weapon ammo processing

## Scope

FinishDehPatch weapon ammo processing

## Native ranges

- `src/gamedata/d_dehacked.cpp` **FinishDehPatch weapon ammo** 3882–3957 (76 lines)

**Credited native LOC:** 76 (tally `nativeLoc` 76).

## Managed

`VmDehackedFinishWeaponAmmo.cs`, `VmDehackedFinishWeaponAmmoTests.cs`
Managed LOC (tally): 44.
Conversion commit: `da50894699b0a25063eaa58684880b56024f0158`.

## Tests

- `VmDehackedFinishDehPatchTests.MarkStateUseWeapon_SetsFlag`
- `VmDehackedFinishWeaponAmmoTests.ResolveAmmoUseFromAttack_UsesBfgCellsWhenNegative`
- `VmDehackedFinishWeaponAmmoTests.ClearIntryMove_ClearsMf6`
- `VmDehackedFinishWeaponAmmoTests.TryProcessWeaponAmmo_ConsumeAmmoStateClearsFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 317 --to 317` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
