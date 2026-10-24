# Run 9 recognition vs conversion re-audit

## Rules applied

- **Conversion** credits only native behavior newly implemented and covered by named regression tests in this run.
- **Recognition** credits native ranges already reproduced in managed code before run 9; every range lists the tests that exercise it.
- Ranges without a named test receive **zero** credit in either bucket.

## Conversion (run 9)

| Chunk | Native LOC | Managed LOC | Native range | Tests |
| --- | ---: | ---: | --- | --- |
| `p-enemy-cantleave-chase-rollback` | 14 | 18 | `p_enemy.cpp` `A_DoChase` 2740–2753 | `CantLeaveFloorPicTests.NativeChaseUsesFloorRestriction` |
| `p-map-use-fail-noway` | 47 | 65 | `p_map.cpp` `P_NoWayTraverse` 5972–5993; `P_UseLines` 6003–6027 (use-fail rumble path) | `UseRumbleTests.BlockedNonSpecialLineEmitsUseFailPattern`, `UseTraceTests` |

**Run 9 conversion total:** 61 native, 83 managed.

## Recognition (run 9)

### `p-enemy-chase-core-chunk3-recognition` — 1499 native, 0 managed

| Symbol | Lines | Tests |
| --- | --- | --- |
| `P_CheckMeleeRange` | 253–292 | `NativeChaseMovementTests`, `ChaseMeleeAvoidanceTests`, `DoomMonsterAttackTests` |
| `P_CheckMissileRange` | 307–365 | `MissileSightTests`, `NativeChaseMovementTests` |
| `P_Move` | 482–717 | `NativeChaseMovementTests`, `MonsterBlockedMoveTests`, `WanderSubstepTests` |
| `P_SmartMove` | 725–757 | `NativeChaseMovementTests`, `ChaseDirectionTests` |
| `P_TryWalk` | 773–781 | `NativeChaseMovementTests`, `WanderSubstepTests` |
| `P_DoNewChaseDir` | 794–912 | `ChaseDirectionTests`, `NativeChaseMovementTests` |
| `P_NewChaseDir` | 935–1098 | `ChaseDirectionTests`, `ChaseDropOffEscapeTests` |
| `P_RandomChaseDir` | 1107–1257 | `ChaseDirectionTests`, `WanderFloatAdjustmentTests` |
| `P_IsVisible` | 1268–1350 | `StealthVisibilityTests`, `MissileSightTests` |
| `isTargetablePlayer` | 1316–1363 | `StealthVisibilityTests`, `ThingHateTests` |
| `P_LookForPlayers` | 1789–1933 | `ThingHateTests`, `TargetSwitchContractTests`, `StealthVisibilityTests` |
| `A_DoChase` (excl. 2740–2753) | 2397–2739, 2754–2764 | `NativeChaseMovementTests`, `ChasePostAttackCountTests`, `ChaseHazardAvoidanceTests` |

### `p-map-try-use-lineattack-chunk3-recognition` — 1555 native, 0 managed

| Symbol | Lines | Tests |
| --- | --- | --- |
| `P_CheckPosition` | 1817–2036 | `TryMoveTests`, `ActorPhysicsTests`, `MissileSpawnCheckTests` |
| `P_TryMove` | 2368–2889 | `TryMoveTests`, `WanderSubstepTests`, `CantLeaveFloorPicTests` |
| `FSlide::SlideMove` | 3018–3371 | `SlideMoveTests`, `PlayerSlideTests` |
| `P_LineAttack` | 4699–5050 | `GeometryLineAttackTests`, `HitscanPlaneDamageTests`, `CombatTraceTests` |
| `P_UseTraverse` | 5829–5957 | `UseTraceTests`, `UseRumbleTests` |

### `p-mobj-movement-thinker-chunk3-recognition` — 1594 native, 0 managed

| Symbol | Lines | Tests |
| --- | --- | --- |
| `P_ExplodeMissile` | 1958–2100 | `ProjectileMovementTests`, `MapRadiusAttackTests` |
| `P_XYMovement` | 2421–2948 | `ProjectileMovementTests`, `WanderSubstepTests`, `FallingDamageTests` |
| `P_ZMovement` | 2955–3480 | `FallingDamageTests`, `CanJumpTests`, `FlyingVerticalFrictionTests` |
| `AActor::Tick` | 4521–5075 | `WanderSubstepTests`, `SpawnedSoulRestoreTests` |
| Missile step collide | 2680–2765 | `ProjectileMovementTests`, `CombatAiTests` |

**Run 9 recognition total:** 4648 native (1499 + 1555 + 1594).

## Cumulative buckets (after this correction)

| Bucket | Native LOC |
| --- | ---: |
| Conversion (cumulative) | 8027 |
| Recognition (cumulative) | 4648 |
| Combined credited behavior | 12675 |
| Remaining engine scope | 500059 |

`remainingNative` counts only **unconverted** engine scope: `baselineNative − conversionNative`.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green after correction commits.
