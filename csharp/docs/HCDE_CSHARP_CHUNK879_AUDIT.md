# Chunk 879 — Cajun `Check_LOS`, `IsLeader`, `Choose_Mate`, and `Find_enemy` from `b_func.cpp` (subset; **Reachable** / **Dofire** deferred).

## Scope

Cajun `Check_LOS`, `IsLeader`, `Choose_Mate`, and `Find_enemy` from `b_func.cpp` (subset; **Reachable** / **Dofire** deferred).

## Native ranges

- `src/playsim/bots/b_func.cpp` **b_func headers** 1–44 (44 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 125–133 (9 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Check_LOS** 134–144 (11 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 145–150 (6 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 267–267 (1 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::IsLeader** 268–280 (13 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 281–283 (3 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 324–326 (3 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Choose_Mate** 327–395 (69 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 396–397 (2 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Find_enemy** 398–449 (52 lines)

**Credited native LOC:** 213 (tally `nativeLoc` 213).

## Managed

`VmBBotFunc.cs`
Managed LOC (tally): 164.
Conversion commit: `99a6720f4c8fe7b6099d8211da922f013f8d30ae`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `DBot::Check_LOS` | **yes** |
| `FCajunMaster::IsLeader` | **yes** |
| `DBot::Choose_Mate` | **yes** |
| `DBot::Find_enemy` | **yes** (DM scan + coop `P_RoughMonsterSearch` via `ActorEnemySearchActions`) |
| `DBot::Reachable` (**45–124**) | **no** (`FPathTraverse` not wired) |
| `DBot::Dofire` (**151–266**) | **no** (`FireRox` / body nodes) |
| `FCajunMaster::BotTick` (**284–322**) | **no** (`WhatToGet`) |

| Managed | Native (`b_func.cpp`) |
|---------|----------------------|
| `CheckLos` | **134–144** |
| `IsLeader` | **268–280** |
| `ChooseMate` | **327–395** |
| `FindEnemy` | **398–449** |
| (headers / `pr_botdofire` decl / separators) | **1–44**, **125–133**, **145–150**, **267**, **281–283**, **324–326**, **396–397** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **213** |
| Net behavior-ported | **213** |

**Post-review ratio:** 164/213 managed/native ≈ 77% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotFuncTests.CheckLos_RequiresSightAndFov`
- `VmBBotFuncTests.CheckLos_BlockedLineFails`
- `VmBBotFuncTests.IsLeader_WhenAnotherBotTargetsPlayer`
- `VmBBotFuncTests.ChooseMate_PicksVisibleCoopPartner`
- `VmBBotFuncTests.ChooseMate_SkipsPlayerAlreadyChosenAsLeaderMate`
- `VmBBotFuncTests.FindEnemy_DeathmatchPicksClosestVisibleOpponent`
- `VmBBotFuncTests.FindEnemy_CoopUsesRoughMonsterSearch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 879 --to 879` (0 hard / 0 soft).

## Seams

Direct `CombatTrace.CheckSight`, `Actor.CheckFov`, and `ActorEnemySearchActions.RoughMonsterSearch` — no host hooks. **`DBot::Reachable`** omitted until managed `FPathTraverse` is wired on `AuthoritySimulation`. **`DBot::Dofire`** omitted (`FireRox` / `SetBodyAt` / `FakeFire` not landed). **`FCajunMaster::BotTick`** omitted (`WhatToGet` not landed).

## Credit rationale

First honest `b_func.cpp` band after retired **878**; sight/mate/enemy paths unblock `ThinkForMove` re-land without duplicating **875** movement credit.
