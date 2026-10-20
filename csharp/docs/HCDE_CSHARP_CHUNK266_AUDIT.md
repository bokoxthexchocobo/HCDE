# Chunk 266 — PatchThing blood, drop, crispy, MBF21 through obituaries. (re-audit 265-286: tested ranges only)

## Scope

PatchThing blood, drop, crispy, MBF21 through obituaries. (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchThing combat extras** 1419–1561 (143 lines)

**Credited native LOC:** 143 (tally `nativeLoc` 43).

## Managed

`VmDehackedPatchThingCombatExtras.cs`, `VmDehackedPatchThingCombatExtrasTests.cs`, `VmDehackedPatchThingDecal.cs`, `VmDehackedPatchThingDecalTests.cs`
Managed LOC (tally): 153.
Conversion commit: `2e77671ae40d696b30537becf04ffac885d4c72b`.

## Tests

- `VmDehackedPatchThingCombatExtrasTests.TryApplyLine_SetsBloodColorIndex`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 266 --to 266` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
