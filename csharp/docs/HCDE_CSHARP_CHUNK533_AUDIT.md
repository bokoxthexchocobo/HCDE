# Chunk 533 — FxAddSub::Resolve, FxMulDiv::Resolve

## Scope

FxAddSub::Resolve, FxMulDiv::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxAddSub::Resolve** 3309–3397 (89 lines)
- `src/common/scripting/backend/codegen.cpp` **FxMulDiv::Resolve** 3533–3669 (137 lines)

**Credited native LOC:** 226 (tally `nativeLoc` 226).

## Managed

`csharp/src/HCDE.Scripting/ZccBinaryArithmeticResolve.cs` (src-only credit).

## Tests

- `ZccBinaryArithmeticResolveTests.FoldFloatBinary_AddSubMulDiv`
- `ZccBinaryArithmeticResolveTests.VectorBinaryCompatible_AllowsV3PlusV2`
- `ZccBinaryArithmeticResolveTests.ClassifyMulDivVector_RejectsModOnVector`
- `ZccBinaryArithmeticResolveTests.FoldFloatBinary_DivisionByZero_ReturnsNaN`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `reaudit_chunks.py audit --from 533 --to 533` (0 hard / 0 soft).

## Seams

`Promote`, `FxFloatCast`, `FxConstant`, and `Emit` are not hosted.

## Credit rationale

Credits ZScript add/sub and mul/div resolve typing, vector/quaternion rules, and constant folding from `codegen.cpp` (3309–3669).

## Padding review (2026-10-21)

| Managed method | Native (FxAddSub::Resolve 3309–3397) |
|----------------|--------------------------------------|
| `CustomAdditionHook` | `CheckForCustomAddition` 3321–3328 |
| `TextureIdPlusInteger` | 3331–3334 |
| `QuaternionBinary` | 3335–3338 |
| `VectorBinaryCompatible` | 3339–3349 |
| `ClassifyAddSub` | type-dispatch + `error` 3355–3358 |
| `TryFoldBinaryConstants` / `FoldFloatBinary` / `FoldIntBinary` | `goon:` constant fold 3361–3390 |

| Managed method | Native (FxMulDiv::Resolve 3533–3669) |
|----------------|--------------------------------------|
| `ClassifyMulDivVector` + vector helpers | vector/quaternion switch 3551–3603 |
| `ClassifyMulDiv` | numeric path 3606–3608 + fold 3616–3661 |

**Removed (padding):** `NumericBinary` (unused `&&` duplicate), `NeedsFloatCastForIntegerScale` (identity, unused), `ClassifyAddSubEnd` (thin `TryFoldBinaryConstants` wrapper).

**Ratio ~92%:** Managed orchestration splits AddSub vs MulDiv paths; integer scale still requires `FxFloatCast` at resolve time (seam, not duplicated here).
