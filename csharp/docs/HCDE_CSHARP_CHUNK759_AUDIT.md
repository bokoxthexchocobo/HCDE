# Chunk 759 — `EV_Thing_Projectile` and `EV_Thing_Damage` (`p_things.cpp` 250–420).

## Scope

`EV_Thing_Projectile` and `EV_Thing_Damage` (`p_things.cpp` 250–420).

## Native ranges

- `src/playsim/p_things.cpp` **FLevelLocals::EV_Thing_Projectile** 250–385 (136 lines)
- `src/playsim/p_things.cpp` **FLevelLocals::EV_Thing_Damage** 387–420 (34 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmPThingsProjectileDamage.cs`
Managed LOC (tally): 220.
Conversion commit: `9b710f0e727b6cb289ea7e05ab1736c88ee79e08`.

## Padding review (2026-10-21)

| Managed | Native (`p_things.cpp`) |
|---------|----------------------|
| `EvThingProjectile` | `FLevelLocals::EV_Thing_Projectile` **250–385** |
| `EvThingDamage` | `FLevelLocals::EV_Thing_Damage` **387–420** |


**Post-review ratio:** 220/170 managed/native ≈ 129% (src file LOC at HEAD; see tally).

## Tests

- `VmPThingsProjectileDamageTests.EvThingProjectile_RejectsUnknownKind`
- `VmPThingsProjectileDamageTests.EvThingProjectile_SpawnsMissileWithGravity`
- `VmPThingsProjectileDamageTests.EvThingDamage_HealsWhenAmountNegative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 759 --to 759` (0 hard / 0 soft).

## Seams

`Spawn`, `P_GetSpawnableType`, `PClass::FindActor`, `VelIntercept`, `P_CheckMissileSpawn`, `P_TestMobjLocation`, and `P_DamageMobj` are injected.

## Credit rationale

Whole-function orchestration for projectile spawn loops and TID damage/heal iteration.
