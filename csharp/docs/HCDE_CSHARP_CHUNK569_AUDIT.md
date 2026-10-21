# Chunk 569 — FxTypeCast::Emit, FxPlusSign, FxMinusSign (2025–2210; unary-not nodes stay chunks 56/61)

## Scope

FxTypeCast::Emit, FxPlusSign, FxMinusSign (2025–2210; unary-not nodes stay chunks 56/61)

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxTypeCast::Emit** 2025–2030 (6 lines)
- `src/common/scripting/backend/codegen.cpp` **FxPlusSign** 2038–2084 (47 lines)
- `src/common/scripting/backend/codegen.cpp` **FxMinusSign** 2092–2210 (119 lines)

**Credited native LOC:** 172 (tally `nativeLoc` 172).

## Managed

`ZccUnarySignCodegen.cs`
Managed LOC (tally): 105.
Conversion commit: `d7b085cc6bfc3ad46681278b25752276a23ea29a`.

## Padding review (2026-10-21)

| Managed | Native (`codegen.cpp`) |
|---------|---------------------------|
| `TypeCastEmitIsUnreachable` | `FxTypeCast::Emit` 2025–2030 |
| `ClassifyPlusResolve` / `PlusSignResolveAllowed` / `PlusSignEmitForwardsOperand` | `FxPlusSign` ctor–`Emit` 2038–2084 |
| `ResolveMinusSign` | `FxMinusSign::Resolve` 2115–2146 |
| `NegateKonstIntFromPool` / `PlanMinusEmit` / `SelectMinusEmitOp` / `MinusEmitUsesInPlaceTarget` / `MinusEmitExpectsMatchingRegCount` | `FxMinusSign::Emit` 2154–2210 |

**Post-review ratio:** 105/172 managed/native ≈ 61% (src file LOC at HEAD; see tally).

## Tests

- `ZccUnarySignCodegenTests.TypeCastEmitIsUnreachable`
- `ZccUnarySignCodegenTests.PlusSignResolveAllowed_AcceptsQuaternion`
- `ZccUnarySignCodegenTests.ClassifyPlusResolve_UnwrapsNumeric`
- `ZccUnarySignCodegenTests.ResolveMinusSign_FoldsIntConstant`
- `ZccUnarySignCodegenTests.ResolveMinusSign_CastsBoolWhenNonConst`
- `ZccUnarySignCodegenTests.SelectMinusEmitOp_PicksNegV3`
- `ZccUnarySignCodegenTests.NegateKonstIntFromPool_MatchesEmitBranch`
- `ZccUnarySignCodegenTests.PlanMinusEmit_AllocatesWhenFixed`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 569 --to 569` (0 hard / 0 soft).

## Seams

`VMFunctionBuilder`, `ExpEmit`, `FxIntCast`, and `build->Emit` are not hosted.

## Credit rationale

Faithful managed mirrors for unary plus/minus lifecycle, resolve, and emit planning. Boolean/bitwise unary-not resolve/emit remain on earlier chunks.
