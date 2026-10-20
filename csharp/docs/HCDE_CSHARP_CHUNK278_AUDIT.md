# Chunk 278 — PatchMisc powerups and armor defaults (re-audit 265-286: tested ranges only)

## Scope

PatchMisc powerups and armor defaults (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchMisc armor and player tail** 2687–2836 (150 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 5).

## Managed

`VmDehackedPatchMiscArmor.cs`, `VmDehackedPatchMiscArmorTests.cs`, `VmDehackedPatchMiscPowerups.cs`, `VmDehackedPatchMiscPowerupsTests.cs`
Managed LOC (tally): 72.
Conversion commit: `d56ab9e965314e10ee7c26a35fc074e1c241c883`.

## Tests

- `VmDehackedPatchMiscArmorTests.NormalizeInfighting_MapsDd`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 278 --to 278` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
