# Chunk 759 — `EV_Thing_Projectile` and `EV_Thing_Damage` (`p_things.cpp` 250–420). Retired (2026-10-23): out-of-body credit removed; honest **34** < **150**.

## Scope

`EV_Thing_Projectile` and `EV_Thing_Damage` (`p_things.cpp` 250–420). Retired (2026-10-23): out-of-body credit removed; honest **34** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `54c21e5471804bd694e43fa7e938d1b125962f07`.

## Padding review (2026-10-21)

| Managed | Native (`p_things.cpp`) |
|---------|----------------------|
| `EvThingProjectile` | `FLevelLocals::EV_Thing_Projectile` **250–385** |
| `EvThingDamage` | `FLevelLocals::EV_Thing_Damage` **387–420** |

### Out-of-body uncredit (759)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **170** |
| **Out-of-body removed** | **136** |
| **Honest** | **34** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 759 --to 759` (0 hard / 0 soft).

## Seams

`Spawn`, `P_GetSpawnableType`, `PClass::FindActor`, `VelIntercept`, `P_CheckMissileSpawn`, `P_TestMobjLocation`, and `P_DamageMobj` are injected.

## Credit rationale

Whole-function orchestration for projectile spawn loops and TID damage/heal iteration.
