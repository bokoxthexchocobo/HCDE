# Chunk 879 — Cajun `Check_LOS`, `IsLeader`, `Choose_Mate`, and `Find_enemy` from `b_func.cpp` (subset; **Reachable** / **Dofire** deferred).

## Scope

Cajun `Check_LOS`, `IsLeader`, `Choose_Mate`, and `Find_enemy` from `b_func.cpp` (subset; **Reachable** / **Dofire** deferred).

## Native ranges

- `src/playsim/bots/b_func.cpp` **b_func headers** 44–44 (1 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 126–133 (8 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Check_LOS** 134–144 (11 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 146–150 (5 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::IsLeader** 268–280 (13 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 324–326 (3 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Choose_Mate** 327–395 (69 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 397–397 (1 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Find_enemy** 398–449 (52 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmBBotFunc.cs`
Managed LOC (tally): 164.
Conversion commit: `d5fa317a364d718918fafc8bf1d3515b055fb6eb`.

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

### Test setup notes (879 debugging, 2026-10-23)

| Test | Setup | Native alignment |
|------|--------|------------------|
| `CheckLos_*` / `OpenRoom` | `LightLevel = 160` on sector | `Find_enemy` dark skip uses `WHATS_DARK` (**50**); open-room LOS must not false-fail when `Distance2D > DARK_DIST` (~76) with default light **0** (`b_bot.h` **57–58**) |
| `FindEnemy_DeathmatchPicksVisiblePlayerOpponent` | Two coop-spawned `PlayerPawn`s in slots; `deathmatch: true` on `FindEnemy` | Exercises DM player scan (**420–445**) without relying on DM-only map start filtering in `AuthoritySimulation.Start` |
| `FindEnemy_DeathmatchPicksVisibleBotTarget` | `PlayerPawn` vs non-friendly `BotPawn` (`AddBot`) | DM may target any shootable non-teammate in sight; invasion `BotPawn` stands in for a non-player mobj |
| `FindEnemy_CoopPicksMapMonsterNotPlayers` | Cajun `player->mo` = `PlayerPawn` at (**192**,**192**); fellow coop player + `Type` **3001** monster | Coop `Find_enemy` calls `P_RoughMonsterSearch(player->mo, 20)` (**405–407**); `IsOkayToAttack` blocks coop player-vs-player (**3996–3997** `p_mobj.cpp`) so **3001** monster wins over fellow player |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **213** |
| Net behavior-ported | **213** |

### Out-of-body uncredit (879)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **213** |
| **Out-of-body removed** | **50** |
| **Honest** | **163** |


**Post-review ratio:** 164/163 managed/native ≈ 101% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotFuncTests.CheckLos_RequiresSightAndFov`
- `VmBBotFuncTests.CheckLos_BlockedLineFails`
- `VmBBotFuncTests.IsLeader_WhenAnotherBotTargetsPlayer`
- `VmBBotFuncTests.ChooseMate_PicksVisibleCoopPartner`
- `VmBBotFuncTests.ChooseMate_SkipsPlayerAlreadyChosenAsLeaderMate`
- `VmBBotFuncTests.FindEnemy_CoopPicksMapMonsterNotPlayers`
- `VmBBotFuncTests.FindEnemy_DeathmatchPicksVisiblePlayerOpponent`
- `VmBBotFuncTests.FindEnemy_DeathmatchPicksVisibleBotTarget`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 879 --to 879` (0 hard / 0 soft).

## Seams

Direct `CombatTrace.CheckSight`, `Actor.CheckFov`, and `ActorEnemySearchActions.RoughMonsterSearch` — no host hooks. **`DBot::Reachable`** omitted until managed `FPathTraverse` is wired on `AuthoritySimulation`. **`DBot::Dofire`** omitted (`FireRox` / `SetBodyAt` / `FakeFire` not landed). **`FCajunMaster::BotTick`** omitted (`WhatToGet` not landed).

## Credit rationale

First honest `b_func.cpp` band after retired **878**; sight/mate/enemy paths unblock `ThinkForMove` re-land without duplicating **875** movement credit.
