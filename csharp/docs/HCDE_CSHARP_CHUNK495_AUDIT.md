# Chunk 495 — P_AdjustFloorCeil, above/below intersectors, P_DoCrunch compat head

## Scope

P_AdjustFloorCeil, above/below intersectors, P_DoCrunch compat head

## Native ranges

- `src/playsim/p_map.cpp` **P_ChangeSector** 6482–6511 (30 lines)
- `src/playsim/p_map.cpp` **P_AdjustFloorCeil** 6523–6566 (44 lines)
- `src/playsim/p_map.cpp` **P_FindAboveIntersectors** 6574–6622 (49 lines)
- `src/playsim/p_map.cpp` **P_FindBelowIntersectors** 6630–6678 (49 lines)
- `src/playsim/p_map.cpp` **P_DoCrunch** 6686–6700 (15 lines)

**Credited native LOC:** 187 (tally `nativeLoc` 187).

## Managed

`VmPMapSectorIntersectors.cs`, `VmPMapSectorIntersectorsTests.cs`
Managed LOC (tally): 73.
Conversion commit: `c2b39e795506a6d9e4ae42385d2e188a2eb12350`.

## Tests

- `VmPMapSectorIntersectorsTests.AdjustFloor_MidtexPassmobj`
- `VmPMapSectorIntersectorsTests.AdjustFloor_BridgeSkipsSync`
- `VmPMapSectorIntersectorsTests.Above_SkipsNoclip`
- `VmPMapSectorIntersectorsTests.Above_IntersectionBand`
- `VmPMapSectorIntersectorsTests.Below_IntersectionBand`
- `VmPMapSectorIntersectorsTests.Crunch_CompatSkipsCorpse`
- `VmPMapSectorIntersectorsTests.Crunch_DamageCadence`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 495 --to 495` (0 hard / 0 soft).

## Seams

`P_CheckPosition`, `P_FindFloorCeiling`, `FMultiBlockThingsIterator`, `P_DamageMobj`, and blood spawn are not hosted.

## Credit rationale

Credits 3DMidTex passmobj, bridge spawn-pos floor sync, intersector filtering, crush compat corpse skip, and crush damage cadence from `p_map.cpp` (6482–6700).
