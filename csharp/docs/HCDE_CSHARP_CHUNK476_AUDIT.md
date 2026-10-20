# Chunk 476 — CalcBones, GetBoneTRS, GetBoneMatrix, GetBonePosition head

## Scope

CalcBones, GetBoneTRS, GetBoneMatrix, GetBonePosition head

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::CalcBones** 4308–4351 (44 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetBoneTRS** 4353–4360 (8 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetBoneMatrix** 4362–4373 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetBoneEulerAngles** 4375–4456 (82 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetBonePosition** 4458–4474 (17 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmPMobjCalcBones.cs`, `VmPMobjCalcBonesTests.cs`
Managed LOC (tally): 80.
Conversion commit: `96f35b7637fe3532edfb5ecc75d1d12dd03ac35a`.

## Tests

- `VmPMobjCalcBonesTests.CalcBones_SkipsWhenNoModelData`
- `VmPMobjCalcBonesTests.CalcBones_RunsWhenEligible`
- `VmPMobjCalcBonesTests.BoneQuery_ValidIndices`
- `VmPMobjCalcBonesTests.BoneQuery_BlocksPicnum`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 476 --to 476` (0 hard / 0 soft).

## Seams

`ProcessModelFrame`, `FindModelFrame`, `Models[]`, and matrix math are not hosted.

## Credit rationale

Credits model bone refresh gates, per-model override loop, and bone matrix/euler query guards from `p_mobj.cpp` (4308–4474).
