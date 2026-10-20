# Chunk 504 — P_AutoUseHealth, P_AutoUseStrifeHealth, ReactToDamage, CallReactToDamage, TriggerPainChance

## Scope

P_AutoUseHealth, P_AutoUseStrifeHealth, ReactToDamage, CallReactToDamage, TriggerPainChance

## Native ranges

- `src/playsim/p_interaction.cpp` **P_AutoUseHealth** 826–833 (8 lines)
- `src/playsim/p_interaction.cpp` **P_AutoUseStrifeHealth** 842–849 (8 lines)
- `src/playsim/p_interaction.cpp` **ReactToDamage** 870–960 (91 lines)
- `src/playsim/p_interaction.cpp` **CallReactToDamage** 962–975 (14 lines)
- `src/playsim/p_interaction.cpp` **AActor::ReactToDamage** 977–988 (12 lines)
- `src/playsim/p_interaction.cpp` **TriggerPainChance** 990–1048 (59 lines)
- `src/playsim/p_interaction.cpp` **AActor::TriggerPainChance** 1053–1059 (7 lines)

**Credited native LOC:** 199 (tally `nativeLoc` 199).

## Managed

`VmPInteractionReactPain.cs`, `VmPInteractionReactPainTests.cs`
Managed LOC (tally): 172.
Conversion commit: `033d415fe7f8cd9be965feb90877846c903d4cbc`.

## Tests

- `VmPInteractionReactPainTests.React_BlocksWhenDead`
- `VmPInteractionReactPainTests.React_Godmode2Blocks`
- `VmPInteractionReactPainTests.React_WoundAtThreshold`
- `VmPInteractionReactPainTests.FakePain_FromCausePain`
- `VmPInteractionReactPainTests.Pain_ForcedAlwaysRolls`
- `VmPInteractionReactPainTests.Pain_ElectricFlinch`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 504 --to 504` (0 hard / 0 soft).

## Seams

`VMCall`, `FindState`, `SetState`, `Howl`, and `DamageMobj` are not hosted.

## Credit rationale

Credits auto-health VM hooks, wound/pain gating, target wake/switch policy, virtual react dispatch, and typed pain rolls from `p_interaction.cpp` (826–1059).
