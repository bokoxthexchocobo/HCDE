# Chunk 519 — FxCompareEq::Resolve

## Scope

FxCompareEq::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxCompareEq::Resolve** 4175–4391 (217 lines)

**Credited native LOC:** 217 (tally `nativeLoc` 217).

## Managed

`ZccCompareEqResolve.cs`
Managed LOC (tally): 151.
Conversion commit: `246ec0fa02de204b4fbebfb65872d9e14cd16e7c`.

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


**Post-review ratio:** 151/217 managed/native ≈ 70% (src file LOC at HEAD; see tally).

## Tests

- `ZccCompareEqResolveTests.FoldStringEqual_ApproxIgnoresCase`
- `ZccCompareEqResolveTests.FoldFloatEqual_Neq`
- `ZccCompareEqResolveTests.ShouldUnaryNotRight_EqAndZero`
- `ZccCompareEqResolveTests.TryGetNullConstant_IntZero`
- `ZccCompareEqResolveTests.TryFoldConstantCompare_IntEquality`
- `ZccCompareEqResolveTests.ShouldRewriteEqToUnaryNot_WhenRightNull`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 519 --to 519` (0 hard / 0 soft).

## Seams

`FxTypeCast`, `StringConstToChar`, `AreCompatiblePointerTypes`, and `ScriptPosition.Message` are not hosted.

## Credit rationale

Credits equality comparison type coercion, constant folding, and zero-simplify paths from `codegen.cpp` (4175–4391).
