# Chunk 560 — AnimInfoToAnimLayer, RestoreAnimLayer, SetAnimationLayer* actions (7080–7244)

## Scope

AnimInfoToAnimLayer, RestoreAnimLayer, SetAnimationLayer* actions (7080–7244)

## Native ranges

- `src/playsim/p_actionfunctions.cpp` **AnimInfoToAnimLayer** 7080–7120 (41 lines)
- `src/playsim/p_actionfunctions.cpp` **RestoreAnimLayer** 7122–7130 (9 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetAnimationLayerAnimation** 7132–7163 (32 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetAnimationLayerAnimationUI** 7165–7196 (32 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetAnimationLayerFrameRate** 7198–7220 (23 lines)
- `src/playsim/p_actionfunctions.cpp` **DEFINE_ACTION_FUNCTION(AActor, SetAnimationLayerFrameRateUI** 7222–7244 (23 lines)

**Credited native LOC:** 160 (tally `nativeLoc` 160).

## Managed

`VmActionSetAnimationLayer.cs`
Managed LOC (tally): 117.
Conversion commit: `1b5223cb570f5e6407713088d2f5fbb760670320`.

## Padding review (2026-10-21)

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `AnimInfoToAnimLayerNative` | `AnimInfoToAnimLayer` 7080–7120 |
| `RestoreAnimLayerNative` | `RestoreAnimLayer` 7122–7130 |
| `SetAnimationLayerAnimationNative` | `SetAnimationLayerAnimation` 7132–7163 |
| `SetAnimationLayerAnimationUiNative` | `SetAnimationLayerAnimationUI` 7165–7196 |
| `SetAnimationLayerFrameRateNative` | `SetAnimationLayerFrameRate` 7198–7220 |
| `SetAnimationLayerFrameRateUiNative` | `SetAnimationLayerFrameRateUI` 7222–7244 |

**Post-review ratio:** 117/160 managed/native ≈ 73% (src file LOC at HEAD; see tally).

## Tests

- `VmActionSetAnimationLayerTests.SetAnimationLayerFrameRateNative_CopiesPrecalcFrame`
- `VmActionSetAnimationLayerTests.SetAnimationLayerAnimationNative_RestoresOnFailure`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 560 --to 560` (0 hard / 0 soft).

## Seams

`DAnimationLayer` allocation, `SetAnimationInternal`, and `TArray<TRS>` move are not hosted.

## Credit rationale

Whole-function ports for layer animation DEFINE bodies and helpers.
