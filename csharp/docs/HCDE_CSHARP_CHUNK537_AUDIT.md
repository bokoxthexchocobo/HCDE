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

`ZccBuiltinMathResolve.cs`, `ZccBuiltinMathResolveTests.cs`
Managed LOC (tally): 153.
Conversion commit: `b8a5e9b796bb17a5067fb30d469211f1a82a89c0`.

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
