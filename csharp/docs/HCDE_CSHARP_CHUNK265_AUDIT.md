# Chunk 265 — PatchThing setup and numeric property dispatch. (re-audit 265-286: tested ranges only)

## Scope

PatchThing setup and numeric property dispatch. (re-audit 265-286: tested ranges only)

## Native ranges

- `src/gamedata/d_dehacked.cpp` **PatchThing setup** 1234–1281 (48 lines)
- `src/gamedata/d_dehacked.cpp` **PatchThing numeric properties** 1283–1365 (83 lines)
- `src/gamedata/d_dehacked.cpp` **PatchThing No Ice Death** 1376–1386 (11 lines)
- `src/gamedata/d_dehacked.cpp` **PatchThing fast speed and melee range** 1410–1418 (9 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 27).

## Managed

`VmDehackedPatchThingContext.cs`, `VmDehackedPatchThingContextTests.cs`, `VmDehackedPatchThingNumeric.cs`, `VmDehackedPatchThingNumericTests.cs`
Managed LOC (tally): 157.
Conversion commit: `62d10e264c674a87c8ed5e3771e51ef08f6c0bca`.

## Tests

- `VmDehackedPatchThingContextTests.TryResolveThingType_RejectsOutOfRange`
- `VmDehackedPatchThingNumericTests.TryApplyNumericProperty_SetsHitPoints`
- `VmDehackedPatchThingNumericTests.TryApplyNumericProperty_AppliesTranslucency`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 265 --to 265` (0 hard / 0 soft).

## Seams

DEHSUPP / dehacked paths use callbacks for `PClass`, `FState`, and lump scanner I/O.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
