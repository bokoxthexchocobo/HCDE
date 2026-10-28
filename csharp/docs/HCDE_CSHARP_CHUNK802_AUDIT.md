# Chunk 802 — Whole `LookForEnemiesEx`, `A_Look`, and `A_Wander` (`p_enemy.cpp` 1451–1478, 1944–2054, 2283–2322). Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-lookex-chunk469 src/playsim/p_enemy.cpp 2063-2094).

## Scope

Whole `LookForEnemiesEx`, `A_Look`, and `A_Wander` (`p_enemy.cpp` 1451–1478, 1944–2054, 2283–2322). Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-lookex-chunk469 src/playsim/p_enemy.cpp 2063-2094).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `34512d5a2d556d2a44184b8508650eba5bfe95a2`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `LookForEnemiesEx` | `AActor::LookForEnemiesEx` **1451–1478** |
| `Look` | `AActor::A_Look` **1944–2054** |
| `Wander` | `A_Wander` **2283–2322** |

### Out-of-body uncredit (802)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **179** |
| **Out-of-body removed** | **0** |
| **Honest** | **179** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 802 --to 802` (0 hard / 0 soft).

## Seams

Blockmap iterators, `ValidEnemyInBlock`, `P_LookForPlayers`, `P_CheckSight`, `SetState`, `S_Sound`, and `P_SmartMove` are injected.

## Credit rationale

Faithful whole-function look/wake glue after look-core chunk721; retires slice overlap on chunk469.
