# Chunk 560 — AnimInfoToAnimLayer, RestoreAnimLayer, SetAnimationLayer* actions (7080–7244) (merged chunk 561) (merged chunk 561) (merged chunk 561)

## Scope

AnimInfoToAnimLayer, RestoreAnimLayer, SetAnimationLayer* actions (7080–7244) (merged chunk 561) (merged chunk 561) (merged chunk 561)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AnimInfoToAnimLayer** 7080–7120 (41 lines)
- `src/playsim/p_actionfunctions.cpp` **AnimInfoToAnimLayer** 7122–7130 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **AnimInfoToAnimLayer** 7132–7163 (32 lines)
- `src/playsim/p_actionfunctions.cpp` **AnimInfoToAnimLayer** 7165–7196 (32 lines)
- `src/playsim/p_actionfunctions.cpp` **AnimInfoToAnimLayer** 7198–7220 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **AnimInfoToAnimLayer** 7222–7244 (23 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmActionSetAnimationLayer.cs`, `ZccArrayElementResolve.cs`
Managed LOC (tally): 183.
Conversion commit: `8beab6f17546f9346f745b730897de964c82199c`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `AnimInfoToAnimLayerNative` | `AnimInfoToAnimLayer` 7080–7120 |
| `RestoreAnimLayerNative` | `RestoreAnimLayer` 7122–7130 |
| `SetAnimationLayerAnimationNative` | `SetAnimationLayerAnimation` 7132–7163 |
| `SetAnimationLayerAnimationUiNative` | `SetAnimationLayerAnimationUI` 7165–7196 |
| `SetAnimationLayerFrameRateNative` | `SetAnimationLayerFrameRate` 7198–7220 |
| `SetAnimationLayerFrameRateUiNative` | `SetAnimationLayerFrameRateUI` 7222–7244 |

### Out-of-body uncredit (560)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **160** |
| **Out-of-body removed** | **0** |
| **Honest** | **160** |


**Post-review ratio:** 183/160 managed/native ≈ 114% (src file LOC at HEAD; see tally).

## Tests

- `VmActionSetAnimationLayerTests.SetAnimationLayerFrameRateNative_CopiesPrecalcFrame`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 560 --to 560` (0 hard / 0 soft).

## Seams

`DAnimationLayer` allocation, `SetAnimationInternal`, and `TArray<TRS>` move are not hosted.

## Credit rationale

Whole-function ports for layer animation DEFINE bodies and helpers.
