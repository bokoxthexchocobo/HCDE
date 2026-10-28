# Chunk 535 — FxCompareRel prologue, FxSpaceship, FxBitOp/Shift/Concat, FxBinaryLogical (consolidated chunk534–535) Retired (2026-10-24): `--band` hull overlaps existing credit (zcc-compare-rel-chunk55 src/common/scripting/backend/codegen.cpp 3934-4046).

## Scope

FxCompareRel prologue, FxSpaceship, FxBitOp/Shift/Concat, FxBinaryLogical (consolidated chunk534–535) Retired (2026-10-24): `--band` hull overlaps existing credit (zcc-compare-rel-chunk55 src/common/scripting/backend/codegen.cpp 3934-4046).

## Native ranges


**Credited native LOC:** 0 (tally `nativeLoc` 0).

## Managed

(see conversion commit file list)
Managed LOC (tally): 0.
Conversion commit: `d6bb05e53cbee35dfd430ccbbebef3033218dbcb`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits compare-rel **3876–3933**, spaceship **4704–4752**, bit/shift/concat/logical **4497–5082** (`codegen.cpp`). Numeric compare fold remains other chunks.

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `NeedsTranslationCastToInt` / `StringComparisonPath` / `ReplaceLeftViaStringConstToChar` / `ReplaceRightViaStringConstToChar` / `PlanStringPathCoercions` / `CanPromoteNumericCompare` / `EntersNumericCompareBody` | `FxCompareRel::Resolve` **3876–3933** |
| `OperatorVersionPolicy` / `ShouldDeleteOnVersionError` / `ShouldWarnDeprecatedOperator` / `BothNumeric` / `ClassifyOperands` / `FoldSpaceshipCompare` / `ShouldPromoteNumeric` / `NonNumericErrorOperatorName` | `FxSpaceship::Resolve` **4704–4752** |
| `BitOpBoolOperands` / `BitOpNumericOperands` / `FoldBitOp` / `NeedsBoolCast` | `FxBitOp::Resolve` **4497–4538** |
| `RewriteRShiftForUnsignedRight` / `FoldShift` | `FxShift::Resolve` **4592–4629** |
| `ConcatOperandValid` / `ConcatConstantFoldEligible` / `FoldStringConcat` | `FxConcat::Resolve` **4809–4839** |
| `LogicalAndFold` / `LogicalOrFold` / `Flatten` helpers | `FxBinaryLogical::Resolve` **4961–5045**, `Flatten` **5053–5082** |

### Out-of-body uncredit (535)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **333** |
| **Out-of-body removed** | **0** |
| **Honest** | **333** |


## Tests


## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 535 --to 535` (0 hard / 0 soft).

## Seams

`StringConstToChar`, `FxStringCast`, `Promote`, `FxBoolCast`, `FxConstant`, `SAFE_RESOLVE`, and `Emit` are not hosted.

## Credit rationale

Credits compare-rel coercion (3876–3933), spaceship (4704–4752), and bitwise/shift/concat/logical resolve (4497–5082); numeric compare fold remains chunk 55.
