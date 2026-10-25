# Chunk 537 — FxAbs, FxATan2, FxATan2Vec, FxNew, FxMinMax::Resolve

## Scope

FxAbs, FxATan2, FxATan2Vec, FxNew, FxMinMax::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxAbs::Resolve** 5576–5618 (43 lines)
- `src/common/scripting/backend/codegen.cpp` **FxATan2::Resolve** 5683–5713 (31 lines)
- `src/common/scripting/backend/codegen.cpp` **FxATan2Vec::Resolve** 5774–5787 (14 lines)
- `src/common/scripting/backend/codegen.cpp` **FxNew::Resolve** 5833–5871 (39 lines)
- `src/common/scripting/backend/codegen.cpp` **FxMinMax::Resolve** 5962–6092 (131 lines)

**Credited native LOC:** 258 (tally `nativeLoc` 258).

## Managed

`ZccBuiltinMathResolve.cs`
Managed LOC (tally): 143.
Conversion commit: `b6ae222e27a002b6996557084acf5fbfa7c53973`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `FxAbs` **5576–5618**, `FxATan2` **5683–5713**, `FxATan2Vec` **5774–5787**, `FxNew` **5833–5871**, `FxMinMax::Resolve` **5962–6092** (`codegen.cpp`).

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `ClassifyAbs` / `FoldAbsInt` / `FoldAbsFloat` | **5576–5618** |
| `Atan2RequiresNumeric` / `PlanAtan2FloatCasts` / `FoldAtan2Degrees` / `NeedsFloatCast` | **5683–5713** |
| `Atan2Vec` policy (tests) | **5774–5787** |
| `NewScopeMismatch` / `ResolveOuterSide` | **5833–5871** |
| `ClassifyMinMaxOperands` / `MinMaxValueType` / `MinMaxNeedsIntToFloatCast` / `PickMinMaxFloat` / `PickMinMaxInt` / `AllChoicesFoldedToSingle` | **5962–6092** |

### Out-of-body uncredit (537)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **258** |
| **Out-of-body removed** | **0** |
| **Honest** | **258** |


**Post-review ratio:** 143/258 managed/native ≈ 55% (src file LOC at HEAD; see tally).

## Tests

- `ZccBuiltinMathResolveTests.Abs_BoolPassthrough`
- `ZccBuiltinMathResolveTests.Abs_FoldInt`
- `ZccBuiltinMathResolveTests.Atan2_FoldConstants`
- `ZccBuiltinMathResolveTests.Atan2Vec_RequiresVector`
- `ZccBuiltinMathResolveTests.New_RejectsAbstractClass`
- `ZccBuiltinMathResolveTests.New_ScopeBarrierMismatch`
- `ZccBuiltinMathResolveTests.MinMax_PicksFloatDomain`
- `ZccBuiltinMathResolveTests.MinMax_FoldConstants`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 537 --to 537` (0 hard / 0 soft).

## Seams

`FxFloatCast`, `FxConstant`, `FScopeBarrier`, `BuiltinNew`, and `Emit` are not hosted.

## Credit rationale

Credits unary abs, atan2, vector atan2, `new` class scope checks, and min/max resolve from `codegen.cpp` (5576–6092).
