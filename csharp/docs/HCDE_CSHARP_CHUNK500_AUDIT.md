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

`VmPMobjTeamFriendDamage.cs`, `VmPMobjTeamFriendDamageTests.cs`
Managed LOC (tally): 212.
Conversion commit: `65203c4c98b00870aa28715892dad5d63030b544`.

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
