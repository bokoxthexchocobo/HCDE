# Chunk 732 — Poison apply/damage pipeline: `P_PoisonMobj`, `P_PoisonPlayer`, `P_PoisonDamage` (`p_interaction.cpp`). Retired (2026-10-24): `--band` hull overlaps existing credit (p-interaction-damage-dispatch-chunk733 src/playsim/p_interaction.cpp 1734-1801).

## Scope

Poison apply/damage pipeline: `P_PoisonMobj`, `P_PoisonPlayer`, `P_PoisonDamage` (`p_interaction.cpp`). Retired (2026-10-24): `--band` hull overlaps existing credit (p-interaction-damage-dispatch-chunk733 src/playsim/p_interaction.cpp 1734-1801).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `3b44a2bbcdaacba4d799bf51184f2e09557962e0`.

## Padding review (2026-10-21)

| Managed | Native (`p_interaction.cpp`) |
|---------|---------------------------|
| `VmPInteractionPoisonDamage` (aggregate) | 1660–1713, 1834–1864, 1881–1980 |
| `PoisonMobj` / `MirrorOrVisualSkips` | `P_PoisonMobj` 1660–1713 |
| `PoisonPlayer` | `P_PoisonPlayer` 1834–1864 |
| `PoisonDamage` | `P_PoisonDamage` 1881–1980 |

### Out-of-body uncredit (732)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **185** |
| **Out-of-body removed** | **0** |
| **Honest** | **185** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 732 --to 732` (0 hard / 0 soft).

## Seams

`Net_Is*` mirror gates, `GetModifiedDamage`, `P_AutoUseHealth`, `CallDie`, and pain `SetState` are injected.

## Credit rationale

Credits poison assignment, player poison counters, and poison damage death/pain cadence (1660–1713, 1834–1864, 1881–1980).
