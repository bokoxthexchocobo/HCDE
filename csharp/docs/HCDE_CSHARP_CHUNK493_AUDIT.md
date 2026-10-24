# Chunk 493 — P_HitFloor liquid probe, net missile registration, GetDefaultSpeed, P_SpawnMissileXYZ aim

## Scope

P_HitFloor liquid probe, net missile registration, GetDefaultSpeed, P_SpawnMissileXYZ aim

## Native ranges

- `src/playsim/p_mobj.cpp` **P_HitFloor** 7672–7677 (6 lines)
- `src/playsim/p_mobj.cpp` **P_HitFloor** 7679–7760 (82 lines)
- `src/playsim/p_mobj.cpp` **P_CheckMissileSpawn** 7762–7769 (8 lines)
- `src/playsim/p_mobj.cpp` **Net_RegisterMonsterReplicatedMissile** 7771–7777 (7 lines)
- `src/playsim/p_mobj.cpp` **GetDefaultSpeed** 7910–7922 (13 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnMissileXYZ** 7933–7979 (47 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmPMobjHitFloorSpawn.cs`, `VmPMobjHitFloorSpawnTests.cs`
Managed LOC (tally): 95.
Conversion commit: `ba9447ff0353b1a3b7802bb27195e104ba86a412`.

## Tests

- `VmPMobjHitFloorSpawnTests.HitFloor_TouchyCrush`
- `VmPMobjHitFloorSpawnTests.HitFloor_3dFloorTopSplash`
- `VmPMobjHitFloorSpawnTests.NetRegister_SkipsNull`
- `VmPMobjHitFloorSpawnTests.DefaultSpeed_FastMonster`
- `VmPMobjHitFloorSpawnTests.SpawnMissile_RejectNullDest`
- `VmPMobjHitFloorSpawnTests.SpawnMissile_HuggerClearsZ`
- `VmPMobjHitFloorSpawnTests.SpawnMissile_RaiseOverTallTarget`
- `VmPMobjHitFloorSpawnTests.SpawnMissile_SpectralSetsFriend`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 493 --to 493` (0 hard / 0 soft).

## Seams

`P_HitWater`, `P_DamageMobj`, `Spawn`, `P_PlaySpawnSound`, `P_CheckMissileSpawn`, and shadow handling are not hosted.

## Credit rationale

Credits touchy landing crush, sector/3D-floor splash delegation, replicated missile registration, fast-monster speed pick, and spawn/velocity policy from `p_mobj.cpp` (7672–7979).
