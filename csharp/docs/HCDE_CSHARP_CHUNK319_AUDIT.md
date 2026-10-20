# Chunk 319 — FindLevelInfo and FindLevelByWarpTrans

## Scope

FindLevelInfo and FindLevelByWarpTrans

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **FindWadLevelInfo FindLevelInfo** 64–97 (34 lines)
- `src/gamedata/g_mapinfo.cpp` **FindLevelByWarpTrans** 120–127 (8 lines)

**Credited native LOC:** 42 (tally `nativeLoc` 42).

## Managed

`VmMapInfoLevelLookup.cs`, `VmMapInfoLevelLookupTests.cs`
Managed LOC (tally): 62.
Conversion commit: `1f8e8ffc9fbb2e04d11196c237230c5563ba1530`.

## Tests

- `VmMapInfoLevelLookupTests.GetLevelOrDefault_SeedsDefaultSky`
- `VmMapInfoLevelLookupTests.FindLevelByWarpTrans_ScansBackwards`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 319 --to 319` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
