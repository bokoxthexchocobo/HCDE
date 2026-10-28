# Chunk 703 — Whole `P_Move` (`p_enemy.cpp` 482–717). Absorbs run-9 recognition `P_Move` spans and retires `p-enemy-move-blocked-chunk2` spechit/dropoff slices.

## Scope

Whole `P_Move` (`p_enemy.cpp` 482–717). Absorbs run-9 recognition `P_Move` spans and retires `p-enemy-move-blocked-chunk2` spechit/dropoff slices.

## Native ranges

- `src/playsim/p_enemy.cpp` **P_Move** 482–717 (236 lines)

**Credited native LOC:** 236 (tally `nativeLoc` 236).

## Managed

`VmPEnemyMove.cs`
Managed LOC (tally): 233.
Conversion commit: `76868ba51d2dc09651a2a466a7ac15b721137b12`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|-------------------|
| `VmPEnemyMove` | `P_Move` 482–717 |

### Out-of-body uncredit (703)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **236** |
| **Out-of-body removed** | **0** |
| **Honest** | **236** |


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
