# Chunk 535 — FxBitOp::Resolve, FxShift::Resolve, FxConcat::Resolve, FxBinaryLogical::Resolve/Flatten

## Scope

FxBitOp::Resolve, FxShift::Resolve, FxConcat::Resolve, FxBinaryLogical::Resolve/Flatten

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxBitOp::Resolve** 4497–4538 (42 lines)
- `src/common/scripting/backend/codegen.cpp` **FxShift::Resolve** 4592–4629 (38 lines)
- `src/common/scripting/backend/codegen.cpp` **FxConcat::Resolve** 4809–4839 (31 lines)
- `src/common/scripting/backend/codegen.cpp` **FxBinaryLogical::Resolve** 4961–5045 (85 lines)
- `src/common/scripting/backend/codegen.cpp` **FxBinaryLogical::Flatten** 5053–5082 (30 lines)

**Credited native LOC:** 226 (tally `nativeLoc` 226).

## Managed

`ZccBitwiseShiftConcatLogicalResolve.cs`, `ZccBitwiseShiftConcatLogicalResolveTests.cs`
Managed LOC (tally): 118.
Conversion commit: `5043cccf8b890fa516d7dc94027b57e82e43060a`.

## Tests

- `ZccBitwiseShiftConcatLogicalResolveTests.BitOpFold_AndOrXor`
- `ZccBitwiseShiftConcatLogicalResolveTests.BitOpBoolPath_SetsBoolType`
- `ZccBitwiseShiftConcatLogicalResolveTests.ShiftFold_LeftRightUnsigned`
- `ZccBitwiseShiftConcatLogicalResolveTests.ShiftRewritesRShiftToUnsigned`
- `ZccBitwiseShiftConcatLogicalResolveTests.ConcatFold_StringConstants`
- `ZccBitwiseShiftConcatLogicalResolveTests.ConcatRejectsNilRegisterOperands`
- `ZccBitwiseShiftConcatLogicalResolveTests.LogicalAnd_FoldFalse`
- `ZccBitwiseShiftConcatLogicalResolveTests.LogicalOr_PrunesKnownSide`
- `ZccBitwiseShiftConcatLogicalResolveTests.Flatten_MergesSameOperatorLists`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 535 --to 535` (0 hard / 0 soft).

## Seams

`Promote`, `FxBoolCast`, `FxConstant`, `SAFE_RESOLVE`, and `Emit` are not hosted.

## Credit rationale

Credits bitwise/shift/concat resolve and &&/|| short-circuit constant folding plus flatten from `codegen.cpp` (4497–4538, 4592–4629, 4809–4839, 4961–5082).
