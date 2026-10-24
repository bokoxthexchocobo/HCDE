# Chunk 474 — DamageMobj team scaling, player armor/health, drain, and death tail Merged: [473].

## Scope

DamageMobj team scaling, player armor/health, drain, and death tail Merged: [473].

## Native ranges

- `src/playsim/p_interaction.cpp` **DamageMobj** 1320–1459 (140 lines)
- `src/playsim/p_interaction.cpp` **DamageMobj** 1460–1514 (55 lines)
- `src/playsim/p_interaction.cpp` **DamageMobj** 1516–1571 (56 lines)
- `src/playsim/p_interaction.cpp` **DamageMobj** 1281–1319 (39 lines)
- `src/playsim/p_interaction.cpp` **DamageMobj** 1573–1603 (31 lines)
- `src/playsim/p_interaction.cpp` **DamageMobj** 1617–1657 (41 lines)
- `src/playsim/p_interaction.cpp` **DamageMobj** 1715–1726 (12 lines)

**Credited native LOC:** 374 (tally `nativeLoc` 374).

## Managed

`VmPInteractionDamageLate.cs`, `VmPInteractionVirtualDamage.cs`, `VmPInteractionDamageEarly.cs`, `VmPInteractionTouchObituary.cs`
Managed LOC (tally): 546.
Conversion commit: `4b76b7b4a75b372857afa3b297711db66b24f1b9`.

## Padding review (2026-10-21)

| Managed file | Native (`p_interaction.cpp`) |
|--------------|---------------------------|
| `VmPInteractionDamageLate.cs` | `DamageMobj` 1320–1571 (`ArmorAbsorbPath`, `EvaluateTeamDamage`, `EvaluatePlayerArmor`, `EvaluatePowerDrainDamage`, `EvaluateMonsterBuddha`, `ResolveDamageType`, …) |
| `VmPInteractionVirtualDamage.cs` | `DamageMobj` 1281–1319, 1573–1603, 1617–1657, 1715–1726 (`EvaluateReflectionScan`, `KickbackEligible`, `PoisonMobjRequiresInflictor`, …) |
| `VmPInteractionDamageEarly.cs` | `DamageMobj` 1281–1319 early guards |
| `VmPInteractionTouchObituary.cs` | `DamageMobj` touch/obituary bands within 1281–1657 |

No `p_mobj.cpp` symbols are credited on this row; `VmPMobj*` sources are intentionally excluded from `managedSourceFiles`.

**Post-review ratio:** 546/374 managed/native ≈ 146% (src file LOC at HEAD; see tally).

## Tests

- `VmPInteractionDamageLateTests.TeamDamage_ScalesTeammateHit`
- `VmPInteractionDamageLateTests.PlayerInvuln_BlocksNonTelefrag`
- `VmPInteractionDamageLateTests.ScaleTeamDamage_AppliesMultiplier`
- `VmPInteractionDamageLateTests.EndLevelHack_ClampsLethalDamage`
- `VmPInteractionDamageLateTests.PlayerPhase_StopsOnInvuln`
- `VmPInteractionDamageLateTests.PlayerArmor_UsesInventory`
- `VmPInteractionDamageLateTests.DrainEligible_SkipsSamePlayer`
- `VmPInteractionDamageLateTests.PowerDrain_PicksMaxStrength`
- `VmPInteractionDamageLateTests.ClampDamageCount_CapsAt100`
- `VmPInteractionDamageLateTests.MonsterBuddha_SavesAtOneHp`
- `VmPInteractionDamageLateTests.DeathProceeds_WhenNoBuddha`
- `VmPInteractionDamageLateTests.ReturnNonNegative_PreservesCancelSentinel`
- `VmPInteractionDamageLateTests.ResolveDamageType_UsesInflictorDeathType`
- `VmPInteractionVirtualDamageTests.ReflectionScan_PicksHigherReflect`
- `VmPInteractionVirtualDamageTests.Kickback_BlocksThrustlessFlag`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 474 --to 474` (0 hard / 0 soft).

## Seams

`P_DamageMobj` hosted paths, `CallDie`, `P_GiveBody`, `S_Sound`, and event managers are not fully hosted.

## Credit rationale

Credits friendly-fire scaling, player invuln/armor/buddha, power-drain selection, monster buddha, and death attribution from `p_interaction.cpp` (1320–1571).
