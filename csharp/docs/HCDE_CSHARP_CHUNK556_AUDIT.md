# Chunk 556 — Bone info static natives + GetBoneBaseTRS bodies (5656–5882); thin DEFINE forwards not credited

## Scope

Bone info static natives + GetBoneBaseTRS bodies (5656–5882); thin DEFINE forwards not credited

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **GetRootBonesNative** 5656–5664 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetRootBones** 5666–5674 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneNameNative** 5676–5681 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneIndexNative** 5683–5692 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneParentNative** 5710–5715 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **GetNamedBoneParentNative** 5717–5726 (10 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneChildrenNative** 5744–5752 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **GetNamedBoneChildrenNative** 5754–5766 (13 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetBoneBaseTRS** 5790–5827 (38 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneBaseTRS** 5829–5868 (40 lines)
- `src/playsim/p_actionfunctions.cpp` **GetBoneCountNative** 5870–5875 (6 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetBoneCount** 5877–5882 (6 lines)

**Credited native LOC:** 162 (tally `nativeLoc` 162).

## Managed

`VmActorBoneInfoGetters.cs`
Managed LOC (tally): 179.
Conversion commit: `3d8b3e656f5fbe260c159053acb365654c4c633e`.

## Padding review (2026-10-21)

Credit shrink (2026-10-21): dropped thin `DEFINE_ACTION_FUNCTION_NATIVE` forwards (GetBoneName, GetBoneParent, etc.). No ratio padding.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `GetRootBonesNative` | `GetRootBonesNative` 5656–5664 |
| `GetRootBonesActionWrapper` | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetRootBones` 5666–5674 |
| `GetBoneNameNative` | `GetBoneNameNative` 5676–5681 |
| `GetBoneIndexNative` | `GetBoneIndexNative` 5683–5692 |
| `GetBoneParentNative` | `GetBoneParentNative` 5710–5715 |
| `GetNamedBoneParentNative` | `GetNamedBoneParentNative` 5717–5726 |
| `GetBoneChildrenNative` | `GetBoneChildrenNative` 5744–5752 |
| `GetNamedBoneChildrenNative` | `GetNamedBoneChildrenNative` 5754–5766 |
| `GetBoneBaseTrsNative` | `DEFINE_ACTION_FUNCTION(AActor, GetBoneBaseTRS` 5790–5827 |
| `GetNamedBoneBaseTrsNative` | `DEFINE_ACTION_FUNCTION(AActor, GetNamedBoneBaseTRS` 5829–5868 |
| `GetBoneCountNative` | `GetBoneCountNative` 5870–5875 |
| `GetBoneCountActionWrapper` | `DEFINE_ACTION_FUNCTION_NATIVE(AActor, GetBoneCount` 5877–5882 |

### Out-of-body uncredit (556)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **162** |
| **Out-of-body removed** | **0** |
| **Honest** | **162** |


**Post-review ratio:** 179/162 managed/native ≈ 110% (src file LOC at HEAD; see tally).

## Tests

- `VmActorBoneInfoGettersTests.GetRootBonesNative_SkipsNullOut`
- `VmActorBoneInfoGettersTests.GetBoneNameNative_ReturnsJointNameIndex`
- `VmActorBoneInfoGettersTests.GetBoneIndexNative_ReturnsResolvedIndex`
- `VmActorBoneInfoGettersTests.GetBoneParentNative_ReturnsParent`
- `VmActorBoneInfoGettersTests.GetBoneBaseTrsNative_CapsNumretAtThree`
- `VmActorBoneInfoGettersTests.GetBoneCountNative_ReturnsJointCount`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 556 --to 556` (0 hard / 0 soft).

## Seams

`SetGetBoneShared`, `GetBoneShared`, `FModel` joint queries, and `TArray` fill are not hosted.

## Credit rationale

Credits match whole `static` bodies and full `GetBoneBaseTRS` DEFINE implementations only; PARAM/ACTION_RETURN wrappers are not credited.
