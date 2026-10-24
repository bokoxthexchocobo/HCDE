# Chunk 324 — MAPINFO mapbackground exit text music flat pic defaultenvironment hazard

## Scope

MAPINFO mapbackground exit text music flat pic defaultenvironment hazard

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **MAPINFO mapbackground text handlers** 1567–1608 (42 lines)
- `src/gamedata/g_mapinfo.cpp` **MAPINFO defaultenvironment header and named branch** 1610–1616 (7 lines)
- `src/gamedata/g_mapinfo.cpp` **MAPINFO defaultenvironment assign hazard** 1621–1652 (32 lines)

**Credited native LOC:** 81 (tally `nativeLoc` 81).

## Managed

`VmMapInfoLevelScalars.cs`, `VmMapInfoMapOptionsExitEnvironment.cs`, `VmMapInfoMapOptionsExitEnvironmentTests.cs`
Managed LOC (tally): 95.
Conversion commit: `a2d5b79c229688cb669590f76d328560cf99477a`.

## Tests

- `VmMapInfoMapOptionsExitEnvironmentTests.ApplyMapBackground_StoresLump`
- `VmMapInfoMapOptionsExitEnvironmentTests.MergeExitText_InsertsLookupFlag`
- `VmMapInfoMapOptionsExitEnvironmentTests.MergeExitMusic_UpdatesExistingRow`
- `VmMapInfoMapOptionsExitEnvironmentTests.MergeExitBackdrop_SetsPicFlag`
- `VmMapInfoMapOptionsExitEnvironmentTests.ResolveEnvironmentId_NumericPair`
- `VmMapInfoMapOptionsExitEnvironmentTests.ResolveEnvironmentId_UnknownNameReturnsZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 324 --to 324` (0 hard / 0 soft).

## Seams

`ParseLumpOrTextureName`, `V_GetColor`, and `S_FindEnvironment` are injected; exit-table merges mirror `FExitText` without hosting `FMapInfoParser` scanner state.

## Credit rationale

Fills the MAPINFO gap between chunk 323 (`f1`) and the spot-credited fog/light scalars on chunk 264: `mapbackground` lump assignment, the four `text*` handlers that key `ExitMapTexts` by destination map, numeric/named `defaultenvironment` ID composition, and `hazardcolor`/`hazardflash` color assignment. Does not re-credit `teamdamage` or fog/light handlers already on chunk 264.
