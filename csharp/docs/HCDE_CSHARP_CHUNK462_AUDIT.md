# Chunk 462 — OkayToSwitchTarget, P_PoisonPlayer, P_PoisonDamage

## Scope

OkayToSwitchTarget, P_PoisonPlayer, P_PoisonDamage

## Native ranges

- `src/playsim/p_interaction.cpp` **AActor::OkayToSwitchTarget** 1734–1801 (68 lines)
- `src/playsim/p_interaction.cpp` **P_PoisonPlayer** 1834–1864 (31 lines)
- `src/playsim/p_interaction.cpp` **P_PoisonDamage** 1881–1980 (100 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmPInteractionPoisonSwitch.cs`
Managed LOC (tally): 198.
Conversion commit: `f4b7b73256bf9e476c0923b130eecad38f4d9fee`.

## Tests

- `VmPInteractionPoisonSwitchTests.InfightingSameGroup_Blocks`
- `VmPInteractionPoisonSwitchTests.InvulnBlocksPoison_WhenNotFoil`
- `VmPInteractionPoisonSwitchTests.PoisonPlayerGodmode_Blocks`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 462 --to 462` (0 hard / 0 soft).

## Seams

`VMCall`, `P_DamageMobj`, `CallDie`, and skill/damage pipelines are not hosted.

## Credit rationale

Credits target-switch infight policy, player poison setup, and poison damage telefrag/buddha gates from `p_interaction.cpp` (P_PoisonMobj already chunk57).
