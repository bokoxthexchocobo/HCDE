# Chunk 320 — MAPINFO map options levelnum through sky2

## Scope

MAPINFO map options levelnum through sky2

## Native ranges

- `src/gamedata/g_mapinfo.cpp` **MAPINFO levelnum through secretnext** 1083–1127 (45 lines)
- `src/gamedata/g_mapinfo.cpp` **MAPINFO cluster** 1129–1150 (22 lines)
- `src/gamedata/g_mapinfo.cpp` **MAPINFO sky1 sky2** 1152–1178 (27 lines)

**Credited native LOC:** 94 (tally `nativeLoc` 94).

## Managed

`VmMapInfoMapOptionsCoreTests.cs`
Managed LOC (tally): 58.
Conversion commit: `cae6d238f92c55ef599595e01b7ffac30fbdc91c`.

## Tests

- `VmMapInfoMapOptionsCoreTests.ApplyLevelNum_SetsField`
- `VmMapInfoMapOptionsCoreTests.ApplySecretNext_Trims`
- `VmMapInfoMapOptionsCoreTests.ApplyCluster_SetsHubForHexen`
- `VmMapInfoMapOptionsCoreTests.ApplySky1_ScalesHexenSpeed`
- `VmMapInfoMapOptionsCoreTests.ApplySky2_AssignsPic`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 320 --to 320` (0 hard / 0 soft).

## Seams

MAPINFO handlers omit live `FMapInfoParser` scanner state; managed code models option semantics only.

## Credit rationale

Only lines whose control flow and validation are exercised by the named `[Fact]` methods above are credited; scanner framing and VM hooks outside those paths are not counted.
