# Chunk 497 — PIT_FloorRaise, PIT_CeilingLower, PIT_CeilingRaise sector thing moves Retired (2026-10-23): out-of-body credit removed; honest **144** < **150**.

## Scope

PIT_FloorRaise, PIT_CeilingLower, PIT_CeilingRaise sector thing moves Retired (2026-10-23): out-of-body credit removed; honest **144** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `d2180d8f92269bce01771ed0629912754b72cd1e`.

## Padding review (2026-10-21)

| Managed | Native (`p_map.cpp`) |
|---------|-------------------|
| `FloorRaise_UnchangedFloor` / `FloorRaise_OnRidingFloor` / `FloorRaise_RestoreZ` / `FloorRaise_TakeRidingBranch` | `PIT_FloorRaise` 6941–6945 |
| `FloorRaise_BridgeBlocks` / `FloorRaise_RelativeNoGravity` / `FloorRaise_AboveFloorNoMove` / `FloorRaise_CrunchDefault` / `PlayerViewMo` / `ViewOffset` / `FloorRaise_TakeRelativeBranch` | `PIT_FloorRaise` 6947–7007 (6954: `if (oldfloorz == thing->floorz) return;`; 6957: `if (thing->Z() <= thing->floorz)`; 6959: `if (thing->flags4 & MF4_ACTLIKEBRIDGE)`) |
| `CeilingLower_TargetZ` | `PIT_CeilingLower` 7009–7013 |
| `CeilingLower_TopAboveCeiling` / `CeilingLower_BridgeBlocks` / `CeilingLower_FitHeight` / `CeilingLower_StartedOnFloor` / `CeilingLower_CrunchPaths` / `CeilingLower_RevertToFloor` / `CeilingLower_NeedsPush` | `PIT_CeilingLower` 7015–7061 (7023: `if (thing->Top() > thing->ceilingz)`; 7025: `if (thing->flags4 & MF4_ACTLIKEBRIDGE)`; 7028: `return; // do not move bridge things`) |
| `CeilingRaise_PassobjRelocate` | `PIT_CeilingRaise` 7063–7067 |
| `CeilingRaise_SkipBridge` / `CeilingRaise_DoomMove` / `MinStackZ` / `CeilingRaise_AdjustFailed` | `PIT_CeilingRaise` 7069–7106 (7074: `if (thing->flags4 & MF4_ACTLIKEBRIDGE) return; // do not move bridge …`; 7079: `if (thing->Z() < thing->floorz &&`; 7084: `if (thing->Top() > thing->ceilingz)`) |

### Out-of-body uncredit (497)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **161** |
| **Out-of-body removed** | **17** |
| **Honest** | **144** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 497 --to 497` (0 hard / 0 soft).

## Seams

`P_AdjustFloorCeil`, `P_PushUp`, `P_PushDown`, `P_DoCrunch`, and `P_TestMobjZ` are not hosted.

## Credit rationale

Credits floor-raise snap/relative paths, ceiling-lower clamp/push outcomes, and ceiling-raise DOOM/passobj policy from `p_map.cpp` (6947–7104).
