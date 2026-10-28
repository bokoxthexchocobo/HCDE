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

`VmPInteractionReactPain.cs`
Managed LOC (tally): 205.
Conversion commit: `40fdcc6952b72c586eafa4d456ce320540b1c58e`.

## Padding review (2026-10-21)

| Managed | Native (`p_interaction.cpp`) |
|---------|-------------------|
| `P_AutoUseHealth` | `P_AutoUseHealth` 826–833 |
| `P_AutoUseStrifeHealth` | `P_AutoUseStrifeHealth` 842–849 |
| `DeadOrNoDamage` / `GodmodeBlocksReact` / `WoundStateTriggers` / `ShouldWakeUp` / `SetJustHit` / `TryReactToDamage` | `ReactToDamage` 870–960 (882: `if (target == nullptr || target->health < 1 || damage < 0)`; 883: `return false;`; 886: `if (player && player->mo)`) |
| `CallReactToDamage` | `CallReactToDamage` 962–975 (974: `return res;`) |
| `AActor::ReactToDamage` | `ReactToDamage` 977–988 |
| `MustForcePain` / `IsFakePain` / `NoPainFlag` / `PainRollEligible` / `TriggerPainBlocked` / `PainRollSucceeded` / `ElectricFlinchRoll` / `ElectricHowlRoll` / `PoisonCloudHowlRoll` / `EvaluateTriggerPain` | `TriggerPainChance` 990–1048 (992: `if (target == nullptr || target->flags5 & MF5_NOPAIN || target->healt…`; 993: `return false;`; 999: `if (pc.first == mod)`) |
| `AActor::TriggerPainChance` | `TriggerPainChance` 1053–1059 |

### Out-of-body uncredit (504)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **199** |
| **Out-of-body removed** | **0** |
| **Honest** | **199** |


**Post-review ratio:** 205/199 managed/native ≈ 103% (src file LOC at HEAD; see tally).

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
