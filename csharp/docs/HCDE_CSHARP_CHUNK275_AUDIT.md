# Chunk 275 — PatchWeapon ammo/decal/MBF21 tail (re-audit 265-286: tested ranges only)

## Scope

PatchWeapon ammo/decal/MBF21 tail (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchWeapon wbits table** 2244–2251 (8 lines)
- `src/gamedata/d_dehacked.cpp` **PatchWeapon tail** 2291–2437 (147 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 21).

## Managed

`VmDehackedPatchWeaponAmmo.cs`, `VmDehackedPatchWeaponAmmoTests.cs`
Managed LOC (tally): 79.
Conversion commit: `924256d7ceaef7add06dfa266429841231fb0a9f`.

## Tests

- `VmDehackedPatchWeaponAmmoTests.ApplyAmmoType_DoublesGiveAmount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 275 --to 275` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
