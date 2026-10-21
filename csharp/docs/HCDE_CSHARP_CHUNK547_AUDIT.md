# Chunk 547 — A_SetHealth, A_ResetHealth, A_SetSpecies, A_SetChaseThreshold, CheckBlock CBF

## Scope

A_SetHealth, A_ResetHealth, A_SetSpecies, A_SetChaseThreshold, CheckBlock CBF

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **A_SetHealth** 4652–4681 (30 lines)
- `src/playsim/p_actionfunctions.cpp` **A_ResetHealth** 4690–4712 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetSpecies** 4719–4733 (15 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetChaseThreshold** 4743–4761 (19 lines)
- `src/playsim/p_actionfunctions.cpp` **CBF** 4790–4801 (12 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckBlock** 4815–4838 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckBlock** 4844–4864 (21 lines)
- `src/playsim/p_actionfunctions.cpp` **CheckBlock** 4871–4888 (18 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmActorHealthAndCheckBlock.cs`
Managed LOC (tally): 94.
Conversion commit: `9933a68926060be3af849789f4d3496755b2e7af`.

## Tests

- `VmActorHealthAndCheckBlockTests.ResolveHealthAssignment_ClampsNonPositiveToOne`
- `VmActorHealthAndCheckBlockTests.ShouldResetPlayerHealth_WhenAlive`
- `VmActorHealthAndCheckBlockTests.ShouldSkipHealthPointer_WhenNull`
- `VmActorHealthAndCheckBlockTests.ClampChaseThreshold_NegativeBecomesZero`
- `VmActorHealthAndCheckBlockTests.CheckBlockReturnValue_DropoffBranch`
- `VmActorHealthAndCheckBlockTests.PlanCheckBlockOffset_AbsolutePos`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 547 --to 547` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `SpawnHealth`, `P_CheckMove`, and `P_TestMobjLocation` are not hosted.

## Credit rationale

Credits native health/chase guards and CheckBlock angle/offset/return policy mirrored by `VmActorHealthAndCheckBlock` and tests.
