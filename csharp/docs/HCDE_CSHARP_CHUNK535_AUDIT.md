# Chunk 535 — FxCompareRel prologue, FxSpaceship, FxBitOp/Shift/Concat, FxBinaryLogical (consolidated chunk534–535)

## Scope

FxCompareRel prologue, FxSpaceship, FxBitOp/Shift/Concat, FxBinaryLogical (consolidated chunk534–535)

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxCompareRel::Resolve** 3876–3933 (58 lines)
- `src/common/scripting/backend/codegen.cpp` **FxSpaceship::Resolve** 4704–4752 (49 lines)
- `src/common/scripting/backend/codegen.cpp` **FxBitOp::Resolve** 4497–4538 (42 lines)
- `src/common/scripting/backend/codegen.cpp` **FxShift::Resolve** 4592–4629 (38 lines)
- `src/common/scripting/backend/codegen.cpp` **FxConcat::Resolve** 4809–4839 (31 lines)
- `src/common/scripting/backend/codegen.cpp` **FxBinaryLogical::Resolve** 4961–5045 (85 lines)
- `src/common/scripting/backend/codegen.cpp` **FxBinaryLogical::Flatten** 5053–5082 (30 lines)

**Credited native LOC:** 333 (tally `nativeLoc` 333).

## Managed

(see conversion commit file list)
Managed LOC (tally): 244.
Conversion commit: `4694cbaeb9621338892467c9b10923eb25b3f7b9`.

## Tests

- `ZccCompareRelResolveTests.NeedsTranslationCast_Below412`
- `ZccCompareRelResolveTests.StringComparisonPath_EitherSideString`
- `ZccCompareRelResolveTests.FirstNumericStringCoerce_PrefersRightReplacement`
- `ZccCompareRelResolveTests.PlanStringPathCoercions_IncludesCastsAndResultType`
- `ZccSpaceshipResolveTests.OperatorVersionPolicy_Requires4151`
- `ZccSpaceshipResolveTests.TryFoldConstantSpaceship_Ordering`
- `ZccSpaceshipResolveTests.ClassifyOperands_RejectsNonNumeric`
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

`StringConstToChar`, `FxStringCast`, `Promote`, `FxBoolCast`, `FxConstant`, `SAFE_RESOLVE`, and `Emit` are not hosted.

## Credit rationale

Credits compare-rel coercion (3876–3933), spaceship (4704–4752), and bitwise/shift/concat/logical resolve (4497–5082); numeric compare fold remains chunk 55.
