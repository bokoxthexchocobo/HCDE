# Chunk 329 — MAPINFO edata loadacs intro outro and MapFlagHandlers table head

## Scope

MAPINFO edata loadacs intro outro and MapFlagHandlers table head

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **edata loadacs intro outro** 1892–1914 (23 lines)
- `src/gamedata/g_mapinfo.cpp` **MapFlagHandlers table head** 1923–1978 (56 lines)

**Credited native LOC:** 79 (tally `nativeLoc` 79).

## Managed

`VmMapInfoFlagHandlers.cs`, `VmMapInfoFlagHandlersTests.cs`, `VmMapInfoLevelScalars.cs`, `VmMapInfoMapOptionsScript.cs`, `VmMapInfoMapOptionsScriptTests.cs`
Managed LOC (tally): 45.
Conversion commit: `9531f1c0f019d32272ee67cc657ad01404522e20`.

## Tests

- `VmMapInfoMapOptionsScriptTests.ApplyEdName_SetsField`
- `VmMapInfoMapOptionsScriptTests.ApplyIntroCutscene_SetsLump`
- `VmMapInfoFlagHandlersTests.TryGetHandler_FindsE1M8Special`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 329 --to 329` (0 hard / 0 soft).

## Seams

`ParseCutscene` is reduced to lump string assignment; `MapFlagHandlers` rows keep placeholder flag constants matching the native table layout without loading `level_info_t` flag words.

## Credit rationale

Credits the uncredited `g_mapinfo.cpp` band before the falling-damage rows: `edata`/`loadacs` string options, intro/outro cutscene lumps, the `EMIType`/`MapInfoFlagHandler` schema, and the first special/boss flag entries through `nojump` that extend the managed handler catalog tested here.
