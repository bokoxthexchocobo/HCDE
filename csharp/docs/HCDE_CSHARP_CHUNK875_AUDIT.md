# Chunk 875 — Cajun bot movement from `b_move.cpp` (1–374).

## Scope

Cajun bot movement from `b_move.cpp` (1–374).

## Native ranges

- `src/playsim/bots/b_move.cpp` **b_move headers** 48–48 (1 lines)
- `src/playsim/bots/b_move.cpp` **b_move separators** 49–49 (1 lines)
- `src/playsim/bots/b_move.cpp` **DBot::Roam** 50–74 (25 lines)
- `src/playsim/bots/b_move.cpp` **DBot::Move** 76–134 (59 lines)
- `src/playsim/bots/b_move.cpp` **DBot::TryWalk** 136–143 (8 lines)
- `src/playsim/bots/b_move.cpp` **DBot::NewChaseDir** 145–268 (124 lines)
- `src/playsim/bots/b_move.cpp` **b_move separators** 271–276 (6 lines)
- `src/playsim/bots/b_move.cpp` **FCajunMaster::CleanAhead** 277–320 (44 lines)
- `src/playsim/bots/b_move.cpp` **DBot::TurnToAng** 326–358 (33 lines)
- `src/playsim/bots/b_move.cpp` **DBot::Pitch** 360–368 (9 lines)
- `src/playsim/bots/b_move.cpp` **b_move separators** 370–370 (1 lines)
- `src/playsim/bots/b_move.cpp` **FCajunMaster::IsDangerous** 371–374 (4 lines)

**Credited native LOC:** 315 (tally `nativeLoc` 315).

## Managed

`VmBBotMove.cs`
Managed LOC (tally): 376.
Conversion commit: `a0da488192010241c65853dd965dbd241290e3f3`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `DBot::Roam` | yes |
| `DBot::Move` | yes (`VmPSpecTestActivateLine.TestActivateLine` on spechit lines) |
| `DBot::TryWalk` | yes |
| `DBot::NewChaseDir` | yes |
| `FCajunMaster::CleanAhead` | yes |
| `DBot::TurnToAng` | yes |
| `DBot::Pitch` | yes |
| `FCajunMaster::IsDangerous` | yes |

| Managed | Native (`b_move.cpp`) |
|---------|----------------------|
| `Roam` | **50–74** |
| `Move` | **76–134** |
| `TryWalk` | **136–143** |
| `NewChaseDir` | **145–268** |
| `CleanAhead` | **277–320** |
| `TurnToAng` | **326–358** |
| `Pitch` | **360–368** |
| `IsDangerous` | **371–374** |
| (headers / RNG decl / defines / separators) | **1–48**, **49**, **75**, **135**, **144**, **269–276**, **321–325**, **359**, **369–370** |

**`CleanAhead` jump test (native order **292–310**):** `VmBBotMoveTests.CleanAhead_SetsJumpOverHighStep` uses actor **Z=10**, destination `tm.floorz=30`, `MaxStepHeight=24`, sector floor **0** — satisfies **30 > 0+24** (sets `BT_JUMP`) and **30−10 ≤ 24** (not rejected as too big a step). `CleanAhead_RejectsStepUpAboveMaxStepHeightAfterJumpBand` uses **Z=0**, `tm.floorz=26`: jump band fires but **26−0 > 24** returns **false** (port matches native sequencing).

### Gross vs net

| | Value |
|--|------:|
| `nativeLoc` | **374** |

### Out-of-body uncredit (875)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **374** |
| **Out-of-body removed** | **59** |
| **Honest** | **315** |


**Post-review ratio:** 376/315 managed/native ≈ 119% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotMoveTests.NewChaseDir_PicksEastWhenDestIsRight`
- `VmBBotMoveTests.CleanAhead_RejectsLowCeiling`
- `VmBBotMoveTests.TryWalk_SetsMovecountFromRandom`
- `VmBBotMoveTests.Roam_DecrementsMovecountAndChases`
- `VmBBotMoveTests.Move_SetsForwardRunWhenCleanAhead`
- `VmBBotMoveTests.NewChaseDir_ReturnsEarlyWhenDestNull`
- `VmBBotMoveTests.CleanAhead_SetsJumpOverHighStep`
- `VmBBotMoveTests.CleanAhead_RejectsStepUpAboveMaxStepHeightAfterJumpBand`
- `VmBBotMoveTests.TurnToAng_ClampsLargeDelta`
- `VmBBotMoveTests.Pitch_AimsAtTargetHeight`
- `VmBBotMoveTests.IsDangerous_MatchesSectorDamage`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 875 --to 875` (0 hard / 0 soft).

## Seams

`CajunReachable`, `VmBBotFuncReachFire.SafeCheckPosition`, `VmBBotFunc.CheckLos`, `VmPSpecTestActivateLine.TestActivateLine`, bot RNG on `AuthoritySimulation`. `P_CheckKeys` via inventory color keys.

## Credit rationale

Whole `b_move.cpp` TU; door/spechit branch calls off-tally `VmPSpecTestActivateLine` — **no** `p_spec.cpp` lines credited on this row (single credit per native line).
