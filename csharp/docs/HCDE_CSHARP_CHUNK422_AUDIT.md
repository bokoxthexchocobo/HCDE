# Chunk 422 — Automap line flags, camera change, thing move, quake/puzzle

## Scope

Automap line flags, camera change, thing move, quake/puzzle

## Native ranges

- `src/playsim/p_lnspec.cpp` **LS_Line_SetAutomapFlags** 2767–2798 (32 lines)
- `src/playsim/p_lnspec.cpp` **LS_Line_SetAutomapStyle** 2800–2815 (16 lines)
- `src/playsim/p_lnspec.cpp` **LS_ChangeCamera** 2817–2880 (64 lines)
- `src/playsim/p_lnspec.cpp` **LS_Thing_Move** 1849–1853 (5 lines)
- `src/playsim/p_lnspec.cpp` **LS_Radius_Quake / LS_UsePuzzleItem** 2150–2182 (33 lines)

**Credited native LOC:** 150 (tally `nativeLoc` 150).

## Managed

`VmPLnspecLineCameraLineSpecials.cs`
Managed LOC (tally): 75.
Conversion commit: `01cecaa9a7bbcdbe42abff362250cd43f7053f3b`.

## Tests

- `VmPLnspecLineCameraTests.AutomapFlags_RejectsZeroId`
- `VmPLnspecLineCameraTests.AutomapStyle_RejectsOutOfRange`
- `VmPLnspecLineCameraTests.ChangeCamera_SetsRevertCheatWhenArg2`
- `VmPLnspecLineCameraTests.ThingMove_InvertsFogWhenArg2Zero`
- `VmPLnspecLineCameraTests.PuzzleItem_RejectsNullActivator`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 422 --to 422` (0 hard / 0 soft).

## Seams

Line iterators, `R_ClearPastViewer`, inventory scan, and `P_StartQuake` are not hosted.

## Credit rationale

Credits automap flag shifts, camera revert cheat policy, `EV_Thing_Move` fog arg, and puzzle-item scan gates from `p_lnspec.cpp`.
