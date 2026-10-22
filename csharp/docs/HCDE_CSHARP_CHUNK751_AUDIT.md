# Chunk 751 — View roll, blood type, portal transitions, model timer, bone queries (`p_mobj.cpp` 4191–4456).

## Scope

View roll, blood type, portal transitions, model timer, bone queries (`p_mobj.cpp` 4191–4456).

## Native ranges

- `src/playsim/p_mobj.cpp` **AActor::SetViewRoll** 4191–4201 (11 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetBloodType** 4203–4214 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetPortalTransition** 4216–4247 (32 lines)
- `src/playsim/p_mobj.cpp` **AActor::CheckPortalTransition** 4249–4286 (38 lines)
- `src/playsim/p_mobj.cpp` **DEFINE_ACTION_FUNCTION(AActor, CheckPortalTransition)** 4288–4294 (7 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetModelTimer** 4296–4306 (11 lines)
- `src/playsim/p_mobj.cpp` **AActor::CalcBones** 4308–4351 (44 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetBoneTRS** 4353–4360 (8 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetBoneMatrix** 4362–4373 (12 lines)
- `src/playsim/p_mobj.cpp` **AActor::GetBoneEulerAngles** 4375–4456 (82 lines)

**Credited native LOC:** 257 (tally `nativeLoc` 257).

## Managed

`VmPMobjPortalBones.cs`
Managed LOC (tally): 296.
Conversion commit: `96da6dc6ea92f54cdee80a6a08b2e525c104cddd`.

## Padding review (2026-10-21)

| Managed | Native (`p_mobj.cpp`) |
|---------|-------------------|
| `SetViewRoll` | `AActor::SetViewRoll` 4191–4201 |
| `GetBloodType` | `AActor::GetBloodType` 4203–4214 |
| `GetPortalTransition` | `AActor::GetPortalTransition` 4216–4247 |
| `CheckPortalTransition` | `AActor::CheckPortalTransition` 4249–4286 |
| `CheckPortalTransition` | `DEFINE_ACTION_FUNCTION(AActor, CheckPortalTransition)` 4288–4294 |
| `GetModelTimer` | `AActor::GetModelTimer` 4296–4306 |
| `CalcBones` / `ShouldCalcBones` | `AActor::CalcBones` 4308–4351 |
| `GetBoneTrs` | `AActor::GetBoneTRS` 4353–4360 |
| `GetBoneMatrix` | `AActor::GetBoneMatrix` 4362–4373 |
| `GetBoneEulerAngles` | `AActor::GetBoneEulerAngles` 4375–4456 |


**Post-review ratio:** 296/257 managed/native ≈ 115% (src file LOC at HEAD; see tally).

## Tests

- `VmPMobjPortalBonesTests.SetViewRoll_SetsInterpCheat`
- `VmPMobjPortalBonesTests.GetModelTimer_ClientSide`
- `VmPMobjPortalBonesTests.GetPortalTransition_CeilingCrossing`
- `VmPMobjPortalBonesTests.CheckPortalTransition_LinksWhenMoved`
- `VmPMobjPortalBonesTests.CalcBones_ProcessesWhenEligible`
- `VmPMobjPortalBonesTests.GetBoneTrs_ReturnsStoredBone`
- `VmPMobjPortalBonesTests.GetBoneMatrix_CopiesFlatMatrix`
- `VmPMobjPortalBonesTests.GetBoneEulerAngles_ReturnsZeroWhenPicnumOverrides`
- `VmPMobjPortalBonesTests.GetBoneEulerAngles_DerivesYawFromForward`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 751 --to 751` (0 hard / 0 soft).

## Seams

`VMCall`, sector portal hosting, `ProcessModelFrame`, and matrix/quaternion math are injected.

## Credit rationale

Whole p_mobj portal/bone band; stops before `GetBonePosition` (4458+).
