# Chunk 732 — Poison apply/damage pipeline: `P_PoisonMobj`, `P_PoisonPlayer`, `P_PoisonDamage` (`p_interaction.cpp`).

## Scope

Poison apply/damage pipeline: `P_PoisonMobj`, `P_PoisonPlayer`, `P_PoisonDamage` (`p_interaction.cpp`).

## Native ranges

- `src/playsim/p_interaction.cpp` **P_PoisonMobj** 1660–1713 (54 lines)
- `src/playsim/p_interaction.cpp` **P_PoisonPlayer** 1834–1864 (31 lines)
- `src/playsim/p_interaction.cpp` **P_PoisonDamage** 1881–1980 (100 lines)

**Credited native LOC:** 185 (tally `nativeLoc` 185).

## Managed

`VmPInteractionPoisonDamage.cs`
Managed LOC (tally): 191.
Conversion commit: `45ded304105bf51d72e78658f523741c3ebae059`.

## Padding review (2026-10-21)

| Managed | Native (`p_interaction.cpp`) |
|---------|---------------------------|
| `VmPInteractionPoisonDamage` (aggregate) | 1660–1713, 1834–1864, 1881–1980 |
| `PoisonMobj` / `MirrorOrVisualSkips` | `P_PoisonMobj` 1660–1713 |
| `PoisonPlayer` | `P_PoisonPlayer` 1834–1864 |
| `PoisonDamage` | `P_PoisonDamage` 1881–1980 |


**Post-review ratio:** 191/185 managed/native ≈ 103% (src file LOC at HEAD; see tally).

## Tests

- `VmPInteractionPoisonDamageTests.PoisonMobj_AdditiveDamage`
- `VmPInteractionPoisonDamageTests.MirrorOrVisualSkips_BlocksPoisonMobj`
- `VmPInteractionPoisonDamageTests.PoisonPlayer_GodmodeBlocks`
- `VmPInteractionPoisonDamageTests.PoisonDamage_DeathCallsDie`
- `VmPInteractionPoisonDamageTests.PoisonDamage_SkipsWhenHealthZero`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 732 --to 732` (0 hard / 0 soft).

## Seams

`Net_Is*` mirror gates, `GetModifiedDamage`, `P_AutoUseHealth`, `CallDie`, and pain `SetState` are injected.

## Credit rationale

Credits poison assignment, player poison counters, and poison damage death/pain cadence (1660–1713, 1834–1864, 1881–1980).
