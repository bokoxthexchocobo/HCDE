# Chunk 498 — P_ChangeSector attached 3D floors, iterator dispatch, fake-floor water refresh

## Scope

P_ChangeSector attached 3D floors, iterator dispatch, fake-floor water refresh

## Native ranges

- `src/playsim/p_map.cpp` **P_ChangeSector** 7116–7175 (60 lines)
- `src/playsim/p_map.cpp` **P_ChangeSector** 7176–7237 (62 lines)
- `src/playsim/p_map.cpp` **P_ChangeSector** 7239–7271 (33 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmPMapChangeSector.cs`, `VmPMapChangeSectorTests.cs`
Managed LOC (tally): 60.
Conversion commit: `4e0a9a9bf817bf9aaafbd962f3d97673cb8401a7`.

## Tests

- `VmPMapChangeSectorTests.Change_PickFloorDrop`
- `VmPMapChangeSectorTests.Change_MoveNoblockmapUnlessFlag`
- `VmPMapChangeSectorTests.Change_MidtexDual`
- `VmPMapChangeSectorTests.Change_AbsMove`
- `VmPMapChangeSectorTests.Change_FakeFloorUpdate`
- `VmPMapChangeSectorTests.Change_SkipInvalidMode`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 498 --to 498` (0 hard / 0 soft).

## Seams

`P_Recalculate3DFloors`, sector thing lists, pit iterators, and `UpdateWaterLevel` are not hosted.

## Credit rationale

Credits move amount setup, attached-sector iterator inversion, thing visit loops, midtex dual dispatch, portal checks, and fake-floor water updates from `p_map.cpp` (7116–7271).
