# Chunk 803 — Whole `A_ClearLastHeard`, `A_Look2`, `A_MonsterRail`, `A_Pain`, `A_ClassBossHealth` (`p_enemy.cpp` 2255–2260, 2330–2381, 3155–3196, 3217–3269, 3476–3488).

## Scope

Whole `A_ClearLastHeard`, `A_Look2`, `A_MonsterRail`, `A_Pain`, `A_ClassBossHealth` (`p_enemy.cpp` 2255–2260, 2330–2381, 3155–3196, 3217–3269, 3476–3488).

## Native ranges

- `src/playsim/p_enemy.cpp` **AActor::A_ClearLastHeard** 2255–2260 (6 lines)
- `src/playsim/p_enemy.cpp` **AActor::A_Look2** 2330–2381 (52 lines)
- `src/playsim/p_enemy.cpp` **AActor::A_MonsterRail** 3155–3196 (42 lines)
- `src/playsim/p_enemy.cpp` **AActor::A_Pain** 3217–3269 (53 lines)
- `src/playsim/p_enemy.cpp` **AActor::A_ClassBossHealth** 3476–3488 (13 lines)

**Credited native LOC:** 166 (tally `nativeLoc` 166).

## Managed

`VmPEnemyMiscActions.cs`
Managed LOC (tally): 147.
Conversion commit: `bb1f6f34c03170d6535359f05c89f45619f0b42e`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `ClearLastHeard` | `AActor::A_ClearLastHeard` **2255–2260** |
| `Look2` | `AActor::A_Look2` **2330–2381** |
| `MonsterRail` | `AActor::A_MonsterRail` **3155–3196** |
| `Pain` | `AActor::A_Pain` **3217–3269** |
| `ClassBossHealth` | `AActor::A_ClassBossHealth` **3476–3488** |


**Post-review ratio:** 147/166 managed/native ≈ 89% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyMiscActionsTests.ClearLastHeard_ClearsPointer`
- `VmPEnemyMiscActionsTests.Look2_SkipsConversation`
- `VmPEnemyMiscActionsTests.MonsterRail_RequiresTarget`
- `VmPEnemyMiscActionsTests.Pain_PlayerUsesHealthBand`
- `VmPEnemyMiscActionsTests.ClassBossHealth_CoopScalesOnce`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 803 --to 803` (0 hard / 0 soft).

## Seams

`P_CheckSight`, `P_LookForPlayers`, `P_AimLineAttack`, `P_RailAttack`, `S_Sound`, and VM rumble hooks are injected.

## Credit rationale

Faithful whole-function misc monster actions; retires legacy `p-enemy-chunk1` slice row.
