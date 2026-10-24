# Chunk 590 — OverwriteBones*, SetBonesRange/Mask*, ForceRecalculate, model-flag DEFINE natives, GetRenderStyle (7575–7809)

## Scope

OverwriteBones*, SetBonesRange/Mask*, ForceRecalculate, model-flag DEFINE natives, GetRenderStyle (7575–7809)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, OverwriteBones** 7568–7590 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetBonesRange** 7592–7619 (28 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetBonesRangeUI** 7621–7648 (28 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, OverwriteBonesRange** 7650–7674 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetBonesMask** 7676–7702 (27 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetBonesMaskUI** 7704–7730 (27 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, OverwriteBonesMask** 7732–7755 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, ForceRecalculateBones** 7757–7764 (8 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION_NATIVE(AActor, SetModelFlag** 7766–7797 (32 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, GetRenderStyle** 7801–7809 (9 lines)

**Credited native LOC:** 231 (tally `nativeLoc` 231).

## Managed

`VmActionSetBonesOverrides.cs`
Managed LOC (tally): 143.
Conversion commit: `556407f8b483c561680f25dd1bca3d55d78eb284`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `OverwriteModeUsesMode` | `OverwriteBones` 7568–7589 |
| `RangeLoopEnd` / `RangeLoopInBounds` | `SetBonesRange*` / `OverwriteBonesRange` 7592–7673 |
| `MaskSelectsBone` | `SetBonesMask*` / `OverwriteBonesMask` 7676–7754 |
| `ForceRecalculateInvokesCalcBones` | `ForceRecalculateBones` 7757–7763 |
| `SetModelFlagNativeForwards` | DEFINE natives 7766–7796 |
| `GetRenderStyleScansStyles` | `GetRenderStyle` 7801–7808 |

**Post-review ratio:** 143/231 managed/native ≈ 62% (src file LOC at HEAD; see tally).

## Tests

- `VmActionSetBonesOverridesTests.OverwriteModeUsesMode`
- `VmActionSetBonesOverridesTests.RangeLoopEnd_CapsByLength`
- `VmActionSetBonesOverridesTests.RangeLoopInBounds`
- `VmActionSetBonesOverridesTests.MaskSelectsBone_WhenTrue`
- `VmActionSetBonesOverridesTests.ForceRecalculate_WhenSelf`
- `VmActionSetBonesOverridesTests.SetModelFlagNativeForwards`
- `VmActionSetBonesOverridesTests.RenderStyleNotFound_ReturnsNegOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 590 --to 590` (0 hard / 0 soft).

## Seams

`SetGetBoneShared`, `BoneOverride::Set`, `CalcBones`, and `LegacyRenderStyles` are not hosted.

## Credit rationale

Credits override loops, mask gating, UI tic paths, and render-style scan after chunk 589 blend/offset band.
