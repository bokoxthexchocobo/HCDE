# Chunk 720 — Whole `P_CheckMeleeRange`, `P_CheckMissileRange`, and `P_RandomChaseDir` (`p_enemy.cpp` 253–365, 1107–1257). `P_HitFriend` stays chunk508. Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-move-chunk703 src/playsim/p_enemy.cpp 482-717).

## Scope

Whole `P_CheckMeleeRange`, `P_CheckMissileRange`, and `P_RandomChaseDir` (`p_enemy.cpp` 253–365, 1107–1257). `P_HitFriend` stays chunk508. Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-move-chunk703 src/playsim/p_enemy.cpp 482-717).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `734d131411c7ad4d5c688918ff046baf17c13f75`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `CheckMeleeRange` | `P_CheckMeleeRange` 253–292 |
| `CheckMissileRange` | `P_CheckMissileRange` 307–365 |
| `RandomChaseDir` | `P_RandomChaseDir` 1107–1257 |

### Out-of-body uncredit (720)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **250** |
| **Out-of-body removed** | **0** |
| **Honest** | **250** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 720 --to 720` (0 hard / 0 soft).

## Seams

`P_CheckSight`, player slot iteration, and `P_TryWalk` remain injected.

## Credit rationale

Melee/missile range gates and random friendly chase direction.
