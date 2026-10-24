# Chunk 517 — SetAnimationInternal

## Scope

SetAnimationInternal

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **SetAnimationInternal** 6507–6676 (170 lines)

**Credited native LOC:** 170 (tally `nativeLoc` 170).

## Managed

(see conversion commit file list)
Managed LOC (tally): 111.
Conversion commit: `6ce8572c5a15bfeae191b24d133c4c89972582ba`.

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
