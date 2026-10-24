# Chunk 349 — DeHackEd lookup tables sprite map and section modes

## Scope

DeHackEd lookup tables sprite map and section modes

## Native ranges

- `src/gamedata/d_dehacked.cpp` **FindInfoName through Modes table** 118–462 (345 lines)

**Credited native LOC:** 345 (tally `nativeLoc` 345).

## Managed

`VmDehackedLookupCore.cs`, `VmDehackedLookupCoreTests.cs`, `VmDehackedPatchModes.cs`, `VmDehackedPatchModesTests.cs`, `VmDehackedSpriteClassMap.cs`
Managed LOC (tally): 64.
Conversion commit: `102214eeceef2c793fa646aac7c714c205c28a42`.

## Tests

- `VmDehackedLookupCoreTests.DsSoundLumpName_PrefixesDs`
- `VmDehackedPatchModesTests.IsKnownSection_RejectsUnknown`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 349 --to 349` (0 hard / 0 soft).

## Seams

VM `PClassActor` creation for DSDHacked indices and live `soundEngine` mutation are not hosted; managed layer mirrors naming and default `DehInfo` scalars.

## Credit rationale

Credits uncredited `d_dehacked.cpp` 118–462: `FindInfoName`/`DehFindSound` resolution, sprite remap bookkeeping, default `deh` struct constants, `DehSpriteMappings`, and the `Modes[]` section dispatch table used by `DoDehPatch`.
