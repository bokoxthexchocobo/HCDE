# Chunk 803 — Whole `A_ClearLastHeard`, `A_Look2`, `A_MonsterRail`, `A_Pain`, `A_ClassBossHealth` (`p_enemy.cpp` 2255–2260, 2330–2381, 3155–3196, 3217–3269, 3476–3488). Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-chunk1 src/playsim/p_enemy.cpp 2980-2998).

## Scope

Whole `A_ClearLastHeard`, `A_Look2`, `A_MonsterRail`, `A_Pain`, `A_ClassBossHealth` (`p_enemy.cpp` 2255–2260, 2330–2381, 3155–3196, 3217–3269, 3476–3488). Retired (2026-10-24): `--band` hull overlaps existing credit (p-enemy-chunk1 src/playsim/p_enemy.cpp 2980-2998).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `dc5e6bca75da98e821b9f122eb6a837a302ac70a`.

## Padding review (2026-10-21)

| Managed | Native (`p_enemy.cpp`) |
|---------|----------------------|
| `ClearLastHeard` | `AActor::A_ClearLastHeard` **2255–2260** |
| `Look2` | `AActor::A_Look2` **2330–2381** |
| `MonsterRail` | `AActor::A_MonsterRail` **3155–3196** |
| `Pain` | `AActor::A_Pain` **3217–3269** |
| `ClassBossHealth` | `AActor::A_ClassBossHealth` **3476–3488** |

### Out-of-body uncredit (803)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **166** |
| **Out-of-body removed** | **0** |
| **Honest** | **166** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 803 --to 803` (0 hard / 0 soft).

## Seams

`P_CheckSight`, `P_LookForPlayers`, `P_AimLineAttack`, `P_RailAttack`, `S_Sound`, and VM rumble hooks are injected.

## Credit rationale

Faithful whole-function misc monster actions; retires legacy `p-enemy-chunk1` slice row.
