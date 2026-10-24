# Chunk 371 — AM_CheckSecret and drawWalls line classification

## Scope

AM_CheckSecret and drawWalls line classification

## Native ranges

- `src/am_map.cpp` **AM_CheckSecret** 2315–2337 (23 lines)
- `src/am_map.cpp` **DAutomap::drawWalls** 2631–2784 (154 lines)

**Credited native LOC:** 177 (tally `nativeLoc` 177).

## Managed

`VmAutomapDrawWalls.cs`, `VmAutomapDrawWallsTests.cs`
Managed LOC (tally): 175.
Conversion commit: `bfdee9ef93c3464c2da37e8d376634a74ad43093`.

## Tests

- `VmAutomapDrawWallsTests.CheckSecret_BoomStyle_ReturnsSecretSector`
- `VmAutomapDrawWallsTests.ShouldDrawPortalPass_SkipsMapPortalGroupPass`
- `VmAutomapDrawWallsTests.ClassifyVisibleWall_OneSided_WhenNoBack`
- `VmAutomapDrawWallsTests.ClassifyUnseenWall_Revealed_DrawsNotSeen`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 371 --to 371` (0 hard / 0 soft).

## Seams

`drawMline`, portal displacement, and boundary helpers are not hosted; managed code mirrors secret checks and wall style routing.

## Credit rationale

Credits `AM_CheckSecret` and `DAutomap::drawWalls` from `am_map.cpp`.
