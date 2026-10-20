# Chunk 474 — DamageMobj team scaling, player armor/health, drain, and death tail

## Scope

DamageMobj team scaling, player armor/health, drain, and death tail

## Native ranges

- `src/playsim/p_interaction.cpp` **DamageMobj** 1320–1459 (140 lines)
- `src/playsim/p_interaction.cpp` **DamageMobj** 1460–1514 (55 lines)
- `src/playsim/p_interaction.cpp` **DamageMobj** 1516–1571 (56 lines)

**Credited native LOC:** 251 (tally `nativeLoc` 251).

## Managed

`VmPInteractionDamageLate.cs`, `VmPInteractionDamageLateTests.cs`
Managed LOC (tally): 130.
Conversion commit: `247523d7da2192c8c423651c310b00c1c8daa2ba`.

## Tests

- `VmPInteractionDamageLateTests.TeamDamage_ScalesTeammateHit`
- `VmPInteractionDamageLateTests.PlayerInvuln_BlocksNonTelefrag`
- `VmPInteractionDamageLateTests.PlayerArmor_UsesInventory`
- `VmPInteractionDamageLateTests.DrainEligible_SkipsSamePlayer`
- `VmPInteractionDamageLateTests.MonsterBuddha_SavesAtOneHp`
- `VmPInteractionDamageLateTests.DeathProceeds_WhenNoBuddha`
- `VmPInteractionDamageLateTests.ReturnNonNegative_PreservesCancelSentinel`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 474 --to 474` (0 hard / 0 soft).

## Seams

`P_DamageMobj` hosted paths, `CallDie`, `P_GiveBody`, `S_Sound`, and event managers are not fully hosted.

## Credit rationale

Credits friendly-fire scaling, player invuln/armor/buddha, power-drain selection, monster buddha, and death attribution from `p_interaction.cpp` (1320–1571).
