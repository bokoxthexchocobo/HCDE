# Chunk 543 — DoCheckClass/Species, DoDamage, A_SetTics, A_DamageSelf/Target/Tracer/Master

## Scope

DoCheckClass/Species, DoDamage, A_SetTics, A_DamageSelf/Target/Tracer/Master

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **DoCheckSpecies** 3751–3756 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **DoCheckClass** 3758–3763 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **DMSS** 4033–4045 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **A_SetTics** 3999–4014 (16 lines)
- `src/playsim/p_actionfunctions.cpp` **DoDamage** 4047–4080 (34 lines)
- `src/playsim/p_actionfunctions.cpp` **A_DamageSelf** 4087–4103 (17 lines)
- `src/playsim/p_actionfunctions.cpp` **A_DamageTarget** 4110–4127 (18 lines)
- `src/playsim/p_actionfunctions.cpp` **A_DamageTracer** 4134–4151 (18 lines)
- `src/playsim/p_actionfunctions.cpp` **A_DamageMaster** 4158–4175 (18 lines)
- `src/playsim/p_actionfunctions.cpp` **A_DamageChildren** 4196–4204 (9 lines)

**Credited native LOC:** 155 (tally `nativeLoc` 155).

## Managed

`VmActorDoDamage.cs`, `VmActorDoDamageTests.cs`
Managed LOC (tally): 80.
Conversion commit: `f3aac8519b7672dbf27aea8d8044e2536ba529b6`.

## Tests

- `VmActorDoDamageTests.CheckClass_ExcludeInverts`
- `VmActorDoDamageTests.AssembleDamageFlags_KillAddsNoFactor`
- `VmActorDoDamageTests.SetTicsOnActor_WhenFromActor`
- `VmActorDoDamageTests.PassesFilterSpecies_EitherMode`
- `VmActorDoDamageTests.PlanDoDamage_HealWhenNegative`
- `VmActorDoDamageTests.AdjustKillAmount_AddsHealth`
- `VmActorDoDamageTests.DamageMasterTarget_RequiresMaster`
- `VmActorDoDamageTests.ShouldDamageChild_WhenMasterMatches`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 543 --to 543` (0 hard / 0 soft).

## Seams

`P_DamageMobj`, `P_GiveBody`, `COPY_AAPTR`, and player psprite lookup are not hosted.

## Credit rationale

Credits native bands mirrored by `VmActorDoDamage` and named tests — not full `DEFINE_ACTION_FUNCTION` prologues beyond target guards.
