# Chunk 711 — Whole `P_CanResurrect` and `P_CheckForResurrection` (`p_enemy.cpp` 2777–2972). Retires chunk471.

## Scope

Whole `P_CanResurrect` and `P_CheckForResurrection` (`p_enemy.cpp` 2777–2972). Retires chunk471.

## Native ranges

- `src/playsim/p_enemy.cpp` **P_CanResurrect** 2777–2818 (42 lines)
- `src/playsim/p_enemy.cpp` **P_CheckForResurrection** 2827–2972 (146 lines)

**Credited native LOC:** 188 (tally `nativeLoc` 188).

## Managed

`VmPEnemyResurrection.cs`
Managed LOC (tally): 175.
Conversion commit: `14d8b6058c9cdea38626cbcad54cbf79d4ac377a`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|-------------------|
| `EvaluateCanResurrect` / `ActiveVirtualBlocks` / `PassiveSelfCheckSkipsSecond` | `P_CanResurrect` 2777–2818 |
| `EvaluateResurrectionCandidate` / `TouchDistanceBlocks` / `DifferentPortalNeedsSight` | `P_CheckForResurrection` 2827–2896 |
| `EvaluateFriendlyTargetCleanup` / `EvaluateHealStateEntry` | 2898–2927 |
| `EvaluateCorpseDimensions` / `VileGhostsEnabled` | 2930–2967 |


**Post-review ratio:** 175/188 managed/native ≈ 93% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyResurrectionTests.CanResurrect_RejectsNullRaiser`
- `VmPEnemyResurrectionTests.ResurrectionCandidate_AcceptsWhenEligible`
- `VmPEnemyResurrectionTests.ResurrectionCandidate_BlocksFarCorpse`
- `VmPEnemyResurrectionTests.CorpseDimensions_GhostAlphaWhenZeroHeight`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 711 --to 711` (0 hard / 0 soft).

## Seams

`VMCall`, iterators, `P_CheckSight`, `P_CheckPosition`, `SetState`, `S_Sound`, and `Revive` are injected.

## Credit rationale

Arch-vile resurrection virtual gates and corpse probe policy.
