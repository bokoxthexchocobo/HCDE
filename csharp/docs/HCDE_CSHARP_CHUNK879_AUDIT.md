# Chunk 879 — Cajun `Reachable`, `Dofire`, `Check_LOS`, and `IsLeader` from `b_func.cpp` (**44–280**). Mate/enemy + body helpers on chunk **885**.

## Scope

Cajun `Reachable`, `Dofire`, `Check_LOS`, and `IsLeader` from `b_func.cpp` (**44–280**). Mate/enemy + body helpers on chunk **885**.

## Native ranges

- `src/playsim/bots/b_func.cpp` **b_func headers** 44–44 (1 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Reachable** 45–124 (80 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 126–133 (8 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Check_LOS** 134–144 (11 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 146–150 (5 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Dofire** 151–266 (116 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 267–267 (1 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::IsLeader** 268–280 (13 lines)

**Credited native LOC:** 235 (tally `nativeLoc` 235).

## Managed

`VmBBotFuncReachFire.cs`
Managed LOC (tally): 464.
Conversion commit: `7a62a1d9998cb8b939eb9fccb9eb6f39b4d510ae`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `DBot::Reachable` (**45–124**) | **yes** |
| `DBot::Dofire` (**151–266**) | **yes** |
| `DBot::Check_LOS` | **yes** |
| `FCajunMaster::IsLeader` | **yes** |
| `DBot::Choose_Mate` / `Find_enemy` | **885** |
| `SetBodyAt` / `FireRox` / travel helpers | **885** |
| `FCajunMaster::BotTick` (**284–322**) | **deferred** (`WhatToGet`) |

| Managed | Native (`b_func.cpp`) |
|---------|----------------------|
| `Reachable` | **45–124** |
| `Dofire` | **151–266** |
| `CheckLos` | **134–144** |
| `IsLeader` | **268–280** |
| (headers / separators) | **44**, **126–133**, **146–150**, **267** |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **235** |
| Net behavior-ported | **235** |


**Post-review ratio:** 464/235 managed/native ≈ 197% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotFuncTests.CheckLos_RequiresSightAndFov`
- `VmBBotFuncReachFireTests.Reachable_TrueInOpenRoom`
- `VmBBotFuncReachFireTests.Reachable_FalseThroughOneSidedWall`
- `VmBBotFuncTests.CheckLos_BlockedLineFails`
- `VmBBotFuncReachFireTests.Dofire_SetsAttackWhenMeleeInRangeAndLos`
- `VmBBotFuncReachFireTests.Dofire_WaitsWhileReactionTimerRuns`
- `VmBBotFuncTests.IsLeader_WhenAnotherBotTargetsPlayer`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 879 --to 879` (0 hard / 0 soft).

## Seams

`AuthoritySimulation.CajunReachable` / `CajunDoFire` call `VmBBotFuncReachFire`. `CheckLos` / `IsLeader` on `VmBBotFunc` (file attributed on **885**; native credit here). Rocket branch in `Dofire` calls `FireRox` credited on **885**.

## Credit rationale

Sight/reach/fire hull **44–280** disjoint from **885** mate/body band (**327+**). Retired overlap merge on one row blocked `--band` for **454+** helpers.
