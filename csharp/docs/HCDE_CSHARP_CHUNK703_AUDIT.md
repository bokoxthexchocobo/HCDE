# Chunk 703 — Whole `P_Move` (`p_enemy.cpp` 482–717). Absorbs run-9 recognition `P_Move` spans from `p-enemy-chase-core-chunk3-recognition`.

## Scope

Whole `P_Move` (`p_enemy.cpp` 482–717). Absorbs run-9 recognition `P_Move` spans from `p-enemy-chase-core-chunk3-recognition`.

## Native ranges

- `src/playsim/p_enemy.cpp` **P_Move** 482–717 (236 lines)

**Credited native LOC:** 236 (tally `nativeLoc` 236).

## Managed

`VmPEnemyMove.cs`
Managed LOC (tally): 233.
Conversion commit: `46dde38037a90e6f57c79d5c85803acf5fd73c8d`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|-------------------|
| `VmPEnemyMove` | `P_Move` 482–717 |


**Post-review ratio:** 233/236 managed/native ≈ 99% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyMoveTests.Move_BlastedActorAlwaysSucceeds`
- `VmPEnemyMoveTests.Move_RejectsInvalidMovedir`
- `VmPEnemyMoveTests.Move_BlocksWhenFloatingAboveFloor`
- `VmPEnemyMoveTests.Move_ActivatesSpechitWhenBlocked`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 703 --to 703` (0 hard / 0 soft).

## Seams

`P_TryMove`, `P_GetMoveFactor`, `P_TestMobjZ`, `P_ActivateLine`, sector actions, and `P_CheckFor3DFloorHit` are injected.

## Credit rationale

Monster step core after DoChase chunk; retires recognition credit for the same walk body.
