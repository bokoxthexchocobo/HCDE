# Chunk 755 — `SpawnMissileAngleZSpeed` / `SpawnSubMissile` defines and whole `P_SpawnPlayerMissile` (`p_mobj.cpp` 8112–8362).

## Scope

`SpawnMissileAngleZSpeed` / `SpawnSubMissile` defines and whole `P_SpawnPlayerMissile` (`p_mobj.cpp` 8112–8362).

## Native ranges

- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, SpawnMissileZAimed)** 8112–8119 (8 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, SpawnMissileAngleZSpeed)** 8166–8177 (12 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, SpawnSubMissile)** 8214–8220 (7 lines)
- `src/playsim/p_mobj.cpp` **P_SpawnPlayerMissile** 8230–8342 (113 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, SpawnPlayerMissile)** 8344–8362 (19 lines)

**Credited native LOC:** 159 (tally `nativeLoc` 159).

## Managed

`VmPMobjSpawnPlayerMissile.cs`
Managed LOC (tally): 103.
Conversion commit: `31d61f536ab4bf0d7f0b8db613b9a99454cacfa5`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, SpawnMissileZAimed)` 8112–8119 |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, SpawnMissileAngleZSpeed)` 8166–8177 |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, SpawnSubMissile)` 8214–8220 |
| `SanitizeAimFlags` / `RejectNullSpawn` / `ShouldSuppressLocalCoopSpawn` | `P_SpawnPlayerMissile` preamble 8234–8251 |
| `UseWeaponAimLock` / `ResolvePlayerMissileAim` / `LineTargetRange` | auto-aim loop 8263–8300 |
| `CustomSpawnZ` / `AdjustMissileZForAttackOffset` / `ClampSpawnZToFloor` | custom Z branch 8302–8311 |
| `HuggerUsesAngleVelocity` / `ApplySpectralFriend` / `SpawnPlayerMissileBlocked` | spawn finalize 8318–8341 |
| `DefaultSpawnAngle` | `SpawnPlayerMissile` default angle 8357 |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, SpawnPlayerMissile)` 8344–8362 |


**Post-review ratio:** 103/159 managed/native ≈ 65% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjSpawnPlayerMissileTests.RejectNullSpawn_WhenTypeMissing`
- `VmPMobjSpawnPlayerMissileTests.CustomSpawnZ_DistinguishesFloorConstants`
- `VmPMobjSpawnPlayerMissileTests.ResolvePlayerMissileAim_WeaponLockKeepsPitch`
- `VmPMobjSpawnPlayerMissileTests.ClampSpawnZToFloor_RaisesZ`
- `VmPMobjSpawnPlayerMissileTests.DefaultSpawnAngle_Sentinel`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 755 --to 755` (0 hard / 0 soft).

## Seams

`Spawn`, `P_AimLineAttack`, `P_CheckMissileSpawn`, `P_PlaySpawnSound`, and `Net_RegisterCoopReplicatedPlayerMissile` are injected.

## Credit rationale

Closes uncredited gap after chunk494 sub-missile tail with player-fired spawn aim/Z policy and VM wrappers.
