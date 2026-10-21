# Chunk 526 — AActor::A_Teleport

## Scope

AActor::A_Teleport

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AActor::A_Teleport** 3255–3423 (169 lines)

**Credited native LOC:** 169 (tally `nativeLoc` 169).

## Managed

`csharp/src/HCDE.Playsim/VmActionTeleport.cs` (src-only credit).

Managed LOC (tally): 97.

## Tests

- `VmActionTeleportTests.BlockedByNoteleport_WithoutOverride`
- `VmActionTeleportTests.RandomDecideIndex_Clamps`
- `VmActionTeleportTests.ComputeFinalZ_CeilingClamp`
- `VmActionTeleportTests.ShouldAbortRandomDecide_WhenRollFails`
- `VmActionTeleportTests.SourceFogUsesSpawnedClass_WhenNotActorFog`
- `VmActionTeleportTests.PositionAtFinalZ_MatchesComputeFinalZ`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 526 --to 526` (0 hard / 0 soft).

## Seams

`P_TeleportMove`, `GetSpotState`, `P_SpawnTeleportFog`, and state jumps are not hosted.

## Credit rationale

Credits A_Teleport random-decide table, sensitive-Z gate, final Z selection, fog emission policy, and post-teleport Z/yaw/velocity flags from `p_actionfunctions.cpp` (3255–3423).

## Padding review (2026-10-21)

| Managed | Native lines |
|---------|----------------|
| `RandomDecideIndex` / `ShouldAbortRandomDecide` | 3291–3308 |
| `SensitiveZFails` | 3332–3337 |
| `ComputeFinalZ`, `PositionAtFinalZ` | 3341–3349, `PosAtZ` |
| `WantsFogBlock`, `EmitSourceFog` / `EmitDestFog`, `SourceFogUsesSpawnedClass` | 3362–3389 |
| `PostMoveZ`, `YawAfterTeleport`, `ZeroVelocityAfterTeleport`, `StateJumpAllowed`, `TelefragMove` | 3392–3416 |

**Removed:** `RefMissing`, `SpotStateMissing`, `TeleportSpotMissing`, `AboveFloorDelta`, `TeleportSucceeded`, `ShouldReturnInventoryPrimed`, `DefaultTeleportLabelNeeded` (identity/duplicate predicates); duplicate `DestFogUsesActor`.

**Ratio ~57%:** Remaining gap is inventory `numret` priming, `COPY_AAPTR`, spot lookup, and `P_TeleportMove` (seams).
