# p_mobj.cpp P_CheckMissileSpawn (live) audit

## Scope

Native `src/playsim/p_mobj.cpp` `P_CheckMissileSpawn` (**7781–7861**, 81 lines behavior-credited): velocity nudge inside shooter radius, sector update, `P_TryMove`, ripper/dontrip pass-through, owner overlap tolerance, horizon destroy, blocked `P_ExplodeMissile`.

## Implementation

- `MissileSpawnCheck.Check` wired from `SpawnProjectileCore` and `ActorCombatActions.SpawnProjectile`
- Owner/blocker/horizon handling aligned with managed `ActorPhysics.TryMove`

## Tests

`MissileSpawnCheckTests`; spawn-offset / hugger / tracer tests updated for post-check geometry.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green (17,019).
