# Chunk 497 — PIT_FloorRaise, PIT_CeilingLower, PIT_CeilingRaise sector thing moves

## Scope

PIT_FloorRaise, PIT_CeilingLower, PIT_CeilingRaise sector thing moves

## Native ranges

- `src/playsim/p_map.cpp` **PIT_FloorRaise** 6941–6945 (5 lines)
- `src/playsim/p_map.cpp` **PIT_FloorRaise** 6947–7007 (61 lines)
- `src/playsim/p_map.cpp` **PIT_CeilingLower** 7009–7013 (5 lines)
- `src/playsim/p_map.cpp` **PIT_CeilingLower** 7015–7061 (47 lines)
- `src/playsim/p_map.cpp` **PIT_CeilingRaise** 7063–7067 (5 lines)
- `src/playsim/p_map.cpp` **PIT_CeilingRaise** 7069–7104 (36 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 149).

## Managed

`VmPMapPitFloorCeiling.cs`, `VmPMapPitFloorCeilingTests.cs`
Managed LOC (tally): 81.
Conversion commit: `32eaa120d97606c3a92c01fb2e84cc038e8681d3`.

## Tests

- `VmPMapPitFloorCeilingTests.FloorRaise_BridgeNoFit`
- `VmPMapPitFloorCeilingTests.FloorRaise_CrunchOnCeiling`
- `VmPMapPitFloorCeilingTests.CeilingLower_TargetUsesFloor`
- `VmPMapPitFloorCeilingTests.CeilingLower_RevertWhenOnFloor`
- `VmPMapPitFloorCeilingTests.CeilingRaise_DoomCompat`
- `VmPMapPitFloorCeilingTests.CeilingRaise_PassobjStack`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 497 --to 497` (0 hard / 0 soft).

## Seams

`P_AdjustFloorCeil`, `P_PushUp`, `P_PushDown`, `P_DoCrunch`, and `P_TestMobjZ` are not hosted.

## Credit rationale

Credits floor-raise snap/relative paths, ceiling-lower clamp/push outcomes, and ceiling-raise DOOM/passobj policy from `p_map.cpp` (6947–7104).
