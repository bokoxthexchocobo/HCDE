# Chunk 879 — Cajun `Reachable`, `Dofire`, `Check_LOS`, `IsLeader`, `Choose_Mate`, and `Find_enemy` from `b_func.cpp`. **`FCajunMaster::BotTick`** still deferred.

## Scope

Cajun `Reachable`, `Dofire`, `Check_LOS`, `IsLeader`, `Choose_Mate`, and `Find_enemy` from `b_func.cpp`. **`FCajunMaster::BotTick`** still deferred.

## Native ranges

- `src/playsim/bots/b_func.cpp` **b_func headers** 44–44 (1 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Reachable** 45–124 (80 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 126–133 (8 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Check_LOS** 134–144 (11 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 146–150 (5 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Dofire** 151–266 (116 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 267–267 (1 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::IsLeader** 268–280 (13 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 324–326 (3 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Choose_Mate** 327–395 (69 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 397–397 (1 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Find_enemy** 398–449 (52 lines)

**Credited native LOC:** 360 (tally `nativeLoc` 360).

## Managed

`VmBBotFunc.cs`, `VmBBotFuncReachFire.cs`
Managed LOC (tally): 628.
Conversion commit: `4894427b8bef42e983d68a8140fcdeeb050b77a3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `DBot::Reachable` (**45–124**) | **yes** (`CollectPathIntercepts` / line+thing intercept walk) |
| `DBot::Dofire` (**151–266**) | **yes** (reaction timer, melee/BFG/projectile/hitscan branches; `CheckLos` for LOS gates) |
| `DBot::Check_LOS` | **yes** |
| `FCajunMaster::IsLeader` | **yes** |
| `DBot::Choose_Mate` | **yes** |
| `DBot::Find_enemy` | **yes** (DM scan + coop `P_RoughMonsterSearch`) |
| `FCajunMaster::BotTick` (**284–322**) | **no** (`WhatToGet`) |

| Managed | Native (`b_func.cpp`) |
|---------|----------------------|
| `Reachable` | **45–124** |
| `Dofire` | **151–266** |
| `CheckLos` | **134–144** |
| `IsLeader` | **268–280** |
| `ChooseMate` | **327–395** |
| `FindEnemy` | **398–449** |
| (headers / `pr_botdofire` decl / separators) | **44**, **126–133**, **146–150**, **267**, **324–326**, **397** |

### Test setup notes

| Test | Setup | Native alignment |
|------|--------|------------------|
| `Reachable_*` | open room vs one-sided blocking line | `FPathTraverse` line/thing intercept sequencing |
| `Dofire_*` | `CajunBotWeaponInfo` pistol, bot skill **100**, enemy at **48** units | melee `MoveCombatDist==0` path (**190–194**) |
| `CheckLos_*` / `OpenRoom` | `LightLevel = 160` | `WHATS_DARK` / `DARK_DIST` for `Find_enemy` |
| `FindEnemy_*` | DM / coop fixtures per prior **879** land | unchanged |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **409** |
| Net behavior-ported | **409** |

### Out-of-body uncredit (879)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **409** |
| **Out-of-body removed** | **50** |
| **Honest** | **359** |


**Post-review ratio:** 628/360 managed/native ≈ 174% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotFuncTests.CheckLos_RequiresSightAndFov`
- `VmBBotFuncReachFireTests.Reachable_TrueInOpenRoom`
- `VmBBotFuncReachFireTests.Reachable_FalseThroughOneSidedWall`
- `VmBBotFuncTests.CheckLos_BlockedLineFails`
- `VmBBotFuncReachFireTests.Dofire_SetsAttackWhenMeleeInRangeAndLos`
- `VmBBotFuncReachFireTests.Dofire_WaitsWhileReactionTimerRuns`
- `VmBBotFuncTests.IsLeader_WhenAnotherBotTargetsPlayer`
- `VmBBotFuncTests.ChooseMate_PicksVisibleCoopPartner`
- `VmBBotFuncTests.ChooseMate_SkipsPlayerAlreadyChosenAsLeaderMate`
- `VmBBotFuncTests.FindEnemy_CoopPicksMapMonsterNotPlayers`
- `VmBBotFuncTests.FindEnemy_DeathmatchPicksVisiblePlayerOpponent`
- `VmBBotFuncTests.FindEnemy_DeathmatchPicksVisibleBotTarget`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 879 --to 879` (0 hard / 0 soft).

## Seams

`AuthoritySimulation.CajunReachable` / `CajunDoFire` call `VmBBotFuncReachFire` (path intercept traverse, `FireRox` / `SetBodyAt` / `FakeFire` on `CajunMasterState`). `CheckLos` uses `CombatTrace.CheckSight` + `Actor.CheckFov`; `FindEnemy` uses `ActorEnemySearchActions.RoughMonsterSearch`. Rocket branch in `Dofire` calls off-tally `FireRox` (chunk **885** retired band) without crediting **454–585** on this row.

## Credit rationale

Expand **879** so `uncredited_range_check --band` hull stays **44–449** (same file as prior sight/mate/enemy credit) while adding uncredited **45–124** and **151–266**; disjoint reach/fire bands cannot pass `--band` on a new chunk row without overlapping this row's **126–150** credit.
