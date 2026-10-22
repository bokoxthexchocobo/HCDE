# Chunk 758 — `EV_Thing_Spawn`, `P_MoveThing`, `EV_Thing_Move`, `InterceptDefaultAim`, `VelIntercept`, and `DEFINE VelIntercept` (`p_things.cpp` 41–248).

## Scope

`EV_Thing_Spawn`, `P_MoveThing`, `EV_Thing_Move`, `InterceptDefaultAim`, `VelIntercept`, and `DEFINE VelIntercept` (`p_things.cpp` 41–248).

## Native ranges

- `src/playsim/p_things.cpp` **FLevelLocals::EV_Thing_Spawn** 41–101 (61 lines)
- `src/playsim/p_things.cpp` **P_MoveThing** 106–127 (22 lines)
- `src/playsim/p_things.cpp` **FLevelLocals::EV_Thing_Move** 129–146 (18 lines)
- `src/playsim/p_things.cpp` **InterceptDefaultAim** 154–159 (6 lines)
- `src/playsim/p_things.cpp` **VelIntercept** 163–235 (73 lines)
- `src/playsim/p_things.cpp` **DEFINE_ACTION_FUNCTION(AActor, VelIntercept)** 237–248 (12 lines)

**Credited native LOC:** 192 (tally `nativeLoc` 192).

## Managed

`VmPThingsSpawnMoveIntercept.cs`
Managed LOC (tally): 361.
Conversion commit: `9b710f0e727b6cb289ea7e05ab1736c88ee79e08`.

## Padding review (2026-10-21)

| Managed | Native (`p_things.cpp`) |
|---------|----------------------|
| `EvThingSpawn` | `FLevelLocals::EV_Thing_Spawn` **41–101** |
| `P_MoveThing` | `P_MoveThing` **106–127** |
| `EvThingMove` | `FLevelLocals::EV_Thing_Move` **129–146** |
| `InterceptDefaultAim` | `InterceptDefaultAim` **154–159** |
| `VelIntercept` / `Vec3` / `RotationMatrixFromAxis` | `VelIntercept` **163–235** |
| `VelInterceptAction` | `DEFINE_ACTION_FUNCTION(AActor, VelIntercept)` **237–248** |


**Post-review ratio:** 361/192 managed/native ≈ 188% (src file LOC at HEAD; see tally).

## Tests

- `VmPThingsSpawnMoveInterceptTests.EvThingSpawn_CountsSuccessfulSpawns`
- `VmPThingsSpawnMoveInterceptTests.EvThingSpawn_RejectsWhenMonstersDisabled`
- `VmPThingsSpawnMoveInterceptTests.P_MoveThing_RestoresOriginWhenBlocked`
- `VmPThingsSpawnMoveInterceptTests.P_MoveThing_SetsRenderFlagOnSuccess`
- `VmPThingsSpawnMoveInterceptTests.VelIntercept_LeadTargetRotatesAimVector`
- `VmPThingsSpawnMoveInterceptTests.VelIntercept_DefaultAimWhenTargetVelZero`
- `VmPThingsSpawnMoveInterceptTests.VelInterceptAction_UsesSelfSpeedWhenNegative`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 758 --to 758` (0 hard / 0 soft).

## Seams

`Spawn`, `P_GetSpawnableType`, `P_TestMobjLocation`, `P_SpawnTeleportFog`, `GetActorIterator`, and `AngleTo` are injected.

## Credit rationale

Faithful whole-function VM ports for ACS spawn/move and lead-target intercept math (cross-product rotation matrix).
