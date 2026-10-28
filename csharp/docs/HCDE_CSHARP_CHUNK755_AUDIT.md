# Chunk 755 — `SpawnMissileAngleZSpeed` / `SpawnSubMissile` defines and whole `P_SpawnPlayerMissile` (`p_mobj.cpp` 8112–8362). Retired (2026-10-23): out-of-body credit removed; honest **46** < **150**.

## Scope

`SpawnMissileAngleZSpeed` / `SpawnSubMissile` defines and whole `P_SpawnPlayerMissile` (`p_mobj.cpp` 8112–8362). Retired (2026-10-23): out-of-body credit removed; honest **46** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `f08ca3b158f9a05d9cba8f7df5921b22599ed3e8`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, SpawnMissileZAimed)` **8112–8119** |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, SpawnMissileAngleZSpeed)` **8166–8177** |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, SpawnSubMissile)` **8214–8220** |
| `RejectNullSpawn` / `ShouldSuppressLocalCoopSpawn` / `SanitizeAimFlags` / `UseWeaponAimLock` / `LineTargetRange` / `AutoAimAngleOffsetsDeg` / `ShouldBreakAutoAimLoop` / `ResolvePlayerMissileAim` / `CustomSpawnZ` / `AdjustMissileZForAttackOffset` / `ClampSpawnZToFloor` / `HuggerUsesAngleVelocity` / `ApplySpectralFriend` / `SpawnPlayerMissileBlocked` / `DefaultSpawnAngle` | `P_SpawnPlayerMissile` **8230–8342** |
| (native macro only) | `DEFINE_ACTION_FUNCTION(AActor, SpawnPlayerMissile)` **8344–8362** |

### Out-of-body uncredit (755)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **159** |
| **Out-of-body removed** | **113** |
| **Honest** | **46** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 755 --to 755` (0 hard / 0 soft).

## Seams

`Spawn`, `P_AimLineAttack`, `P_CheckMissileSpawn`, `P_PlaySpawnSound`, and `Net_RegisterCoopReplicatedPlayerMissile` are injected.

## Credit rationale

Closes uncredited gap after chunk494 sub-missile tail with player-fired spawn aim/Z policy and VM wrappers.
