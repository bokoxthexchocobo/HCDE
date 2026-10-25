# Chunk 489 — SpawnMapThing index, P_SpawnPuff, and blood splatter (consolidated chunk489–490)

## Scope

SpawnMapThing index, P_SpawnPuff, and blood splatter (consolidated chunk489–490)

## Native ranges

- `src/playsim/p_mobj.cpp` **FLevelLocals::SpawnMapThing** 7095–7106 (12 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnPuff** 7116–7200 (85 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, SpawnPuff)** 7202–7215 (14 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnBlood** 7223–7318 (96 lines)
- `src/playsim/p_mobj.cpp` **P_BloodSplatter** 7337–7368 (32 lines)
- `src/playsim/p_mobj.cpp` **P_BloodSplatter2** 7376–7408 (33 lines)
- `src/playsim/p_mobj.cpp` **P_RipperBlood** 7429–7467 (39 lines)

**Credited native LOC:** 311 (tally `nativeLoc` 311).

## Managed

`VmPMobjSpawnPuff.cs`, `VmPMobjSpawnBlood.cs`
Managed LOC (tally): 217.
Conversion commit: `28623f7c1ae1b65529bf09af972bf2b9bdf6ed78`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `BeginInvasionRegistration` | `SpawnMapThing` 7095–7106 (7098: `if (dumpspawnedthings)`; 7105: `return spawned;`) |
| `PuffTypeValid` / `RandomizePuffTics` / `ClampPuffTics` / `AssignVictimTarget` / `AssignVictimMaster` / `AssignVictimTracer` / `AssignOwnerTarget` / `OppositeHitYaw` / `UseSkyCrashState` / `UseWallCrashState` / `UseExtremeBleedState` / `UseMeleeState` / `DrawSplashParticles` / `HidePuffAfterSplash` / `MeleeRumbleFromPlayer` / `PlaySeeSoundOnHit` / `PlayAttackSoundFallback` / `PickSkyCrash` / `PickMissCrash` / `PickMeleeCrash` / `SkipEffectsWhenTemporary` / `DoomChexRandomizeTics` / `StrifeLowDamageBump` / `UseDoomStrifeChexStatePick` / `InvisibleRenderFlags` / `OppositeHitAngle` / `ClampLowDamageStrife` | `P_SpawnPuff` 7116–7200 (7121: `if (pufftype == nullptr) return nullptr;`; 7123: `if (!(flags & PF_NORANDOMZ)) pos.Z += pr_spawnpuff.Random2() / 64.;`; 7125: `if (puff == NULL) return NULL;`) |
| `DEFINE_ACTION_FUNCTION(AActor, SpawnPuff)` | `DEFINE_ACTION_FUNCTION(AActor, SpawnPuff)` 7202–7215 |
| `ApplyRandomZJitter` / `ApplyInvisibleRenderFlags` / `BloodClassValid` / `ClampBloodTics` / `ApplyBloodTranslation` / `StrifeSprayState` / `DamageStateAdvance` / `HideActorForBloodType` / `TryAdvanceOwnedState` / `WalkParentForOwnedState` / `RemainingAdvanceSteps` / `ShouldPickStateAfterSpray` / `SelectBloodStateAdvance` / `ContinueParentWalk` / `ApplyAdvanceOnStep` / `CoopBloodTranslation` | `P_SpawnBlood` 7223–7318 (7232: `if (bloodcls != nullptr && !(GetDefaultByType(bloodcls)->flags4 & MF4…`; 7235: `if (bloodcls != nullptr)`; 7241: `if (th->flags5 & MF5_PUFFGETSOWNER) th->target = originator;`) |
| `SplatterClassValid` / `SplatterVelocityComponent` / `Splatter2Offset` / `SplatterVelocityFromRandom` | `P_BloodSplatter` 7337–7368 (7343: `if (bloodcls != NULL && !(GetDefaultByType(bloodcls)->flags4 & MF4_AL…`; 7346: `if (bloodcls != NULL)`; 7357: `if (!(mo->flags2 & MF2_DONTTRANSLATE))`) |
| `P_BloodSplatter2` | `P_BloodSplatter2` 7376–7408 (7382: `if (bloodcls != NULL && !(GetDefaultByType(bloodcls)->flags4 & MF4_AL…`; 7389: `if (bloodcls != NULL)`; 7397: `if (!(mo->flags2 & MF2_DONTTRANSLATE))`) |
| `PickBleedCrash` / `ForceBloodTypeOff` / `RipperClassValid` / `RipperHereticNoGravity` / `RipperOffsetComponent` / `RipperSplashUsesAngleTo` / `RipperSplashAngle` / `RipperSplashWhenEnabled` | `P_RipperBlood` 7429–7467 (7440: `if (bloodcls != NULL && !(GetDefaultByType(bloodcls)->flags4 & MF4_AL…`; 7443: `if (bloodcls != NULL)`; 7448: `if (th->flags5 & MF5_PUFFGETSOWNER) th->target = bleeder;`) |

### Out-of-body uncredit (489)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **311** |
| **Out-of-body removed** | **0** |
| **Honest** | **311** |


**Post-review ratio:** 217/311 managed/native ≈ 70% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSpawnPuffTests.Index_BeginsInvasionOnZero`
- `VmPMobjSpawnPuffTests.Puff_RequiresType`
- `VmPMobjSpawnPuffTests.Puff_SkyCrashState`
- `VmPMobjSpawnPuffTests.Puff_MeleeStateWhenClose`
- `VmPMobjSpawnPuffTests.Puff_DrawSplashWhenAllowed`
- `VmPMobjSpawnPuffTests.Puff_PlaySeeSoundOnHit`
- `VmPMobjSpawnPuffTests.Puff_CrashPriorityPrefersSky`
- `VmPMobjSpawnPuffTests.Puff_SkipEffectsWhenTemporary`
- `VmPMobjSpawnBloodTests.Blood_DisablesParticlesWhenFlagClear`
- `VmPMobjSpawnBloodTests.Blood_StrifeSprayWhenHighDamage`
- `VmPMobjSpawnBloodTests.Blood_DamageAdvanceMedium`
- `VmPMobjSpawnBloodTests.Blood_SelectAdvanceSkipsSpray`
- `VmPMobjSpawnBloodTests.Splatter_OppositeAngle`
- `VmPMobjSpawnBloodTests.Blood_HidesForModernBloodType`
- `VmPMobjSpawnBloodTests.Ripper_HereticFloats`
- `VmPMobjSpawnBloodTests.Ripper_SplashAngleAdds180`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 489 --to 489` (0 hard / 0 soft).

## Seams

`Spawn`, `VMCall`, `P_DrawSplash2`, `S_Sound`, `FindState`, and net registration are not hosted.

## Credit rationale

Credits invasion/coop mapthing wrapper, puff spawn/state/sound policy, and blood/splatter/ripper flows from `p_mobj.cpp` (7095–7215, 7223–7467).
