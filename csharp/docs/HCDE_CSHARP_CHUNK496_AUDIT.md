# Chunk 496 — Whole `P_PushUp`, `P_PushDown`, and `PIT_FloorDrop` (`p_map.cpp` 6762–6939).

## Scope

Whole `P_PushUp`, `P_PushDown`, and `PIT_FloorDrop` (`p_map.cpp` 6762–6939).

## Native ranges

- `src/playsim/p_map.cpp` **P_PushUp** 6762–6821 (60 lines)
- `src/playsim/p_map.cpp` **P_PushDown** 6831–6885 (55 lines)
- `src/playsim/p_map.cpp` **PIT_FloorDrop** 6893–6939 (47 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmPMapSectorPush.cs`
Managed LOC (tally): 144.
Conversion commit: `ab7a9a726bf3d034c93034af0f6fcae7e071061a`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `PushUp_BlocksOnCeiling` / `PushUp_SkipThruActors` / `SkipSameSpecies` / `SkipReflectiveMissile` / `CannotPushIntersect` / `SkipPushIntersect` / `PushUp_KeepPositionAndCrunch` / `PushUp_CrunchAndUpdate` / `PushUp_RestoreOldZ` / `CeilingLower_AboveCeiling` / `CeilingLower_FitsBetween` / `CeilingLower_CrunchOnPushFail` / `CeilingRaise_SkipBridge` / `CeilingRaise_DoomCompatMove` / `CeilingRaise_PassobjStack` / `CeilingRaise_StackZ` / `ViewZDelta` | `P_PushUp` **6762–6821** |
| `PushDown_BlocksOnFloor` / `OnlyPushDownWhenAbove` / `StackBelow` | `P_PushDown` **6831–6885** |
| `FloorDrop_NoChange` / `FloorDrop_SkipBridge` / `FloorDrop_GravityMove` / `RelativeFloorFollow` / `LiftDropWhenNoGravityChange` / `FloorRaise_OnFloor` / `FloorRaise_MarkNoFitBridge` / `FloorRaise_UseRelativePath` / `FloorRaise_SnapToFloor` / `CeilingLower_ClampToFloor` / `CeilingLower_OnFloorAtStart` / `CeilingLower_ResetToFloor` | `PIT_FloorDrop` **6893–6939** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **162** |
| Net behavior-ported | **162** |


**Post-review ratio:** 144/162 managed/native ≈ 89% (src file LOC at HEAD; see tally).

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

Credits ceiling/floor push blocks, thru-species/reflective skips, push mass/bridge gates, and floor-drop gravity policy from `p_map.cpp` (6762–6939). Disjoint from retired **499** crush-tail slices (no active tally credit on **6822–6830**).
