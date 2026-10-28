# Chunk 589 — FindAnimationFrameUI and BlendAnimationFrames (7416–7565)

## Scope

FindAnimationFrameUI and BlendAnimationFrames (7416–7565)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, FindAnimationFrameUI** 7416–7449 (34 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, BlendAnimationFrames** 7454–7481 (28 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, BlendAnimationFrames** 7483–7574 (92 lines)

**Credited native LOC:** 154 (tally `nativeLoc` 154).

## Managed

`VmActionFindAnimationFrame.cs`
Managed LOC (tally): 80.
Conversion commit: `2d132978a20a1077f6ea2da69b63a6f1b4445202`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `UiFindTic` / `UiFindUsesFrozenAwareTic` | `FindAnimationFrameUI` 7416–7449 |
| `SetRet*` | return slot guards 7433–7446 |
| `BlendSizesMatch` / `BlendOneMinusT` / `BlendLoopCount` | `BlendAnimationFrames` 7454–7476 |

**Post-review ratio:** 80/154 managed/native ≈ 52% (src file LOC at HEAD; see tally).

## Tests

- `VmActionFindAnimationFrameTests.UiFindTic_AddsFracWhenRunning`
- `VmActionFindAnimationFrameTests.SetRetInterWhenThird`
- `VmActionFindAnimationFrameTests.BlendSizesMatch_RequiresEqual`
- `VmActionFindAnimationFrameTests.ThrowWhenBlendSizesDiffer_FlagsMismatch`
- `VmActionFindAnimationFrameTests.BlendOneMinusT_Complements`
- `VmActionFindAnimationFrameTests.ShouldInterpolateBone_InRange`
- `VmActionFindAnimationFrameTests.ResizeBlendOutput_WhenPositive`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 589 --to 589` (0 hard / 0 soft).

## Seams

`FindAnimationFrameInternal`, `InterpolateBone`, VM `ret[]` writes, and frame allocation are not hosted.

## Credit rationale

Credits UI tic sampling, multi-return guards, and blend size check from action functions.
