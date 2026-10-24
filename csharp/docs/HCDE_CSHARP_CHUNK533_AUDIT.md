# Chunk 533 — FxAddSub::Resolve, FxMulDiv::Resolve, FxPow::Resolve

## Scope

FxAddSub::Resolve, FxMulDiv::Resolve, FxPow::Resolve

## Native ranges

- `src/common/scripting/backend/codegen.cpp` **FxAddSub::Resolve** 3309–3360 (52 lines)
- `src/common/scripting/backend/codegen.cpp` **FxMulDiv::Resolve** 3533–3605 (73 lines)
- `src/common/scripting/backend/codegen.cpp` **FxPow::Resolve** 3801–3836 (36 lines)

**Credited native LOC:** 161 (tally `nativeLoc` 161).

## Managed

`ZccBinaryArithmeticResolve.cs`, `ZccBinaryArithmeticResolveTests.cs`
Managed LOC (tally): 156.
Conversion commit: `a13b38a8440a0bbbc44bdfb70c5471e7061fbfa4`.

## Tests

- `ZccBinaryArithmeticResolveTests.VectorBinaryCompatible_AllowsV3PlusV2`
- `ZccBinaryArithmeticResolveTests.ClassifyMulDivVector_RejectsModOnVector`
- `ZccBinaryArithmeticResolveTests.PowFoldConstants`
- `ZccBinaryArithmeticResolveTests.PowRequiresNumericOperands`

## Verified

`dotnet build csharp/HCDE.sln -warnaserror`; `dotnet test csharp/HCDE.sln`; `python3 tools/reaudit_chunks.py audit --from 533 --to 533` (0 hard / 0 soft).

## Seams

`Promote`, `FxFloatCast`, `FxConstant`, and `Emit` are not hosted.

## Credit rationale

Credits ZScript add/sub and mul/div vector resolve typing and FxPow numeric resolve from `codegen.cpp` (3309–3360, 3533–3605, 3801–3836); mul/div constant fold remains chunk 51.
