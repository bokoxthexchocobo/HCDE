# Chunk 689 — Whole `P_SetHealthGroupHealth`, `P_DamageHealthGroup`, `P_DamageLinedef`, `P_DamageSector`, and `P_InitHealthGroups` (`p_destructible.cpp` 43–294). Retired (2026-10-24): `--band` hull overlaps existing credit (p-destructible-serialize-script-chunk691 src/playsim/p_destructible.cpp 220-224).

## Scope

Whole `P_SetHealthGroupHealth`, `P_DamageHealthGroup`, `P_DamageLinedef`, `P_DamageSector`, and `P_InitHealthGroups` (`p_destructible.cpp` 43–294). Retired (2026-10-24): `--band` hull overlaps existing credit (p-destructible-serialize-script-chunk691 src/playsim/p_destructible.cpp 220-224).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `7c9cc38bfb20e434fc4198841fe220173240dc12`.

## Padding review (2026-10-21)

| Managed | Native (`p_destructible.cpp`) |
|---------|-------------------------------|
| `VmPDestructibleHealthGroups` | `P_SetHealthGroupHealth` 43–71; `P_DamageHealthGroup` 73–110; `P_DamageLinedef` 112–146; `P_DamageSector` 148–211; `P_InitHealthGroups` 239–294 |

### Out-of-body uncredit (689)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **222** |
| **Out-of-body removed** | **1** |
| **Honest** | **221** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 689 --to 689` (0 hard / 0 soft).

## Seams

`P_GetHealthGroup`, `P_ActivateLine`, `WorldLineDamaged`, and sector action triggers remain native.

## Credit rationale

Destructible health-group wiring before geometry line attack bands.
