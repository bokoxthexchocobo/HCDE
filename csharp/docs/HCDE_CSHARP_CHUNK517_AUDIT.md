# Chunk 517 — SetAnimationInternal

## Scope

SetAnimationInternal

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **SetAnimationInternal** 6507–6676 (170 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

`VmActionSetAnimationInternal.cs`
Managed LOC (tally): 93.
Conversion commit: `65d914c5c0352542bab10cc23fd460bcb26c7137`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|-------------------|
| `RequiresDecoupledAnimations` / `NormalizeInterpolateTics` / `IsClearAnimation` / `AlreadyNone` / `ShouldClearToNone` / `AnimationNotFound` / `SameAnimationBlocked` / `StartFramePastEnd` / `LoopFramePastEnd` / `EndFramePastEnd` / `UseInstantPath` / `UseLoopFlag` / `ResolveLastFrame` / `ResolveStartFrame` / `ResolveLoopFrame` / `ComputeStartTic` / `ComputeSwitchOffset` / `ShouldUpdateInterpolation` / `StartTicInFuture` / `NegativeFramerateUsesModel` / `AnimationLength` / `WorldPausedOrFrozen` / `ModelTimerWithFrac` / `LoopFlagsFromSaf` / `ClearCurAnimToNone` / `MissingModelDef` / `NotFoundResult` / `FloorTic` / `IntFloorTic` / `InterpSameTic` / `ResetPrevAnimToInterp` / `FramerateAsFloat` / `PrintFramePastEnd` / `ModelAnimLoopFlag` / `ModelAnimNoneFlag` | `SetAnimationInternal` 6507–6676 (6509: `if(!self) ThrowAbortException(X_READ_NIL, "In function parameter self");`; 6511: `if(!(self->flags9 & MF9_DECOUPLEDANIMATIONS))`; 6518: `if(!BaseSpriteModelFrames.CheckKey(smf_class))`) |

### Out-of-body uncredit (517)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **170** |
| **Out-of-body removed** | **0** |
| **Honest** | **170** |


**Post-review ratio:** 93/170 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `VmActionSetAnimationInternalTests.SameAnimationBlocked_WhenNoOverrideAndSameStart`
- `VmActionSetAnimationInternalTests.ResolveLastFrame_NegativeEndUsesAnimEnd`
- `VmActionSetAnimationInternalTests.NormalizeInterpolateTics_MinOne`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 517 --to 517` (0 hard / 0 soft).

## Seams

`FindFModel`, `calcFrames`, `ModelAnimFramePrecalculatedIQM`, and `CalcBones` are not hosted.

## Credit rationale

Credits decoupled model animation selection, SAF flags, frame bounds checks, and interpolation tic scheduling from `p_actionfunctions.cpp` (6507–6676).
