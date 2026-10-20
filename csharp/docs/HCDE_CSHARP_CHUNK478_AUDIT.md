# Chunk 478 — GetBonePosition world multiply, GetObjectToWorldMatrix, CheckNoDelay, sector transition, UpdateWaterDepth head

## Scope

GetBonePosition world multiply, GetObjectToWorldMatrix, CheckNoDelay, sector transition, UpdateWaterDepth head

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::GetBonePosition** 4475–4499 (25 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetObjectToWorldMatrix** 4501–4518 (18 lines)
- `src/playsim/p_mobj.cpp` **AActor::CheckNoDelay** 5200–5225 (26 lines)
- `src/playsim/p_mobj.cpp` **AActor::CheckSectorTransition** 5241–5275 (35 lines)
- `src/playsim/p_mobj.cpp` **UpdateWaterDepth** 5289–5369 (81 lines)

**Credited native LOC:** 185 (tally `nativeLoc` 185).

## Managed

`VmPMobjBoneWorldTick.cs`, `VmPMobjBoneWorldTickTests.cs`
Managed LOC (tally): 234.
Conversion commit: `78fa1fea7127f5f79b06ba571fef446317af475c`.

## Tests

- `VmPMobjBoneWorldTickTests.BonePositionGate_BlocksPicnum`
- `VmPMobjBoneWorldTickTests.BoneWorld_SwizzlesAxes`
- `VmPMobjBoneWorldTickTests.ObjectToWorldMatrix_GateRequiresBoneFlag`
- `VmPMobjBoneWorldTickTests.CheckNoDelay_ClearsFlagWhenEligible`
- `VmPMobjBoneWorldTickTests.SectorTransition_BuildsHitFloor`
- `VmPMobjBoneWorldTickTests.WaterDepth_UnderwaterSectorFillsHeight`
- `VmPMobjBoneWorldTickTests.Skip3dFloor_MissingOrSolid`
- `VmPMobjBoneWorldTickTests.RoverWater_FindsSubmergedDepth`
- `VmPMobjBoneWorldTickTests.ClassifyWaterLevel_DeepPlayerView`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 478 --to 478` (0 hard / 0 soft).

## Seams

`VSMatrix`, `FindModelFrame`, `FState::CallAction`, and 3D-floor rover loops are not fully hosted.

## Credit rationale

Credits bone/world axis swizzle, matrix export gates, no-delay state dispatch, sector enter/exit actions, and water-depth setup from `p_mobj.cpp` (4475–4518, 5200–5369).
