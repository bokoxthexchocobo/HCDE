# Chunk 556 — GetRootBones, bone name/index/parent/children, GetBoneBaseTRS, GetBoneCount (5656–5882)

## Scope

GetRootBones, bone name/index/parent/children, GetBoneBaseTRS, GetBoneCount (5656–5882)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **GetRootBonesNative** 5656–5664 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetRootBones** 5666–5674 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneNameNative** 5676–5681 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneIndexNative** 5683–5692 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetBoneName** 5694–5700 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetBoneIndex** 5702–5708 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneParentNative** 5710–5715 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **GetNamedBoneParentNative** 5717–5726 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetBoneParent** 5728–5734 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetNamedBoneParent** 5736–5742 (7 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneChildrenNative** 5744–5752 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **GetNamedBoneChildrenNative** 5754–5766 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetBoneChildren** 5768–5777 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetNamedBoneChildren** 5779–5788 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetBoneBaseTRS** 5790–5827 (38 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneBaseTRS** 5829–5868 (40 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneCountNative** 5870–5875 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetBoneCount** 5877–5882 (6 lines)

**Credited native LOC:** 210 (tally `nativeLoc` 210).

## Managed

`VmActorBoneInfoGetters.cs`
Managed LOC (tally): 209.
Conversion commit: `9ba6f21c7ee10d88177d51d29ffeb64a4b12df27`.

## Padding review (2026-10-21)

Whole-function ports only.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `GetRootBonesNative` | `GetRootBonesNative` 5656–5664 |
| `GetBoneNameNative` | `GetBoneNameNative` 5676–5681 |
| `GetBoneIndexNative` | `GetBoneIndexNative` 5683–5692 |
| `GetBoneParentNative`, `GetNamedBoneParentNative` | 5710–5726 |
| `GetBoneChildrenNative`, `GetNamedBoneChildrenNative` | 5744–5766 |
| `GetBoneBaseTrsNative`, `FinalizeBoneBaseTrsNumret`, `GetBoneBaseTrsAssigns*` | `GetBoneBaseTRS` 5790–5827 |
| `GetNamedBoneBaseTrsNative` | `GetNamedBoneBaseTRS` 5829–5868 |
| `GetBoneCountNative` | `GetBoneCountNative` 5870–5875 |

**Post-review ratio:** 209/210 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `VmActorBoneInfoGettersTests.GetRootBonesNative_FillsWhenModelOk`
- `VmActorBoneInfoGettersTests.GetRootBonesNative_RequiresOutPointer`
- `VmActorBoneInfoGettersTests.GetBoneNameNative_ReturnsModelIndexWhenOk`
- `VmActorBoneInfoGettersTests.GetBoneIndexNative_ReturnsResolvedIndex`
- `VmActorBoneInfoGettersTests.GetBoneParentNative_UsesJointParent`
- `VmActorBoneInfoGettersTests.GetBoneChildrenNative_RequiresValidBone`
- `VmActorBoneInfoGettersTests.GetBoneBaseTrsNative_DefaultsWithoutModel`
- `VmActorBoneInfoGettersTests.FinalizeBoneBaseTrsNumret_CapsAtThree`
- `VmActorBoneInfoGettersTests.GetBoneBaseTrsAssignsScaling_WhenNumretAboveTwo`
- `VmActorBoneInfoGettersTests.GetBoneCountNative_ReturnsJointCount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 556 --to 556` (0 hard / 0 soft).

## Seams

`SetGetBoneShared`, `GetBoneShared`, `FModel` joint queries, and `TArray` fill are not hosted.

## Credit rationale

One managed method per credited whole native `static` body or `DEFINE_ACTION` implementation; credits match ported control flow only.
