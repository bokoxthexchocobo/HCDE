# Chunk 712 — Whole `CheckBossDeath` and `A_BossDeath` (`p_enemy.cpp` 3271–3444). Trims `p-enemy-chunk1` overlap.

## Scope

Whole `CheckBossDeath` and `A_BossDeath` (`p_enemy.cpp` 3271–3444). Trims `p-enemy-chunk1` overlap.

## Native ranges

- `src/playsim/p_enemy.cpp` **CheckBossDeath** 3271–3308 (38 lines)
- `src/playsim/p_enemy.cpp` **A_BossDeath** 3314–3444 (131 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`VmPEnemyBossDeath.cs`
Managed LOC (tally): 159.
Conversion commit: `e3c0ef7e57b2536463ee3abc3b4debc4a14569d4`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|-------------------|
| `CheckBossDeath` / `AnyPlayerAlive` / `LivingBossRemains` | `CheckBossDeath` 3271–3308 |
| `PlanBossDeath` / `ShouldRunBossExit` / `SpecialActionMatches` | `A_BossDeath` 3314–3392 |
| `Map07BossFlags` / `ExitBlockedByDmflags` | 3394–3444 |


**Post-review ratio:** 159/169 managed/native ≈ 94% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyBossDeathTests.CheckBossDeath_FailsWhenNoLivingPlayers`
- `VmPEnemyBossDeathTests.CheckBossDeath_FailsWhenOtherBossAlive`
- `VmPEnemyBossDeathTests.PlanBossDeath_RunsSpecialActionWhenTypeMatches`
- `VmPEnemyBossDeathTests.PlanBossDeath_ExitWhenAllDeadAndMap07`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 712 --to 712` (0 hard / 0 soft).

## Seams

`P_ExecuteSpecial`, `EV_DoFloor`, `EV_DoDoor`, `Massacre`, and `ExitLevel` are injected via `BossDeathPlan`.

## Credit rationale

Boss exit gating, UMAPINFO special actions, and map07/spec victory floors.
