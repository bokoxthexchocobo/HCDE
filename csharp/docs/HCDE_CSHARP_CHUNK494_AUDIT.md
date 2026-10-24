# Chunk 494 — P_SpawnMissileXYZ tail, P_SpawnMissile/Z, OldSpawn, aimed and angle spawn, sub-missile

## Scope

P_SpawnMissileXYZ tail, P_SpawnMissile/Z, OldSpawn, aimed and angle spawn, sub-missile

## Native ranges

- `src/playsim/p_mobj.cpp` **P_SpawnMissileXYZ** 7980–7994 (15 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnMissile** 8009–8016 (8 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnMissileZ** 8027–8034 (8 lines)
- `src/playsim/p_mobj.cpp` **P_OldSpawnMissile** 8045–8071 (27 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnMissileZAimed** 8082–8110 (29 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnMissileAngleZSpeed** 8130–8164 (35 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnSubMissile** 8179–8212 (34 lines)

**Credited native LOC:** 156 (tally `nativeLoc` 156).

## Managed

`VmPMobjSpawnMissileVariants.cs`, `VmPMobjSpawnMissileVariantsTests.cs`
Managed LOC (tally): 94.
Conversion commit: `f3826f8026cb8a1c71dffbecc0839ade84cac92a`.

## Tests

- `VmPMobjSpawnMissileVariantsTests.XyzTail_FailsWhenCheckSpawnBlocks`
- `VmPMobjSpawnMissileVariantsTests.OldSpawn_RejectsNullType`
- `VmPMobjSpawnMissileVariantsTests.Aimed_ComputesVerticalSpeed`
- `VmPMobjSpawnMissileVariantsTests.AngleSpawn_AdjustsCustomZ`
- `VmPMobjSpawnMissileVariantsTests.SubMissile_InheritsSpectralFromMissileSource`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 494 --to 494` (0 hard / 0 soft).

## Seams

`P_SpawnMissileXYZ_ShadowHandling`, `Spawn`, `P_PlaySpawnSound`, `P_CheckMissileSpawn`, and `P_AimLineAttack` are not hosted.

## Credit rationale

Credits spawn check failure, spectral friend setup, legacy spawn Z/velocity, aimed vertical speed, angle spawn Z clip, and sub-missile spectral inheritance from `p_mobj.cpp` (7980–8212).
