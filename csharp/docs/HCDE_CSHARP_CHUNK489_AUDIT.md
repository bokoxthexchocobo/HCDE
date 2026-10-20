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

(see conversion commit file list)
Managed LOC (tally): 231.
Conversion commit: `d3dd612a2fe802fe542ad669765ef774afbf56e8`.

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
