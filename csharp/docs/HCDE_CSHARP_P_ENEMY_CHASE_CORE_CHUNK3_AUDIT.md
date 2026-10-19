# p_enemy.cpp chase / movement core chunk 3 audit

## Scope

Native `src/playsim/p_enemy.cpp` chase stack reproduced in `MonsterBrain`, `ActorWanderActions`, and `ActorPhysics` (flat subset), with existing regression suites plus `CantLeaveFloorPic` chase rollback (`A_DoChase` **2740–2753**).

| Symbol | Lines | Managed |
| --- | --- | --- |
| `P_CheckMeleeRange` | 253–292 | `MonsterBrain.CheckNativeMeleeRange` |
| `P_CheckMissileRange` | 307–365 | `MonsterBrain.CheckMissileRange` |
| `P_Move` | 482–717 | `ActorWanderActions.MoveCore` |
| `P_SmartMove` | 725–757 | `ActorWanderActions.Move` |
| `P_TryWalk` | 773–781 | `ActorWanderActions.TryWalk` |
| `P_DoNewChaseDir` | 794–912 | `ActorWanderActions.ChooseChaseDirection` |
| `P_NewChaseDir` | 935–1098 (excl. `#if 0` 1029–1040) | `MonsterBrain.ChooseNativeChaseDirection`, dropoff escape |
| Dropoff escape | 967–1027 | `ActorPhysics.TryDropOffEscapeVector` |
| `P_RandomChaseDir` | 1107–1257 (excl. comment 1220–1245) | `ActorWanderActions.RandomDirection` |
| `P_IsVisible` | 1268–1350 | `ActorVisibility.IsVisible` |
| `isTargetablePlayer` | 1316–1363 | `MonsterBrain.IsTargetablePlayer` |
| `P_LookForPlayers` | 1789–1933 | `MonsterBrain.FindForLook` / `FindPlayerTarget` |
| `A_DoChase` | 2397–2764 | `MonsterBrain.TickCore` native movement path |

**Behavior native LOC credited:** 1513.

## Tests

`NativeChaseMovementTests`, `ChaseDirectionTests`, `ChaseDropOffEscapeTests`, `ChaseMeleeAvoidanceTests`, `ChasePostAttackCountTests`, `ChaseHazardAvoidanceTests`, `MissileSightTests`, `CantLeaveFloorPicTests.NativeChaseUsesFloorRestriction`, `MonsterBlockedMoveTests`, `DoomMonsterAttackTests`, and related wander/chase suites.

## Verified

`dotnet build -warnaserror`, full `HCDE.Playsim.Tests` green.
