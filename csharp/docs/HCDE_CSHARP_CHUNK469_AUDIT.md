# Chunk 469 — A_LookEx goal, sound-target, ambush, and seeyou tail (no P_LookForPlayers) Merged: [459].

## Scope

A_LookEx goal, sound-target, ambush, and seeyou tail (no P_LookForPlayers) Merged: [459].

## Native ranges

- `src/playsim/p_enemy.cpp` **A_LookEx** 2063–2094 (32 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2096–2131 (36 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2133–2145 (13 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2153–2171 (19 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2174–2197 (24 lines)
- `src/playsim/p_enemy.cpp` **A_LookEx** 2211–2244 (34 lines)

**Credited native LOC:** 158 (tally `nativeLoc` 158).

## Managed

`VmPEnemyLookEx.cs`, `VmPEnemyLookPlayers.cs`, `VmPEnemyValidLook.cs`
Managed LOC (tally): 437.
Conversion commit: `da508d1f3cd12a6e3bffbf32740c74ca1ce5786c`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|-------------------|
| `AmbushSightRequired` | `A_LookEx` 2174–2197 |
| `ChaseGoalFromArg` / `ConversationExit` / `DistInSeedRange` | `A_LookEx` 2063–2094 |
| `EvaluateEnemyAmbushSeeYou` | `A_LookEx` 2154–2230 |
| `EvaluateFriendWander` | `A_LookEx` 2153–2171 |
| `EvaluatePatrolGoal` | `A_LookEx` 2063–2094 |
| `EvaluateSeeYouGoalAndState` | `A_LookEx` 2154–2230 |
| `EvaluateSeeYouTail` | `A_LookEx` 2211–2244 |
| `EvaluateSoundTarget` | `A_LookEx` 2096–2131 |
| `FullVolumeSee` | `A_LookEx` 2211–2244 |
| `GoalReactionBlocks` | `A_LookEx` 2154–2230 |
| `HearDistanceBlocks` | `A_LookEx` 2096–2131 |
| `InchaseSkipsSee` | `A_LookEx` 2211–2244 |
| `NoJumpBlocks` | `A_LookEx` 2154–2230 |
| `NonAmbushSeesYou` | `A_LookEx` 2063–2094 |
| `NosightcheckSkipsPlayerScan` | `A_LookEx` 2154–2230 |
| `PatrolGoalSpecial` | `A_LookEx` 2063–2094 |
| `PickActorSeeStateOnJump` / `PickParamSeeStateOnJump` / `ReactiontimeBlocksGoal` / `SeeSoundPlays` | `A_LookEx` 2154–2230 |
| `ShootableTarget` | `A_LookEx` 2139 |
| `SoundTargetDead` | `A_LookEx` 2096–2131 |
| `StandstillBlocksWander` | `A_LookEx` 2153–2171 |
| `TidHateUsesTarget` / `WanderWhenNoSeeState` | `A_LookEx` 2063–2094 |
| `WanderWhenNotInchase` | `A_LookEx` 2154–2230 |
| `AmbushNeedsSight` / `BossSeeSound` | `A_Look` 1966–1968 |
| `GoalReactionGate` | `A_Look` 2032–2046 |
| `HuntPlayersRequired` / `LastEnemyRetarget` / `LookAllaround` / `MaxHearDistBlocks` | `A_Look` 1966–1968 |
| `MaxSeeDist` | `A_LookEx` 2068–2189 |
| `MinSeeDist` | `A_LookEx` 2067–2188 |
| `SinglePlayerDeadMonsters` | `A_Wander` 2291–2294 |
| `TidHateUsesLookForTid` | `A_Look` 1966–1968 |

<!-- merged from chunk459 -->
| Managed | Native (`p_enemy.cpp`) |
|---------|-------------------|
| `BlockSound` / `EmitterBlocksSound` / `EvaluateLookForEnemies` / `EvaluateLookForMonsters` / `EvaluateLookForPlayers` / `EvaluateLookForTid` / `EvaluateValidEnemyInBlock` | `ValidEnemyInBlock` 1365–1488 |
| `FriendlyChainRetarget` | `ValidEnemyInBlock` 1369–1402 |
| `FriendlyOtherEnemy` / `FriendlyRetargetShootable` | `ValidEnemyInBlock` 1413–1761 |
| `IsMonster` | `ValidEnemyInBlock` 1381 |
| `LookContinueScan` / `LookEnemySameSpecies` / `LookExFov` / `LookExMaxDist` / `LookExPitch` / `LookForEnemiesSpecies` / `LookForPlayersBot` / `LookForPlayersDead` / `LookForPlayersSpectator` / `LookForTidFriendSkip` / `LookParamsNull` | `ValidEnemyInBlock` 1365–1488 |
| `LookRejectDeadTarget` / `LookRejectDormant` | `ValidEnemyInBlock` 1369–1402 |
| `LookRejectNevertarget` / `LookRejectNonMonster` / `LookRejectNonShootable` | `ValidEnemyInBlock` 1369–1400 |
| `LookSectorEmpty` | `ValidEnemyInBlock` 1365–1488 |
| `LookTidMatch` / `LookTidShootable` | `ValidEnemyInBlock` 1369–1400 |
| `MbfSkipMutualFight` | `ValidEnemyInBlock` 1365–1488 |
| `NeverTarget` | `ValidEnemyInBlock` 1384 |
| `NoiseMarkInRange` | `ValidEnemyInBlock` 1365–1488 |
| `NonFriendlyKeepChecking` | `ValidEnemyInBlock` 1413–1761 |
| `NotSelf` | `ValidEnemyInBlock` 1381–1761 |
| `PlayerTargetable` | `ValidEnemyInBlock` 1365–1488 |
| `SeeFriendlyMonsters` | `ValidEnemyInBlock` 1413 |
| `ShootableAlive` / `SplashMarksTarget` / `ValidEnemyCorpse` | `ValidEnemyInBlock` 1365–1488 |

### Out-of-body uncredit (469)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **158** |
| **Out-of-body removed** | **0** |
| **Honest** | **158** |


**Post-review ratio:** 437/158 managed/native ≈ 277% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyLookExTests.PatrolGoal_WhenSetGoal`
- `VmPEnemyLookExTests.ConversationExit_Blocks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 469 --to 469` (0 hard / 0 soft).

## Seams

`P_LookForPlayers`, `P_CheckSight`, `SetState`, and `S_Sound` are not hosted.

## Credit rationale

Credits parameterized look setup, sound-target distance, stealth, ambush distance, and seestate tail from `p_enemy.cpp` (2063–2245).
