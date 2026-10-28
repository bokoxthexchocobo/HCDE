# Chunk 550 — A_FaceMovementDirection, A_CopySpriteFrame, A_SetVisibleRotation, A_SetTranslation Merged: [549]. Retired (2026-10-23): out-of-body credit removed; honest **144** < **150**.

## Scope

A_FaceMovementDirection, A_CopySpriteFrame, A_SetVisibleRotation, A_SetTranslation Merged: [549]. Retired (2026-10-23): out-of-body credit removed; honest **144** < **150**.

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `c319421c886a730ef522059b489f2ca8d82b692a`.

## Padding review (2026-10-21)

Removed `PlanFaceMovement` / `VelocityYawDegrees` orchestration (not a single native function). Remaining methods map 1:1 to credited bands.

| Managed | Native (`p_actionfunctions.cpp`) |
|---------|----------------------------------|
| `EarlyFaceMovementFail`, `FmdfFlags` | 4897–4902, 4903 guards |
| `ShouldAdjustYaw`, `ClampYawDelta`, `ClampPitchDelta`, `PitchFromVelocity` | `A_FaceMovementDirection` 4903–4983 |
| `CopySpriteFrameShouldFail`, `ShouldCopySprite`, `ShouldCopyFrame` | `A_CopySpriteFrame` 4997–5015 |
| `SetVisibleRotationShouldFail`, `ShouldAssignVisible*` | `A_SetVisibleRotation` 5032–5067 |
| `SetTranslationRequiresName` | `A_SetTranslation` 5075–5082 |

<!-- merged from chunk549 -->
Deepen pass (2026-10-21) had inflated ratio via `*EmitPlan` record structs, string opcode sequences, and identity helpers (`MaskCvarShift`, `SelfEmitIncludesCheck`, etc.). Removed; see method map below.

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `TryResolveSuper` | `FxSuper::Resolve` 7318–7329 |
| `GlobalVariableRequestAddress`, `GlobalVariableWritable` | `FxGlobalVariable::RequestAddress` 7348–7353 |
| `MapCvarRealType` | `ECVarType` values via `FxCVar::Resolve` 7415–7437 |
| `ShouldUseBitLoad`, `GlobalVariableBitMask`, `GlobalVariableLoadOp` | `FxGlobalVariable::Emit` 7380–7388 |
| `MapCvarRealType`, `CvarResolveDeletesOnUnknown` | `FxCVar::Resolve` 7415–7443 |
| `EmitLoadOp`, `CvarEmitUsesStringRegister` | `FxCVar::Emit` 7447–7477 |
| `ApplyFlagCvarShift`, `ApplyFlagCvarMask` | `CVAR_Flag` emit 7487–7488 |
| `ApplyMaskCvar` | `CVAR_Mask` emit 7497–7498 |

### Out-of-body uncredit (550)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **150** |
| **Out-of-body removed** | **6** |
| **Honest** | **144** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 550 --to 550` (0 hard / 0 soft).

## Seams

`COPY_AAPTR`, `AActor::SetAngle`/`SetPitch`, and `SetTranslation` VM calls are not hosted.

## Credit rationale

Credits native guard and angle/pitch/copy/visible-rotation bands mirrored by `VmActorFaceMovementCopySprite` and tests.
