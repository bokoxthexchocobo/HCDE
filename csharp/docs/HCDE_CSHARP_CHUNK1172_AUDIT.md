# Chunk 1172 — Floor raise, ceiling lower, ceiling raise, and `P_ChangeSector` from `p_map.cpp` (**6940–7271**).

## Scope

Floor raise, ceiling lower, ceiling raise, and `P_ChangeSector` from `p_map.cpp` (**6940–7271**).

## Native ranges

- `src/playsim/p_map.cpp` **P_ChangeSector** 6940–7271 (332 lines)

**Credited native LOC:** 332 (tally `nativeLoc` 332).

## Managed

`VmPMapSectorMove1172Slice.cs`
Managed LOC (tally): 374.
Conversion commit: `2f2cb537dc0e5061df5499b2cf47772dcd473625`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native region | Behavior ported |
|---------------|-----------------|
| `PIT_FloorRaise` (**6947–7007**) | yes |
| `PIT_CeilingLower` (**7015–7061**) | yes |
| `PIT_CeilingRaise` (**7069–7104**) | yes |
| `P_ChangeSector` (**7116–7271**) | yes |
| Band hull (**6940–7271**) | yes |

**Excluded (zero credit)**

| Native | Reason |
|--------|--------|
| **6893–6939** | `PIT_FloorDrop` is chunk **496** |
| **6523–6573** | `P_AdjustFloorCeil` is outside this band |
| **7272–7391** | decal banner through end of file stays uncredited |
| **6101–6522** | radius damage stays off the queue |
| `P_PushUp` / `P_PushDown` / `P_DoCrunch` | the push and crunch results stay host-supplied |

A floor that did not move returns immediately. A bridge inside the new floor sets no-fit and returns. An intersecting actor clears the intersector list, and an instant move adds the floor delta to the previous Z and sets the no-interpolate view flag before the actor is placed on the floor. An actor above the floor moves only when it has no gravity and is relative to the floor; otherwise it returns. Push result **1** crunches, checks the fake-floor trigger at the old Z, and updates the render list. Result **2** crunches and restores the old Z. Any other result checks the trigger and updates the render list. A player body then adds the Z change to the view Z.

A ceiling lower records whether the actor was on the floor before the adjust. A bridge whose top is through the ceiling sets no-fit and returns. Otherwise the actor is placed at ceiling minus height when that is at least the floor, and on the floor otherwise. Push results **1** and **2** put an actor that started on the floor back on the floor, then crunch and check the trigger. The view Z is updated even when the ceiling missed.

A ceiling raise reads the old Z after the adjust. A bridge returns immediately. An actor below the floor, whose top reaches the ceiling minus the move amount, and who is not marked no-lift-drop, is placed on the floor and then clamped under the ceiling. A pass-mobj actor that failed the position check and is below the ceiling is lifted onto the thing below it when that thing's Z is not above the actor, using the lower of the ceiling top and the other thing's top. The view Z uses the post-adjust Z as its baseline.

`P_ChangeSector` stores the absolute move amount and the instant flag. Attached 3D floors are skipped for a mid-texture move. A ceiling change runs the floor iterators on the attached sectors, and a floor change runs the ceiling iterators. An attached sector whose height sector is the control sector is recalculated and then skipped. The main sector is recalculated after that. Floor movement uses floor drop when the amount is negative and floor raise otherwise. Ceiling movement uses ceiling lower or ceiling raise. A mid-texture move sets the mid-texture flag and runs both pairs. Any other part returns false. Things are visited one at a time from the start of the list. A no-blockmap thing is skipped unless it moves with the sector. After a fit that is not a reset, each fake-floor thing whose sector is that fake floor updates water and checks the trigger at Z minus the signed amount. The function returns no-fit.

| Managed (`VmPMapSectorMove1172Slice.cs`) | Native (`p_map.cpp`) |
|-------------------------------------------|----------------------|
| Floor, ceiling, and sector change | **6940–7271** |
| Band hull (exclusive slice) | **6940–7271** |

### Chunk **1172** gross vs net

| | Value |
|--|------:|
| Row `nativeLoc` | **332** |
| **Net headline** | **+332** vs post-**1171** |


**Post-review ratio:** 374/332 managed/native ≈ 113% (src file LOC at HEAD; see tally).

## Tests

- `VmPMapSectorMove1172SliceTests.FloorRaise_LiftsTheIntersectingThingAndRestoresARejectedPush`
- `VmPMapSectorMove1172SliceTests.Ceiling_DropsOntoTheFloorAndRidesAFailedPass`
- `VmPMapSectorMove1172SliceTests.Change_InvertsAttachedMovementAndUpdatesFakeWater`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 1172 --to 1172` (0 hard / 0 soft).

## Seams

`P_AdjustFloorCeil`, `P_PushUp`, `P_PushDown`, `P_DoCrunch`, and `PIT_FloorDrop` stay outside this band. This slice chooses which iterator runs and what it does to the actor.

## Credit rationale

UNCredited **`p_map.cpp`** **6940–7271** (+332). Chunk **496** ends at **6939**. **7272–7391** is 120 lines and stays uncredited. A full tally line scan shows no overlap. Radius damage **6101–6522** is not this band.
