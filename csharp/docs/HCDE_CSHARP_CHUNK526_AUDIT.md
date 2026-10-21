# Chunk 526 — AActor::A_Teleport

## Scope

AActor::A_Teleport

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::A_Teleport** 3255–3423 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc`).

## Managed

`csharp/src/HCDE.Playsim/VmActionTeleport.cs` (src-only credit).

## Tests

- `VmActionTeleportTests.BlockedByNoteleport_WithoutOverride`
- `VmActionTeleportTests.RandomDecideIndex_Clamps`
- `VmActionTeleportTests.ComputeFinalZ_CeilingClamp`
- `VmActionTeleportTests.ShouldAbortRandomDecide_WhenRollFails`
- `VmActionTeleportTests.SourceFogUsesSpawnedClass_WhenNotActorFog`

## Seams

`P_TeleportMove`, `GetSpotState`, `P_SpawnTeleportFog`, and state jumps are not hosted.

## Credit rationale

Credits A_Teleport random-decide table, sensitive-Z gate, final Z selection, fog emission policy, and post-teleport Z/yaw/velocity flags from `p_actionfunctions.cpp` (3255–3423).

## Padding review (2026-10-21)

Reviewed deepen commit `080fffc9` against the padding checklist.

**Removed:** `TeleportPlanStep` / `PlanAfterSpotFound` (switch restatement only); identity helpers (`UseBossSpotWhenTargetNull`, `NeedsDefaultTeleportLabel`, `ClearsStatePointer`, duplicate `InventoryChainPrimed`); tuple wrapper `PositionAtFinalZ`; thin duplicates of `KeepAngle` / `KeepVelocity` (`YawAfterMove` / `VelocityZeroed` retained as single expressions tied to native branches); redundant one-liners that only forwarded a single flag with no combined native condition.

**Kept:** `RandomDecideChance` indexing; `BlockedByNoteleport`; `SensitiveZFails` (honors `TF_USESPOTZ` for Z probe); `ComputeFinalZ`; `ShouldAbortRandomDecide`; `ShouldForceOrigin`; fog block `WantsFogBlock` plus source/dest emission paths (`EmitSourceFog`, `EmitDestFog`, actor vs spawned-class source fog); `PostMoveZ`, `YawAfterTeleport`, `ZeroVelocityAfterTeleport`, `StateJumpAllowed`, `TelefragMove`.

Remeasured surviving src LOC at HEAD after trim (see tally `managedLoc`).
