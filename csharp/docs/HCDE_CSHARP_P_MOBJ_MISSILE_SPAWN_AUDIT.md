# p_mobj.cpp missile spawn audit

## Scope

Native `src/playsim/p_mobj.cpp` `P_CheckMissileSpawn` subset: velocity nudge inside shooter radius, sector update, `P_TryMove` probe, horizon destroy, blocked spawn explosion via `P_ExplodeMissile` path.

## Implementation

- `MissileSpawnCheck.cs` (callable; not yet on every `SpawnProjectile` to preserve existing monster/player spawn tests)

## Tests

`MissileSpawnCheckTests` (open-room advance, `A_ClearLastHeard` dispatch).

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
