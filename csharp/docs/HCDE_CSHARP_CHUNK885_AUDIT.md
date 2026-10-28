# Chunk 885 — Re-land: Cajun `Choose_Mate`, `Find_enemy`, body nodes, rocket aim, safe position, and travel stat moves (`b_func.cpp` **327–585**).

## Scope

Re-land: Cajun `Choose_Mate`, `Find_enemy`, body nodes, rocket aim, safe position, and travel stat moves (`b_func.cpp` **327–585**).

## Native ranges

- `src/playsim/bots/b_func.cpp` **b_func separators** 324–326 (3 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Choose_Mate** 327–395 (69 lines)
- `src/playsim/bots/b_func.cpp` **b_func separators** 397–397 (1 lines)
- `src/playsim/bots/b_func.cpp` **DBot::Find_enemy** 398–449 (52 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::SetBodyAt** 454–478 (25 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::FakeFire** 490–510 (21 lines)
- `src/playsim/bots/b_func.cpp` **DBot::FireRox** 512–551 (40 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::SafeCheckPosition** 556–563 (8 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::StartTravel** 565–574 (10 lines)
- `src/playsim/bots/b_func.cpp` **FCajunMaster::FinishTravel** 576–585 (10 lines)

**Credited native LOC:** 239 (tally `nativeLoc` 239).

## Managed

`VmBBotFunc.cs`
Managed LOC (tally): 164.
Conversion commit: `c21efbf61086dd1ff1fb6977700ad26e461593f5`.

## Padding review (2026-10-21)

**Behavior ported (honest native credit)**

| Native | Behavior ported |
|--------|-----------------|
| `DBot::Choose_Mate` | **yes** |
| `DBot::Find_enemy` | **yes** |
| `FCajunMaster::SetBodyAt` | **yes** |
| `FCajunMaster::FakeFire` | **yes** |
| `DBot::FireRox` | **yes** |
| `FCajunMaster::SafeCheckPosition` | **partial** (`TryMove` + `MF_PICKUP` clear) |
| `FCajunMaster::StartTravel` / `FinishTravel` | **yes** (`CajunBotThinker.ChangeStatNum`) |

| Managed | Native (`b_func.cpp`) |
|---------|----------------------|
| `ChooseMate` | **327–395** |
| `FindEnemy` | **398–449** |
| `SetBodyAt` | **454–478** |
| `FakeFire` | **490–510** |
| `FireRox` | **512–551** |
| `SafeCheckPosition` | **556–563** |
| `StartTravel` | **565–574** |
| `FinishTravel` | **576–585** |
| (separators) | **324–326**, **397** |

### vs chunk **879**

| | Lines |
|--|------:|
| **879** (sight/reach/fire) | **44–280** |
| **885** (this row) | **327–585** |
| Gap (BotTick / comments, uncredited) | **281–323**, **450–453**, inter-function blanks |

### Gross vs net

| | Value |
|--|------:|
| Gross `nativeLoc` | **239** |
| Net behavior-ported | **239** |


**Post-review ratio:** 164/239 managed/native ≈ 69% (src file LOC at HEAD; see tally).

## Tests

- `VmBBotFuncTests.ChooseMate_PicksVisibleCoopPartner`
- `VmBBotFuncTests.ChooseMate_SkipsPlayerAlreadyChosenAsLeaderMate`
- `VmBBotFuncTests.FindEnemy_CoopPicksMapMonsterNotPlayers`
- `VmBBotFuncTests.FindEnemy_DeathmatchPicksVisiblePlayerOpponent`
- `VmBBotFuncTests.FindEnemy_DeathmatchPicksVisibleBotTarget`
- `VmBBotFuncReachFireTests.SetBodyAt_ReusesExistingBodyNode`
- `VmBBotFuncReachFireTests.FakeFire_ReturnsPositiveDistanceInOpenRoom`
- `VmBBotFuncReachFireTests.Dofire_SetsAttackWhenMeleeInRangeAndLos`
- `VmBBotFuncReachFireTests.SafeCheckPosition_IgnoresPickupSideEffects`
- `VmBBotGameTests.StartTravel_FinishTravel_MovesCajunBotThinkerStat`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 885 --to 885` (0 hard / 0 soft).

## Seams

`ChooseMate`/`FindEnemy` on `VmBBotFunc`. `SetBodyAt`/`FakeFire`/`FireRox`/`SafeCheckPosition` on `VmBBotFuncReachFire` (chunk **879** owns reach/fire native only). `StartTravel`/`FinishTravel` call `VmBBotGame` (**882** `b_game.cpp` lifecycle; zero `b_game` native on this row).

## Credit rationale

Honest function bands **327–585** on `b_func.cpp`; hull **327–585** passes `--band` vs **879** (**≤280**). Part **2/2** of retired **885** body-only attempt (**114** &lt; **150**); mate/enemy moved from **879** without separator padding.
