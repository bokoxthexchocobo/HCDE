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
Managed LOC (tally): 356.
Conversion commit: `cba6d6e5e5b6276a89f7cee38580f2fd2f2812dc`.

## Padding review (2026-10-21)

**Fidelity review (2026-10-21):** Credits `FxAbs` **5576–5618**, `FxATan2` **5683–5713**, `FxATan2Vec` **5774–5787**, `FxNew` **5833–5871**, `FxMinMax::Resolve` **5962–6092** (`codegen.cpp`).

| Managed | Native (`codegen.cpp`) |
|---------|------------------------|
| `RunAbsResolve` / `ClassifyAbs` / `FoldAbsInt` / `FoldAbsFloat` | **5576–5618** |
| `RunAtan2Resolve` / `Atan2RequiresNumeric` / `PlanAtan2FloatCasts` / `FoldAtan2Degrees` / `NeedsFloatCast` | **5683–5713** |
| `RunAtan2VecResolve` | **5774–5787** |
| `RunNewResolve` / `NewScopeMismatch` / `ResolveOuterSide` | **5833–5871** |
| `RunMinMaxResolve` / `ClassifyMinMaxOperand` / `MinMaxValueType` / `PlanMinMaxIntToFloatCastIndices` / `FoldMinMax*Constants` / `ShouldSwapConstantToSecond` | **5962–6092** |

### Out-of-body uncredit (537)

| | `nativeLoc` |
|--|----------:|
| **Gross** | **258** |
| **Out-of-body removed** | **0** |
| **Honest** | **258** |


**Post-review ratio:** 356/258 managed/native ≈ 138% (src file LOC at HEAD; see tally).

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
