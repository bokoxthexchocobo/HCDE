# Chunk 469 — A_LookEx goal, sound-target, ambush, and seeyou tail (no P_LookForPlayers)

## Scope

A_LookEx goal, sound-target, ambush, and seeyou tail (no P_LookForPlayers)

## Native ranges

- `src/playsim/p_enemy.cpp` **A_LookEx** 2063–2094 (32 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2096–2131 (36 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2133–2145 (13 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2153–2171 (19 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2174–2197 (24 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2211–2244 (34 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmPEnemyLookEx.cs`, `VmPEnemyLookExTests.cs`
Managed LOC (tally): 163.
Conversion commit: `5e5d6d3ce3b43dc6449a4ae85effdbe03c61f7d9`.

## Tests

- `VmPEnemyLookExTests.PatrolGoal_WhenSetGoal`
- `VmPEnemyLookExTests.ConversationExit_Blocks`
- `VmPEnemyLookExTests.SoundTarget_DeadClears`
- `VmPEnemyLookExTests.HearDistance_BlocksFar`
- `VmPEnemyLookExTests.StealthVisdir`
- `VmPEnemyLookExTests.StandstillBlocksWander`
- `VmPEnemyLookExTests.FriendWander_PicksParamSeeState`
- `VmPEnemyLookExTests.AmbushSightRequired`
- `VmPEnemyLookExTests.EnemyAmbush_DistInRange`
- `VmPEnemyLookExTests.FullVolumeSee`
- `VmPEnemyLookExTests.InchaseSkipsSee`
- `VmPEnemyLookExTests.SeeYouTail_GoalReactionBlocks`
- `VmPEnemyLookExTests.SeeYouGoal_PicksSeeState`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 469 --to 469` (0 hard / 0 soft).

## Seams

`P_LookForPlayers`, `P_CheckSight`, `SetState`, and `S_Sound` are not hosted.

## Credit rationale

Credits parameterized look setup, sound-target distance, stealth, ambush distance, and seestate tail from `p_enemy.cpp` (2063–2245).
