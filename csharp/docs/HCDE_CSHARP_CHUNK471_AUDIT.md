# Chunk 471 — P_CanResurrect virtual gate and P_CheckForResurrection corpse policy

## Scope

P_CanResurrect virtual gate and P_CheckForResurrection corpse policy

## Native ranges

- `src/playsim/p_enemy.cpp` **P_CanResurrect** 2777–2818 (42 lines)
- `src/playsim/p_enemy.cpp` **P_CheckForResurrection** 2827–2896 (70 lines)
- `src/playsim/p_enemy.cpp` **P_CheckForResurrection** 2898–2927 (30 lines)
- `src/playsim/p_enemy.cpp` **P_CheckForResurrection** 2930–2967 (38 lines)

**Credited native LOC:** 180 (tally `nativeLoc` 180).

## Managed

`VmPEnemyResurrect.cs`, `VmPEnemyResurrectTests.cs`
Managed LOC (tally): 196.
Conversion commit: `17ba0115dab225bbb7ffcd0f37b9a93914de4724`.

## Tests

- `VmPEnemyResurrectTests.CanResurrect_RejectsNullRaiser`
- `VmPEnemyResurrectTests.CanResurrect_BlocksActiveVirtual`
- `VmPEnemyResurrectTests.CanResurrect_SelfSkipsPassive`
- `VmPEnemyResurrectTests.ResurrectionCandidate_BlocksNoMoveDir`
- `VmPEnemyResurrectTests.ResurrectionCandidate_BlocksFarCorpse`
- `VmPEnemyResurrectTests.ResurrectionCandidate_RequiresSightAcrossPortals`
- `VmPEnemyResurrectTests.ResurrectionCandidate_AcceptsWhenEligible`
- `VmPEnemyResurrectTests.FriendlyTargetCleanup_ClearsStaleEnemy`
- `VmPEnemyResurrectTests.HealStatePick_UsesArchvileFallback`
- `VmPEnemyResurrectTests.CorpseDimensions_RestoreWhenNoGhostCompat`
- `VmPEnemyResurrectTests.CorpseDimensions_GhostAlphaWhenZeroHeight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 471 --to 471` (0 hard / 0 soft).

## Seams

`VMCall`, `FMultiBlockThingsIterator`, `P_CheckSight`, `P_CheckPosition`, `SetState`, `S_Sound`, and `Revive` are not hosted.

## Credit rationale

Credits dual CanResurrect virtual acceptance, resurrection probe distance, portal/3D-floor gates, friendly target cleanup, heal-state selection, and vile-ghost dimension policy from `p_enemy.cpp` (2777–2972).
