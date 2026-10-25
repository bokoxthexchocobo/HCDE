# Chunk 496 — P_PushUp, P_PushDown, and PIT_FloorDrop sector move policy Retired (2026-10-24): `--band` hull overlaps existing credit (p-map-crush-tail-decal-activate-chunk499 src/playsim/p_map.cpp 6822-6830).

## Scope

P_PushUp, P_PushDown, and PIT_FloorDrop sector move policy Retired (2026-10-24): `--band` hull overlaps existing credit (p-map-crush-tail-decal-activate-chunk499 src/playsim/p_map.cpp 6822-6830).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `ee89053440a814537fbb7c1b0d9e77dc4cfec4e6`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `PushUp_BlocksOnCeiling` / `PushUp_SkipThruActors` / `SkipSameSpecies` / `SkipReflectiveMissile` / `CannotPushIntersect` / `SkipPushIntersect` / `PushUp_KeepPositionAndCrunch` / `PushUp_CrunchAndUpdate` / `PushUp_RestoreOldZ` / `CeilingLower_AboveCeiling` / `CeilingLower_FitsBetween` / `CeilingLower_CrunchOnPushFail` / `CeilingRaise_SkipBridge` / `CeilingRaise_DoomCompatMove` / `CeilingRaise_PassobjStack` / `CeilingRaise_StackZ` / `ViewZDelta` | `P_PushUp` 6762–6821 (6768: `if (thing->Top() > thing->ceilingz)`; 6770: `return 1;`; 6773: `if (thing->flags2 & MF2_THRUACTORS)`) |
| `PushDown_BlocksOnFloor` / `OnlyPushDownWhenAbove` / `StackBelow` | `P_PushDown` 6831–6885 (6837: `if (thing->Z() <= thing->floorz)`; 6839: `return 1;`; 6851: `if (thing->flags6 & MF6_THRUSPECIES && thing->GetSpecies() == interse…`) |
| `FloorDrop_NoChange` / `FloorDrop_SkipBridge` / `FloorDrop_GravityMove` / `RelativeFloorFollow` / `LiftDropWhenNoGravityChange` / `FloorRaise_OnFloor` / `FloorRaise_MarkNoFitBridge` / `FloorRaise_UseRelativePath` / `FloorRaise_SnapToFloor` / `CeilingLower_ClampToFloor` / `CeilingLower_OnFloorAtStart` / `CeilingLower_ResetToFloor` | `PIT_FloorDrop` 6893–6939 (6900: `if (oldfloorz == thing->floorz) return;`; 6901: `if (thing->flags4 & MF4_ACTLIKEBRIDGE) return; // do not move bridge …`; 6903: `if (thing->Vel.Z == 0 &&`) |

### Out-of-body uncredit (496)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **162** |
| **Out-of-body removed** | **0** |
| **Honest** | **162** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 496 --to 496` (0 hard / 0 soft).

## Seams

`P_FindAboveIntersectors`, `P_AdjustFloorCeil`, `P_CanCollideWith`, and `P_CheckFakeFloorTriggers` are not hosted.

## Credit rationale

Credits ceiling/floor push blocks, thru-species/reflective skips, push mass/bridge gates, and floor-drop gravity policy from `p_map.cpp` (6762–6939).
