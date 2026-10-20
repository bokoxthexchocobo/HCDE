# Chunk 270 — PatchSprite, PatchAmmo, PatchWeapon head. (re-audit 265-286: tested ranges only)

## Scope

PatchSprite, PatchAmmo, PatchWeapon head. (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchSprite** 2120–2160 (41 lines)
- `src/gamedata/d_dehacked.cpp` **PatchAmmo** 2162–2242 (81 lines)
- `src/gamedata/d_dehacked.cpp` **PatchWeapon** 2253–2290 (38 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 6).

## Managed

`VmDehackedPatchAmmo.cs`, `VmDehackedPatchAmmoTests.cs`, `VmDehackedPatchSprite.cs`, `VmDehackedPatchSpriteTests.cs`, `VmDehackedPatchWeapon.cs`, `VmDehackedPatchWeaponTests.cs`
Managed LOC (tally): 149.
Conversion commit: `43b24d70992c88bc91a0f6548abdc657a4f7c159`.

## Tests

- `VmDehackedPatchSpriteTests.ComputeNameOffset_SubtractsTableBase`
- `VmDehackedPatchAmmoTests.ApplyBackpackAmounts_DoublesMax`
- `VmDehackedPatchWeaponTests.TryMapWeaponFrameLabel_SwapsSelectDeselect`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 270 --to 270` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
