# Chunk 490 — P_SpawnBlood, P_BloodSplatter, P_BloodSplatter2, P_RipperBlood

## Scope

P_SpawnBlood, P_BloodSplatter, P_BloodSplatter2, P_RipperBlood

## Native ranges

- `src/playsim/p_mobj.cpp` **P_SpawnBlood** 7223–7318 (96 lines)
- `src/playsim/p_mobj.cpp` **P_BloodSplatter** 7337–7368 (32 lines)
- `src/playsim/p_mobj.cpp` **P_BloodSplatter2** 7376–7408 (33 lines)
- `src/playsim/p_mobj.cpp` **P_RipperBlood** 7429–7467 (39 lines)

**Credited native LOC:** 200 (tally `nativeLoc` 200).

## Managed

`VmPMobjSpawnBlood.cs`, `VmPMobjSpawnBloodTests.cs`
Managed LOC (tally): 72.
Conversion commit: `27f5a51c6f9fdbcb0c79e42d73366c969f118047`.

## Tests

- `VmPMobjSpawnBloodTests.Blood_DisablesParticlesWhenFlagClear`
- `VmPMobjSpawnBloodTests.Blood_StrifeSprayWhenHighDamage`
- `VmPMobjSpawnBloodTests.Blood_DamageAdvanceMedium`
- `VmPMobjSpawnBloodTests.Splatter_OppositeAngle`
- `VmPMobjSpawnBloodTests.Blood_HidesForModernBloodType`
- `VmPMobjSpawnBloodTests.Ripper_HereticFloats`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 490 --to 490` (0 hard / 0 soft).

## Seams

`Spawn`, `P_DrawSplash2`, `FindState`, and class state walks are not hosted.

## Credit rationale

Credits blood type gating, Strife spray/state advance, splatter velocity, and ripper gravity policy from `p_mobj.cpp` (7223–7467).
