# Chunk 779 — Ceiling surround, shortest texture, model sector, surrounding light, and highest floor point (`p_sectors.cpp` 453–711; stops before `FindLowestCeilingPoint`).

## Scope

Ceiling surround, shortest texture, model sector, surrounding light, and highest floor point (`p_sectors.cpp` 453–711; stops before `FindLowestCeilingPoint`).

## Native ranges

- `src/playsim/p_sectors.cpp` **FindLowestCeilingSurrounding** 453–486 (34 lines)
- `src/playsim/p_sectors.cpp` **FindHighestCeilingSurrounding** 492–525 (34 lines)
- `src/playsim/p_sectors.cpp` **CheckShortestTex** 537–551 (15 lines)
- `src/playsim/p_sectors.cpp` **FindShortestTextureAround** 553–566 (14 lines)
- `src/playsim/p_sectors.cpp` **FindShortestUpperAround** 578–591 (14 lines)
- `src/playsim/p_sectors.cpp` **FindModelFloorSector** 607–622 (16 lines)
- `src/playsim/p_sectors.cpp` **FindModelCeilingSector** 639–654 (16 lines)
- `src/playsim/p_sectors.cpp` **FindMinSurroundingLight** 659–672 (14 lines)
- `src/playsim/p_sectors.cpp` **FindHighestFloorPoint** 677–711 (35 lines)

**Credited native LOC:** 192 (tally `nativeLoc` 192).

## Managed

`VmPSectorCeilingTextureModel.cs`
Managed LOC (tally): 279.
Conversion commit: `43c45629e49215a7aa10b3dedd90a148cc1f533f`.

## Padding review (2026-10-21)

| Managed | Native (`p_sectors.cpp`) |
|---------|---------------------------|
| `FindLowestCeilingSurrounding` | **453–486** |
| `FindHighestCeilingSurrounding` | **492–525** |
| `CheckShortestTex` | **537–551** |
| `FindShortestTextureAround` | **553–566** |
| `FindShortestUpperAround` | **578–591** |
| `FindModelFloorSector` | **607–622** |
| `FindModelCeilingSector` | **639–654** |
| `FindMinSurroundingLight` | **659–672** |
| `FindHighestFloorPoint` | **677–711** |


**Post-review ratio:** 279/192 managed/native ≈ 145% (src file LOC at HEAD; see tally).

## Tests

- `VmPSectorCeilingTextureModelTests.FindLowestCeilingSurrounding_PrefersLowerNeighbor`
- `VmPSectorCeilingTextureModelTests.FindHighestCeilingSurrounding_PrefersHigherNeighbor`
- `VmPSectorCeilingTextureModelTests.FindShortestTextureAround_UsesSmallestLowerTexture`
- `VmPSectorCeilingTextureModelTests.FindModelFloorSector_MatchesNeighborHeight`
- `VmPSectorCeilingTextureModelTests.FindMinSurroundingLight_TakesMinimum`
- `VmPSectorCeilingTextureModelTests.FindHighestFloorPoint_FlatUsesNegD`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 779 --to 779` (0 hard / 0 soft).

## Seams

`getNextSector`, `TexMan`, `FLevelLocals::i_compatflags`, and `sector_t::floorplane` slope/`fD` access are modeled via `VmPSectorSurroundHeights` seams and injectable texture delegates.

## Credit rationale

Faithful whole-function ports for the mid `p_sectors.cpp` query band after chunk776 surround heights and before ceiling low-point helpers at 717+.
