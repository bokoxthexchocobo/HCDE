# Chunk 519 — FxCompareEq::Resolve

## Scope

FxCompareEq::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxCompareEq::Resolve** 4175–4391 (217 lines)

**Credited native LOC:** 217 (tally `nativeLoc` 217).

## Managed

`ZccCompareEqResolve.cs`
Managed LOC (tally): 218.
Conversion commit: `0d2e2f99841e6cd183d9c5fd7128f924cb9884fe`.

## Padding review (2026-10-21)

| Managed | Native (`codegen.cpp`) |
|---------|-------------------|
| `IdenticalValueTypes` / `BothNumeric` / `IsApproxEqOperator` / `ApproxEqAllowedForRegType` / `BothConstant` / `FoldStringEqual` / `FoldFloatEqual` / `FoldIntEqual` / `LeftIntIsNull` / `LeftFloatIsNull` / `LeftPointerIsNull` / `ShouldUnaryNotRight` / `ShouldBoolCastRight` / `PointerTypesCompatible` / `StateLabelNullCompare` / `ValueTypeNil` / `StringConstToCharEligible` / `TranslationIdLegacyCast` / `CanImplicitCastStringToName` / `RequestAddressForPointerCompare` / `RightNumericLeftString` / `VectorTypesComparable` / `CanCompareStateLabelToNull` / `CanCompareNullToStateLabel` / `PointerRegTypesMatch` / `CompatiblePointersOrNull` / `ShouldPromoteAfterNumeric` / `SimplifyEqAgainstZero` / `SimplifyNeqAgainstZero` / `SkipZeroSimplifyWhenApprox` / `EmitStringCompareUsesApprox` / `CmpCheckFlag` / `CmpApproxFlag` / `ClassifyReg` / `TryGetNullConstant` / `TryFoldConstantCompare` / `ShouldRewriteEqToUnaryNot` / `ShouldRewriteNeqToBoolCast` / `OperandTypesNeedPromotion` / `IncompatibleOperands` / `ResultValueTypeBool` | `Resolve` 4175–4391 (4181: `if (!left || !right)`; 4184: `return nullptr;`; 4188: `if (left->ValueType != right->ValueType && !(left->IsVector2() && rig…`) |

### Out-of-body uncredit (519)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **217** |
| **Out-of-body removed** | **0** |
| **Honest** | **217** |


**Post-review ratio:** 218/217 managed/native ≈ 100% (src file LOC at HEAD; see tally).

## Tests

- `ZccCompareEqResolveTests.FoldStringEqual_ApproxIgnoresCase`
- `ZccCompareEqResolveTests.FoldFloatEqual_Neq`
- `ZccCompareEqResolveTests.ShouldUnaryNotRight_EqAndZero`
- `ZccCompareEqResolveTests.TryGetNullConstant_IntZero`
- `ZccCompareEqResolveTests.TryFoldConstantCompare_IntEquality`
- `ZccCompareEqResolveTests.ShouldRewriteEqToUnaryNot_WhenRightNull`
- `ZccCompareEqResolveTests.RunCompareEqResolvePipeline_FoldsIntConstant`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 519 --to 519` (0 hard / 0 soft).

## Seams

`FxTypeCast`, `StringConstToChar`, `AreCompatiblePointerTypes`, and `ScriptPosition.Message` are not hosted.

## Credit rationale

Credits equality comparison type coercion, constant folding, and zero-simplify paths from `codegen.cpp` (4175–4391).
