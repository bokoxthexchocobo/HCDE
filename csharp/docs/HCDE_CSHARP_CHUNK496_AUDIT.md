# Chunk 496 — P_PushUp, P_PushDown, and PIT_FloorDrop sector move policy

## Scope

P_PushUp, P_PushDown, and PIT_FloorDrop sector move policy

## Native ranges

- `src/playsim/p_map.cpp` **P_PushUp** 6762–6821 (60 lines)
- `src/playsim/p_map.cpp` **P_PushDown** 6831–6885 (55 lines)
- `src/playsim/p_map.cpp` **PIT_FloorDrop** 6893–6939 (47 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmPMapSectorPush.cs`, `VmPMapSectorPushTests.cs`
Managed LOC (tally): 70.
Conversion commit: `a4f822a883fb229f470462ccdd3ef8218ac62128`.

## Tests

- `VmPMapSectorPushTests.PushUp_CeilingBlock`
- `VmPMapSectorPushTests.PushUp_ThruActors`
- `VmPMapSectorPushTests.Push_SkipReflective`
- `VmPMapSectorPushTests.Push_CannotPushBridge`
- `VmPMapSectorPushTests.PushDown_FloorBlock`
- `VmPMapSectorPushTests.FloorDrop_MovesWithSector`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 496 --to 496` (0 hard / 0 soft).

## Seams

`P_FindAboveIntersectors`, `P_AdjustFloorCeil`, `P_CanCollideWith`, and `P_CheckFakeFloorTriggers` are not hosted.

## Credit rationale

Credits ceiling/floor push blocks, thru-species/reflective skips, push mass/bridge gates, and floor-drop gravity policy from `p_map.cpp` (6762–6939).
