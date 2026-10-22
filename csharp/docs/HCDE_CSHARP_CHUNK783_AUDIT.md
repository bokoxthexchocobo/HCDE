# Chunk 783 — Terrain lookup, portal plane obstruction, portal-aware height queries, and sector friction (`p_sectors.cpp` 968–1159; follows chunk782 ending 961).

## Scope

Terrain lookup, portal plane obstruction, portal-aware height queries, and sector friction (`p_sectors.cpp` 968–1159; follows chunk782 ending 961).

## Native ranges

- `src/playsim/p_sectors.cpp` **GetTerrain** 968–971 (4 lines)
- `src/playsim/p_sectors.cpp` **GetFloorTerrain_S** 973–976 (4 lines)
- `src/playsim/p_sectors.cpp` **CheckPortalPlane** 983–992 (10 lines)
- `src/playsim/p_sectors.cpp` **HighestCeilingAt** 1000–1014 (15 lines)
- `src/playsim/p_sectors.cpp` **LowestFloorAt** 1022–1036 (15 lines)
- `src/playsim/p_sectors.cpp` **NextHighestCeilingAt** 1043–1084 (42 lines)
- `src/playsim/p_sectors.cpp` **NextLowestFloorAt** 1091–1133 (43 lines)
- `src/playsim/p_sectors.cpp` **GetFriction** 1141–1159 (19 lines)

**Credited native LOC:** 152 (tally `nativeLoc` 152).

## Managed

`VmPSectorTerrainPortalAt.cs`
Managed LOC (tally): 259.
Conversion commit: `43c6d0d5e458dca75536ffb51f1204be338474ef`.

## Padding review (2026-10-21)

| Managed | Native (`p_sectors.cpp`) |
|---------|---------------------------|
| `GetTerrain` | **968–971** |
| `GetFloorTerrainS` | **973–976** |
| `CheckPortalPlane` | **983–992** |
| `HighestCeilingAt` | **1000–1014** |
| `LowestFloorAt` | **1022–1036** |
| `NextHighestCeilingAt` | **1043–1084** |
| `NextLowestFloorAt` | **1091–1133** |
| `GetFriction` | **1141–1159** |


**Post-review ratio:** 259/152 managed/native ≈ 170% (src file LOC at HEAD; see tally).

## Tests

- `VmPSectorTerrainPortalAtTests.GetTerrain_UsesExplicitTerrainNum`
- `VmPSectorTerrainPortalAtTests.CheckPortalPlane_SetsObstructedWhenFloorAbovePortal`
- `VmPSectorTerrainPortalAtTests.HighestCeilingAt_StopsAtBlockingPortal`
- `VmPSectorTerrainPortalAtTests.NextLowestFloorAt_PicksSolid3dFloor`
- `VmPSectorTerrainPortalAtTests.GetFriction_UsesSectorFrictionWhenFlagged`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 783 --to 783` (0 hard / 0 soft).

## Seams

`TerrainTypes`/`Terrains` tables and `sector_t::Level::PointInSector` are host-provided; `F3DFloor` lists modeled as `SectorXFloor.FFloors`.

## Credit rationale

Faithful whole-function ports for terrain and portal height band after chunk782 special transfer.
