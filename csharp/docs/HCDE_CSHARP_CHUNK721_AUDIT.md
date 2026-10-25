# Chunk 721 — Whole `P_IsVisible`, `isTargetablePlayer`, `ValidEnemyInBlock`, and `P_LookForPlayers` (`p_enemy.cpp` 1268–1440, 1789–1933). Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-look-targets-whole-chunk707 src/playsim/p_enemy.cpp 1489-1531).

## Scope

Whole `P_IsVisible`, `isTargetablePlayer`, `ValidEnemyInBlock`, and `P_LookForPlayers` (`p_enemy.cpp` 1268–1440, 1789–1933). Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-look-targets-whole-chunk707 src/playsim/p_enemy.cpp 1489-1531).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `5777317202ffda3a78eca722c752e5a16ab8a6e8`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `IsVisible` | `P_IsVisible` 1268–1314 |
| `IsTargetablePlayer` | `isTargetablePlayer` 1316–1363 |
| `ValidEnemyInBlock` | `ValidEnemyInBlock` 1365–1440 |
| `LookForPlayers` | `P_LookForPlayers` 1789–1933 |

### Out-of-body uncredit (721)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **316** |
| **Out-of-body removed** | **0** |
| **Honest** | **316** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 721 --to 721` (0 hard / 0 soft).

## Seams

`P_CheckSight`, blockmap iterators, `P_LookForTID`/`P_LookForEnemies`/`P_LookForMonsters`, and `A_Look` action glue remain native.

## Credit rationale

Core visibility and player scan drivers for monster look.
