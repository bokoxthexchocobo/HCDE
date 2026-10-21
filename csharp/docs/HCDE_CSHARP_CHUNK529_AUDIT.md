# Chunk 529 — AActor::CheckIfTargetInLOS, CheckIfInTargetLOS

## Scope

AActor::CheckIfTargetInLOS, CheckIfInTargetLOS

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::CheckIfTargetInLOS** 2551–2678 (128 lines)
- `src/playsim/p_actionfunctions.cpp` **AActor::CheckIfInTargetLOS** 2688–2760 (73 lines)

**Credited native LOC:** 201 (tally `nativeLoc` 201).

## Managed

`VmActionCheckTargetInLos.cs`, `VmActionCheckTargetInLosTests.cs`
Managed LOC (tally): 204.
Conversion commit: `511896439302c390839eedb3ab7e9140b89f7f2e`.

## Tests

- `VmActionCheckTargetInLosTests.Plan_NonPlayer_NoTarget_Fails`
- `VmActionCheckTargetInLosTests.Plan_CombatantOnly_RejectsNonMonster`
- `VmActionCheckTargetInLosTests.Plan_AllyNoJump_BlocksFriends`
- `VmActionCheckTargetInLosTests.Plan_CloseDistance_NoJump_Fails`
- `VmActionCheckTargetInLosTests.Plan_InTargetLos_FovOutside_Fails`
- `VmActionCheckTargetInLosTests.ResolveNonPlayer_ProjectileNonSeeker_NoTarget`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 529 --to 529` (0 hard / 0 soft).

## Seams

`P_AimLineAttack`, `P_CheckSight`, `Distance3D`, and `ACTION_RETURN_BOOL` are not hosted.

## Credit rationale

Credits JLOSF target resolution, player aim branches, distance/close gates, viewport swap, and FOV/sight planning from `p_actionfunctions.cpp` (2551–2678, 2688–2760).
