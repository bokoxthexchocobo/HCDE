# Chunk 478 — GetBonePosition world multiply, GetObjectToWorldMatrix, CheckNoDelay, sector transition, UpdateWaterDepth head Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-poison-kill-sector-chunk57 src/playsim/p_mobj.cpp 5098-5108).

## Scope

GetBonePosition world multiply, GetObjectToWorldMatrix, CheckNoDelay, sector transition, UpdateWaterDepth head Retired (2026-10-24): `--band` hull overlaps existing credit (p-mobj-poison-kill-sector-chunk57 src/playsim/p_mobj.cpp 5098-5108).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `188d93d05fefbc2f62f63b7d5ce6c6b87ad4ddae`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `BuildEnterSectorActions` | `AActor::GetBonePosition` 4475–4499 |
| `Check3dCeilingHitAtTop` / `Check3dFloorHitAtFloor` | `AActor::CheckNoDelay` 5200–5327 |
| `CheckNoDelayEligible` | `AActor::CheckNoDelay` 5202 |
| `ClampNonNegativeDepth` | `AActor::GetBonePosition` 4475–4499 |
| `ClassifyWaterLevel` | `UpdateWaterDepth` 5289–5369 |
| `ClearHandleNoDelayFlag` | `AActor::CheckNoDelay` 5202–5204 |
| `CopyMatrixFlat` | `AActor::CheckNoDelay` 5202 |
| `EvaluateBonePositionGate` | `AActor::GetBonePosition` 4475–4499 |
| `EvaluateCheckNoDelay` | `AActor::CheckNoDelay` 5200–5225 |
| `GetObjectToWorldMatrixGate` | `AActor::CheckNoDelay` 5202 |
| `HeightSecUnderwater` / `RoverSubmergesActor` | `AActor::GetBonePosition` 4475–4499 |
| `RoverWaterDepth` | `UpdateWaterDepth` 5289–5369 |
| `SectorChanged` | `AActor::GetBonePosition` 4475–4499 |
| `Skip3dFloor` | `UpdateWaterDepth` 5289–5369 |
| `SplashOnBoomTransition` / `Treat3dFloorAsWater` | `AActor::GetBonePosition` 4475–4499 |
| `TryRoverWaterDepth` | `UpdateWaterDepth` 5289–5369 |

### Out-of-body uncredit (478)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **167** |
| **Out-of-body removed** | **0** |
| **Honest** | **167** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 478 --to 478` (0 hard / 0 soft).

## Seams

`VSMatrix`, `FindModelFrame`, `FState::CallAction`, and 3D-floor rover loops are not fully hosted.

## Credit rationale

Credits bone/world axis swizzle, matrix export gates, no-delay state dispatch, sector enter/exit actions, and water-depth setup from `p_mobj.cpp` (4475–4518, 5200–5369).
