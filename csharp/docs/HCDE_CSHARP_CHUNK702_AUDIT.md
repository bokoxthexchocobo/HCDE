# Chunk 702 — Whole `A_DoChase` (`p_enemy.cpp` 2397–2764). Absorbs run-9 recognition spans and `p-enemy-cantleave-chase-rollback` (2740–2753).

## Scope

Whole `A_DoChase` (`p_enemy.cpp` 2397–2764). Absorbs run-9 recognition spans and `p-enemy-cantleave-chase-rollback` (2740–2753).

## Native ranges

- `src/playsim/p_enemy.cpp` **A_DoChase** 2397–2764 (368 lines)

**Credited native LOC:** 368 (tally `nativeLoc` 368).

## Managed

`VmPEnemyDoChase.cs`
Managed LOC (tally): 350.
Conversion commit: `46dde38037a90e6f57c79d5c85803acf5fd73c8d`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|-------------------|
| `VmPEnemyDoChase` | `A_DoChase` 2397–2764 |


**Post-review ratio:** 350/368 managed/native ≈ 95% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyDoChaseTests.DoChase_SkipsWhenInConversation`
- `VmPEnemyDoChaseTests.DoChase_ClearsNotargetPlayer`
- `VmPEnemyDoChaseTests.DoChase_MeleeAttackSetsState`
- `VmPEnemyDoChaseTests.DoChase_CantLeaveFloorPicRollsBack`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 702 --to 702` (0 hard / 0 soft).

## Seams

`P_LookForPlayers`, `P_SmartMove`, `P_NewChaseDir`, `P_TryMove`, `P_CheckMeleeRange`, `P_CheckMissileRange`, `P_CheckSight`, `SetState`, `S_Sound`, patrol iterators, and `A_Wander` are injected.

## Credit rationale

Core chase loop after chunk507 glue; retires recognition credit for the same native body.
