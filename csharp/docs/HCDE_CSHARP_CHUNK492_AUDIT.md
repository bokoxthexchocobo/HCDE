# Chunk 492 — R_OffsetView chase camera, P_TalkFacing, P_UsePuzzleItem, splashfactor CVAR

## Scope

R_OffsetView chase camera, P_TalkFacing, P_UsePuzzleItem, splashfactor CVAR

## Native ranges

- `src/playsim/p_map.cpp` **R_OffsetView** 5692–5700 (9 lines)
- `src/playsim/p_map.cpp` **R_OffsetView** 5702–5744 (43 lines)
- `src/playsim/p_map.cpp` **P_TalkFacing** 5746–5753 (8 lines)
- `src/playsim/p_map.cpp` **P_TalkFacing** 5807–5821 (15 lines)
- `src/playsim/p_map.cpp` **P_TalkFacing** 5823–5827 (5 lines)
- `src/playsim/p_map.cpp` **P_UsePuzzleItem** 6037–6100 (64 lines)
- `src/playsim/p_map.cpp` **splashfactor** 6112–6118 (7 lines)

**Credited native LOC:** 151 (tally `nativeLoc` 151).

## Managed

`VmPMapOffsetTalkPuzzle.cs`, `VmPMapOffsetTalkPuzzleTests.cs`
Managed LOC (tally): 92.
Conversion commit: `33556db8fca95b4413ba97bc0b305b7244622e18`.

## Tests

- `VmPMapOffsetTalkPuzzleTests.OffsetView_OobHardwareSkipsTrace`
- `VmPMapOffsetTalkPuzzleTests.OffsetView_PortalWalkWhenNotOobHw`
- `VmPMapOffsetTalkPuzzleTests.OffsetView_FloorPortalDown`
- `VmPMapOffsetTalkPuzzleTests.TalkFacing_ThreeAngleOffsets`
- `VmPMapOffsetTalkPuzzleTests.TalkFacing_ConversationFlags`
- `VmPMapOffsetTalkPuzzleTests.PuzzleLine_MatchingType`
- `VmPMapOffsetTalkPuzzleTests.PuzzleLine_WrongSide`
- `VmPMapOffsetTalkPuzzleTests.PuzzleThing_MatchingType`
- `VmPMapOffsetTalkPuzzleTests.SplashFactor_ClampNonPositive`
- `VmPMapOffsetTalkPuzzleTests.SplashFactor_InvertsForThrust`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 492 --to 492` (0 hard / 0 soft).

## Seams

`Trace`, `P_AimLineAttack`, `NativeStartConversation`, `FPathTraverse`, and `P_StartScript` are not hosted.

## Credit rationale

Credits chase-camera offset trace/OOB/portal walks, talk-facing aim flags, puzzle line/thing matching, and splash thrust scale from `p_map.cpp` (5692–6118).
