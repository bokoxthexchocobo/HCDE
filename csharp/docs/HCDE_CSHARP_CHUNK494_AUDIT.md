# Chunk 494 — P_SpawnMissileXYZ tail, P_SpawnMissile/Z, OldSpawn, aimed and angle spawn, sub-missile Retired (2026-10-23): out-of-body credit removed; honest **112** < **150**.

## Scope

P_SpawnMissileXYZ tail, P_SpawnMissile/Z, OldSpawn, aimed and angle spawn, sub-missile Retired (2026-10-23): out-of-body credit removed; honest **112** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `55c270c3dd5ac1957c51e19713fc79a1586c0ef6`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `FailCheckMissileSpawn` / `SpectralFriendFromOwner` / `AbortWhenSpawnBlocked` / `ApplySpectralFriend` / `ZSpawnUsesPosAtZ` | `P_SpawnMissileXYZ` 7980–7994 (7985: `if (th->flags4 & MF4_SPECTRAL)`; 7990: `if (checkspawn && !P_CheckMissileSpawn(th, source->radius))`; 7991: `return NULL;`) |
| `SpawnMissileZOffset` | `P_SpawnMissile` 8009–8016 (8011: `if (source == nullptr)`; 8013: `return nullptr;`; 8015: `return P_SpawnMissileXYZ(source->PosPlusZ(32 + source->GetBobOffset()…`) |
| `P_SpawnMissileZ` | `P_SpawnMissileZ` 8027–8034 (8029: `if (source == nullptr)`; 8031: `return nullptr;`; 8033: `return P_SpawnMissileXYZ(source->PosAtZ(z), source, dest, type);`) |
| `OldSpawn_NullArgs` / `OldSpawnVelZ` / `OldSpawnDistanceScale` / `ComputeOldSpawnVelZ` | `P_OldSpawnMissile` 8045–8071 (8047: `if (source == nullptr || type == nullptr)`; 8049: `return nullptr;`; 8063: `if (th->flags4 & MF4_SPECTRAL)`) |
| `AimedVerticalSpeed` | `P_SpawnMissileZAimed` 8082–8110 (8093: `if (source == nullptr || type == nullptr)`; 8095: `return nullptr;`; 8109: `return P_SpawnMissileAngleZSpeed (source, z, type, an, vz, speed);`) |
| `AngleSpawn_NullArgs` / `AdjustAngleSpawnZ` / `AngleSpeedSpawn_Spectral` | `P_SpawnMissileAngleZSpeed` 8130–8164 (8133: `if (source == nullptr || type == nullptr)`; 8135: `return nullptr;`; 8139: `if (z != ONFLOORZ && z != ONCEILINGZ)`) |
| `SubMissile_NullAfterSpawn` / `SubMissileSpectralInherit` / `SubMissileSpectralFromTarget` / `SubMissile_UseTargetPlayer` | `P_SpawnSubMissile` 8179–8212 (8183: `if (source == nullptr || type == nullptr)`; 8185: `return nullptr;`; 8192: `if (other->flags4 & MF4_SPECTRAL)`) |

### Out-of-body uncredit (494)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **156** |
| **Out-of-body removed** | **44** |
| **Honest** | **112** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 494 --to 494` (0 hard / 0 soft).

## Seams

`P_SpawnMissileXYZ_ShadowHandling`, `Spawn`, `P_PlaySpawnSound`, `P_CheckMissileSpawn`, and `P_AimLineAttack` are not hosted.

## Credit rationale

Credits spawn check failure, spectral friend setup, legacy spawn Z/velocity, aimed vertical speed, angle spawn Z clip, and sub-missile spectral inheritance from `p_mobj.cpp` (7980–8212).
