# Chunk 588 — CalculateAnimationInternal and CalculateAnimation DEFINE bodies (7246–7395)

## Scope

CalculateAnimationInternal and CalculateAnimation DEFINE bodies (7246–7395)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **CalculateAnimationInternal** 7246–7266 (21 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, CalculateAnimation** 7268–7285 (18 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, CalculateAnimationUI** 7287–7304 (18 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, CalculateAnimationFrame** 7306–7330 (25 lines)
- `src/playsim/p_actionfunctions.cpp` **FindAnimationFrameInternal** 7332–7355 (24 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, FindAnimationFrameAt** 7357–7385 (29 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, FindAnimationFrame** 7387–7414 (28 lines)

**Credited native LOC:** 163 (tally `nativeLoc` 163).

## Managed

`VmActionCalculateAnimation.cs`
Managed LOC (tally): 85.
Conversion commit: `bbe15cf8a5076d0e707e6540f4bf280dbfc8840f`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `CalculateAnimationInternalNullModel` | `CalculateAnimationInternal` 7248–7253 |
| `CalculateAnimationLayerNull` / `CalculateAnimationTic` | DEFINE `CalculateAnimation` 7268–7285 |
| `UiModelTimer` / `ReturnNullWhenLayerMissing` | DEFINE `CalculateAnimationUI` 7287–7304 |
| `CalculateAnimationFrameNull*` | `CalculateAnimationFrame` 7306–7330 |
| `FindAnimationFrameNegativeTic` / `ShouldCreateInterpFrame` | `FindAnimationFrameInternal` 7332–7355 |

**Post-review ratio:** 85/163 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmActionCalculateAnimationTests.CalculateAnimationInternalNullModel_WhenMissing`
- `VmActionCalculateAnimationTests.CalculateAnimationLayerNull_ReturnsNull`
- `VmActionCalculateAnimationTests.RestoreAnimLayerAfterCalculate_WhenLayerPresent`
- `VmActionCalculateAnimationTests.UiModelTimer_AddsFracWhenUnpaused`
- `VmActionCalculateAnimationTests.CalculateAnimationFrameNullIframe_Aborts`
- `VmActionCalculateAnimationTests.ReturnNullWhenModelMissing`
- `VmActionCalculateAnimationTests.FindAnimationFrameNegativeTic_SkipsWork`
- `VmActionCalculateAnimationTests.ShouldCreateInterpFrame_WhenLayerAndTicOk`
- `VmActionCalculateAnimationTests.FindAnimationFrameAtSetsFloat_WhenThirdRet`
- `VmActionCalculateAnimationTests.CalcUsesModelTimerPlusOne`
- `VmActionCalculateAnimationTests.FindAnimationFrameAtSetsObject_WhenSecondRet`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 588 --to 588` (0 hard / 0 soft).

## Seams

`FindFModel`, `calcFrames`, `PrecalculateFrame`, `RestoreAnimLayer`, and VM object allocation are not hosted.

## Credit rationale

Credits null-layer guards, model timer sampling, and CalculateAnimation* early exits.
