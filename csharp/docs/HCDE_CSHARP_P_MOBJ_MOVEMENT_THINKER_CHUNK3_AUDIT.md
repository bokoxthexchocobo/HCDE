# p_mobj.cpp thinker / movement / explode chunk 3 audit

## Scope

Native `src/playsim/p_mobj.cpp` actor tick movement and missile impact paths represented in managed thinkers (`Actor.Tick` / `ActorPhysics.Step`, `ProjectileActor.TickMovement`) plus live `P_CheckMissileSpawn` (run 8).

| Symbol | Lines | Managed |
| --- | --- | --- |
| `P_XYMovement` | 2421–2948 | `ActorPhysics.StepCore` horizontal integration |
| `P_ZMovement` | 2955–3480 | `ActorPhysics.StepCore` gravity / floor / float |
| `P_MobjThinker` / `AActor::Tick` | 4521–5075 (velocity subset) | `Actor` thinker tick + `ActorPhysics.Step` |
| `P_ExplodeMissile` | 1958–2100 | `ProjectileActor.ExplodeForAction`, radius damage |
| Missile step collide | 2680–2765 | `ProjectileActor` sweep / rip / explode |
| `P_CheckMissileSpawn` | 7781–7861 | `MissileSpawnCheck` (credited in run 8 chunk) |

**Behavior native LOC credited:** 1594 (excludes run-8 `P_CheckMissileSpawn` 81 lines to avoid double count).

## Tests

`ProjectileMovementTests`, `MissileSpawnCheckTests`, `WanderSubstepTests`, `CanJumpTests`, `FallingDamageTests`, rocket/splash and ripper suites.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green (17,020).
