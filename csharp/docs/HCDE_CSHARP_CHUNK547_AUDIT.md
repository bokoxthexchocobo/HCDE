# Chunk 547 — A_SetHealth, A_ResetHealth, A_SetSpecies, A_SetChaseThreshold, CheckBlock CBF Merged: [545].

## Scope

A_SetHealth, A_ResetHealth, A_SetSpecies, A_SetChaseThreshold, CheckBlock CBF Merged: [545].

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_SetHealth** 4652–4681 (30 lines)
- `src/playsim/p_actionfunctions.cpp` **A_ResetHealth** 4690–4712 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetSpecies** 4719–4733 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetChaseThreshold** 4743–4761 (19 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckBlock** 4815–4838 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckBlock** 4844–4864 (21 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckBlock** 4871–4888 (18 lines)
- `src/playsim/p_actionfunctions.cpp` **DoRemove** 4441–4463 (23 lines)

**Credited native LOC:** 173 (tally `nativeLoc` 173).

## Managed

`VmActorHealthAndCheckBlock.cs`, `VmActorDoRemove.cs`
Managed LOC (tally): 182.
Conversion commit: `a18afbb93323f6e5a1b8899be55dfc4db779c1be`.

## Padding review (2026-10-21)

**Superseded chunk numbers:** 545 (retired; health/CheckBlock credit lives in chunk547).

**Fidelity review (2026-10-21):** Credits health/chase/CheckBlock **4652–4888** and `DoRemove` filter **4430–4463** (`p_actionfunctions.cpp`). `VmDNetClearBuffers` mirrors `Net_ClearBuffers` defaults **4337–4417** (`d_net.cpp`) at tally seam (not in `nativeRanges`).

| Managed | Native |
|---------|--------|
| `RemoveMisc` / `RemoveMonster` / `RemoveMissile` / `RemoveEverything` / `PassesRemoveFilter` | `DoRemove` **4443–4462** (`p_actionfunctions.cpp`) |
| `ShouldSkipHealthPointer` | **4660–4662** |
| `ResolveHealthAssignment` / `AssignMobjHealth` / `AssignPlayerHealth` | **4668–4678** |
| `ShouldResetPlayerHealth` / `ShouldResetMobjHealth` | **4703–4709** |
| `ShouldSkipHealthPointer` (species) | **4726–4728** |
| `ClampChaseThreshold` / `AssignChaseThreshold` | **4756–4759** |
| `MergeCheckBlockAngle` | **4821–4823** |
| `PlanCheckBlockOffset` | **4829–4837** |
| `PlanDropoffCheckFlags` | **4852–4854** |
| `CheckBlockReturnValue` | **4866–4888** |
| `ShouldApplyBlockingPointer` / `BlockingSetterIsPointer` | **4871–4879**, **4796** |
| `ClientSlotPlayingInGameAfterClearNative` | `playeringame[i]=false` **4343** (`d_net.cpp`) |
| `ClientSlotAppliedSequenceAfterClearNative` | **4355** |
| `ClientSlotAuthorityEventReplayNextIdAfterClearNative` | **4367** |
| `GlobalPendingLocalHealthRepairValidAfterClearNative` | **4405** |
| `GlobalNetGameAfterClearNative` | **4417** |
| `GlobalPredictionEndCapTicAfterClearNative` | **4408** |

<!-- merged from chunk545 -->
Constant clear-buffer defaults are faithful literals from `Net_ClearBuffers` (`d_net.cpp` 4337–4382).

| Managed | Native (`d_net.cpp`) |
|---------|-------------------|
| `ClientSlotPlayingInGameAfterClearNative` | `playingingame[i]=false` 4337–4340 |
| `ClientSlotAppliedSequenceAfterClearNative` | `AppliedSequence=-1` 4360–4361 |
| `ClientSlotAuthorityEventReplayNextIdAfterClearNative` | `AuthorityEventReplayNextId=0` 4368 |
| `GlobalNetGameAfterClearNative` | `netgame=false` 4375–4376 |
| `GlobalPendingLocalHealthRepairValidAfterClearNative` | `PendingLocalHealthRepairValid=false` 4380 |
| `GlobalPredictionEndCapTicAfterClearNative` | `PredictionEndCapTic=INT_MAX` 4388 |
| `GlobalFullLatencyCycleAfterClearNative` | `FullLatencyCycle=maxsendtics*3` 4408–4409 |

### Out-of-body uncredit (547)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **194** |
| **Out-of-body removed** | **21** |
| **Honest** | **173** |


**Post-review ratio:** 182/173 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmActorHealthAndCheckBlockTests.ResolveHealthAssignment_ClampsNonPositiveToOne`
- `VmActorHealthAndCheckBlockTests.ShouldResetPlayerHealth_WhenAlive`
- `VmActorHealthAndCheckBlockTests.ShouldSkipHealthPointer_WhenNull`
- `VmActorHealthAndCheckBlockTests.ClampChaseThreshold_NegativeBecomesZero`
- `VmActorHealthAndCheckBlockTests.PlanCheckBlockOffset_AbsolutePos`
- `VmActorHealthAndCheckBlockTests.CheckBlockReturnValue_DropoffBranch`
- `VmActorDoRemoveTests.RemoveMisc_SkipsMonsterMissileCombo`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 547 --to 547` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `SpawnHealth`, `P_CheckMove`, and `P_TestMobjLocation` are not hosted.

## Credit rationale

Credits native health/chase guards and CheckBlock angle/offset/return policy mirrored by `VmActorHealthAndCheckBlock` and tests.
