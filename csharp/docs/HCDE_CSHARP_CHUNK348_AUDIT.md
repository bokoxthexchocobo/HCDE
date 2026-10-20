# Chunk 348 — Exit parsers cluster tail ParseNextMap shadow cap

## Scope

Exit parsers cluster tail ParseNextMap shadow cap

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **ParseExitMusic cluster ParseNextMap** 776–1082 (307 lines)

**Credited native LOC:** 307 (tally `nativeLoc` 307).

## Managed

`VmMapInfoClusterExitTextRemap.cs`, `VmMapInfoClusterExitTextRemapTests.cs`, `VmMapInfoParseNextMap.cs`, `VmMapInfoParseNextMapTests.cs`, `VmMapInfoShadowQualityCap.cs`, `VmMapInfoShadowQualityCapTests.cs`
Managed LOC (tally): 38.
Conversion commit: `3996db610eac83f057553c15d536a5906c33f526`.

## Tests

- `VmMapInfoParseNextMapTests.FormatNumericNextMap_HexenUsesWarpTrans`
- `VmMapInfoShadowQualityCapTests.Normalize_SnapsToKnownStep`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 348 --to 348` (0 hard / 0 soft).

## Seams

Cutscene parsing and live `fileSystem` lump probes remain injectable dependencies.

## Credit rationale

Credits `g_mapinfo.cpp` 776–1082: exit music/backdrop writers, cluster definition tail with Hexen `CLUS?MSG` string-table remap, `ParseNextMap` warp/end-sequence formatting, and `NormalizeHcdeShadowQualityCap` quantization ahead of map options.
