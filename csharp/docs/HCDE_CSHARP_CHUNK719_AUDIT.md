# Chunk 719 — Whole `P_SmartMove`, `P_TryWalk`, `P_DoNewChaseDir`, and `P_NewChaseDir` (`p_enemy.cpp` 725–1099).

## Scope

Whole `P_SmartMove`, `P_TryWalk`, `P_DoNewChaseDir`, and `P_NewChaseDir` (`p_enemy.cpp` 725–1099).

## Native ranges

- `src/playsim/p_enemy.cpp` **P_SmartMove** 725–757 (33 lines)
- `src/playsim/p_enemy.cpp` **P_TryWalk** 773–781 (9 lines)
- `src/playsim/p_enemy.cpp` **P_DoNewChaseDir** 794–912 (119 lines)
- `src/playsim/p_enemy.cpp` **P_NewChaseDir** 935–1099 (165 lines)

**Credited native LOC:** 326 (tally `nativeLoc` 326).

## Managed

`VmPEnemyChaseWalk.cs`
Managed LOC (tally): 342.
Conversion commit: `851e96129e91195d9d3160a8d3c732818d057375`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `SmartMove` / lift-hazard gates | `P_SmartMove` 725–757 |
| `TryWalk` | `P_TryWalk` 773–781 |
| `DoNewChaseDir` | `P_DoNewChaseDir` 794–912 |
| `NewChaseDir` / dropoff strafe regroup | `P_NewChaseDir` 935–1099 |

### Out-of-body uncredit (719)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **326** |
| **Out-of-body removed** | **0** |
| **Honest** | **326** |


**Post-review ratio:** 342/326 managed/native ≈ 105% (src file LOC at HEAD; see tally).

## Tests

- `VmPEnemyChaseWalkTests.SmartMove_ClearsMovedirWhenLiftLost`
- `VmPEnemyChaseWalkTests.TryWalk_SetsMoveCountFromRng`
- `VmPEnemyChaseWalkTests.DoNewChaseDir_PicksDiagonalWhenBothAxesLarge`
- `VmPEnemyChaseWalkTests.DoNewChaseDir_SetsNodirWhenAllAttemptsFail`
- `VmPEnemyChaseWalkTests.NewChaseDir_InvertsDeltaWhenFrightened`
- `VmPEnemyChaseWalkTests.NewChaseDir_CallsRandomWhenNoTarget`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 719 --to 719` (0 hard / 0 soft).

## Seams

`P_Move`, `P_IsOnLift`, `P_IsUnderDamage`, `P_CheckTags`, line iterators, `HCDEAIDirectorConsumeRegroupTarget`, and `P_RandomChaseDir` are injected.

## Credit rationale

Smart chase walk and new-direction selection for monster movement.
