# Chunk 533 — FxAddSub::Resolve, FxMulDiv::Resolve, FxPow::Resolve

## Scope

FxAddSub::Resolve (3309–3360), FxMulDiv::Resolve (3533–3605), FxPow::Resolve (3801–3836).

Constant-fold tails for AddSub (3361+) and MulDiv (3606+) remain chunk **46** / **51**.

## Native ranges

- `codegen.cpp` **FxAddSub::Resolve** 3309–3360 (52 lines)
- `codegen.cpp` **FxMulDiv::Resolve** 3533–3605 (73 lines)
- `codegen.cpp` **FxPow::Resolve** 3801–3836 (36 lines)

**Credited native LOC:** 161.

## Managed

`csharp/src/HCDE.Scripting/ZccBinaryArithmeticResolve.cs`.

## Method → native map (padding sanity)

| Managed | Native |
|---------|--------|
| `CustomAdditionHook` | AddSub `CheckForCustomAddition` 3321–3328 |
| `TextureIdPlusInteger` | 3331–3334 |
| `QuaternionBinary` | 3335–3338 |
| `VectorBinaryCompatible` | 3339–3349 |
| `ClassifyAddSub` | dispatch + error 3355–3358 |
| `ClassifyMulDivVector` + helpers | MulDiv vector switch 3551–3603 |
| `ClassifyMulDiv` | numeric path 3606–3608 (typing only; fold not credited) |
| `PowFoldConstants` / `PowRequiresNumericOperands` | Pow 3801–3836 |

Fold helpers in tests mirror chunk 51 seams; they are not credited native LOC on this chunk.

## Tests

- `ZccBinaryArithmeticResolveTests.VectorBinaryCompatible_AllowsV3PlusV2`
- `ZccBinaryArithmeticResolveTests.ClassifyMulDivVector_RejectsModOnVector`
- `ZccBinaryArithmeticResolveTests.PowFoldConstants`
- `ZccBinaryArithmeticResolveTests.PowRequiresNumericOperands`

## Seams

`Promote`, `FxFloatCast`, `FxConstant`, and `Emit` are not hosted.

## Credit rationale

Credits ZScript add/sub and mul/div vector resolve typing and FxPow numeric resolve; mul/div constant fold remains chunk 51.
