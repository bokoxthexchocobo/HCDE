# Chunk 318 — G_ParseMapInfo lump loop and secret marking

## Scope

G_ParseMapInfo lump loop and secret marking

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **G_ParseMapInfo lump dispatch** 3067–3086 (20 lines)
- `src/gamedata/g_mapinfo.cpp` **G_ParseMapInfo post parse** 3089–3114 (26 lines)

**Credited native LOC:** 46 (tally `nativeLoc` 46).

## Managed

`VmMapInfoParseMapInfoLoop.cs`, `VmMapInfoParseMapInfoLoopTests.cs`
Managed LOC (tally): 48.
Conversion commit: `d815d739f5147248fc8a1911d332727334b2c85b`.

## Tests

- `VmMapInfoParseMapInfoLoopTests.ResolveParseAction_UmapinfoBranch`
- `VmMapInfoParseMapInfoLoopTests.ResolveParseAction_EmapinfoCommitsFirst`
- `VmMapInfoParseMapInfoLoopTests.ShouldCollectSecretMap_WhenDistinct`
- `VmMapInfoParseMapInfoLoopTests.ValidateEpisodesAndSkills_ThrowsWhenEmpty`
- `VmMapInfoParseMapInfoLoopTests.ApplySecretLevelFlag_SetsBit`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 318 --to 318` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
