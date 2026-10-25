# Chunk 493 — P_HitFloor liquid probe, net missile registration, GetDefaultSpeed, P_SpawnMissileXYZ aim Retired (2026-10-23): out-of-body credit removed; honest **148** < **150**.

## Scope

P_HitFloor liquid probe, net missile registration, GetDefaultSpeed, P_SpawnMissileXYZ aim Retired (2026-10-23): out-of-body credit removed; honest **148** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `8dc093073018f00c9a7e81f531f435025d99e439`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `Valid3dFloor` / `SpectralFriend` | `P_HitFloor` 7672–7677 |
| `TouchyCrushOnImpact` / `DisarmTouchy` / `ClientSideFloorMatch` / `SolidOrSwimmable3d` / `RoverTopMatches` / `TryHitWaterVia3dFloor` / `TouchingSectorFloorMatch` | `P_HitFloor` 7679–7760 (7683: `if (thing->flags6 & MF6_TOUCHY && ((thing->flags6 & MF6_ARMED) || thi…`; 7685: `thing->flags6 &= ~MF6_ARMED; // Disarm`; 7687: `return false;`) |
| `RejectInvalidSpawn` | `P_CheckMissileSpawn` 7762–7769 |
| `RegisterMissileNet` | `Net_RegisterMonsterReplicatedMissile` 7771–7777 (7773: `if (missile == nullptr || source == nullptr)`; 7774: `return;`) |
| `UseFastMonsterSpeed` / `PickDefaultSpeed` | `GetDefaultSpeed` 7910–7922 (7912: `if (type == NULL)`; 7913: `return 0;`; 7916: `if (G_SkillProperty(SKILLP_FastMonsters))`) |
| `SpawnMissile_NullSourceOrType` / `AdjustSpawnZForFloorclip` / `HuggerClearsVertical` / `RaiseTrajectoryOverTarget` / `TrajectoryZBoost` / `ApplyFloorclipWhenCustomZ` | `P_SpawnMissileXYZ` 7933–7979 (7935: `if (source == nullptr || type == nullptr)`; 7937: `return nullptr;`; 7940: `if (dest == NULL)`) |

### Out-of-body uncredit (493)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **163** |
| **Out-of-body removed** | **15** |
| **Honest** | **148** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 493 --to 493` (0 hard / 0 soft).

## Seams

`P_HitWater`, `P_DamageMobj`, `Spawn`, `P_PlaySpawnSound`, `P_CheckMissileSpawn`, and shadow handling are not hosted.

## Credit rationale

Credits touchy landing crush, sector/3D-floor splash delegation, replicated missile registration, fast-monster speed pick, and spawn/velocity policy from `p_mobj.cpp` (7672–7979).
