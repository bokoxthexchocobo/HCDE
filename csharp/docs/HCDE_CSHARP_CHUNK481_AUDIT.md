# Chunk 481 — StaticSpawn, SpawnClientSide, ClassForSpawn, LevelSpawned, HandleSpawnFlags, BeginPlay Merged: [476]. Merged: [480].

## Scope

StaticSpawn, SpawnClientSide, ClassForSpawn, LevelSpawned, HandleSpawnFlags, BeginPlay Merged: [476]. Merged: [480].

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5630–5662 (33 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5664–5673 (10 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5675–5684 (10 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5686–5695 (10 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5697–5709 (13 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5713–5715 (3 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5717–5723 (7 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5727–5786 (60 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5796–5805 (10 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5807–5812 (6 lines)
- `src/playsim/p_mobj.cpp` **AActor::StaticSpawn** 5814–5823 (10 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`VmPMobjStaticSpawn.cs`, `VmPMobjCalcBones.cs`, `VmPMapAimLineAttack.cs`
Managed LOC (tally): 308.
Conversion commit: `35b81f123460c1c749d43313e867d0a68a76e9e5`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `EnableNetworkingOnServerThinker` / `ClientSideSpawnRequiresClientClass` | `StaticSpawn` / `Spawn` / `SpawnClientSide` / `ClassForSpawn` 5630–5709 |
| `LevelSpawnedTics` | `LevelSpawned` unsynced `pr_spawnmapthing` tics 5713–5715 |
| `ClearDroppedFlag` / `SetMapthingSpawnFlag` | `LevelSpawned` `MF_DROPPED` + `MTF_MAPTHING` 5717–5723 |
| `ApplyHandleSpawnFlags` / `NocountStripsKillAndItem` / `DormantSpawnFlag` / `ShadowSpawnStyle` | `HandleSpawnFlags` 5727–5786 |
| `BeginPlayClearsDormant` / `ClearDormantFlag` | `BeginPlay` + `CallBeginPlay` 5796–5823 |

<!-- merged from chunk476 -->
| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `BoneArraySized` / `BoneIndexValid` / `DecoupledAnimations` | `AActor::CalcBones` 4318 |
| `EvaluateBoneQuery` | `AActor::GetBonePosition` 4458–4474 |
| `EvaluateCalcBones` | `AActor::CalcBones` 4308–4351 |
| `EvaluateSingleModel` | `AActor::CalcBones` 4318 |
| `GimbalRollFromUp` | `AActor::CalcBones` 4308–4351 |
| `MissingModelFrame` | `AActor::CalcBones` 4318 |
| `ModelTimerTic` | `AActor::CalcBones` 4308–4351 |
| `NextFrameForProcess` | `AActor::CalcBones` 4318 |
| `PickMaxAbsComponent` / `RecalcOrBoneFlag` / `SolveRollSin` / `UseXAxisSolve` / `UseYAxisSolve` | `AActor::CalcBones` 4308–4351 |

### Out-of-body uncredit (481)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **176** |
| **Out-of-body removed** | **4** |
| **Honest** | **172** |


**Post-review ratio:** 308/172 managed/native ≈ 179% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjStaticSpawnTests.StaticSpawn_RejectsNullType`
- `VmPMobjStaticSpawnTests.ClientSideSpawn_RequiresClientClass`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 481 --to 481` (0 hard / 0 soft).

## Seams

`ConstructActor`, `CreateThinker`, `VMCall`, and `CallDeactivate` are not hosted.

## Credit rationale

Credits static spawn validation/replacement, clientside spawn gate, class lookup, mapthing tic jitter, spawn-flag mutations, and BeginPlay dormant handling from `p_mobj.cpp` (5630–5823).
