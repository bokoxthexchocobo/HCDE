# p_spec / sector friction and push chunk 4 audit

## Scope

| Symbol | Lines | Managed |
| --- | --- | --- |
| `P_SetSectorFriction` | 751–797 | `SectorEnvironmentLineSpecials.SetFriction` (native amount formula, `SECF_FRICTION`) |
| `P_GetFriction` sector branch | 732–745 | `ActorPhysics.TerrainMovementFactors` |
| `DPusher::Tick` (flat subset) | 149–200 | `ActorPhysics.ApplySectorPush` on sector `PushForce*` |

**Behavior native LOC credited:** 97.

## Tests

`LineSpecialClosureTests.SectorFrictionSpecialUpdatesTaggedSector`, `SectorFrictionMovementTests`, `SectorPushMovementTests`.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
