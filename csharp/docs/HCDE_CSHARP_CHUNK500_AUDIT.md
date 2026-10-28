# Chunk 500 — GetTeam, IsTeammate, species/groups, IsFriend, IsHostile, Do/TakeSpecialDamage

## Scope

GetTeam, IsTeammate, species/groups, IsFriend, IsHostile, Do/TakeSpecialDamage

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::GetTeam** 8364–8380 (17 lines)
- `src/playsim/p_mobj.cpp` **AActor::IsTeammate** 8382–8400 (19 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetSpecies** 8413–8433 (21 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetInfightingGroup** 8435–8438 (4 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetProjectileGroup** 8440–8443 (4 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetSplashGroup** 8445–8448 (4 lines)
- `src/playsim/p_mobj.cpp` **AActor::IsFriend** 8458–8477 (20 lines)
- `src/playsim/p_mobj.cpp` **AActor::IsHostile** 8487–8507 (21 lines)
- `src/playsim/p_mobj.cpp` **AActor::DoSpecialDamage** 8517–8538 (22 lines)
- `src/playsim/p_mobj.cpp` **AActor::CallDoSpecialDamage** 8552–8566 (15 lines)
- `src/playsim/p_mobj.cpp` **AActor::TakeSpecialDamage** 8574–8606 (33 lines)

**Credited native LOC:** 180 (tally `nativeLoc` 180).

## Managed

`VmPMobjTeamFriendDamage.cs`
Managed LOC (tally): 195.
Conversion commit: `3a1cb65cdf037058e64893ec86f0ed1a75b4041d`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `GetTeamFromDesignated` | `GetTeam` 8364–8380 (8366: `if (player)`; 8368: `return player->userinfo.GetTeam();`; 8374: `if (myTeam == TEAM_NONE && FriendPlayer != 0)`) |
| `CoopPlayerTeammate` / `TeamplayTeammate` / `IsTeammate` / `NeitherFriendly` | `IsTeammate` 8382–8400 (8384: `if (!other)`; 8386: `return false;`; 8388: `else if (!deathmatch && player && other->player)`) |
| `SpeciesUsesExplicit` | `GetSpecies` 8413–8433 (8415: `if (Species != NAME_None)`; 8417: `return Species;`; 8422: `if (GetDefaultByType(thistype)->flags3 & MF3_ISMONSTER)`) |
| `AActor::GetInfightingGroup` | `GetInfightingGroup` 8435–8438 (8437: `return GetClass()->ActorInfo()->infighting_group;`) |
| `AActor::GetProjectileGroup` | `GetProjectileGroup` 8440–8443 (8442: `return GetClass()->ActorInfo()->projectile_group;`) |
| `AActor::GetSplashGroup` | `GetSplashGroup` 8445–8448 (8447: `return GetClass()->ActorInfo()->splash_group;`) |
| `FriendDmTeamplay` / `FriendPlayersLinked` / `IsFriend` | `IsFriend` 8458–8477 (8460: `if (flags & other->flags & MF_FRIENDLY)`; 8462: `if (deathmatch && teamplay)`; 8463: `return IsTeammate(other) ||`) |
| `BothFriendly` / `IsHostile` | `IsHostile` 8487–8507 (8490: `if (!((flags | other->flags) & MF_FRIENDLY)) return false;`; 8493: `if (flags & other->flags & MF_FRIENDLY)`; 8495: `if (deathmatch && teamplay)`) |
| `GodmodeBlocksSpecial` / `LegacyPoisonHalve` / `HalveDamage` / `DoSpecialDamage` | `DoSpecialDamage` 8517–8538 (8519: `if (target->player && target->player->mo == target && damage < 1000 &&`; 8522: `return -1;`; 8526: `if (target->player)`) |
| `AActor::CallDoSpecialDamage` | `CallDoSpecialDamage` 8552–8566 (8562: `return retval;`; 8564: `else return DoSpecialDamage(target, damage, damagetype);`) |
| `HasGenericDeathState` / `MassacreBypassesDeathCheck` / `TakeFromInflictorDeathType` / `IceFreezeFallback` / `BlockDamageWhenNoDeath` / `TakeSpecialDamageWithAmount` | `TakeSpecialDamage` 8574–8606 (8584: `if (FindState (NAME_Death) != NULL || !HasSpecialDeathStates() || dam…`; 8586: `return damage;`; 8589: `if (inflictor && inflictor->DeathType != NAME_None)`) |

### Out-of-body uncredit (500)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **180** |
| **Out-of-body removed** | **0** |
| **Honest** | **180** |


**Post-review ratio:** 195/180 managed/native ≈ 108% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjTeamFriendDamageTests.GetTeam_UsesFriendPlayerWhenDesignatedNone`
- `VmPMobjTeamFriendDamageTests.Teammate_CoopFriendlyFlagMatch`
- `VmPMobjTeamFriendDamageTests.Teammate_TeamplaySameTeam`
- `VmPMobjTeamFriendDamageTests.Friend_DmTeamplayRequiresTeammateOrLinked`
- `VmPMobjTeamFriendDamageTests.Hostile_DmDifferentFriendPlayers`
- `VmPMobjTeamFriendDamageTests.DoSpecial_GodmodeBlocks`
- `VmPMobjTeamFriendDamageTests.DoSpecial_PoisonHalves`
- `VmPMobjTeamFriendDamageTests.CallVirtual_Flags`
- `VmPMobjTeamFriendDamageTests.TakeSpecial_MassacreAlwaysApplies`
- `VmPMobjTeamFriendDamageTests.TakeSpecial_IceGenericFreeze`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 500 --to 500` (0 hard / 0 soft).

## Seams

`P_PoisonPlayer`, `FindState`, `VMCall`, and player `mo` teammate chains are not hosted.

## Credit rationale

Credits team resolution, coop/teamplay teammate checks, species and infighting group accessors, friendly/hostile policy, godmode/poison special damage, and typed death gating from `p_mobj.cpp` (8364–8606).
